using System;
using System.Linq.Expressions;
using System.Xml;
using Sceelix.Core.Procedures;

namespace Sceelix.Core.Graphs.Expressions
{
    public class UnaryExpressionNode : ExpressionNode
    {
        private readonly ExpressionNode _operand;



        public UnaryExpressionNode(string name, ExpressionNode operand)
            : base(name)
        {
            _operand = operand;
        }



        public UnaryExpressionNode(XmlElement element) : base(element)
        {
            _operand = InstanciateFromXml((XmlElement) element.FirstChild);
        }



        public override Type ExpressionType => _operand.ExpressionType;



        public override object Clone()
        {
            return new UnaryExpressionNode(Name, (ExpressionNode) _operand.Clone());
        }



        public override Expression GetExpressionTree(ParameterExpression inputDataExpression, ParameterExpression internalEntityExpression, Procedure masterProcedure, Procedure currentProcedure)
        {
            switch (Name)
            {
                case "!":
                    return Expression.Call(typeof(ExpressionHelper).GetMethod("Not"), _operand.GetExpressionTree(inputDataExpression, internalEntityExpression, masterProcedure, currentProcedure));
                //return Expression.Not(_operand.GetExpressionTree(parameterExpression, masterProcedure, currentProcedure));
                case "-":
                    return Expression.Call(typeof(ExpressionHelper).GetMethod("Negate"), _operand.GetExpressionTree(inputDataExpression, internalEntityExpression, masterProcedure, currentProcedure));
                //return Expression.Negate(_operand.GetExpressionTree(parameterExpression, masterProcedure, currentProcedure));
            }

            return null;
        }



        public override string GetOriginalExpression()
        {
            return Name + _operand.GetOriginalExpression();
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