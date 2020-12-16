using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Security.Cryptography.X509Certificates;

namespace Example
{
    public class Echo : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            var msg = System.Text.Encoding.UTF8.GetString(e.RawData);
            Console.WriteLine("Got Message: " + msg);
            Send(msg);
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var wssv = new WebSocketServer(9000, true);
            wssv.SslConfiguration.ServerCertificate = new X509Certificate2("cert.pks");
            wssv.AddWebSocketService<Echo>("/echo");
            wssv.Start();
            Console.ReadKey(true);
            wssv.Stop();
        }
    }
}