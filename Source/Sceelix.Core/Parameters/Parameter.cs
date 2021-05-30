using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Sceelix.Core.Data;
using Sceelix.Core.Environments;
using Sceelix.Core.IO;
using Sceelix.Core.Messages;
using Sceelix.Core.Parameters.Infos;
using Sceelix.Core.Procedures;
using Sceelix.Core.Resources;
using Sceelix.Logging;

namespace Sceelix.Core.Parameters
{
    /*public interface IParameter
    {
        Object Get();

        void Set(Object value);

        void Set(Func<InputData, Object> expression);

        String Label { get; }

        String Description { get;}

        String Group { get; }
    }*/

    /// <summary>
    /// An aspect that is used to control the outcome of the procedures.
    /// Has a certain value by default, but can be changed when the owning procedure is used within another.
    /// </summary>
    public abstract class Parameter //: IParameter : Aspect
    {
        protected Func<IEntity, object> _entityExpression;

        protected Func<InputData, IEntity, object> _expression;

        protected List<Input> _inputs = new List<Input>();

        protected List<Output> _outputs = new List<Output>();



        protected Parameter(string label)
        {
            //if (!Regex.IsMatch(label, "^[a-zA-Z][a-zA-Z0-9\\s]*$"))
            //    throw new ArgumentException("Label '" + label + "' is not valid for a parameter. Labels can only contain alphanumeric and whitespace characters and must start with a letter of the alphabet.");

            Label = label;
        }



        protected Parameter(ParameterInfo parameterInfo)
        {
            Label = parameterInfo.Label;
            EntityEvaluation = parameterInfo.EntityEvaluation;
            IsPublic = parameterInfo.IsPublic;

            Description = parameterInfo.Description;
        }



        /// <summary>
        /// Description of what this parameter controls or does. 
        /// If set, it will overwrite defined code comments.
        /// </summary>
        public string Description
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether this parameter value will be evaluated based on individual entity or subentity properties/attributes
        /// inside the procedure, as opposed to be evaluated on each procedure round.
        /// </summary>
        public bool EntityEvaluation
        {
            get;
            set;
        }


        public string FullLabel => Root.GetFullLabel(this);
        /*if (Parent == null)
                    return Label;
                
                return Parent.FullName + "." + Label;*/


        /// <summary>
        /// The identifier of this parameter, following the standards of most programming languages:
        /// Starts with a letter, aftewards contains only alphanumeric characters, with no spaces or special characters. 
        /// </summary>
        public string Identifier => GetIdentifier(Label);



        /// <summary>
        /// Indicates whether this parameter is set as an expression. It can be set as true in the procedure definition to 
        /// force the parameter to appear as an expression by default.
        /// </summary>
        /// <value> <c>true</c> if this instance is expression; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsExpression
        {
            get { return _expression != null; }
            set
            {
                if (value)
                    _expression = delegate { return null; };
                else
                    _expression = null;
            }
        }



        /// <summary>
        /// Indicates if the parameter will be visible and accessible externally. Default is true.
        /// </summary>
        public bool IsPublic
        {
            get;
            protected set;
        } = true;


        /// <summary>
        /// Label of the aspect. Must be unique within its procedure.
        /// </summary>
        public string Label
        {
            get;
            set;
        }


        /// <summary>
        /// Special execution options/behaviours will be indicated here. Can be null.
        /// </summary>
        /*protected IExecutionBinding ExecutionBinding
        {
            get { return ProcedureEnvironment.ExecutionBinding; }
            //set { _executionTracer = value; }
        }*/


        /// <summary>
        /// A shortcut to the ProcedureEnvironment.Logger..
        /// </summary>
        protected ILogger Logger => ProcedureEnvironment.GetService<ILogger>();


        /// <summary>
        /// A shortcut to the ProcedureEnvironment.Messenger.
        /// </summary>
        protected IMessenger Messenger => ProcedureEnvironment.GetService<IMessenger>();



        /// <summary>
        /// A list of all the parameter parents and their parents, ordered from the least distant parent to the most. 
        /// The parameter itself is not included, even if it a root.
        /// </summary>
        protected IEnumerable<Parameter> ParameterAncestors
        {
            get
            {
                var parent = Parent;

                while (parent is Parameter)
                {
                    yield return (Parameter) parent;

                    parent = ((Parameter) parent).Parent;
                }
            }
        }



        /// <summary>
        /// The parent of this parameter. Could be another parameter (if this is a subparameter) or a procedure.
        /// </summary>
        internal object Parent
        {
            get;
            set;
        }



        /// <summary>
        /// Procedure to which this parameter belongs.
        /// </summary>
        public virtual Procedure Procedure
        {
            get
            {
                if (Parent is Procedure)
                    return (Procedure) Parent;

                return ((Parameter) Parent).Procedure;
            }
        }



        /// <summary>
        /// The ProcedureEnvironment defined for the procedure that this parameter belongs to.
        /// </summary>
        protected IProcedureEnvironment ProcedureEnvironment => Procedure.Environment;


        /// <summary>
        /// A shortcut to the ProcedureEnvironment.Resources.
        /// </summary>
        protected IResourceManager Resources => ProcedureEnvironment.GetService<IResourceManager>();



        /// <summary>
        /// Root parameter that lies on the base of this one. Returns itself if it is the root.
        /// </summary>
        public Parameter Root
        {
            get
            {
                var parameter = this;

                while (parameter.Parent is Parameter)
                    parameter = (Parameter) parameter.Parent;

                return parameter;
            }
        }



