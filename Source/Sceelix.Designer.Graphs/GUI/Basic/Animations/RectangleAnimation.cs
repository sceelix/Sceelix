using System;

namespace Sceelix.Designer.Graphs.GUI.Basic.Animations
{
    public abstract class RectangleAnimation
    {
        private bool _isFinished;

        protected float AnimationTime;
        protected float ElapsedAnimationTime;



        protected RectangleAnimation(float animationTime = 2000)
        {
            AnimationTime = animationTime;
            ElapsedAnimationTime = 0;
        }



        public bool IsFinished
        {
            get { return _isFinished; }
        }



        public RectangleF UpdateAnimation(TimeSpan deltaTime, RectangleF rectangle)
        {
            if (IsFinished)
                return rectangle;

            ElapsedAnimationTime += deltaTime.Milliseconds;

            if (ElapsedAnimationTime > AnimationTime)
            {
                ElapsedAnimationTime = AnimationTime;
                _isFinished = true;

                //warn any interested fellas
                if (AnimationFinished != null)
                    AnimationFinished.Invoke(this, null);
            }

            return UpdateAnimation(ElapsedAnimationTime/AnimationTime, rectangle);
        }



        protected abstract RectangleF UpdateAnimation(float animationPercentage, RectangleF rectangle);

        public event EventHandler AnimationFinished;
    }
}