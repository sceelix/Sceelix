using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.Utils;
using Sceelix.Extensions;

namespace Sceelix.Core.Procedures
{
    /// <summary>
    /// Base class for all C#-based procedures/nodes.
    /// </summary>
    public abstract class SystemProcedure : Procedure
    {
        protected SystemProcedure()
        {
            ReadSubclassFields();
        }



        protected sealed override bool ShouldDeleteVariables => base.ShouldDeleteVariables;



        public virtual IEnumerable<string> Tags
        {
            get
            {
                var tags = GetType().GetCustomAttribute<ProcedureAttribute>().Tags;
                if (!string.IsNullOrWhiteSpace(tags))
                    return tags.Split(',').Union(GetAutomaticTags());

                //var strings = GetType().GetCustomAttribute<ProcedureAttribute>().Tags.Split(',');

                return GetAutomaticTags();
            } //return GetType().GetCustomAttribute<ProcedureAttribute>().Tags.Split(',').Union();
        }



        public static SystemProcedure FromGuid(Guid guid)
        {
            return SystemProcedureManager.FromGuid(guid.ToString());
        }



        public static T FromGuid<T>(Guid guid) where T : SystemProcedure
        {
            return SystemProcedureManager.FromGuid<T>(guid.ToString());
        }



        protected void ReadSubclassFields()
        {
            _parameters.Clear();
            _inputs.Clear();
            _outputs.Clear();

            ProcedureParameterHelper.ReadParametersInputsOutputs(this, _parameters, _inputs, _outputs);
        }
    }
}