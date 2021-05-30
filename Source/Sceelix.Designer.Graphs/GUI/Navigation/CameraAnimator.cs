using System;
using Microsoft.Xna.Framework;

namespace Sceelix.Designer.Graphs.GUI.Navigation
{
    public class CameraAnimator
    {
        private readonly int _animationTime;
        private readonly Matrix _originalTransform = Matrix.Identity;
        private readonly Matrix _targetTransform = Matrix.Identity;
        private int _elapsedAnimationTime;



        public CameraAnimator(Matrix originalTransform, Matrix targetTransform, int animationTime)
        {
            _originalTransform = originalTransform;
            _targetTransform = targetTransform;
            _animationTime = animationTime;
        }



        public bool IsOver
        {
            get { return _elapsedAnimationTime >= _animationTime; }
        }



        public Matrix Update(TimeSpan deltaTime)
        {
            _elapsedAnimationTime += deltaTime.Milliseconds;
            float percentage = Math.Min(_elapsedAnimationTime/(float) _animationTime, 1);


            return Matrix.Lerp(_originalTransform, _targetTransform, percentage);
        }
    }
}