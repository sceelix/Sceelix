using System;
using Microsoft.Xna.Framework;

namespace Sceelix.Designer.Graphs.GUI.Basic.Animations
{
    public class ShrinkAnimation : RectangleAnimation
    {
        public ShrinkAnimation(float animationTime = 500)
            : base(animationTime)
        {
        }



        protected override RectangleF UpdateAnimation(float animationPercentage, RectangleF rectangle)
        {
            var elapsedInterval = MathHelper.Pi*animationPercentage;

            double amount = (Math.Sin(3*MathHelper.PiOver4 - elapsedInterval) - Math.Sin(MathHelper.PiOver4))*50;
            rectangle.Expand((float) amount);

            return rectangle;
        }
    }
}