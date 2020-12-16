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
            Console.WriteLine("Got Message: " + msg + ". From: " + ID);
            Send(msg);
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var wssv = new WebSocketServer(9000, true);
            wssv.SslConfiguration.EnabledSslProtocols = (
                System.Security.Authentication.SslProtocols.Default |
                System.Security.Authentication.SslProtocols.Tls12
            );
            wssv.SslConfiguration.ServerCertificate = new X509Certificate2("cert.pks");
            wssv.AddWebSocketService<Echo>("/game");
            wssv.Start();
            Console.ReadKey(true);
            wssv.Stop();
        }
    }
}