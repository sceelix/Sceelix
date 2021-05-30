using System;
using Microsoft.Xna.Framework;

namespace Sceelix.Designer.Graphs.GUI.Basic
{
    public class TransitionColor
    {
        private Color _previousColor = Color.White;
        private Color _targetColor = Color.White;
        private float _transitionAmount = 1;
        private int _transitionSpeed;



        public TransitionColor(Color initialColor, int transitionSpeed)
        {
            _transitionSpeed = transitionSpeed;
            _targetColor = initialColor;
        }



        public Color Value
        {
            get { return Color.Lerp(_previousColor, _targetColor, _transitionAmount); }
            set
            {
                if (_targetColor != value)
                {
                    _previousColor = _targetColor;
                    _transitionAmount = 1 - _transitionAmount;
                    _targetColor = value;
                }
            }
        }



        public int TransitionSpeed
        {
            get { return _transitionSpeed; }
            set { _transitionSpeed = value; }
        }



        /// <summary>
        /// Updates the transition from one color to another.
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <returns>Boolean indicating if an update took place</returns>
        public bool Update(TimeSpan deltaTime)
        {
            if (_transitionAmount < 1)
            {
                _transitionAmount += (float) deltaTime.TotalMilliseconds/(float) _transitionSpeed;
                if (_transitionAmount > 1)
                    _transitionAmount = 1;

                return true;
            }

            return false;
        }
    }
}