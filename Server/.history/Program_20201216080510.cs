﻿using System;
using WebSocketSharp.NetCore;
using WebSocketSharp.NetCore.Server;
using System.Security.Cryptography.X509Certificates;
using System.Timers;

namespace Example
{
    public class Echo : WebSocketBehavior
    {
        byte[] _buffer = new byte[10];
        System.Text.Encoding _encoding = System.Text.Encoding.UTF8;

        protected override void OnMessage(MessageEventArgs e)
        {
            var msg = System.Text.Encoding.UTF8.GetString(e.RawData);
            Console.WriteLine("Clients: " + Sessions.Count);
            Console.WriteLine("Got message: " + msg);
            Send(msg);
        }

        protected override void OnOpen()
        {
            Console.WriteLine("New Connection: " + ID);
            Array.Clear(_buffer, 0, _buffer.Length);
            System.Text.Encoding.UTF8.GetBytes(ID);
            Send("Open " + ID);
        }
    }

    public class Program
    {
        static WebSocketServiceHost _service;
        public static void Main(string[] args)
        {
            var wssv = new WebSocketServer(9000, true);
            wssv.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.None;
            wssv.SslConfiguration.ServerCertificate = new X509Certificate2("cert.pks");
            wssv.AddWebSocketService<Echo>("/game");
            wssv.Start();

            _service = wssv.WebSocketServices["/game"];

            Timer timer = new Timer(2000);
            timer.Elapsed += SendPositions;
            timer.AutoReset = true;
            timer.Enabled = true;

            Console.ReadKey(true);
            wssv.Stop();
        }

        private static void SendPositions(Object source, ElapsedEventArgs e) {
            _service.Sessions.Broadcast("A broadcast");
        }
    }
}