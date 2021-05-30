using System;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;

namespace Sceelix.Designer.Renderer3D.GameObjects
{
    public class CameraObjectAnimator
    {
        private readonly int _animationTime;
        private readonly Matrix _originalTransform = Matrix.Identity;
        private readonly Matrix _targetTransform = Matrix.Identity;
        private int _elapsedAnimationTime;



        public CameraObjectAnimator(Matrix originalTransform, Vector3F position, Vector3F target, Vector3F upVector, int animationTime)
        {
            _originalTransform = originalTransform;
            _targetTransform = Matrix44F.CreateLookAt(position, target, upVector).Inverse.ToXna();
            _animationTime = animationTime;
        }



        public bool IsOver
        {
            get { return _elapsedAnimationTime >= _animationTime; }
        }



        public Matrix Update(TimeSpan deltaTime)
        {
            _elapsedAnimationTime += deltaTime.Milliseconds;
            float percentage = _elapsedAnimationTime/(float) _animationTime;

            return Matrix.Lerp(_originalTransform, _targetTransform, percentage);
        }
    }
}