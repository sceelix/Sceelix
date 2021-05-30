using System;
using System.Collections.Generic;
using Sceelix.Core.Data;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Mathematics.Data;

namespace Sceelix.Mathematics.Parameters
{
    /// <summary>
    /// Generates random 3D vectors within a specified range.
    /// </summary>
    public class Vector3DRandomParameters : RandomProcedure.RandomParameter
    {
        /// <summary>
        /// Inclusive lower bound of the random vector returned.
        /// </summary>
        private readonly Vector3DParameter _parameterMin = new Vector3DParameter("Minimum", Vector3D.Zero);

        /// <summary>
        /// Exclusive upper bound of the random vector returned.
        /// </summary>
        private readonly Vector3DParameter _parameterMax = new Vector3DParameter("Maximum", Vector3D.One * 10);

        /// <summary>
        /// Attribute where to store the random value.
        /// </summary>
        private readonly AttributeParameter<Vector3D> _attributeValue = new AttributeParameter<Vector3D>("Value", AttributeAccess.Write);



        public Vector3DRandomParameters()
            : base("Vector3D")
        {
        }



        public override void Execute(Random random, List<IEntity> entities)
        {
            var difference = _parameterMax.Value - _parameterMin.Value;

            foreach (var entity in entities)
                _attributeValue[entity] = new Vector3D(Convert.ToSingle(_parameterMin.Value.X + random.NextDouble() * difference.X),
                    Convert.ToSingle(_parameterMin.Value.Y + random.NextDouble() * difference.Y),
                    Convert.ToSingle(_parameterMin.Value.Z + random.NextDouble() * difference.Z));
        }
    }
}