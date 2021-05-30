using Sceelix.Actors.Data;
using Sceelix.Collections;
using Sceelix.Conversion;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;

namespace Sceelix.Actors.Parameters
{
    /// <summary>
    /// Reads/calculates properties from actor entities.
    /// </summary>
    /// <seealso cref="Sceelix.Core.Procedures.PropertyProcedure.PropertyParameter" />
    public class ActorPropertyParameter : PropertyProcedure.PropertyParameter
    {
        /// <summary>
        /// Actor entity from which to read the properties.
        /// </summary>
        private readonly SingleInput<IActor> _input = new SingleInput<IActor>("Input");

        /// <summary>
        /// Actor entity from which the properties were read.
        /// </summary>
        private readonly Output<IActor> _output = new Output<IActor>("Output");

        /// <summary>
        /// 3D Scope of the Actor.
        /// </summary>
        private readonly AttributeParameter<SceeList> _parameterScope = new AttributeParameter<SceeList>("Scope", AttributeAccess.Write);



        public ActorPropertyParameter()
            : base("Actor")
        {
        }



        public override void Run()
        {
            var actor = _input.Read();

            if (_parameterScope.IsMapped)
                _parameterScope[actor] = ConvertHelper.Convert<SceeList>(actor.BoxScope);

            _output.Write(actor);
        }
    }
}