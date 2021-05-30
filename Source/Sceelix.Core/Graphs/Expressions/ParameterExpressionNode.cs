using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml;
using Sceelix.Core.Data;
using Sceelix.Core.Exceptions;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;

namespace Sceelix.Core.Graphs.Expressions
{
    public class ParameterExpressionNode : ExpressionNode
    {
        //private Type _expressionType;



        public ParameterExpressionNode(string name) : base(name)
        {
        }



        public ParameterExpressionNode(XmlElement element) : base(element)
        {
        }



        public override Type ExpressionType => null; //_expressionType; }



        public override object Clone()
        {
            return new ParameterExpressionNode(Name);
        }



        public override Expression GetExpressionTree(ParameterExpression inputDataExpression, ParameterExpression internalEntityExpression, Procedure masterProcedure, Procedure currentProcedure)
        {
            try
            {
                if (masterProcedure == null)
                    throw new ExpressionParsingException("The provided expression references a parameter '" + Name + "', but a parent procedure has not been assigned.");


                Parameter parameter = masterProcedure.SubParameters.FirstOrDefault(x => Parameter.GetIdentifier(x.Label) == Name);
                if (parameter == null)
                    throw new ExpressionParsingException("The referenced graph parameter '" + Name + "' does not exist in the current graph.");

                //_expressionType = parameter.GetType();

                ConstantExpression globalParameter = Expression.Constant(parameter);

                if (parameter.EntityEvaluation)
                    //return Expression.Call(globalParameter, typeof(Parameter).GetMethod("Get", new Type[] { typeof(IEntity), typeof(bool) }), internalEntityExpression,Expression.Constant(false));
                    return Expression.Call(globalParameter, typeof(Parameter).GetMethod("Get", new[] {typeof(IEntity)}), internalEntityExpression);
                return Expression.Call(globalParameter, typeof(Parameter).GetMethod("Get", new Type[] { }));

                //var callExpression = 

                //MemberExpression property = Expression.Property(globalParameter, "Value");
                //Expression convertedProperty = Expression.Convert(property, _expressionType);

                //return callExpression;
            }
            catch (KeyNotFoundException)
            {
                throw new ExpressionParsingException("The referenced graph parameter '" + Name + "' does not exist in the current graph.");
            }
        }



        public override string GetOriginalExpression()
        {
            return Name; //"{" + Name + "}";
        }



        public override bool HasReferenceToParameter(string label)
        {
            return Name == label;
        }



        public override void PrintTree(int level)
        {
            base.PrintTree(level);
            Console.WriteLine("Parameter - " + Name);
        }



        public override void RefactorGlobalParameters(string oldLabel, string newLabel)
        {
            if (oldLabel == Name)
                Name = newLabel;
        }



        public void RenameTo(string newLabel)
        {
            Name = newLabel;
        }
    }
}