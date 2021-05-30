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
using Sceelix.Designer.Renderer3D.GameObjects;
using Sceelix.Designer.Renderer3D.Interfaces;
using Sceelix.Designer.Services;
using Sceelix.Extensions;
using Sceelix.Meshes.Data;
using Sceelix.Points.Data;

namespace Sceelix.Designer.Points.Components
{
    public class PointCloudDrawComponent : EntityObjectComponent, IDrawableElement, IServiceable
    {
        private static readonly Color DefaultPointColor = Color.Blue;
        private readonly Vector3F _centroid;
        private readonly String _entityText;

        private readonly List<PointData> _pointDataList = new List<PointData>();

        private DebugRenderer _debugRenderer;



        public PointCloudDrawComponent(PointSetEntity pointSetEntity)
        {
            var pointDefaultColor = ToXnaColorOrDefault(pointSetEntity.GetGlobalAttribute("Color"), DefaultPointColor);

            //Initialize everything on the constructor,
            //so that we don't have to repeat the whole process in the Draw
            foreach (Vertex vertex in pointSetEntity.Vertices)
            {
                var text = String.Empty;
                foreach (var attribute in vertex.Attributes.Where(x => x.Key is AttributeKey && ((AttributeKey) x.Key).HasMeta<HighlightMeta>()))
                    text += attribute.Value.ToString() + "\n";

                _pointDataList.Add(new PointData()
                {
                    Position = vertex.Position.ToVector3F(),
                    Color = ToXnaColorOrDefault(vertex.GetGlobalAttribute("Color"), pointDefaultColor),
                    Text = text
                });
            }

            _centroid = pointSetEntity.Centroid.ToVector3F();

            _entityText = String.Empty;
            foreach (var attribute in pointSetEntity.Attributes.Where(x => x.Key is AttributeKey && ((AttributeKey) x.Key).HasMeta<HighlightMeta>()))
                _entityText += attribute.Value.SafeToString() + "\n";
        }



        public void Initialize(IServiceLocator services)
        {
            _debugRenderer = services.Get<DebugRenderer>();
        }


        /// <summary>
        /// Draw the points and their descriptions.
        /// </summary>
        /// <param name="renderContext"></param>
        public void Draw(RenderContext renderContext)
        {
            foreach (var point in _pointDataList)
            {
                var position = point.Position;
                _debugRenderer.DrawPoint(point.Position, point.Color, false);

                if (point.Text != null)
                    _debugRenderer.DrawText(point.Text, position, Color.Black, true);
            }

            if (!String.IsNullOrWhiteSpace(_entityText))
                _debugRenderer.DrawText(_entityText, _centroid, Color.Black, true);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defaultColor"></param>
        /// <returns></returns>
        private Color ToXnaColorOrDefault(object obj, Color defaultColor)
        {
            var color = obj as Mathematics.Data.Color?;
            if (color.HasValue)
                return color.Value.ToXnaColor();

            return defaultColor;
        }


        /// <summary>
        /// Simple structure to spare some repeated CPU processing.
        /// </summary>
        class PointData
        {
            public Vector3F Position
            {
                get;
                set;
            }



            public Color Color
            {
                get;
                set;
            }



            public string Text
            {
                get;
                set;
            }
        }
    }
}