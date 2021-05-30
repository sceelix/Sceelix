using System.Collections.Generic;
using System.Linq;
using Sceelix.Actors.Data;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Parameters;
using Sceelix.Props.Data;

namespace Sceelix.Props.Procedures
{
    /// <summary>
    /// Creates Sceelix Props, i.e. entities that only exist in
    /// the Sceelix Designer to build a surrounding environment.
    /// </summary>
    [Procedure("285161ca-4505-4cb2-be5c-399f3622864e", Label = "Prop Create", Category = "Props")]
    public class PropCreateProcedure : SystemProcedure
    {
        /// <summary>
        /// List of props to create.
        /// </summary>
        public ListParameter<PropParameter> ParameterPrimitive = new ListParameter<PropParameter>("Primitive");


        public override IEnumerable<string> Tags => base.Tags.Union(ParameterPrimitive.SubParameterLabels);



        protected override void Run()
        {
            foreach (var propParameter in ParameterPrimitive.Items) propParameter.Run();
        }



        #region Abstract Parameter

        public abstract class PropParameter : CompoundParameter
        {
            protected PropParameter(string label)
                : base(label)
            {
            }



            protected internal abstract void Run();
        }

        #endregion

        #region Ocean

        /// <summary>
        /// Creates an ocean plane at a given height, with infinite extents.
        /// </summary>
        /// <seealso cref="PropParameter" />
        public class OceanPropParameter : PropParameter
        {
            /// <summary>
            /// The ocean prop that was created.
            /// </summary>
            private readonly Output<IEntity> _output = new Output<IEntity>("Output");

            /// <summary>
            /// Height of the water level.
            /// </summary>
            private readonly FloatParameter _heightParameter = new FloatParameter("Water Level", 1);

            /// <summary>
            /// Color of the ocean water.
            /// </summary>
            private readonly ColorParameter _waterColorParameter = new ColorParameter("Water Color", new Color(10, 30, 79, 255));

            /// <summary>
            /// Scale of the ocean waves.
            /// </summary>
            private readonly FloatParameter _waveScaleParameter = new FloatParameter("Wave Scale", 3);



            protected OceanPropParameter()
                : base("Ocean")
            {
            }



            protected internal override void Run()
            {
                Ocean ocean = new Ocean();

                foreach (Parameter parameter in SubParameters) ocean.Attributes.TrySet(parameter.Label, parameter.Get());

                _output.Write(ocean);
            }
        }

        #endregion

        #region Fire

        /// <summary>
        /// Creates an fire prop at the origin.
        /// </summary>
        public class FirePropParameter : PropParameter
        {
            /// <summary>
            /// The fire prop that was created. 
            /// </summary>
            private readonly Output<IActor> _output = new Output<IActor>("Output");



            protected FirePropParameter()
                : base("Fire")
            {
            }



            protected internal override void Run()
            {
                _output.Write(new FireEntity());
            }
        }

        #endregion
    }
}