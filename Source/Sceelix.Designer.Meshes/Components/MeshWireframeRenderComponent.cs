using System.Collections.Generic;
using System.Linq;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Sceelix.Designer.Actors.Utils;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.Interfaces;
using Sceelix.Designer.Renderer3D.Settings;
using Sceelix.Designer.Services;
using Sceelix.Designer.Settings;
using Sceelix.Meshes.Data;

namespace Sceelix.Designer.Meshes.Components
{
    public class MeshWireFrameRenderComponent : EntityObjectComponent, IDrawableElement, IServiceable
    {
        private readonly List<MeshEntity> _entities;
        private DebugRenderer _debugRenderer;
        private List<Edge> _faceEdges;
        private Renderer3DSettings _render3DSettings;



        public MeshWireFrameRenderComponent(List<MeshEntity> entities)
        {
            _entities = entities;
        }


        public void Initialize(IServiceLocator services)
        {
            _debugRenderer = services.Get<DebugRenderer>();
            _render3DSettings = services.Get<SettingsManager>().Get<Renderer3DSettings>();
        }

        public override void OnLoad()
        {
            _faceEdges = _entities.SelectMany(val => val.FaceEdges.Distinct()).ToList();
        }



        public void Draw(RenderContext renderContext)
        {
            if (_render3DSettings.ShowFaceEdges.Value)
            {
                foreach (var edge in _faceEdges)
                {
                    var sourcePosition = edge.V0.Position.ToVector3F();
                    var targetPosition = edge.V1.Position.ToVector3F();

                    _debugRenderer.DrawLine(sourcePosition, targetPosition, Color.Black, false);
                }
            }
        }
    }
}