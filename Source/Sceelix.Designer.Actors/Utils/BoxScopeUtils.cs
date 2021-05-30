using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
using Sceelix.Mathematics.Data;

namespace Sceelix.Designer.Actors.Utils
{
    public static class BoxScopeUtils
    {
        public static Pose ToPose(this BoxScope boxScope)
        {
            Vector3F position = boxScope.Translation.ToVector3F();

            Vector3F xDirection = boxScope.XAxis.ToVector3F();
            Vector3F yDirection = boxScope.YAxis.ToVector3F();

            QuaternionF xRotation = QuaternionF.CreateRotation(xDirection, Vector3F.UnitX);

            Vector3F rotatedYDirection = xRotation.Rotate(yDirection);

            QuaternionF yRotation = QuaternionF.CreateRotation(rotatedYDirection, Vector3F.UnitY);

            return new Pose(position, xRotation.Inverse.ToRotationMatrix33()*yRotation.Inverse.ToRotationMatrix33()*Matrix33F.CreateScale(boxScope.Sizes.X, boxScope.Sizes.Y, boxScope.Sizes.Z)); //xRotation.Inverse * yRotation.Inverse
        }



        public static Pose ToPoseWithoutScale(this BoxScope boxScope)
        {
            Vector3F position = boxScope.Translation.ToVector3F();

            Vector3F xDirection = boxScope.XAxis.ToVector3F();
            Vector3F yDirection = boxScope.YAxis.ToVector3F();

            QuaternionF xRotation = QuaternionF.CreateRotation(xDirection, Vector3F.UnitX);

            Vector3F rotatedYDirection = xRotation.Rotate(yDirection);

            QuaternionF yRotation = QuaternionF.CreateRotation(rotatedYDirection, Vector3F.UnitY);

            return new Pose(position, xRotation.Inverse.ToRotationMatrix33()*yRotation.Inverse.ToRotationMatrix33()); //xRotation.Inverse * yRotation.Inverse
        }



        public static SceneNode GetDrawableNode(this BoxScope boxScope)
        {
            return GetDrawableNode(boxScope, boxScope.ToPose());
        }



        public static SceneNode GetSizedIdentityNode(this BoxScope boxScope)
        {
            return GetDrawableNode(boxScope, new Pose(Matrix33F.CreateScale(boxScope.Sizes.X, boxScope.Sizes.Y, boxScope.Sizes.Z)));
        }



        public static SceneNode GetIdentityScopeNode()
        {
            return GetDrawableNode(BoxScope.Identity, Pose.Identity);
        }



