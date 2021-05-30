using System;
using System.Collections.Generic;
using Sceelix.Core.Data;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Mathematics.Data;

namespace Sceelix.Mathematics.Parameters
{
    /// <summary>
    /// Generates random 2D vectors within a specified range.
    /// </summary>
    public class Vector2DRandomParameters : RandomProcedure.RandomParameter
    {
        /// <summary>
        /// Inclusive lower bound of the random vector returned.
        /// </summary>
        private readonly Vector2DParameter _parameterMin = new Vector2DParameter("Minimum", Vector2D.Zero);

        /// <summary>
        /// Exclusive upper bound of the random vector returned.
        /// </summary>
        private readonly Vector2DParameter _parameterMax = new Vector2DParameter("Maximum", Vector2D.One * 10);

        /// <summary>
        /// Attribute where to store the random value.
        /// </summary>
        private readonly AttributeParameter<Vector2D> _attributeValue = new AttributeParameter<Vector2D>("Value", AttributeAccess.Write);



        public Vector2DRandomParameters()
            : base("Vector2D")
        {
        }



        public override void Execute(Random random, List<IEntity> entities)
        {
            var difference = _parameterMax.Value - _parameterMin.Value;

            foreach (var entity in entities)
                _attributeValue[entity] = new Vector2D(Convert.ToSingle(_parameterMin.Value.X + random.NextDouble() * difference.X),
                    Convert.ToSingle(_parameterMin.Value.Y + random.NextDouble() * difference.Y));
        }
    }
}