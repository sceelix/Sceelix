using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Sceelix.Core.Attributes;
using Sceelix.Core.Extensions;
using Sceelix.Designer.Actors.Utils;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Renderer3D;
using Sceelix.Designer.Renderer3D.Attributes;
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.GameObjects.Components;
using Sceelix.Designer.Renderer3D.Interfaces;
using Sceelix.Designer.Renderer3D.Settings;
using Sceelix.Designer.Services;
using Sceelix.Designer.Settings;
using Sceelix.Paths.Data;

namespace Sceelix.Designer.Paths.Components
{
    public class PathDrawComponent : EntityObjectComponent, IDrawableElement, IServiceable
    {

        private readonly PathEntity _pathEntity;

        private DebugRenderer _debugRenderer;
        private Renderer3DSettings _renderSettings;
        private List<VertexInfo> _vertexInfos;
        private List<EdgeInfo> _edgeInfos;
        private bool _isSelected;


        public PathDrawComponent(PathEntity pathEntity)
        {
            _pathEntity = pathEntity;
            _vertexInfos = _pathEntity.Vertices.Select(x => new VertexInfo(x)).ToList();
            _edgeInfos = _pathEntity.Edges.Select(x => new EdgeInfo(x)).ToList();
        }



        public void Initialize(IServiceLocator services)
        {
            _debugRenderer = services.Get<DebugRenderer>();
            _renderSettings = services.Get<SettingsManager>().Get<Renderer3DSettings>();
        }


        public override void OnLoad()
        {
            var selectableComponent = GetComponentOrDefault<SelectableEntityComponent>();
            selectableComponent.IsSelected.Changed += (field, value, newValue) => _isSelected = newValue;
        }



        public void Draw(RenderContext renderContext)
        {
            #if LINUX || MACOS

            foreach (var edge in _pathEntity.Edges)
            {
                var sourcePosition = edge.Source.Position.ToVector3F();
                var targetPosition = edge.Target.Position.ToVector3F();

                _debugRenderer.DrawLine(sourcePosition, targetPosition, Color.Black, false);
            }

            #endif

            var showVertexConnectivity = _renderSettings.ShowPathConnectivity.Value;
            var showEdgeDirection = _renderSettings.ShowPathDirection.Value;

            foreach (var vertexInfo in _vertexInfos)
            {
                _debugRenderer.DrawPoint(vertexInfo.Position, vertexInfo.Color, false);

                if (showVertexConnectivity)
                    _debugRenderer.DrawText(vertexInfo.ConnectivityInfo, vertexInfo.Position, Color.Red, true);
                
                _debugRenderer.DrawText(vertexInfo.AttributeInfo, vertexInfo.Position, Color.Black, true);
            }

            foreach (var edge in _edgeInfos)
            {
                _debugRenderer.DrawText(edge.AttributeInfo, edge.Position, Color.Black, true);
                
                if(!_isSelected)
                    _debugRenderer.DrawLine(edge.SourcePosition, edge.TargetPosition, edge.Color, true);

                if (showEdgeDirection)
                    _debugRenderer.DrawArrow(edge.SourcePosition, edge.TargetPosition, Color.Orange, true);
            }
        }



        private class VertexInfo
        {
            public VertexInfo(PathVertex vertex)
            {
                Position = vertex.Position.ToVector3F();
                ConnectivityInfo = vertex.Edges.Count.ToString();
                AttributeInfo = String.Empty;

                foreach (var attribute in vertex.Attributes.Where(x => x.Key is AttributeKey && ((AttributeKey)x.Key).HasMeta<HighlightMeta>()))
                    AttributeInfo += attribute.Value.ToString() + "\n";

                var rendererMetaValue = vertex.GetAttributeWithMeta<RendererMeta>(new GlobalAttributeKey("Color"));
                if (rendererMetaValue != null && rendererMetaValue is Sceelix.Mathematics.Data.Color)
                {
                    Color = ((Sceelix.Mathematics.Data.Color)rendererMetaValue).ToXnaColor();
                }
                else
                    Color = Color.Black;
            }



            public string AttributeInfo
            {
                get;
                set;
            }



            public string ConnectivityInfo
            {
                get;
                private set;
            }



            public Vector3F Position
            {
                get;
                private set;
            }

            public Color Color
            {
                get; private set;
            }
        }

        private class EdgeInfo
        {
            public EdgeInfo(PathEdge edge)
            {
                SourcePosition = edge.Source.Position.ToVector3F();
                TargetPosition = edge.Target.Position.ToVector3F();

                Position = (edge.Target.Position + (edge.Source.Position - edge.Target.Position) / 2).ToVector3F();
                AttributeInfo = String.Empty;

                foreach (var attribute in edge.Attributes.Where(x => x.Key is AttributeKey && ((AttributeKey)x.Key).HasMeta<HighlightMeta>()))
                    AttributeInfo += attribute.Value.ToString() + "\n";

                var rendererMetaValue = edge.GetAttributeWithMeta<RendererMeta>(new GlobalAttributeKey("Color"));
                if (rendererMetaValue != null && rendererMetaValue is Sceelix.Mathematics.Data.Color)
                {
                    Color = ((Sceelix.Mathematics.Data.Color) rendererMetaValue).ToXnaColor();
                }
                else
                    Color = Color.Black;
            }



            public Vector3F TargetPosition
            {
                get;
                private set;
            }



            public Vector3F SourcePosition
            {
                get;
                private set;
            }



            public string AttributeInfo
            {
                get;
                set;
            }



            public Vector3F Position
            {
                get;
                private set;
            }

            public Color Color
            {
                get; private set;
            }
        }
    }
}