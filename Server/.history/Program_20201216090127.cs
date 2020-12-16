using System;
using WebSocketSharp.NetCore;
using WebSocketSharp.NetCore.Server;
using System.Security.Cryptography.X509Certificates;
using System.Timers;
using System.Collections;
using NetStack.Quantization;
using NetStack.Serialization;

namespace Example
{
    public class Echo : WebSocketBehavior
    {
        byte[] _buffer = new byte[50];
        System.Text.Encoding _encoding = System.Text.Encoding.UTF8;
        BitBuffer _bitBuffer = new BitBuffer(20);
        public QuantizedVector3 Position;

        protected override void OnMessage(MessageEventArgs e)
        {
            _bitBuffer.Clear();
            _bitBuffer.FromArray(e.RawData, e.RawData.Length);

            QuantizedVector3 position = new QuantizedVector3(
                _bitBuffer.ReadUInt(),
                _bitBuffer.ReadUInt(),
                _bitBuffer.ReadUInt()
            );
            Position = position;
        }

        protected override void OnOpen()
        {
            Console.WriteLine("New Connection: " + ID);
            Array.Clear(_buffer, 0, _buffer.Length);
            byte[] idBytes = _encoding.GetBytes(ID);
            _buffer[1] = (byte)idBytes.Length;
            idBytes.CopyTo(_buffer, 2);
            foreach (IWebSocketSession session in Sessions.Sessions) {
                if (session.ID == ID) {
                    // Send ownership creation
                    _buffer[0] = 1;
                }
                else {
                    // Send non-ownership creation
                    _buffer[0] = 2;
                }

                Sessions.SendTo(_buffer, session.ID);
            }
        }

        protected override void OnClose(CloseEventArgs e)
        {
            Console.WriteLine("Connection closed: " + ID);
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
            foreach(Echo session in _service.Sessions.Sessions) {
                Console.WriteLine(session.Position);
            }
            //_service.Sessions.Broadcast("A broadcast");
        }
    }
}