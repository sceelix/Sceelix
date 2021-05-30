using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml;
using Sceelix.Core.Procedures;

namespace Sceelix.Core.Graphs.Expressions
{
    public class IndexExpressionNode : ExpressionNode
    {
        private readonly ExpressionNode _operand1;

        private readonly ExpressionNode _operand2;
        //private Type _expressionType;



        public IndexExpressionNode(XmlElement element)
            : base(element)
        {
            _operand1 = InstanciateFromXml((XmlElement) element.ChildNodes[0]);
            _operand2 = InstanciateFromXml((XmlElement) element.ChildNodes[1]);
        }



        public IndexExpressionNode(string name, ExpressionNode operand1, ExpressionNode operand2)
            : base(name)
        {
            _operand1 = operand1;
            _operand2 = operand2;
        }



        public override Type ExpressionType => null;



        public override object Clone()
        {
            return new IndexExpressionNode(Name, (ExpressionNode) _operand1.Clone(), (ExpressionNode) _operand2.Clone());
        }



        public override Expression GetExpressionTree(ParameterExpression inputDataExpression, ParameterExpression internalEntityExpression, Procedure masterProcedure, Procedure currentProcedure)
        {
            return Expression.Call(typeof(ExpressionHelper).GetMethod("Index"),
                _operand1.GetExpressionTree(inputDataExpression, internalEntityExpression, masterProcedure, currentProcedure),
                _operand2.GetExpressionTree(inputDataExpression, internalEntityExpression, masterProcedure, currentProcedure),
                Expression.Constant(masterProcedure));
        }



        public override string GetOriginalExpression()
        {
            return _operand1.GetOriginalExpression() + "[" + _operand2.GetOriginalExpression() + "]";
        }



        public override IEnumerable<ExpressionNode> GetSubtree()
        {
            yield return _operand1;
            foreach (var expressionNode in _operand1.GetSubtree())
                yield return expressionNode;

            yield return _operand2;
            foreach (var expressionNode in _operand2.GetSubtree())
                yield return expressionNode;
        }



        public override bool HasReferenceToParameter(string label)
        {
            return _operand1.HasReferenceToParameter(label) || _operand2.HasReferenceToParameter(label);
        }



        public override bool HasReferenceToVariable(string label)
        {
            return _operand1.HasReferenceToVariable(label) || _operand2.HasReferenceToVariable(label);
        }



        public bool IsPrimitive(Type type)
        {
            return type.IsPrimitive || type == typeof(string);
        }



        public override void PrintTree(int level)
        {
            base.PrintTree(level);
            Console.WriteLine(Name);

            _operand1.PrintTree(level + 1);
            _operand2.PrintTree(level + 1);
        }



        public override void RefactorGlobalParameters(string oldLabel, string newLabel)
        {
            _operand1.RefactorGlobalParameters(oldLabel, newLabel);
            _operand2.RefactorGlobalParameters(oldLabel, newLabel);
        }



        public override void RefactorPortVariables(string oldLabel, string newLabel)
        {
            _operand1.RefactorPortVariables(oldLabel, newLabel);
            _operand2.RefactorPortVariables(oldLabel, newLabel);
        }



        public override void WriteSpecificXML(XmlWriter writer)
        {
            _operand1.WriteXML(writer);
            _operand2.WriteXML(writer);
        }
    }
}