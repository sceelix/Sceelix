using System;
using Microsoft.Xna.Framework;

namespace Sceelix.Designer.Graphs.GUI.Basic.Animations
{
    public class PortAnimation
    {
        private readonly int _maxSize;
        private readonly int _minSize;
        private readonly float _speed;
        private float _amount;

        private TimeSpan _totalTime;



        /// <summary>
        /// 
        /// </summary>
        /// <param name="speed">A value between 0 (stopped) and 1 (fast)</param>
        /// <param name="minSize"></param>
        /// <param name="maxSize"></param>
        public PortAnimation(float speed, int minSize, int maxSize)
        {
            _speed = speed;
            _minSize = minSize;
            _maxSize = maxSize;
        }



        public float Amount
        {
            get { return _amount; }
        }



        public void Process(TimeSpan deltaTime)
        {
            _totalTime += deltaTime;

            _amount = (float) Math.Sin((_totalTime.TotalMilliseconds - MathHelper.PiOver2)*0.01f*_speed)*_minSize + (_maxSize - _minSize);
        }
    }
}