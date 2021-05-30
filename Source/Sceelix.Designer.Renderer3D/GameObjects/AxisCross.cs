using System;
using System.Text;
using DigitalRune.Game;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Renderer3D.Interfaces;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.Renderer3D.GameObjects
{
    // Draws a coordinate cross (3 colored arrows) at the world space origin
    // using the debug renderer.
    public class AxisCross : GameObject, IDrawableElement
    {
        //private readonly IServiceLocator _services;
        private DebugRenderer _debugRenderer;
        private IScene _scene;


        public AxisCross(DebugRenderer debugRenderer, IScene scene)
        {
            _debugRenderer = debugRenderer;
            _scene = scene;
        }



        public bool Enabled
        {
            get;
            set;
        }



        public void Draw(RenderContext context)
        {
            if (Enabled)
            {
                //_debugRenderer.DrawAxes(Pose.Identity, 10, true);
                _debugRenderer.DrawAxes(new Pose(DigitalRuneUtils.ZUpToYUpRotationMatrix), 10, true); //Pose.Identity
                _debugRenderer.DrawText("X", new Vector3F(10, 0, 0), new Color(255, 0, 0), true);
                _debugRenderer.DrawText("Y", new Vector3F(0, 0, -10), new Color(0, 127, 0), true);
                _debugRenderer.DrawText("Z", new Vector3F(0, 11, 0), new Color(0, 0, 255), true);
            }
        }



        protected override void OnUnload()
        {
            _debugRenderer = null;
        }



        private void CreateGizmo() //SpriteFont spriteFont
        {
            var gizmoNode = new SceneNode
            {
                Name = "Gizmo",
                Children = new SceneNodeCollection(),
                PoseLocal = new Pose(new Vector3F(0, 0, 0)),
                ScaleLocal = new Vector3F(3f)
            };

            // Red arrow
            var arrow = new PathFigure2F();
            arrow.Segments.Add(new LineSegment2F {Point1 = new Vector2F(0, 0), Point2 = new Vector2F(1, 0)});
            arrow.Segments.Add(new LineSegment2F {Point1 = new Vector2F(1, 0), Point2 = new Vector2F(0.9f, 0.02f)});
            arrow.Segments.Add(new LineSegment2F {Point1 = new Vector2F(1, 0), Point2 = new Vector2F(0.9f, -0.02f)});
            var figureNode = new FigureNode(arrow)
            {
                Name = "Gizmo X",
                StrokeThickness = 2,
                StrokeColor = new Vector3F(1, 0, 0),
                PoseLocal = new Pose(new Vector3F(0, 0, 0))
            };
            gizmoNode.Children.Add(figureNode);

            // Green arrow
            var transformedArrow = new TransformedFigure(arrow)
            {
                Pose = new Pose(Matrix33F.CreateRotationZ(MathHelper.ToRadians(90)))
            };
            figureNode = new FigureNode(transformedArrow)
            {
                Name = "Gizmo Y",
                StrokeThickness = 2,
                StrokeColor = new Vector3F(0, 1, 0),
                PoseLocal = new Pose(new Vector3F(0, 0, 0))
            };
            gizmoNode.Children.Add(figureNode);

            // Blue arrow
            transformedArrow = new TransformedFigure(arrow)
            {
                Pose = new Pose(Matrix33F.CreateRotationY(MathHelper.ToRadians(-90)))
            };
            figureNode = new FigureNode(transformedArrow)
            {
                Name = "Gizmo Z",
                StrokeThickness = 2,
                StrokeColor = new Vector3F(0, 0, 1),
                PoseLocal = new Pose(new Vector3F(0, 0, 0))
            };
            gizmoNode.Children.Add(figureNode);

            // Red arc
            /*var arc = new PathFigure2F();
            arc.Segments.Add(
              new StrokedSegment2F(
                new LineSegment2F { Point1 = new Vector2F(0, 0), Point2 = new Vector2F(1, 0), },
                false));
            arc.Segments.Add(
              new ArcSegment2F
              {
                  Point1 = new Vector2F(1, 0),
                  Point2 = new Vector2F(0, 1),
                  Radius = new Vector2F(1, 1)
              });
            arc.Segments.Add(
              new StrokedSegment2F(
              new LineSegment2F { Point1 = new Vector2F(0, 1), Point2 = new Vector2F(0, 0), },
              false));
            var transformedArc = new TransformedFigure(arc)
            {
                Scale = new Vector3F(0.333f),
                Pose = new Pose(Matrix33F.CreateRotationY(MathHelper.ToRadians(-90)))
            };
            figureNode = new FigureNode(transformedArc)
            {
                Name = "Gizmo YZ",
                StrokeThickness = 2,
                StrokeColor = new Vector3F(1, 0, 0),
                FillColor = new Vector3F(1, 0, 0),
                FillAlpha = 0.5f,
                PoseLocal = new Pose(new Vector3F(0, 0, 0))
            };
            gizmoNode.Children.Add(figureNode);

            // Green arc
            transformedArc = new TransformedFigure(arc)
            {
                Scale = new Vector3F(0.333f),
                Pose = new Pose(Matrix33F.CreateRotationX(MathHelper.ToRadians(90)))
            };
            figureNode = new FigureNode(transformedArc)
            {
                Name = "Gizmo XZ",
                StrokeThickness = 2,
                StrokeColor = new Vector3F(0, 1, 0),
                FillColor = new Vector3F(0, 1, 0),
                FillAlpha = 0.5f,
                PoseLocal = new Pose(new Vector3F(0, 0, 0))
            };
            gizmoNode.Children.Add(figureNode);

            // Blue arc
            transformedArc = new TransformedFigure(arc)
            {
                Scale = new Vector3F(0.333f),
            };
            figureNode = new FigureNode(transformedArc)
            {
                Name = "Gizmo XY",
                StrokeThickness = 2,
                StrokeColor = new Vector3F(0, 0, 1),
                FillColor = new Vector3F(0, 0, 1),
                FillAlpha = 0.5f,
                PoseLocal = new Pose(new Vector3F(0, 0, 0))
            };
            gizmoNode.Children.Add(figureNode);*/

            // Labels "X", "Y", "Z"
            /*var spriteNode = new SpriteNode(new TextSprite("X", spriteFont))
            {
                Color = new Vector3F(1, 0, 0),
                Origin = new Vector2F(0, 1),
                PoseLocal = new Pose(new Vector3F(1, 0, 0))
            };
            gizmoNode.Children.Add(spriteNode);
            spriteNode = new SpriteNode(new TextSprite("Y", spriteFont))
            {
                Color = new Vector3F(0, 1, 0),
                Origin = new Vector2F(0, 1),
                PoseLocal = new Pose(new Vector3F(0, 1, 0))
            };
            gizmoNode.Children.Add(spriteNode);
            spriteNode = new SpriteNode(new TextSprite("Z", spriteFont))
            {
                Color = new Vector3F(0, 0, 1),
                Origin = new Vector2F(0, 1),
                PoseLocal = new Pose(new Vector3F(0, 0, 1))
            };
            gizmoNode.Children.Add(spriteNode);*/


            _scene.Children.Add(gizmoNode);
        }
    }
}