using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography.X509Certificates;
using Timer = System.Timers.Timer;

namespace ServerRPi
{
    internal enum ClientTypes
    {
        None = 0,
        Mcu,
        Phone,
        Pc
    }
    
    internal enum MessageTypes
    {
        ReceiveClient = 0,
        SendMessage,
        DeactivateAlarm
    }
    
	[DataContract]
    internal class NetworkData
    {
	    public NetworkData()
	    {
		    
	    }

	    public NetworkData(MessageTypes types, string message, ClientTypes clientType)
	    {
			MessageType = types;
		    Message = message;
		    ClientType = clientType;
	    }
        [DataMember] internal MessageTypes MessageType;
        [DataMember] internal string Message;
        [DataMember] internal ClientTypes ClientType = ClientTypes.None;
    }
    
    public class Server
    {
        private Timer _mUdpTimer;
        private readonly UdpClient _mNetworkDiscoveryClient = new UdpClient(10500);
        private TcpListener _mTcpServer;
        private readonly List<TcpClient> _clients = new List<TcpClient>();
	    private const int buzzerPin = 2;
	    private bool isAlarmActive = false;

	    public void Handle()
	    {
		    if (_mTcpServer.Pending())
		    {
			    var connectedClient = _mTcpServer.AcceptTcpClient();
			    _clients.Add(connectedClient);
			    Console.WriteLine($"Connected: {connectedClient.Client.RemoteEndPoint}");
		    }
		    var disconnectedClients = new List<TcpClient>();
		    foreach (var client in _clients)
		    {
			    if (!client.Connected)
			    {
				    disconnectedClients.Add(client);
				    continue;
			    }
			    if (client.Available <= 0) continue;
			    var bytes = new byte[client.Available];
			    client.Client.Receive(bytes);
			    var stream = new MemoryStream();
			    stream.Write(bytes, 0, bytes.Length);
			    var serializer = new DataContractJsonSerializer(typeof(NetworkData));
			    NetworkData data = null;
			    stream.Position = 0;
			    try
			    {
				    data = (NetworkData) serializer.ReadObject(stream);
			    }
			    catch (Exception e)
			    {
				    Console.WriteLine(e);
			    }
			    if (data == null) continue;
			    switch (data.ClientType)
			    {
				    case ClientTypes.Mcu:
					    if (data.Message == "activate alarm" && !isAlarmActive)
					    {
						    WiringPi.GPIO.digitalWrite(buzzerPin, 1);
						    isAlarmActive = true;
						    Console.WriteLine("Alarma activada!");
						    try
						    {
							    SmtpClient smtpServer =
								    new SmtpClient
								    {
									    Host = "smtp.gmail.com",
									    UseDefaultCredentials = false,
										Credentials = new System.Net.NetworkCredential("alan232531@gmail.com", "Alan23253"),
									    DeliveryMethod = SmtpDeliveryMethod.Network,
									    EnableSsl = true,
									    Port = 587
								    };
							    var mail = new MailMessage
							    {
								    From = new MailAddress("alan232531@gmail.com"),
								    Subject = "Aviso!",
								    Body = $"¡La alarma ha sido activada a las {DateTime.Now}!"
							    };
							    ServicePointManager.ServerCertificateValidationCallback =
								    delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
								    { return true; };
								mail.To.Add("alan5142@hotmail.com");
							    // mail.To.Add("diskman199@gmail.com");
							    smtpServer.Send(mail);
						    }
						    catch (Exception e)
						    {
							    Console.WriteLine(e);
						    }
					    }
					    break;
				    case ClientTypes.Phone:
				    case ClientTypes.Pc:
					    if (data.Message == "deactivate" && isAlarmActive)
					    {
						    WiringPi.GPIO.digitalWrite(buzzerPin, 0);
						    isAlarmActive = false;
					    }
					    break;
				    default:
					    Console.WriteLine("Unknown client");
					    break;
			    }
		    }
		    foreach (var disconnectedClient in disconnectedClients)
		    {
			    _clients.Remove(disconnectedClient);
		    }
	    }

	    public void Init()
        {
	        WiringPi.GPIO.pinMode(buzzerPin, 1);
            _mTcpServer = new TcpListener(IPAddress.Any, 10501);
            _mTcpServer.Start();
	        Console.WriteLine($"Inited on {_mTcpServer.Server.LocalEndPoint}");
            InitUdpDiscovery();
	        _mUdpTimer.Start();
	        Console.WriteLine($"UDP search_ {_mNetworkDiscoveryClient.Client.RemoteEndPoint}");
			WiringPi.GPIO.digitalWrite(buzzerPin, 0);

        }
        private void InitUdpDiscovery()
        {
            if (_mUdpTimer != null) return;
	        Console.WriteLine("Begin discovery");
            _mUdpTimer = new Timer(5000);
	        _mUdpTimer.Elapsed += _DiscoverNetwork;
			_mUdpTimer.AutoReset = true;
	        _mUdpTimer.Start();
            _mNetworkDiscoveryClient.EnableBroadcast = true;
	        _mNetworkDiscoveryClient.Connect(new IPEndPoint(IPAddress.Broadcast, 10500));
		}

        private void _DiscoverNetwork(object obj, EventArgs eventArgs)
        {
	        Console.WriteLine("Sending broadcast...");
	        try
	        {
		        string[] ep = _mNetworkDiscoveryClient.Client.LocalEndPoint.ToString().Split(':');
				var data = new NetworkData
		        {
			        Message = ep[0],
			        MessageType = MessageTypes.ReceiveClient
		        };
		        var stream = new MemoryStream();
		        var serializer = new DataContractJsonSerializer(typeof(NetworkData));
		        serializer.WriteObject(stream, data);
		        _mNetworkDiscoveryClient.Send(stream.GetBuffer(), (int)stream.Length);
			}
	        catch (Exception e)
	        {
		        Console.WriteLine(e);
		        throw;
	        }
            
        }
    }
}
