using System;
using System.Collections.Generic;
using Sceelix.Core.Data;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Mathematics.Data;

namespace Sceelix.Mathematics.Parameters
{
    /// <summary>
    /// Generates random colors through different possible methods.
    /// </summary>
    public class ColorRandomParameter : RandomProcedure.RandomParameter
    {
        /// <summary>
        /// Type of color generation method to use.<br/>
        /// <b>Standard</b> means that a random value will be set for each RGB component.<br/>
        /// <b>Offset</b> means that the colors will differ from a chosen random offset. Returns more interesting results.<br/>
        /// <b>Hue</b> means that a random hue will be chosen (with maximum saturation and value). Returns more colorful results.
        /// </summary>
        private readonly ChoiceParameter _parameterType = new ChoiceParameter("Type", "Standard", "Standard", "Offset", "Hue");

        /// <summary>
        /// Attribute where to store the random color.
        /// </summary>
        private readonly AttributeParameter<Color> _attributeValue = new AttributeParameter<Color>("Value", AttributeAccess.Write);



        public ColorRandomParameter()
            : base("Color")
        {
        }



        public override void Execute(Random random, List<IEntity> entities)
        {
            switch (_parameterType.Value)
            {
                case "Standard":
                    foreach (var baseEntity in entities)
                        _attributeValue[baseEntity] = new Color((byte) random.Next(0, 256), (byte) random.Next(0, 256), (byte) random.Next(0, 256));
                    break;
                case "Offset":
                {
                    var baseColor = new Color((byte) random.Next(0, 256), (byte) random.Next(0, 256), (byte) random.Next(0, 256));
                    var startingColor = baseColor;

                    foreach (var baseEntity in entities)
                    {
                        _attributeValue[baseEntity] = startingColor;
                        startingColor += baseColor;
                    }
                }
                    break;
                case "Hue":
                    foreach (var baseEntity in entities) _attributeValue[baseEntity] = Color.HsvToRgb(random.Next(361), 1, 1);
                    break;
            }
        }
    }
}