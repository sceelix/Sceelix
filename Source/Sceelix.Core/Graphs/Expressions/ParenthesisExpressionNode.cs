using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml;
using Sceelix.Core.Procedures;

namespace Sceelix.Core.Graphs.Expressions
{
    public class ParenthesisExpressionNode : ExpressionNode
    {
        private readonly ExpressionNode _operand;



        public ParenthesisExpressionNode(ExpressionNode operand)
            : base("()")
        {
            _operand = operand;
        }



        public ParenthesisExpressionNode(XmlElement element) : base(element)
        {
            _operand = InstanciateFromXml((XmlElement) element.FirstChild);
        }



        public override Type ExpressionType => _operand.ExpressionType;



        public override object Clone()
        {
            return new ParenthesisExpressionNode((ExpressionNode) _operand.Clone());
        }



        public override Expression GetExpressionTree(ParameterExpression inputDataExpression, ParameterExpression internalEntityExpression, Procedure masterProcedure, Procedure currentProcedure)
        {
            return _operand.GetExpressionTree(inputDataExpression, internalEntityExpression, masterProcedure, currentProcedure);
        }



        public override string GetOriginalExpression()
        {
            return "(" + _operand.GetOriginalExpression() + ")";
        }



        public override IEnumerable<ExpressionNode> GetSubtree()
        {
            yield return _operand;
            foreach (var expressionNode in _operand.GetSubtree())
                yield return expressionNode;
        }



        public override bool HasReferenceToParameter(string label)
        {
            return _operand.HasReferenceToParameter(label);
        }



        public override bool HasReferenceToVariable(string label)
        {
            return _operand.HasReferenceToVariable(label);
        }



        public override void PrintTree(int level)
        {
            base.PrintTree(level);
            Console.WriteLine(Name);
            _operand.PrintTree(level + 1);
        }



        public override void RefactorGlobalParameters(string oldLabel, string newLabel)
        {
            _operand.RefactorGlobalParameters(oldLabel, newLabel);
        }



        public override void RefactorPortVariables(string oldLabel, string newLabel)
        {
            _operand.RefactorPortVariables(oldLabel, newLabel);
        }



        public override void WriteSpecificXML(XmlWriter writer)
        {
            _operand.WriteXML(writer);
        }
    }
}