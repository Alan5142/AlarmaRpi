using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Networking.Sockets;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AlarmDeactivate
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

	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
    {
	    private StreamSocket m_client;
		private UdpClient m_udpListener;
	    private Timer m_timer;
        public MainPage()
        {
            this.InitializeComponent();
	        StartUdpListener();
        }

	    void StartUdpListener()
	    {
		    m_udpListener = new UdpClient(new IPEndPoint(IPAddress.Any, 10500))
		    {
			    EnableBroadcast = true,
			    Client = {SendTimeout = 10000}
		    };

		    Task.Run(async () =>
		    {
			    while (true)
			    {
				    await m_udpListener.SendAsync(new byte[64], 10, new IPEndPoint(IPAddress.Broadcast, 10500));
					m_udpListener.ReceiveAsync();
					if (m_udpListener.Available > 0)
					{
						var buffer = new byte[m_udpListener.Available];
						m_udpListener.Client.Receive(buffer);
						string str = Encoding.ASCII.GetString(buffer);
						if (str == string.Empty)
							continue;
						var stream = new MemoryStream();
						stream.Write(buffer, 0, buffer.Length);
						var serializer = new DataContractJsonSerializer(typeof(NetworkData));
						NetworkData data = null;
						stream.Position = 0;
						data = (NetworkData) serializer.ReadObject(stream);
						m_client = new StreamSocket();
						await m_client.ConnectAsync(new HostName(data.Message), "10501");
						return;
				    }
			    }
		    });

	    }
		private void UdpServerConnection(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
		{
			Stream streamIn = args.GetDataStream().AsStreamForRead();
			StreamReader reader = new StreamReader(streamIn);
		}

	    public void StopAlarm()
	    {
			try
			{
				if (m_client != null)
				{
					NetworkData data = new NetworkData();
					data.ClientType = ClientTypes.Pc;
					data.Message = "deactivate";
					data.MessageType = MessageTypes.DeactivateAlarm;
					var serializer = new DataContractJsonSerializer(typeof(NetworkData));
					serializer.WriteObject(m_client.OutputStream.AsStreamForWrite(), data);
					m_client.OutputStream.AsStreamForWrite().Flush();
				}
			}
			catch (Exception exception)
			{
				string str = exception.Message;
			}
		}

		private void sendStop_Click(object sender, RoutedEventArgs e)
		{
			StopAlarm();
		}
	}
}
