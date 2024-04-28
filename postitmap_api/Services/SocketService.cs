using Microsoft.AspNetCore.Hosting.Server;
using NetCoreServer;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace postitmap_api.Services
{
    public class SocketService : WsServer
    {
        public SocketService(IPAddress address, int port) : base(address, port)
        {
            Console.Write("Server starting...");
            base.Start();
        }
        protected override TcpSession CreateSession() { return new ChatSession(this); }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Chat WebSocket server caught an error with code {error}");
        }


    }

    class ChatSession : WsSession
    {
        public ChatSession(WsServer server) : base(server) { }

        public override void OnWsConnected(NetCoreServer.HttpRequest request)
        {
            Console.WriteLine($"Chat WebSocket session with Id {Id} connected!");

            // Send invite message
            string message = "Hello from WebSocket chat! Please send a message or '!' to disconnect the client!";
            SendTextAsync(message);
        }

        public override void OnWsDisconnected()
        {
            Console.WriteLine($"Chat WebSocket session with Id {Id} disconnected!");
        }

        public override void OnWsReceived(byte[] buffer, long offset, long size)
        {
            string message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            Console.WriteLine("Incoming: " + message);

            // Multicast message to all connected sessions
            ((WsServer)Server).MulticastText(message);

            // If the buffer starts with '!' the disconnect the current session
            if (message == "!")
                Close();
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Chat WebSocket session caught an error with code {error}");
        }
    }
}
