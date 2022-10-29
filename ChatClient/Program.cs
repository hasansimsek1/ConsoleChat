using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient
{
    class Program
    {
        static Shared.TcpClient client = new Shared.TcpClient();

        static void Main(string[] args)
        {
            client.RaiseTextReceivedEvent += HandleTextReceived;

            client.ConnectToServer();
            Console.WriteLine($"Connected to the chat server on: {client.ServerEndpoint}");
            Console.WriteLine("Joined chat. If you want to leave the chat, type EXIT..");
            Console.WriteLine("Type here to send message");

            while (true)
            {
                var message = Console.ReadLine();

                if (message == "EXIT")
                {
                    client.CloseAndDisconnect();
                    break;
                }

                Task messageTask = client.SendToServer(message);
                messageTask.Wait();
            }

            Console.ReadKey();
        }

        static void HandleTextReceived(object sender, TextReceivedEventArgs args)
        {
            Console.WriteLine(args.TextReceived);
        }
    }
}
