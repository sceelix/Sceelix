using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Extensions;

namespace Sceelix.Core.Procedures
{
    /// <summary>
    /// Performs flow filtering according to defined
    /// probability value.
    /// </summary>
    [Procedure("e728b8dd-3ac4-4c29-82dc-eadd39c50b9b", Label = "Stochastic", Category = "Basic")]
    public class StochasticProcedure : SystemProcedure
    {
        private readonly CollectiveInput<IEntity> _input = new CollectiveInput<IEntity>("Input");

        /// <summary>
        /// Seed that controls the random distrubution.
        /// </summary>
        private readonly IntParameter _parameterSeed = new IntParameter("Seed", 1000);

        /// <summary>
        /// Defines how the list of probabilities is handled:<br />
        /// <b>Weighted</b> means that if the sum of probabilities does not reach 1 or goes over 1 (i.e. 100%), the values with be scaled accordingly so as to consider all cases.<br />
        /// <b>Absolute</b> means that the sum of probabilities is capped at 1 (i.e. 100%), meaning that some entities could end up being discarded or some probabilities could end up not being considered.
        /// </summary>
        private readonly ChoiceParameter _parameterMethod = new ChoiceParameter("Method", "Weighted", "Absolute", "Weighted");


        /// <summary>
        /// List of probabilities or weights, each between 0 and 1. 
        /// Does not have to add up to 1 - instead the values are summed and divided by the total amount.
        /// </summary>
        private readonly ListParameter _parameterProbabilities = new ListParameter("Probabilities",
            () => new FloatParameter("Value", 1)
            {
                MinValue = 0, MaxValue = 1, Increment = 0.1f,
                SubOutputs = new OutputCollection(new Output<IEntity>("Probability") {Description = ""}),
                Description = "Probability/Weight, between 0 and 1, that controls how likely is it for the entity to come out through this port."
            });



        /// <b>Flexible</b> means that the sum of probabilities is capped at 1 (i.e. 100%), but if a probability is lower than 1, the probabilities whose value is 0 are increased to as to reach 1.
        protected override void Run()
        {
            List<IEntity> entities = _input.Read().ToList();

            //creates only one random generator
            var randomGenerator = new Random(_parameterSeed.Value);

            var probabilityParameters = _parameterProbabilities.Items.OfType<FloatParameter>().ToList();
            //var probabilities = probabilityParameters.Select(x => x.Value).ToArray();

            //determines the total sum value
            var probabilitySum = _parameterProbabilities.Items.OfType<FloatParameter>().Sum(val => val.Value);
            var maxProbability = _parameterMethod.Value == "Weighted" ? probabilitySum : 1;


            foreach (var entity in entities)
            {
                float randomValue = randomGenerator.Float(0, maxProbability);

                float currentSum = 0;
                for (var index = 0; index < probabilityParameters.Count; index++)
                {
                    var probabilityParameter = probabilityParameters[index];
                    currentSum += probabilityParameter.Value;
                    //once the value has exceeded the 
                    if (currentSum > randomValue)
                    {
                        probabilityParameter.SubOutputs[0].Write(entity);
                        break;
                    }
                }
            }
        }
    }
}