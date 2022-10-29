using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Shared
{
    public class TcpListenerServer
    {
        private readonly IPAddress _ipAddress;
        private readonly int _port;
        private readonly TcpListener _tcpListener;

        public List<System.Net.Sockets.TcpClient> ClientList;
        public bool IsRunning { get; private set; } = false;

        public EventHandler<ClientConnectedEventArgs> RaiseClientConnectedEvent;
        public EventHandler<TextReceivedEventArgs> RaiseTextReceivedEvent;
        public EventHandler<ConnectionDisconnectedEventArgs> RaiseClientDisconnectedEvent;

        public TcpListenerServer()
        {
            _ipAddress = IPAddress.Any;
            _port = 9999;
            _tcpListener = new TcpListener(new IPEndPoint(_ipAddress, _port));
            ClientList = new List<System.Net.Sockets.TcpClient>();
        }

        public async void Start(IPAddress ipaddr = null, int port = 23000)
        {
            try
            {
                _tcpListener.Start();
                IsRunning = true;

                while (IsRunning)
                {
                    var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                    ClientList.Add(tcpClient);
                    HandleClient(tcpClient);
                    var clientConnectedEventArgs = new ClientConnectedEventArgs(tcpClient.Client.RemoteEndPoint.ToString());
                    OnRaiseClientConnectedEvent(clientConnectedEventArgs);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occured on the server. ExMessage: {ex.Message}");
            }
        }

        public void StopServer()
        {
            try
            {
                if (_tcpListener != null)
                {
                    _tcpListener.Stop();
                }

                foreach (System.Net.Sockets.TcpClient c in ClientList)
                {
                    c.Close();
                }

                ClientList.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occured while stopping the server. ExMessage: {ex.Message}");
            }
        }

        private async void HandleClient(System.Net.Sockets.TcpClient tcpClient)
        {
            string clientEndpoint = tcpClient.Client.RemoteEndPoint.ToString();

            try
            {
                NetworkStream networkStream = tcpClient.GetStream();
                StreamReader streamReader = new StreamReader(networkStream);

                char[] buff = new char[64];
                string line = string.Empty;

                while (IsRunning)
                {
                    line = await streamReader.ReadLineAsync();

                    if (line == null || line.Length == 0)
                    {
                        OnRaiseClientDisconnectedEvent(new ConnectionDisconnectedEventArgs(clientEndpoint));
                        RemoveClient(tcpClient);
                        break;
                    }

                    OnRaiseTextReceivedEvent(new TextReceivedEventArgs(clientEndpoint, line));

                    Array.Clear(buff, 0, buff.Length);
                }
            }
            catch (Exception ex)
            {
                OnRaiseClientDisconnectedEvent(new ConnectionDisconnectedEventArgs(clientEndpoint));
                RemoveClient(tcpClient);
                Console.WriteLine($"An exception occured while server is running. ExMessage: {ex.Message}");
            }
        }

        private void RemoveClient(System.Net.Sockets.TcpClient client)
        {
            if (ClientList.Contains(client))
            {
                ClientList.Remove(client);
            }
        }

        public async void SendLineToAll(string message, string clientName)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            try
            {
                foreach (System.Net.Sockets.TcpClient c in ClientList)
                {
                    StreamWriter streamWriter = new StreamWriter(c.GetStream())
                    {
                        AutoFlush = true
                    };

                    await streamWriter.WriteLineAsync($"{clientName} SAYS: {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occured while broadcasting message. ExMessage: {ex.Message}");
            }
        }

        public virtual void OnRaiseClientConnectedEvent(ClientConnectedEventArgs ea)
        {
            RaiseClientConnectedEvent?.Invoke(this, ea);
        }

        public virtual void OnRaiseTextReceivedEvent(TextReceivedEventArgs ea)
        {
            RaiseTextReceivedEvent?.Invoke(this, ea);
        }

        public virtual void OnRaiseClientDisconnectedEvent(ConnectionDisconnectedEventArgs ea)
        {
            RaiseClientDisconnectedEvent?.Invoke(this, ea);
        }
    }
}
