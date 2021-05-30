using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Physics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sceelix.Annotations;
using Sceelix.Core.Data;
using Sceelix.Designer.Graphs.Messages;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.MenuHandling;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.Plugins;
using Sceelix.Designer.Renderer3D.Annotations;
using Sceelix.Designer.Renderer3D.Data;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.GameObjects.Components;
using Sceelix.Designer.Renderer3D.GUI;
using Sceelix.Designer.Renderer3D.Interfaces;
using Sceelix.Designer.Renderer3D.Messages;
using Sceelix.Designer.Renderer3D.Services;
using Sceelix.Designer.Renderer3D.Settings;
using Sceelix.Designer.Services;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Utils;
using Sceelix.Extensions;

namespace Sceelix.Designer.Renderer3D.Translators
{
    [Renderer3DService]
    public class GraphEntityTranslationService : IServiceable, IDrawableElement, IUpdateableElement, IInputHandlerElement, IDisposable
    {
        private readonly Queue<GraphExecutionFinished> _awaitingGraphResults = new Queue<GraphExecutionFinished>();
        private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);
        private EntityTranslatorContainer _entityTranslatorContainer;


        private IServiceLocator _services;

        private EntityObjectDomain _currentEntityObjectDomain;
        //private readonly CollisionObjectPicker _collisionObjectPicker;
        private IGameObjectService _gameObjectService;
        private Guid _lastExecutedOrigin;
        private IScene _scene;
        private Simulation _simulation;

        private bool _waitingForData = true;
        private Renderer3DWindow _window;
        private CameraObject _cameraGameObject;
        private IInputService _inputService;
        private RenderTargetControl _renderTargetControl;

        private Synchronizer synchronizer = new Synchronizer();
        private BarMenuService _barMenuService;

       


        public void Initialize(IServiceLocator services)
        {
            _services = services;

            _renderTargetControl = _services.Get<RenderTargetControl>();
            _window = _services.Get<Renderer3DWindow>();
            _gameObjectService = services.Get<IGameObjectService>();
            _scene = services.Get<IScene>();
            _cameraGameObject = services.Get<CameraObject>();
            _inputService = services.Get<IInputService>();
            _simulation = _services.Get<SimulationManager>().Simulation;
            _barMenuService = _services.Get<BarMenuService>();

            //_newEntityObjectTranslators.Values.Distinct().ForEach(val => val.Initialize(_services));
            _entityTranslatorContainer = _services.Get<EntityTranslatorContainer>();
            //InitializeEntityObjectFactories(services.Get<PluginManager>().PluginAssemblies);

            services.Get<MessageManager>().Register<GraphExecutionFinished>(OnGraphExecutionFinished);

            Thread loadingThread = new Thread(EntityToObjectThreadFunction) { IsBackground = true, Name = this.GetType().Name, CurrentCulture = CultureInfo.InvariantCulture, CurrentUICulture = CultureInfo.InvariantCulture };
            loadingThread.Start();

            _barMenuService.RegisterMenuEntry("Window/Clear", ClearObjects, EmbeddedResources.Load<Texture2D>("Resources/Trash_16x16.png"));
            _barMenuService.RegisterMenuEntry("Camera/Frame", () => Frame(true), EmbeddedResources.Load<Texture2D>("Resources/FullScreen_16x16.png"));
        }


        public void RunInMainThread(Action action)
        {
            synchronizer.Enqueue(action);
        }




        public IEnumerable<EntityObject> EntityObjects
        {
            get
            {
                if (_currentEntityObjectDomain != null)
                    foreach (var componentObject in _currentEntityObjectDomain.ComponentObjects)
                        yield return componentObject;
            }
        }
        

        private void OnGraphExecutionFinished(GraphExecutionFinished obj)
        {
            //if the window is closed, do not process data!
            if (!_window.ActualIsVisible)
                return;

            lock (_awaitingGraphResults)
            {
                //there is not need to add old data, so clear the list at the beginning
                _awaitingGraphResults.Clear();
                _awaitingGraphResults.Enqueue(obj);

                //signal the thread that it should 
                _manualResetEvent.Set();
            }
        }



        public void Dispose()
        {
            //tell the thread to finish
            _waitingForData = false;
            _manualResetEvent.Set();
        }



        private void EntityToObjectThreadFunction()
        {
            while (_waitingForData)
            {
                _manualResetEvent.WaitOne();

                if (_waitingForData)
                    HandleGraphExecution();
            }
        }



