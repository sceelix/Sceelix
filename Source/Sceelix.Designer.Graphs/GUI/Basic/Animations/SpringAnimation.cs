using System;
using Microsoft.Xna.Framework;

namespace Sceelix.Designer.Graphs.GUI.Basic.Animations
{
    public class SpringAnimation : RectangleAnimation
    {
        private readonly float _factor = 30;



        protected override RectangleF UpdateAnimation(float animationPercentage, RectangleF rectangle)
        {
            float amount = (float) Math.Sin((ElapsedAnimationTime - MathHelper.PiOver2)*0.01f)*(_factor*(1 - animationPercentage));
            rectangle.Expand(amount);

            return rectangle;
        }
    }
}