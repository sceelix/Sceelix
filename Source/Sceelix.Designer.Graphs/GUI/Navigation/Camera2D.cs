using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Sceelix.Designer.Graphs.GUI.Basic;
using Sceelix.Designer.Graphs.GUI.Model;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.Messages;

namespace Sceelix.Designer.Graphs.GUI.Navigation
{
    public class Camera2D
    {
        public const double ZoomInLimit = 4;
        public const double ZoomOutLimit = 0.1;


        private readonly GraphControl _control;

        private CameraAnimator _cameraAnimator;
        private Matrix _matrixTransform = Matrix.Identity;
        private Vector2 _viewPortSize;



        public Camera2D(GraphControl control)
        {
            _control = control;
        }



        /// <summary>
        /// Transformation matrix of this camera
        /// </summary>
        public Matrix MatrixTransform
        {
            get { return _matrixTransform; }
            set { _matrixTransform = value; }
        }



        /// <summary>
        /// Updates the camera animations
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(TimeSpan deltaTime)
        {
            if (_cameraAnimator != null)
            {
                _matrixTransform = _cameraAnimator.Update(deltaTime);

                if (_cameraAnimator.IsOver)
                    _cameraAnimator = null;

                //where we're animating, we should render every frame
                _control.ShouldRender = true;
            }
        }



        /// <summary>
        /// Zooms the camera by a specific amount, onto a specific position
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="modelPosition"></param>
        public void ZoomCanvas(float delta, Vector2 modelPosition)
        {
            delta = (delta/360f)*0.1f;

            var newTransform = Matrix.CreateTranslation(-modelPosition.X, -modelPosition.Y, 0)
                               *Matrix.CreateScale(1 + delta, 1 + delta, 1)
                               *Matrix.CreateTranslation(modelPosition.X, modelPosition.Y, 0)
                               *_matrixTransform;

            //set a limit of the outer zoom, otherwise we might run into problems (especially on MacOS)
            var scaleValue = GetScaleValue(newTransform, delta);
            if (scaleValue < ZoomOutLimit || scaleValue > ZoomInLimit)
                return;

            _matrixTransform = newTransform;


            //we zoomed the camera, we should render the scene again
            _control.ShouldRender = true;
        }



        /// <summary>
        /// Returns an indicator of the scale of the given matrix (useful to set limits on the zoom).
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        private static float GetScaleValue(Matrix matrix, float delta)
        {
            Vector3 scale, translation;
            Quaternion quaternion;
            matrix.Decompose(out scale, out quaternion, out translation);

            //Services.IServiceLocator.Current.Get<MessageManager>().Publish(new LogMessageSent(scale + " __ " + delta));

            //the X and Y scale values are the same, so return either one
            return scale.Y;
        }



        /// <summary>
        /// Moves the camera by the indicated offset
        /// </summary>
        /// <param name="offset"></param>
        public void Move(Vector2 offset)
        {
            _matrixTransform = Matrix.CreateTranslation(offset.X, offset.Y, 0)*_matrixTransform;

            //we moved the camera, we should render the scene again
            _control.ShouldRender = true;
        }



        /// <summary>
        /// Starts a camera animation to a certain matrix tranformation
        /// </summary>
        /// <param name="targetTransform"></param>
        public void AnimateCameraTo(Matrix targetTransform)
        {
            _cameraAnimator = new CameraAnimator(_matrixTransform, targetTransform, 500);
        }



        /// <summary>
        /// Converts a location in screen space to model space
        /// </summary>
        /// <param name="screenPosition"></param>
        /// <returns></returns>
        public Vector2 ToModelPosition(Vector2 screenPosition)
        {
            return Vector2.Transform(screenPosition, Matrix.Invert(_matrixTransform));
        }



        public Vector2 ToModelOffset(Vector2 offset)
        {
            return Vector2.Transform(offset, Matrix.Invert(_matrixTransform)) - Vector2.Transform(new Vector2(0, 0), Matrix.Invert(_matrixTransform));
        }



        /// <summary>
        /// Converts a location in model space to screen space
        /// </summary>
        /// <param name="modelPosition"></param>
        /// <returns></returns>
        public Vector2 ToScreenPosition(Vector2 modelPosition)
        {
            return Vector2.Transform(modelPosition, _matrixTransform);
        }



        /// <summary>
        /// Frames the selected nodes, zooms to the whole of there are no selected nodes or zooms to the
        /// default viewport size if there are no nodes at all.
        /// </summary>
        /// <param name="animate">True, If the framing should be animated, false otherwise.</param>
        public void FrameSelectionOrDefault(bool animate)
        {
            var nodesToFrame = _control.VisualGraph.VisualNodes.Where(x => x.IsSelected).ToList();
            if (!nodesToFrame.Any())
                nodesToFrame = _control.VisualGraph.VisualNodes;

            //if there are still no nodes to frame, zoom to the default
            if (nodesToFrame.Any())
            {
                FrameNodes(nodesToFrame, animate);
            }
            else
            {
                FrameToCanvasRectangle(new RectangleF(new Vector2(), _viewPortSize), animate);
            }
        }



        /// <summary>
        /// Frames the specified nodes. Does nothing if the list is empty.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="animate">if set to <c>true</c> [animate].</param>
        public void FrameNodes(List<VisualNode> nodes, bool animate)
        {
            if (nodes.Any())
            {
                var boundingRectangle = RectangleF.Merge(nodes.Select(x => x.RealRectangle));
                boundingRectangle.Expand(50);
                FrameToCanvasRectangle(boundingRectangle, animate);
            }
        }



        private void FrameToCanvasRectangle(RectangleF rectangle, bool animate)
        {
            float widthRelation = _viewPortSize.X/rectangle.Width;
            float heightRelation = _viewPortSize.Y/rectangle.Height;

            float scaleRelation = widthRelation > heightRelation ? heightRelation : widthRelation;

            Vector2 centerViewVector = new Vector2(_viewPortSize.X/scaleRelation/2f - rectangle.Width/2f,
                _viewPortSize.Y/scaleRelation/2f - rectangle.Height/2f);

            Matrix canvasMatrix = Matrix.CreateTranslation(centerViewVector.X, centerViewVector.Y, 0)*Matrix.CreateTranslation(-rectangle.Min.X, -rectangle.Min.Y, 0)*Matrix.CreateScale(scaleRelation, scaleRelation, 1);

            if (animate)
                AnimateCameraTo(canvasMatrix);
            else
            {
                MatrixTransform = canvasMatrix;

                //we messed with the camera, we should 
                _control.ShouldRender = true;
            }
        }



        public void Resize(Point point)
        {
            _viewPortSize = new Vector2(point.X, point.Y);
        }
    }
}