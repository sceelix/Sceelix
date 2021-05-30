using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml;
using Sceelix.Core.Procedures;

namespace Sceelix.Core.Graphs.Expressions
{
    public class MemberExpressionNode : ExpressionNode
    {
        private readonly List<ExpressionNode> _expressionNodes;
        private Type _mainNodeType;



        public MemberExpressionNode(List<ExpressionNode> expressionNodes)
            : base("Member")
        {
            _expressionNodes = expressionNodes;
        }



        public MemberExpressionNode(XmlElement element)
            : base(element)
        {
            _expressionNodes = new List<ExpressionNode>();

            foreach (XmlElement childElement in element.ChildNodes) _expressionNodes.Add(InstanciateFromXml(childElement));
        }



        public override Type ExpressionType => _mainNodeType;



        public override object Clone()
        {
            return new MemberExpressionNode(_expressionNodes.Select(x => (ExpressionNode) x.Clone()).ToList());
        }



        public override Expression GetExpressionTree(ParameterExpression inputDataExpression, ParameterExpression internalEntityExpression, Procedure masterProcedure, Procedure currentProcedure)
        {
            Expression mainNode = _expressionNodes[0].GetExpressionTree(inputDataExpression, internalEntityExpression, masterProcedure, currentProcedure);
            _mainNodeType = _expressionNodes[0].ExpressionType;

            for (int i = 1; i < _expressionNodes.Count; i++)
            {
                if (_expressionNodes[i] is PropertyExpressionNode)
                {
                    PropertyExpressionNode propertyExpressionNode = (PropertyExpressionNode) _expressionNodes[i];

                    mainNode = Expression.Property(mainNode, propertyExpressionNode.Name);
                }
                else if (_expressionNodes[i] is FunctionExpressionNode)
                {
                    FunctionExpressionNode functionExpressionNode = (FunctionExpressionNode) _expressionNodes[i];

                    List<Expression> expressions = functionExpressionNode.GetArgExpressions(inputDataExpression, internalEntityExpression, masterProcedure, currentProcedure).ToList();

                    Type[] argTypes = expressions.Select(x => x.Type).ToArray();

                    MethodInfo methodInfo = _mainNodeType.GetMethod(functionExpressionNode.Name, argTypes);

                    if (methodInfo == null)
                        throw new Exception("There isn't any method called '" + functionExpressionNode.Name + "' with the following parameter types: " + string.Join(",", argTypes.Select(x => x.Name).ToArray()) + ".");


                    //do the implicit casting to the found methodinfo 
                    ParsedExpression.CastCollection(expressions, methodInfo.GetParameters().Select(val => val.ParameterType).ToList());

                    mainNode = Expression.Call(mainNode, methodInfo, expressions);
                }

                _mainNodeType = _expressionNodes[i].ExpressionType;
            }


            return mainNode;
        }



        public override string GetOriginalExpression()
        {
            return string.Join(".", _expressionNodes.Select(val => val.GetOriginalExpression()).ToArray());
        }



        public override IEnumerable<ExpressionNode> GetSubtree()
        {
            foreach (var expressionNode in _expressionNodes)
            {
                yield return expressionNode;

                foreach (var subnode in expressionNode.GetSubtree())
                    yield return subnode;
            }
        }



        public override bool HasReferenceToParameter(string label)
        {
            return _expressionNodes.Any(val => val.HasReferenceToParameter(label));
        }



        public override bool HasReferenceToVariable(string label)
        {
            return _expressionNodes.Any(val => val.HasReferenceToVariable(label));
        }



        public override void PrintTree(int level)
        {
            foreach (ExpressionNode expressionNode in _expressionNodes)
                expressionNode.PrintTree(level + 1);
        }



        public override void RefactorGlobalParameters(string oldLabel, string newLabel)
        {
            foreach (ExpressionNode expressionNode in _expressionNodes)
                expressionNode.RefactorGlobalParameters(oldLabel, newLabel);
        }



        public override void RefactorPortVariables(string oldLabel, string newLabel)
        {
            foreach (ExpressionNode expressionNode in _expressionNodes)
                expressionNode.RefactorPortVariables(oldLabel, newLabel);
        }



        public override void WriteSpecificXML(XmlWriter writer)
        {
            foreach (ExpressionNode expressionNode in _expressionNodes) expressionNode.WriteXML(writer);
        }
    }
}