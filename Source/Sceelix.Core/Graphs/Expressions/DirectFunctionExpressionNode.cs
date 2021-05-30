using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml;
using Sceelix.Core.Graphs.Functions;
using Sceelix.Core.Procedures;
using Sceelix.Logging;

namespace Sceelix.Core.Graphs.Expressions
{
    public class DirectFunctionExpressionNode : ExpressionNode
    {
        public DirectFunctionExpressionNode(string name, List<ExpressionNode> args) : base(name)
        {
            Args = args;
        }



        public DirectFunctionExpressionNode(XmlElement element)
            : base(element)
        {
            Args = new List<ExpressionNode>();

            foreach (XmlElement childElement in element.ChildNodes) Args.Add(InstanciateFromXml(childElement));
        }



        public List<ExpressionNode> Args
        {
            get;
        }



        public override object Clone()
        {
            return new DirectFunctionExpressionNode(Name, Args.Select(x => (ExpressionNode) x.Clone()).ToList());
        }



        /*public Type[] GetArgTypes()
        {
            return _args.Select(val => val.ExpressionType).ToArray();
        }*/



        public IEnumerable<Expression> GetArgExpressions(ParameterExpression parameterExpression, ParameterExpression internalEntityExpression, Procedure masterProcedure, Procedure currentProcedure)
        {
            return Args.Select(val => val.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure));
        }



        public override Expression GetExpressionTree(ParameterExpression inputDataExpression, ParameterExpression internalEntityExpression, Procedure masterProcedure, Procedure currentProcedure)
        {
            FunctionManager.StaticFunction staticFunction = FunctionManager.GetRegisteredStaticFunction(Name, Args.Count);
            if (staticFunction == null)
                throw new Exception("No function with name '" + Name + "' with " + Args.Count + " arguments exists.");

            if (staticFunction.ObsoleteAttribute != null)
            {
                string message = staticFunction.Name + " is obsolete." + staticFunction.ObsoleteAttribute.Message;

                if (staticFunction.ObsoleteAttribute.IsError)
                    throw new Exception(message);

                masterProcedure.Environment.GetService<ILogger>().Log(message, LogType.Warning);
            }


            List<Expression> arguments = new List<Expression>();

            if (staticFunction.ParameterInfos.Length > 0 && staticFunction.ParameterInfos.First().ParameterType == typeof(Procedure)) arguments.Add(Expression.Convert(Expression.Constant(currentProcedure), typeof(Procedure)));

            IEnumerable<Expression> expressions = Args.Select(x => x.GetExpressionTree(inputDataExpression, internalEntityExpression, masterProcedure, currentProcedure));
            arguments.AddRange(expressions);

            var callExpression = Expression.Call(staticFunction.MethodInfo, arguments);

            return callExpression;
        }



        public override string GetOriginalExpression()
        {
            return Name + "(" + string.Join(",", Args.Select(val => val.GetOriginalExpression()).ToArray()) + ")";
        }



        public override IEnumerable<ExpressionNode> GetSubtree()
        {
            foreach (var expressionNode in Args)
            {
                yield return expressionNode;

                foreach (var subnode in expressionNode.GetSubtree())
                    yield return subnode;
            }
        }



        public override bool HasReferenceToParameter(string label)
        {
            return Args.Any(val => val.HasReferenceToParameter(label));
        }



        public override bool HasReferenceToVariable(string label)
        {
            return Args.Any(val => val.HasReferenceToVariable(label));
        }



        public override void RefactorGlobalParameters(string oldLabel, string newLabel)
        {
            foreach (ExpressionNode expressionNode in Args) expressionNode.RefactorGlobalParameters(oldLabel, newLabel);
        }



        public override void RefactorPortVariables(string oldLabel, string newLabel)
        {
            foreach (ExpressionNode expressionNode in Args) expressionNode.RefactorPortVariables(oldLabel, newLabel);
        }



        public override void WriteSpecificXML(XmlWriter writer)
        {
            foreach (ExpressionNode expressionNode in Args) expressionNode.WriteXML(writer);
        }
    }
}