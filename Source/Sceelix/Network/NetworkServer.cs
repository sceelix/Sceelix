using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace Sceelix.Network
{
    public delegate void TcpServerRawMessageEventHandler(NetworkServer.ClientConnection clientConnection, string message);

    public delegate void TcpServerTokenMessageEventHandler(NetworkServer.ClientConnection clientConnection, JToken token);

    public delegate void TcpServerParsedMessageEventHandler(NetworkServer.ClientConnection clientConnection, string message, object data);

    public delegate void TcpServerMessageEventHandler(NetworkServer.ClientConnection clientConnection, string subject, object data);

    public delegate void TcpServerInformationEventHandler(NetworkServer.ClientConnection clientConnection);

    public class NetworkServer
    {
        private readonly Queue<Action> _actions = new Queue<Action>();
        private readonly TcpListener _server;



        public NetworkServer(IPAddress ipAddress, int port)
        {
            _server = new TcpListener(ipAddress, port);
            _server.Start();

            //start the new thread for collecting clients
            new Thread(AwaitClients) {IsBackground = true, CurrentCulture = CultureInfo.InvariantCulture, CurrentUICulture = CultureInfo.InvariantCulture}.Start();
        }



        public List<ClientConnection> Connections
        {
            get;
        } = new List<ClientConnection>();



        private void AwaitClients()
        {
            try
            {
                while (true)
                {
                    //this blocks here
                    TcpClient client = _server.AcceptTcpClient();

                    var clientConnection = new ClientConnection(client, this);

                    //save a reference of the connection
                    Connections.Add(clientConnection);

                    //run this in the main thread
                    if (ClientConnected != null)
                        RunOnMainThread(() => ClientConnected(clientConnection));
                }
            }
            catch
            {
            }
        }



        public event Action<ClientConnection> ClientConnected;
        public event Action<ClientConnection> ClientDisconnected;



        public void Close()
        {
            DisconnectAll();

            _server.Stop();
        }



        public void DisconnectAll()
        {
            lock (Connections)
            {
                foreach (ClientConnection clientConnection in Connections)
                    clientConnection.Disconnect();

                Connections.Clear();
            }
        }



        public event Action<ClientConnection, Exception> ErrorOccurred;
        public event Action<ClientConnection, string, object> ParsedMessageReceived;



        private void PingClients(object state)
        {
            foreach (ClientConnection clientConnection in Connections.ToList())
                try
                {
                    clientConnection.SendRawMessage(string.Empty);
                }
                catch (Exception)
                {
                    clientConnection.Disconnect();
                }
        }



        public event Action<ClientConnection, string> RawMessageReceived;



        private void RunOnMainThread(Action action)
        {
            lock (_actions)
            {
                _actions.Enqueue(action);
            }
        }



        public void Synchronize()
        {
            lock (Connections)
            {
                foreach (ClientConnection clientConnection in Connections.Where(x => !x.IsConnected))
                    if (ClientDisconnected != null)
                        ClientDisconnected.Invoke(clientConnection);

                //remove dead connections
                Connections.RemoveAll(x => !x.IsConnected);
            }

            //execute the actions in the main thread
            lock (_actions)
            {
                while (_actions.Count > 0) _actions.Dequeue().Invoke();
            }
        }



        public event Action<ClientConnection, JToken> TokenMessageReceived;


        public class ClientConnection
        {
            private readonly TcpClient _client;
            private readonly NetworkStream _clientSockStream;
            private readonly StreamReader _clientStreamReader;
            private readonly StreamWriter _clientStreamWriter;
            private readonly NetworkServer _networkServer;



            public ClientConnection(TcpClient client, NetworkServer networkServer)
            {
                _client = client;
                _networkServer = networkServer;

                _clientSockStream = client.GetStream();

                _clientStreamReader = new StreamReader(_clientSockStream);
                _clientStreamWriter = new StreamWriter(_clientSockStream) {AutoFlush = true};

                new Thread(ProcessMessages) {CurrentCulture = CultureInfo.InvariantCulture, CurrentUICulture = CultureInfo.InvariantCulture}.Start();
            }



            public bool IsConnected => _client.Connected;



            public void Disconnect()
            {
                lock (_clientStreamWriter)
                {
                    _clientStreamWriter.Close();
                }

                _clientStreamReader.Close();
                _clientSockStream.Close();
                _client.Close();
            }



            /// <summary>
            /// Processes messages in a separate thread
            /// </summary>
            private void ProcessMessages()
            {
                try
                {
                    while (true)
                    {
                        string rawMessageString = _clientStreamReader.ReadLine();
                        if (!string.IsNullOrEmpty(rawMessageString))
                        {
                            if (_networkServer.RawMessageReceived != null)
                                _networkServer.RunOnMainThread(
                                    () => _networkServer.RawMessageReceived(this, rawMessageString));

                            if (_networkServer.TokenMessageReceived != null)
                            {
                                JToken jToken = JToken.Parse(rawMessageString);
                                _networkServer.RunOnMainThread(() => _networkServer.TokenMessageReceived(this, jToken));
                            }

                            if (_networkServer.ParsedMessageReceived != null)
                            {
                                NetworkMessage message = NetworkMessage.Deserialize(rawMessageString);
                                _networkServer.RunOnMainThread(
                                    () => _networkServer.ParsedMessageReceived(this, message.Subject, message.Data));
                            }
                        }
                    }
                }
                catch (IOException)
                {
                    //call client disconnected
                    _networkServer.RunOnMainThread(() => _networkServer.ClientDisconnected(this));
                }
                catch (Exception ex)
                {
                    _networkServer.RunOnMainThread(() => _networkServer.ErrorOccurred(this, ex));
                }
            }



            public void SendMessage(string subject, object data)
            {
                lock (_clientStreamWriter)
                {
                    string message = new NetworkMessage(subject, data).Serialize();
                    _clientStreamWriter.WriteLine(message);
                }
            }



            public void SendRawMessage(string message)
            {
                lock (_clientStreamWriter)
                {
                    _clientStreamWriter.WriteLine(message);
                    _clientStreamWriter.Flush();
                }
            }
        }
    }
}