        public static SceneNode GetDrawableNode(this BoxScope boxScope, Pose pose)
        {
            //this is the node that will contain everything
            var gizmoNode = new SceneNode
            {
                Name = "Gizmo",
                Children = new SceneNodeCollection(),
                PoseLocal = pose //new Pose(new Vector3F(3, 2, 0)),
            };

            //get the segments of an arrow
            var arrow = new PathFigure2F();
            arrow.Segments.Add(new LineSegment2F {Point1 = new Vector2F(0, 0), Point2 = new Vector2F(1, 0)});
            arrow.Segments.Add(new LineSegment2F {Point1 = new Vector2F(1, 0), Point2 = new Vector2F(0.9f, 0.02f)});
            arrow.Segments.Add(new LineSegment2F {Point1 = new Vector2F(1, 0), Point2 = new Vector2F(0.9f, -0.02f)});

            // Red arrow
            var figureNode = new FigureNode(arrow)
            {
                Name = "Gizmo X",
                StrokeThickness = 3,
                StrokeColor = new Vector3F(1, 0, 0),
                PoseLocal = new Pose(new Vector3F(0, 0, 0))
            };
            gizmoNode.Children.Add(figureNode);


            // Green arrow
            var transformedArrow = new TransformedFigure(arrow)
            {
                Pose = new Pose(Matrix33F.CreateRotationZ(MathHelper.ToRadians(90))) // * Matrix33F.CreateScale(boxScope.Sizes.Y)
            };
            figureNode = new FigureNode(transformedArrow)
            {
                Name = "Gizmo Y",
                StrokeThickness = 3,
                StrokeColor = new Vector3F(0, 1, 0),
                PoseLocal = new Pose(new Vector3F(0, 0, 0))
            };
            gizmoNode.Children.Add(figureNode);


            // Blue arrow
            transformedArrow = new TransformedFigure(arrow)
            {
                Pose = new Pose(Matrix33F.CreateRotationY(MathHelper.ToRadians(-90))) // * Matrix33F.CreateScale(boxScope.Sizes.Z)
            };
            figureNode = new FigureNode(transformedArrow)
            {
                Name = "Gizmo Z",
                StrokeThickness = 3,
                StrokeColor = new Vector3F(0, 0, 1),
                PoseLocal = new Pose(new Vector3F(0, 0, 0))
            };
            gizmoNode.Children.Add(figureNode);

            //var _box = MeshHelper.GetBox(GraphicsService);

            var boxLines = new PathFigure3F();
            boxLines.Segments.Add(new LineSegment3F {Point1 = new Vector3F(1, 0, 0), Point2 = new Vector3F(1, 1, 0)});
            boxLines.Segments.Add(new LineSegment3F {Point1 = new Vector3F(1, 0, 0), Point2 = new Vector3F(1, 0, 1)});
            boxLines.Segments.Add(new LineSegment3F {Point1 = new Vector3F(0, 1, 0), Point2 = new Vector3F(1, 1, 0)});
            boxLines.Segments.Add(new LineSegment3F {Point1 = new Vector3F(0, 1, 0), Point2 = new Vector3F(0, 1, 1)});
            boxLines.Segments.Add(new LineSegment3F {Point1 = new Vector3F(0, 0, 1), Point2 = new Vector3F(1, 0, 1)});
            boxLines.Segments.Add(new LineSegment3F {Point1 = new Vector3F(0, 0, 1), Point2 = new Vector3F(0, 1, 1)});
            boxLines.Segments.Add(new LineSegment3F {Point1 = new Vector3F(1, 1, 1), Point2 = new Vector3F(1, 0, 1)});
            boxLines.Segments.Add(new LineSegment3F {Point1 = new Vector3F(1, 1, 1), Point2 = new Vector3F(0, 1, 1)});
            boxLines.Segments.Add(new LineSegment3F {Point1 = new Vector3F(1, 1, 1), Point2 = new Vector3F(1, 1, 0)});

            //Remaining box edges


            //Remaining box edges
            var boxEdges = new FigureNode(boxLines)
            {
                Name = "Scope Edges",
                StrokeThickness = 3,
                StrokeColor = new Vector3F(1f, 1f, 1f),
                PoseLocal = new Pose(new Vector3F(0, 0, 0)),
                StrokeDashPattern = new Vector4F(1, 1, 1, 1)/10, // 
                DashInWorldSpace = true,
            };
            gizmoNode.Children.Add(boxEdges);

            /*var boxEdges2 = new FigureNode(boxLines)
            {
                Name = "Scope Edges",
                StrokeThickness = 4,
                StrokeColor = new Vector3F(0f, 0f, 0f),
                PoseLocal = new Pose(new Vector3F(0, 0, 0)),
                StrokeDashPattern = new Vector4F(1, 1, 1, 1) / 10,
                DashInWorldSpace = true,
                //StrokeDashPattern = new Vector4F(1, 1, 1, 1) / 10,
                //DashInWorldSpace = true,
            };
            gizmoNode.Children.Add(boxEdges2);*/

            /*
             * This draws the top and bottom blue faces.
             * var rectangle = new RectangleFigure
            {
                IsFilled = true,
                WidthX = 1f,
                WidthY = 1,
            };

            var rectangleNode = new FigureNode(rectangle)
            {
                Name = "Rectangle",
                StrokeThickness = 2,
                StrokeColor = new Vector3F(0.1f, 0.2f, 0.3f),
                FillColor = new Vector3F(0.1f, 0.2f, 0.3f),
                StrokeAlpha = 0.8f,
                FillAlpha = 0.3f,
                PoseLocal = new Pose(new Vector3F(0.5f, 0.5f, 0))
            };
            gizmoNode.Children.Add(rectangleNode);

            rectangleNode = new FigureNode(rectangle)
            {
                Name = "Rectangle",
                StrokeThickness = 2,
                StrokeColor = new Vector3F(0.1f, 0.2f, 0.3f),
                FillColor = new Vector3F(0.1f, 0.2f, 0.3f),
                StrokeAlpha = 0.8f,
                FillAlpha = 0.3f,
                PoseLocal = new Pose(new Vector3F(0.5f, 0.5f, 1))
            };
            gizmoNode.Children.Add(rectangleNode);*/

            /*rectangleNode = new FigureNode(rectangle)
            {
                Name = "Rectangle",
                StrokeThickness = 2,
                StrokeColor = new Vector3F(0.1f, 0.2f, 0.3f),
                FillColor = new Vector3F(0.1f, 0.2f, 0.3f),
                StrokeAlpha = 0.8f,
                FillAlpha = 0.3f,
                PoseLocal = new Pose(new Vector3F(0.5f, 0.5f, 1),Matrix33F.CreateRotationX((float)Math.PI/2f))
            };
            gizmoNode.Children.Add(rectangleNode);*/


            return gizmoNode;
        }



        public static Aabb ToAabb(this BoxScope boxScope, bool rotate)
        {
            var box = boxScope.BoundingBox;

            if (rotate)
                return new Aabb(box.Min.ToVector3F(), box.Max.ToVector3F());

            return new Aabb(box.Min.ToVector3F(), box.Max.ToVector3F());
        }



        public static BoxShape ToShape(this BoxScope boxScope)
        {
            return new BoxShape(boxScope.Sizes.ToVector3F(false));
        }
    }
}