        public string Section
        {
            get;
            internal set;
        }



        /// <summary>
        /// The direct inputs of this parameters.
        /// </summary>
        public virtual InputCollection SubInputs
        {
            get { return new InputCollection(_inputs); }
            set
            {
                _inputs = new List<Input>(value);
                foreach (Input input in _inputs)
                    input.Parent = this;
            }
        }



        /// <summary>
        /// Gets all the outputs from this parameter and subparameters down the tree.
        /// If a parameter is an expression (or non-public) at some point, the search stops there.
        /// </summary>
        /// <value></value>
        public virtual IEnumerable<Input> SubInputTree
        {
            get
            {
                if (IsPublic)
                    foreach (var abstractInput in _inputs)
                        yield return abstractInput;
            }
        }



        /// <summary>
        /// The direct outputs of this parameter.
        /// </summary>
        public virtual OutputCollection SubOutputs
        {
            get { return new OutputCollection(_outputs); }
            set
            {
                _outputs = new List<Output>(value);
                foreach (Output output in _outputs)
                    output.Parent = this;
            }
        }



        /// <summary>
        /// Gets all the outputs from this parameter and subparameters down the tree.
        /// If a parameter is an expression (or non-public)  at some point, the search stops there.
        /// </summary>
        /// <value></value>
        public virtual IEnumerable<Output> SubOutputTree
        {
            get
            {
                if (IsPublic)
                    foreach (var abstractOutput in _outputs)
                        yield return abstractOutput;
            }
        }



        /// <summary>
        /// The direct subparameters of this parameter.
        /// </summary>
        public virtual ParameterCollection SubParameters => new ParameterCollection();



        /// <summary>
        /// Gets all the parameters from this parameter and others down the tree.
        /// 
        /// If a parameter is an expression at some point, the search stops there.
        /// </summary>
        /// <value></value>
        public virtual IEnumerable<Parameter> SubParameterTree
        {
            get { yield break; }
        }



        /// <summary>
        /// Evaluates the expression, if defined.
        /// </summary>
        /// <param name="inputData"></param>
        internal virtual void Evaluate(InputData inputData)
        {
            if (_expression != null)
            {
                /*Set(_expression(inputData, null));

                if (!_roundEvaluation && _customExpression == null)
                    _customExpression = unknownentity => _expression(inputData, unknownentity);*/

                if (!EntityEvaluation)
                    Set(_expression(inputData, inputData.GetFirst()));
                else if (_entityExpression == null)
                    _entityExpression = unknownentity => _expression(inputData, unknownentity);
            }
        }



        /// <summary>
        /// Obtains the value in the parameter.
        /// Each parameter subclass overrides its implementation.
        /// </summary>
        /// <returns></returns>
        public object Get()
        {
            if (_entityExpression != null)
                return _entityExpression;

            return GetData();
        }



        public object Get(IEntity entity)
        {
            if (_entityExpression != null)
            {
                var data = _entityExpression(entity);
                SetData(data);
            }

            return GetData();
        }



        protected internal abstract object GetData();



        public virtual object GetDeep(IEntity entity)
        {
            return Get(entity);
        }



        /*public virtual Object Clone()
        {
            Parameter parameter = (Parameter)MemberwiseClone();

            parameter.Label = _label;
            parameter._expression = _expression;
            parameter.Owner = Owner;
            parameter.Description = Description;
            parameter.IsPublic = IsPublic;

            return parameter;
        }*/

        //this[argument].Set(argument,masterDataBlock,currentDataBlock);
        /*internal Func<InputData, Object> Expression
        {
            get { return _expression; }
            set { _expression = value; }
        }*/



        protected internal virtual string GetFullLabel(Parameter parameter)
        {
            if (parameter == this)
                return Label;

            return null;
        }



        /// <summary>
        /// Static function that determines the identifier of a parameter from its defined label.
        /// </summary>
        /// <param name="label">Label of the parameter</param>
        /// <returns>The identifier of this parameter, following the standards of most programming languages: Starts with a letter, aftewards 
        /// contains only alphanumeric characters, with no spaces or special characters. </returns>
        public static string GetIdentifier(string label)
        {
            return Regex.Replace(label, @"[^\w]", "");
        }



        /// <summary>
        /// Sets the value of the parameter.
        /// Each subclass overrides this function, providing specific behavior.
        /// </summary>
        /// <param name="value"></param>
        public void Set(object value)
        {
            if (value is Func<IEntity, object>)
                _entityExpression = (Func<IEntity, object>) value;
            else
                SetData(value);
        }



        protected internal abstract void Set(ParameterInfo argument, Procedure masterProcedure, Procedure currentProcedure);


        protected abstract void SetData(object value);



        /// <summary>
        /// Sets an expression to be evaluated for each round.
        /// </summary>
        /// <param name="expression">The expression to be evaluated, or null to use the assigned (default) fixed value.</param>
        public void SetExpression(Func<InputData, IEntity, object> expression)
        {
            _expression = expression;
            //if(!_roundEvaluation)
            //    _customExpression = unknownentity => _expression(inputData, unknownentity);
        }



        protected internal abstract ParameterInfo ToParameterInfo();



        public override string ToString()
        {
            return string.Format("Label: {0}, IsPublic: {1}, Type: {2}", Label, IsPublic, GetType());
        }
    }
}