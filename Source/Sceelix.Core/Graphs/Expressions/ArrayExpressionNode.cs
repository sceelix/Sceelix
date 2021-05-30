using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml;
using Sceelix.Core.Procedures;

namespace Sceelix.Core.Graphs.Expressions
{
    public class ArrayExpressionNode : ExpressionNode
    {
        public ArrayExpressionNode(List<ExpressionNode> args)
            : base("[]")
        {
            Args = args;
        }



        public ArrayExpressionNode(XmlElement element)
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
            return new ArrayExpressionNode(Args.Select(x => (ExpressionNode) x.Clone()).ToList());
        }



        public override Expression GetExpressionTree(ParameterExpression inputDataExpression, ParameterExpression internalEntityExpression, Procedure masterProcedure, Procedure currentProcedure)
        {
            var arrayInit = Expression.NewArrayInit(typeof(object), Args.Select(x => x.GetExpressionTree(inputDataExpression, internalEntityExpression, masterProcedure, currentProcedure)));

            return Expression.Call(typeof(ExpressionHelper).GetMethod("CreateList"), arrayInit);
        }



        public override string GetOriginalExpression()
        {
            return "[" + string.Join(",", Args.Select(val => val.GetOriginalExpression()).ToArray()) + "]";
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