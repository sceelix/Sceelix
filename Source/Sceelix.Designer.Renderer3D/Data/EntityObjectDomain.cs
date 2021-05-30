using System;
using System.Collections.Generic;
using DigitalRune.Game;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Physics;
using Sceelix.Core.Environments;
using Sceelix.Core.Resources;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.GUI;
using Sceelix.Designer.Renderer3D.Loading;
using Sceelix.Designer.Services;
using Sceelix.Logging;

namespace Sceelix.Designer.Renderer3D.Data
{
    public class EntityObjectDomain
    {
        private readonly CollisionDomain _collisionDomain;
        private readonly List<EntityObject> _componentObjects;
        private readonly ContentLoader _contentLoader;
        private readonly IProcedureEnvironment _environment;
        private readonly SceneNode _mainNode;
        private readonly GameObjectGroup _mainObject;
        private readonly MessageManager _messageManager;
        private readonly List<RigidBody> _rigidBodies;
        private readonly IServiceLocator _services;
        private readonly MessageManager _parentMessageManager;
        private readonly CameraObject _cameraObject;
        private CollisionObjectPicker _collisionObjectPicker;


        public event Action Loaded;
        public event Action Unloaded;
        public event Action<TimeSpan> Updated;
        public event Action<RenderContext> Drawn;

        public EntityObjectDomain(IServiceLocator services, IProcedureEnvironment environment)
        {
            _services = services;
            _environment = environment;
            _mainNode = new SceneNode() {Children = new SceneNodeCollection()};
            _mainObject = new GameObjectGroup();
            _componentObjects = new List<EntityObject>();
            _rigidBodies = new List<RigidBody>();
            _messageManager = new MessageManager();
            _parentMessageManager = services.Get<MessageManager>();
            _contentLoader = new ContentLoader(services, environment.GetService<IResourceManager>());
            _cameraObject = services.Get<CameraObject>();

            _parentMessageManager.Register<Object>(OnMessageReceivedFromParent);

            _messageManager.Register<Object>(OnMessageReceivedFromThis);

            //_rigidBodyShape = new CompositeShape();
            //_rigidBodyShape.Partition = new AabbTree<int>();

            _collisionDomain = new CollisionDomain(new CollisionDetection()) {BroadPhase = new DualPartition<CollisionObject>()};
            _collisionDomain.BroadPhase.Filter = new DelegatePairFilter<CollisionObject>
            (pair =>
            {
                if (pair.First.GeometricObject.Shape is TriangleMeshShape
                    && pair.Second.GeometricObject.Shape is TriangleMeshShape)
                    return false;

                return true;
            });
        }


        //if this domain's message manager received a message, forward it to the parent.
        private void OnMessageReceivedFromThis(object obj)
        {
            _parentMessageManager.Publish(obj, this);
        }


        //if the parent message manager has received a message, forward it to this manager's subscribers 
        private void OnMessageReceivedFromParent(object obj)
        {
            //republish the message
            _messageManager.Publish(obj, this);
        }

        /// <summary>
        /// This should be run in the main thread.
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="gameObjectManager"></param>
        /// <param name="collisionObjectPicker"></param>
        internal void Load(IScene scene, IGameObjectService gameObjectManager, Simulation simulation)
        {
            scene.Children.Add(_mainNode);
            gameObjectManager.Objects.Add(_mainObject);
            _collisionObjectPicker = new CollisionObjectPicker(Services, Services.Get<Renderer3DControl>(), _collisionDomain);

            simulation.RigidBodies.AddRange(_rigidBodies);


            if (Loaded != null)
                Loaded.Invoke();
        }



        /// <summary>
        ///  This should be run in the main thread.
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="gameObjectManager"></param>
        /// <param name="collisionObjectPicker"></param>
        internal void Unload(IScene scene, IGameObjectService gameObjectManager, Simulation simulation)
        {
            scene.Children.Remove(_mainNode);

            _mainNode.Dispose(true);

            foreach (RigidBody rigidBody in _rigidBodies)
                simulation.RigidBodies.Remove(rigidBody);

            gameObjectManager.Objects.Remove(_mainObject);

            _parentMessageManager.Unregister(this);
            _messageManager.Unregister(this);


            if (Unloaded != null)
                Unloaded.Invoke();
        }



        internal void Update(TimeSpan timeSpan)
        {
            if (Updated != null)
                Updated.Invoke(timeSpan);
        }



        internal void Draw(RenderContext renderContext)
        {
            if (Drawn != null)
                Drawn.Invoke(renderContext);
        }


        public void ProcessInput()
        {
            _collisionObjectPicker.ProcessInput();
        }


        public SceneNodeCollection SceneNodes
        {
            get { return _mainNode.Children; }
        }



        public List<GameObject> GameObjects
        {
            get { return _mainObject.GameObjects; }
        }



        public CollisionObjectCollection CollisionObjects
        {
            get { return _collisionDomain.CollisionObjects; }
        }



        public IProcedureEnvironment Environment
        {
            get { return _environment; }
        }



        public IServiceLocator Services
        {
            get { return _services; }
        }



        public CameraObject CameraObject
        {
            get { return _cameraObject; }
        }



        public List<EntityObject> ComponentObjects
        {
            get { return _componentObjects; }
        }



        public List<RigidBody> RigidBodies
        {
            get { return _rigidBodies; }
        }



        public MessageManager MessageManager
        {
            get { return _messageManager; }
        }



        public ContentLoader ContentLoader
        {
            get { return _contentLoader; }
        }



        public ILogger Logger
        {
            get { return _environment.GetService<ILogger>(); }
        }
        



    }
}