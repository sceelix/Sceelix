using System;
using System.Linq;
using Sceelix.Core.Data;
using Sceelix.Core.ExpressionParsing;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Core.Procedures;

namespace Sceelix.Core.Parameters
{
    public class ParameterReference
    {
        public ParameterReference(Parameter parameter)
        {
            Parameter = parameter;
        }



        /// <summary>
        /// The description of this parameter.
        /// </summary>
        public string Description => Parameter.Description;


        /// <summary>
        /// The collection of direct inputs of this paramter.
        /// </summary>
        public InputReferenceCollection Inputs => new InputReferenceCollection(Parameter.SubInputs);


        /// <summary>
        /// Indicates whether this parameter value will be evaluated based on individual entity or subentity properties/attributes
        /// inside the procedure, as opposed to be evaluated on each procedure round.
        /// </summary>
        public bool IsEntityEvaluation => Parameter.EntityEvaluation;


        /// <summary>
        /// Indicates if the parameter is currently set to use expression evalution (as opposed to a fixed value). 
        /// </summary>
        public bool IsExpression => Parameter.IsExpression;


        /// <summary>
        /// The label of this parameter.
        /// </summary>
        public string Label => Parameter.Label;


        /// <summary>
        /// The collection of direct outputs of this paramter.
        /// </summary>
        public OutputReferenceCollection Outputs => new OutputReferenceCollection(Parameter.SubOutputs);


        internal Parameter Parameter
        {
            get;
        }


        public ParameterInfo ParameterInfo => Parameter.ToParameterInfo();



        /// <summary>
        /// The collection of direct, public subparameters of this parameter.
        /// </summary>
        public ParameterReferenceCollection Parameters
        {
            get { return new ParameterReferenceCollection(Parameter.SubParameters.Where(x => x.IsPublic)); }
        }



        /// <summary>
        /// Gets the value currently set for this parameter.
        /// </summary>
        /// <returns></returns>
        public object Get()
        {
            return Parameter.Get();
        }



        /// <summary>
        /// Sets a fixed value of the parameter, while disabling expression evalution.
        /// </summary>
        /// <param name="value"></param>
        public void Set(object value)
        {
            Parameter.Set(value);
            Parameter.IsExpression = false;
            foreach (var parameter in Parameter.SubParameterTree)
            {
                parameter.IsExpression = false;
            }
        }



        internal void Set(ParameterInfo argument, Procedure masterProcedure, Procedure procedure)
        {
            Parameter.Set(argument, masterProcedure, procedure);
        }



        /// <summary>
        /// Sets this parameter to the indicated expression. Sets the parameter mode to expression evalution.
        /// </summary>
        /// <param name="expression"></param>
        public void SetExpression(Func<InputData, IEntity, object> expression)
        {
            Parameter.SetExpression(expression);
        }



        public void SetExpression(string expression, Procedure masterProcedure = null)
        {
            Parameter.SetExpression(ExpressionParser.Parse(expression).GetCompiledExpressionTree(masterProcedure, Parameter.Procedure, typeof(object)));
        }
    }
}