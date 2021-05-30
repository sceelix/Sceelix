using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DigitalRune.Game;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using Sceelix.Core.Handles;
using Sceelix.Designer.Annotations;
using Sceelix.Designer.Graphs.Inspector.Nodes;
using Sceelix.Designer.Graphs.Messages;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.Plugins;
using Sceelix.Designer.ProjectExplorer.Messages;
using Sceelix.Designer.Renderer3D.Annotations;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.GUI;
using Sceelix.Designer.Renderer3D.Interfaces;
using Sceelix.Designer.Renderer3D.Services;
using Sceelix.Designer.Services;
using Sceelix.Extensions;
using Sceelix.Loading;

namespace Sceelix.Designer.Renderer3D.Handles
{
    [Renderer3DService]
    public class HandleTranslatorManager : IServiceable, IUpdateableElement, IInputHandlerElement, IDisposable
    {
        private GameObjectManager _gameObjectManager;
        private IScene _scene;
        private IServiceLocator _services;

        //private Dictionary<int,List<VisualHandle>> _guides = new Dictionary<int, List<VisualHandle>>();
        private readonly Dictionary<Type, HandleTranslator> _guideObjectFactories = new Dictionary<Type, HandleTranslator>();



        public void Initialize(IServiceLocator services)
        {
            _services = services;
            var messageManager = services.Get<MessageManager>();

            InitializeGuideObjectFactories(SceelixDomain.SceelixAssemblies.Where(x => x.HasCustomAttribute<DesignerLibraryAttribute>()).ToList());

            _gameObjectManager = (GameObjectManager) services.Get<IGameObjectService>();
            _scene = services.Get<IScene>();

            messageManager.Register<ShowPropertiesRequest>(OnShowPropertiesRequest);
            //messageManager.Register<ShowVisualGuides>(OnShowVisualGuides);
        }

        


        public void HandleInput(InputContext context)
        {
            foreach (var handleObjectFactory in _guideObjectFactories.Values)
            {
                handleObjectFactory.HandleInput(context);
            }
        }


        public void Update(TimeSpan deltaTime)
        {
            foreach (var guideObjectFactory in _guideObjectFactories)
                guideObjectFactory.Value.Update(deltaTime);
        }


        private void OnShowPropertiesRequest(ShowPropertiesRequest obj)
        {
            if (obj.ControlToShow is NodeInspectorControl)
            {
                foreach (var guideObjectFactory in _guideObjectFactories)
                    guideObjectFactory.Value.Clear();

                var inspectorControl = (NodeInspectorControl) obj.ControlToShow;
                //inspectorControl.VisualNode.Node.Arguments.Where(x => x.)

                foreach (var visualGuideGroup in inspectorControl.VisualNode.VisualHandles.GroupBy(x => x.GetType()))
                    _guideObjectFactories[visualGuideGroup.Key].Add(visualGuideGroup);
            }
            else // if (obj.ControlToShow == null)
            {
                ClearHandles();
            }
        }



        private void ClearHandles()
        {
            foreach (var guideObjectFactory in _guideObjectFactories)
                guideObjectFactory.Value.Clear();

            _services.Get<Renderer3DControl>().ShouldRender = true;
        }



        private void InitializeGuideObjectFactories(List<Assembly> pluginAssemblies)
        {
            //lock (_guideObjectFactories)
            {
                foreach (Assembly pluginAssembly in pluginAssemblies)
                {
                    foreach (Type type in pluginAssembly.GetTypes().Where(val => !val.IsAbstract && typeof(HandleTranslator).IsAssignableFrom(val)))
                    {
                        HandleTranslator handleTranslator = (HandleTranslator) Activator.CreateInstance(type, _services);
                        _guideObjectFactories.Add(handleTranslator.GuideType, handleTranslator);
                    }
                }
            }
        }



        /*private void OnShowVisualGuides(ShowVisualHandles obj)
        {
            foreach (var guideObjectFactory in _guideObjectFactories)
                guideObjectFactory.Value.Clear();

            foreach (VisualHandle visualGuide in obj.VisualNode.VisualHandles)
                _guideObjectFactories[visualGuide.GetType()].Add(visualGuide);
        }*/


        public void Dispose()
        {
            _services.Get<MessageManager>().Unregister(this);
        }
    }
}