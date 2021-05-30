using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Sceelix.Designer.Annotations;
using Sceelix.Designer.Graphs.Messages;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.Plugins;
using Sceelix.Designer.Services;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Unity3D.Settings;
using Sceelix.Network;
using Sceelix.Serialization;
using Sceelix.Unity.Helpers;
using Sceelix.Unity.Serialization;

namespace Sceelix.Designer.Unity3D
{
    [DesignerService]
    public class UnityDesignerService : IServiceable, IUpdateableElement, IDisposable
    {
        private DefaultContractResolver _contractResolver;

        private NetworkServer _networkServer;
        private IServiceLocator _services;
        private UnityConnectionSettings _settings;


        public void Initialize(IServiceLocator services)
        {
            _services = services;

            //_settings = new UnityConnectionSettings();


            _settings = services.Get<SettingsManager>().Get<UnityConnectionSettings>();

            //services.Get<SettingsManager>().Register("Unity", _settings);

            //if any of these properties change, disconnect the server so that it will update its configuration next time
            _settings.Address.Changed += delegate { RestartServer(); };
            _settings.Port.Changed += delegate { RestartServer(); };

            //register for messages that give as the result of the execution
            services.Get<MessageManager>().Register<GraphExecutionFinished>(OnGraphExecutionFinished);

            //get the serialization handlers for the Unity context
            _contractResolver = new UnityContractResolver();
            
            //ping the server every 3 seconds, to avoid having Unity hang
            StartServer();
        }


        private void RestartServer()
        {
            if (_networkServer != null)
            {
                //restart the server
                _networkServer.Close();
                StartServer();
            }
        }



        private void StartServer()
        {
            _networkServer = new NetworkServer(IPAddress.Parse(_settings.Address.Value), _settings.Port.Value);
            _networkServer.ParsedMessageReceived += NetworkServerOnParsedMessageReceived;
            _networkServer.ClientDisconnected += NetworkServerOnClientDisconnected;
            _networkServer.ClientConnected += NetworkServerOnClientConnected;
        }



        private void NetworkServerOnParsedMessageReceived(NetworkServer.ClientConnection clientConnection, string message, object data)
        {
        }



        private void NetworkServerOnClientConnected(NetworkServer.ClientConnection clientConnection)
        {
            //_services.Get<MessageManager>().Publish(new LogMessageSent("Unity Client has connected."));
        }



        private void NetworkServerOnClientDisconnected(NetworkServer.ClientConnection clientConnection)
        {
            //_services.Get<MessageManager>().Publish(new LogMessageSent("Unity Client has disconnected."));
        }



        private void OnGraphExecutionFinished(GraphExecutionFinished executionFinished)
        {
            /*var unityEntities = executionFinished.Data.OfType<UnityEntity>().ToList();

            //if there's any unity data to send
            if (unityEntities.Any())
            {
                
            }*/

            if (_networkServer != null && _networkServer.Connections.Count > 0)
            {
                SendDataToClients(executionFinished);
            }
        }



        private void SendDataToClients(GraphExecutionFinished executionFinished)
        {
            new Thread(delegate()
                {
                    try
                    {
                        var graphFileFile = SerializationHelper.ToUnityPath(executionFinished.FileItem.ProjectRelativePathWithoutExtension);

                        var jsonSerializationSettings = new JsonSerializerSettings()
                        {
                            ContractResolver = _contractResolver,
                            TypeNameHandling = TypeNameHandling.Auto,
                            PreserveReferencesHandling = PreserveReferencesHandling.None,
                            FloatParseHandling = FloatParseHandling.Double,
                            Formatting = Formatting.None,
                            ReferenceResolverProvider = () => new ServiceReferenceResolver(new SequentialIdGenerator(graphFileFile + "_"), executionFinished.Procedure.Environment)
                        };
                        
                        string message = JsonConvert.SerializeObject(new NetworkMessage("Graph Results", executionFinished), jsonSerializationSettings);
                        foreach (NetworkServer.ClientConnection clientConnection in _networkServer.Connections.ToList())
                        {
                            try
                            {
                                clientConnection.SendRawMessage(message);
                            }
                            catch (Exception ex)
                            {
                                _services.Get<MessageManager>().Publish(new ExceptionThrown(ex));
                                //_services.Get<MessageManager>().Publish(new ExceptionThrown(new Exception("Could not connect to Unity Editor at address '" + _settings.Address.Value + "', port " + _settings.Port.Value + ". Please check that the Unity Editor is listening and with matching configurations.")));
                            }
                        }

                        //#if DEBUG
                        //_services.Get<MessageManager>().Publish(new LogMessageSent("Time took for send: " + newStopWatch.ElapsedMilliseconds + " ms."));
                    }
                    catch (Exception ex)
                    {
                        //if it fails, let it go
                        _services.Get<MessageManager>().Publish(new ExceptionThrown(ex));
                    }
                })
                {IsBackground = true, CurrentCulture = CultureInfo.InvariantCulture, CurrentUICulture = CultureInfo.InvariantCulture}.Start();
        }

        



        public void Update(TimeSpan deltaTime)
        {
            if (_networkServer != null)
                _networkServer.Synchronize();
        }



        public void Dispose()
        {
            if (_networkServer != null)
                _networkServer.Close();
        }
    }
}