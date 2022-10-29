using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    class Program
    {

        /*
         * HOCAM BU BİR SÜRÜ ŞEY YAZMIŞSINIZ YAPILACAKLAR İLE ALAKALI AMA YETİŞMEDİ TABİ.
         * OLDUĞU KADAR GÖNDERDİM.
         * 
         */





        //static SocketServer server = new SocketServer();
        static TcpListenerServer server = new TcpListenerServer();

        static void Main(string[] args)
        {
            Console.WriteLine("ChatServer started..");

            server.RaiseClientConnectedEvent += HandleClientConnected;
            server.RaiseTextReceivedEvent += HandleTextReceived;
            server.RaiseClientDisconnectedEvent += HandleClientDisconnected;
            server.Start();

            Console.ReadKey();
        }

        static void HandleClientConnected(object sender, ClientConnectedEventArgs ea)
        {
            Console.WriteLine($"INFO: New client connected from {ea.NewClient}");
            // TODO: Send client connected message to all clients
        }

        static void HandleTextReceived(object sender, TextReceivedEventArgs ea)
        {
            server.SendLineToAll(ea.TextReceived, ea.ClientWhoSentText);
        }

        static void HandleClientDisconnected(object sender, ConnectionDisconnectedEventArgs ea)
        {
            Console.WriteLine($"INFO: Client disconnected from {ea.DisconnectedClient}");
            // TODO: Send client disconnected message to all clients
        }
    }
}