        private void HandleGraphExecution()
        {
            GraphExecutionFinished message;
            lock (_awaitingGraphResults)
                message = _awaitingGraphResults.Dequeue();


            Stopwatch watch = Stopwatch.StartNew();
            RunInMainThread(() =>
            {
                _window.SetProgressVisibility(true);
                _window.SetStatusBarText("Running...");
            });


            var newMaterializationsDomain = new EntityObjectDomain(_services, message.Procedure.Environment);

            try
            {
                HandleEntityObjects(message, newMaterializationsDomain);

                //delete all objects

                //_currentEntityObjectDomain = newMaterializationsDomain;
                //_gameObjectService.Objects.RemoveRangeSync(objectsToClear);
            }
            catch (Exception ex)
            {
                _services.Get<MessageManager>().Publish(new ExceptionThrown(ex));
                DesignerProgram.Log.Error("Error while Creating Entity.", ex);
            }

            //the only thing left to do, that MUST be done in the main thread, is assigning the data
            RunInMainThread(delegate
            {
                bool shouldGarbageCollect = _currentEntityObjectDomain != null;

                //load the new data domain
                newMaterializationsDomain.Load(_scene, _gameObjectService, _simulation);

                //clear the old data domain, if any
                if (_currentEntityObjectDomain != null)
                    _currentEntityObjectDomain.Unload(_scene, _gameObjectService,  _simulation);

                //set the current domain data
                _currentEntityObjectDomain = newMaterializationsDomain;

                //we may have megabytes of old data, so try to clean up
                if (shouldGarbageCollect)
                    GC.Collect();
            });


            //if there are no more graph results left to process, the thread will block afterwards until new data arrives
            lock (_awaitingGraphResults)
            {
                if (_awaitingGraphResults.Count == 0)
                    _manualResetEvent.Reset();
            }

            //enqueue a 
            RunInMainThread(delegate { _services.Get<Synchronizer>("Renderer").Enqueue(() => _services.Get<MessageManager>().Publish(new RenderDataReady(_services.Get<RenderTargetControl>().GetPrintScreen()))); }); //() => _services.Get<RenderTargetControl>().PrintScreen()
            RunInMainThread(() => _services.Get<RenderTargetControl>().ShouldRender = true);
            RunInMainThread(delegate
            {
                _window.SetProgressVisibility(false);
                _window.SetStatusBarText("Processing took " + (int) (watch.Elapsed.TotalMilliseconds) + "ms.");
            });


            //frame the result, if such an option is enabled and a new procedure is being run
            if (message.FileItem != null &&
                message.FileItem.Guid != _lastExecutedOrigin && _services.Get<SettingsManager>().Get<Renderer3DSettings>().FrameNewData.Value)
            {
                RunInMainThread(() => Frame(false));

                _lastExecutedOrigin = message.FileItem.Guid;
            }
        }



        public void ClearObjects()
        {
            //clear the old data domain, if any
            if (_currentEntityObjectDomain != null)
                _currentEntityObjectDomain.Unload(_scene, _gameObjectService, _simulation);

            //set the current domain data
            _currentEntityObjectDomain = null;

            //we may have megabytes of old data, so try to clean up
            GC.Collect();

            _services.Get<RenderTargetControl>().ShouldRender = true;
        }



        private void HandleEntityObjects(GraphExecutionFinished message, EntityObjectDomain entityObjectDomain)
        {
            _entityTranslatorContainer.ProcessEntityTranslation(message.Data, entityObjectDomain);
        }



      


       



        public void Draw(RenderContext renderContext)
        {
            if (_currentEntityObjectDomain != null)
                _currentEntityObjectDomain.Draw(renderContext);
        }
        


        public void Update(TimeSpan deltaTime)
        {
            if (_currentEntityObjectDomain != null)
                _currentEntityObjectDomain.Update(deltaTime);

            synchronizer.Update(1);
        }
        


        public void HandleInput(InputContext context)
        {
            if(_currentEntityObjectDomain != null)
                _currentEntityObjectDomain.ProcessInput();

            if (!_inputService.IsKeyboardHandled && _window.IsActive)
            {
                if (_inputService.IsPressed(Keys.F, false))
                {
                    Frame(true);
                    _inputService.IsKeyboardHandled = true;
                    _renderTargetControl.ShouldRender = true;
                }
            }
        
        }


        public void Frame(bool onlySelection)
        {
            Aabb? aabb = null;
            Aabb? aabbAll = null;


            foreach (var sceelixGameObject in EntityObjects)
            {
                var selectableComponent = sceelixGameObject.GetComponentOrDefault<SelectableEntityComponent>();
                if (selectableComponent != null)
                {
                    var collisionComponent = selectableComponent.GetComponentOrDefault<CollisionComponent>();
                    if (collisionComponent != null)
                    {
                        if (!onlySelection || (selectableComponent.IsSelected.Value))
                            aabb = !aabb.HasValue ? collisionComponent.Aabb : Aabb.Merge(aabb.Value, collisionComponent.Aabb);

                        aabbAll = !aabbAll.HasValue ? collisionComponent.Aabb : Aabb.Merge(aabbAll.Value, collisionComponent.Aabb);
                    }
                }
            }

            if (aabb.HasValue)
                _cameraGameObject.Frame(aabb.Value);
            else if (onlySelection && aabbAll.HasValue)
                _cameraGameObject.Frame(aabbAll.Value);
        }
    }
}