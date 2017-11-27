using System;
namespace ServerRPi
{
    internal class Program
    {
        public static void Main(string[] args)
        {
	        Console.WriteLine("Initiating");
            var server = new Server();
	        WiringPi.Init.WiringPiSetupSys();
            server.Init();
            while (true)
            {
                server.Handle();
            }
        }
    }
}