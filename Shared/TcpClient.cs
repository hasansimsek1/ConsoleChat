using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Shared
{
    public class TcpClient
    {
        private readonly IPAddress _serverIppAddress;
        private readonly int _serverPort;
        private readonly System.Net.Sockets.TcpClient _tcpClient;

        public string ClientEndpoint { get; private set; }
        public string ServerEndpoint { get; private set; }

        public EventHandler<TextReceivedEventArgs> RaiseTextReceivedEvent;
        public EventHandler<ConnectionDisconnectedEventArgs> RaiseServerDisconnected;
        public EventHandler<ConnectionDisconnectedEventArgs> RaiseServerConnected;

        public TcpClient()
        {
            _serverIppAddress = IPAddress.Parse("127.0.0.1");
            _serverPort = 9999;
            _tcpClient = new System.Net.Sockets.TcpClient();
            ServerEndpoint = $"{_serverIppAddress}:{_serverPort}";
        }

        public async void ConnectToServer()
        {
            try
            {
                await _tcpClient.ConnectAsync(_serverIppAddress, _serverPort);
                await ReadLineAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occured while connecting to the server. ExMessage: {ex.Message}");
                throw;
            }
        }

        public void CloseAndDisconnect()
        {
            if (_tcpClient != null)
            {
                if (_tcpClient.Connected)
                {
                    _tcpClient.Close();
                }
            }
        }

        public async Task SendToServer(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            if (_tcpClient != null)
            {
                if (_tcpClient.Connected)
                {
                    StreamWriter streamWriter = new StreamWriter(_tcpClient.GetStream())
                    {
                        AutoFlush = true
                    };
                    await streamWriter.WriteLineAsync(message);
                }
            }
        }

        private async Task ReadLineAsync()
        {
            try
            {
                StreamReader streamReader = new StreamReader(_tcpClient.GetStream());
                string receivedLine = string.Empty;

                while (true)
                {
                    receivedLine = await streamReader.ReadLineAsync();

                    if (receivedLine.Length <= 0)
                    {
                        Console.WriteLine("Disconnected from server.");
                        //OnRaisePeerDisconnectedEvent(new ConnectionDisconnectedEventArgs(_tcpClient.Client.RemoteEndPoint.ToString()));
                        _tcpClient.Close();
                        break;
                    }

                    OnRaiseTextReceivedEvent(new TextReceivedEventArgs(_tcpClient.Client.LocalEndPoint?.ToString(), receivedLine));
                    receivedLine = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occurred while reading data. ExMessage: {ex.Message}");
                throw;
            }
        }

        public virtual void OnRaiseTextReceivedEvent(TextReceivedEventArgs trea)
        {
            RaiseTextReceivedEvent?.Invoke(this, trea);
        }

        public virtual void OnRaisePeerDisconnectedEvent(ConnectionDisconnectedEventArgs pdea)
        {
            RaiseServerDisconnected?.Invoke(this, pdea);
        }

        public virtual void OnRaisePeerConnectedEvent(ConnectionDisconnectedEventArgs pdea)
        {
            RaiseServerConnected?.Invoke(this, pdea);
        }
    }
}
