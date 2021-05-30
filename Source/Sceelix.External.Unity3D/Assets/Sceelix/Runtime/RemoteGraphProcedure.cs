using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Processors.Entities;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = System.Object;

namespace Assets.Sceelix.Runtime
{
    
    public class RemoteGraphProcedure
    {
        //one-time initialization of the clases that will transform received entities into unity game objects
        private static readonly Dictionary<string, EntityProcessor> EntityProcessors = ProcessorAttribute.GetClassesOfType<EntityProcessor>();
        
        //Event when data has been loaded
        public event Action<IEnumerable<GameObject>> Ready;

        //private fields
        private readonly Guid _guid = Guid.NewGuid();
        private readonly TextAsset _packageAsset;
        private readonly String _projectPath;
        private readonly Dictionary<String, Object> _parameterValues = new Dictionary<String, Object>();

        private readonly SceelixRuntimeManager _sceelixRuntimeManager;
        private readonly Object _lockObject = new object();
        private bool _requestInProgress;
        private Dictionary<string, object> _pendingMessage;
        



        public RemoteGraphProcedure(TextAsset packageAsset, String projectPath)//,
        {
            _packageAsset = packageAsset;
            _projectPath = projectPath;

            //make sure that the runtime manager is running
            _sceelixRuntimeManager = SceelixRuntimeManager.Instance;
            if (_sceelixRuntimeManager == null)
                throw new InvalidProgramException("Could not find a Sceelix Runtime Manager. Please make sure you have added one to the scene.");

            SceelixRuntimeManager.Instance.LoadProject(packageAsset);
        }




        private void MessageClientOnTokenMessageReceived(JToken token)
        {
            var subject = token.Value<String>("Subject");
            var data = token["Data"];

            if (subject == "Graph Results")
            {
                //there is no Guid.Parse in Unity, so we have to do a String comparison
                String requestingGraphGuid = data.Value<String>("Guid");
                if (requestingGraphGuid == _guid.ToString() && Ready != null)
                {
                    RuntimeGenerationContext context = new RuntimeGenerationContext();

                    try
                    {
                        List<GameObject> createdGameObjects = new List<GameObject>();

                        var entityTokens = data["Entities"].Children().ToList();
                        for (int index = 0; index < entityTokens.Count; index++)
                        {
                            JToken entityToken = entityTokens[index];

                            EntityProcessor entityProcessor;

                            //if there is a processor for this entity Type, call it
                            if (EntityProcessors.TryGetValue(entityToken["EntityType"].ToObject<String>(), out entityProcessor))
                            {
                                createdGameObjects.AddRange(entityProcessor.Process(context, entityToken));
                            }
                            else
                            {
                                Debug.LogWarning(String.Format("There is no defined processor for entity type {0}.", entityToken["EntityType"]));
                            }
                        }

                        //we don't need to get events from this messageclient
                        _sceelixRuntimeManager.NetworkClient.TokenMessageReceived -= MessageClientOnTokenMessageReceived;

                        if (Ready != null)
                            Ready.Invoke(createdGameObjects);

                        _requestInProgress = false;
                    }
                    catch (Exception ex)
                    {
                        //log the exception anyway
                        UnityEngine.Debug.LogError(ex);
                    }
                }
            }
        }



        public void SetParameter(String key, Object value)
        {
            _parameterValues[key] = value;
        }




        /// <summary>
        /// Executes the graph with the set parameters.
        /// If a previous request was made and the result is still pending, this new request will be ignored.
        /// </summary>
        public void Execute()
        {
            if (_requestInProgress)
                return;

            lock (_lockObject)
            {
                _pendingMessage = new Dictionary<string, object>();
                _pendingMessage["Guid"] = _guid;
                _pendingMessage["Project"] = _packageAsset.name;
                _pendingMessage["ProjectPath"] = _projectPath;
                _pendingMessage["ParameterValues"] = new Dictionary<String, Object>(_parameterValues);

                if (!_sceelixRuntimeManager.IsReady(_packageAsset))
                {
                    _sceelixRuntimeManager.RuntimeManagerStatusChanged += SceelixRuntimeManagerOnRuntimeManagerStatusChanged;
                }
                else
                {
                    SendPendingMessage();
                }

            }
        }



        private void SendPendingMessage()
        {
            //we want to receive events from the client to receive the results
            _sceelixRuntimeManager.NetworkClient.TokenMessageReceived += MessageClientOnTokenMessageReceived;

            //make our request to the server
            _sceelixRuntimeManager.NetworkClient.SendMessage("Execute Graph", _pendingMessage);

            _requestInProgress = true;
            _pendingMessage = null;
        }



        private void SceelixRuntimeManagerOnRuntimeManagerStatusChanged()
        {
            lock (_lockObject)
            {
                //if we had a pending message and the runtime is now ready
                //send the message
                if (_pendingMessage != null && _sceelixRuntimeManager.IsReady(_packageAsset))
                {
                    SendPendingMessage();
                }

                _sceelixRuntimeManager.RuntimeManagerStatusChanged -= SceelixRuntimeManagerOnRuntimeManagerStatusChanged;
            }
        }
    }
}
