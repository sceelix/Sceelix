using System;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework.Input;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Plugins;
using Sceelix.Designer.Renderer3D.Annotations;
using Sceelix.Designer.Renderer3D.Interfaces;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Renderer3D.Services
{
    [Renderer3DService]
    public class SimulationManager : IServiceable, IUpdateableElement, IInputHandlerElement
    {   
        private IInputService _inputService;

        private Simulation _simulation;
        private bool _isSimulationPaused;


        public SimulationManager()
        {
            CreatePhysicsSimulation();

            _simulation.ForceEffects.Add(new Gravity()); // {Acceleration = new Vector3F(0,0,-9.81f)}
            _simulation.ForceEffects.Add(new Damping());
        }

        private void CreatePhysicsSimulation()
        {
            _simulation = new Simulation();

            // Limit max. number of internal simulation steps to 2.
            // (Simulation.Settings.Timing.FixedTimeStep is 1/60 s per default. If the game is
            // running with 60 fps, Simulation.Update() internally makes on simulation step. If 
            // the game is running with 30 fps, Simulation.Update() makes two internal steps with 
            // 1/60s. If the game is running with less than 30 fps, the simulation will run in 
            // slow motion because more than 2 internal steps would have to be made but the 
            // limit is two. - This way the game will not freeze.)
            _simulation.Settings.Timing.MaxNumberOfSteps = 2;

            // When the simulation is updated with Simulation.Update() the collision detection is 
            // updated first and then the physics simulation computes new forces, velocities and 
            // positions. That means, that after Simulation.Update() the collision detection information
            // (Simulation.CollisionDomain) is not up-to-date! If we manually query the collision 
            // detection using Simulation.CollisionDomain, then we can set the SynchronizeCollisionDomain
            // flag. If this flag is set, the collision detection info is updated at the beginning 
            // and at the end of Simulation.Update().
            //Simulation.Settings.SynchronizeCollisionDomain = true;

            // The collision domain computes collision information between non-moving bodies only once 
            // and caches this information - in case someone wants to check if two static bodies touch.
            // Nevertheless, on less powerful systems, like the Xbox 360, it can still improve performance 
            // to filter collisions between static bodies. This can be done in a broad phase filter based 
            // on collision groups, or with a simple filter like this: 
            _simulation.CollisionDomain.BroadPhase.Filter = new DelegatePairFilter<CollisionObject>(
                pair =>
                {
                    var bodyA = pair.First.GeometricObject as RigidBody;
                    if (bodyA == null || bodyA.MotionType != MotionType.Static)
                        return true;

                    var bodyB = pair.Second.GeometricObject as RigidBody;
                    if (bodyB == null || bodyB.MotionType != MotionType.Static)
                        return true;

                    return false; // Do not compute collisions between two static bodies.
                });

            // Another way to filter collisions is to use the CollisionDetection.CollisionFilter. 
            // Filtering on this level is slower because the filter is applied after the broad phase and 
            // the broad phase filter. However, it is more flexible. It can be changed at runtime, whereas
            // a broad phase filter should not change after the simulation was initialized.
            var filter = (ICollisionFilter)_simulation.CollisionDomain.CollisionDetection.CollisionFilter;
            // We can disable collision for pairs of collision objects or for collision groups. Here,
            // we disable collisions between collision group 1 and 2. The ray for mouse picking will
            // use collision group 2 (see GrabObject.cs). Any objects that should not be pickable can use
            // collision group 1.
            filter.Set(1, 2, false);
        }


        public void Initialize(IServiceLocator services)
        {
            _inputService = services.Get<IInputService>();
            
        }


        public void HandleInput(InputContext context)
        {

            if (_inputService.IsPressed(Keys.P, true))
                _isSimulationPaused = !_isSimulationPaused;
        }


        public void Update(TimeSpan deltaTime)
        {
            if (!_isSimulationPaused || _inputService.IsPressed(Keys.T, true))
            {
                // Update physics simulation.
                _simulation.Update(deltaTime);
            }
        }



        public Simulation Simulation
        {
            get { return _simulation; }
        }
    }
}
