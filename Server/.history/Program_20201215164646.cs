﻿using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Security.Cryptography.X509Certificates;
using System.Timers;

namespace Example
{
    public class Echo : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            var msg = System.Text.Encoding.UTF8.GetString(e.RawData);
            Console.WriteLine("Clients: " + Sessions.Count);
        }

        protected override void OnOpen()
        {
            Console.WriteLine("Open " + ID);
            Send("Open " + ID);
        }
    }

    public class Program
    {
        //WebSocketServiceHost _service;
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

            WebSocketServiceHost _service = wssv.WebSocketServices["/game"];

            Timer timer = new Timer(500);
            timer.Elapsed += SendPositions;

            Console.ReadKey(true);
            wssv.Stop();
        }

        private static void SendPositions(Object source, ElapsedEventArgs e) {
            
        }
    }
}