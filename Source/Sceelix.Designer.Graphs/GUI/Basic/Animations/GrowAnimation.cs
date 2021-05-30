using System;
using Microsoft.Xna.Framework;

namespace Sceelix.Designer.Graphs.GUI.Basic.Animations
{
    public class GrowAnimation : RectangleAnimation
    {
        public GrowAnimation(float animationTime = 500)
            : base(animationTime)
        {
        }



        protected override RectangleF UpdateAnimation(float animationPercentage, RectangleF rectangle)
        {
            var elapsedInterval = MathHelper.Pi*animationPercentage;

            double amount = (Math.Sin(-MathHelper.PiOver4 + elapsedInterval) - Math.Sin(MathHelper.PiOver4))*50;
            rectangle.Expand((float) amount);

            return rectangle;
        }
    }
}