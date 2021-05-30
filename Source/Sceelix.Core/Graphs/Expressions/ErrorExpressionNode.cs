using System;
using System.Linq.Expressions;
using System.Xml;
using Sceelix.Core.Procedures;

namespace Sceelix.Core.Graphs.Expressions
{
    public class ErrorExpressionNode : ExpressionNode
    {
        private readonly int _charPositionInLine;



        public ErrorExpressionNode(string code, int charPositionInLine)
            : base(code)
        {
            _charPositionInLine = charPositionInLine;
        }



        public ErrorExpressionNode(XmlElement element) : base(element)
        {
        }



        public override object Clone()
        {
            return new ErrorExpressionNode(Name, _charPositionInLine);
        }



        public override Expression GetExpressionTree(ParameterExpression inputDataExpression, ParameterExpression internalEntityExpression, Procedure masterProcedure, Procedure currentProcedure)
        {
            throw new Exception("Expression contains syntax errors (position " + _charPositionInLine + ").");
        }



        public override string GetOriginalExpression()
        {
            return Name;
        }
    }
}