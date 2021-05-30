using System;
using System.Linq.Expressions;
using System.Xml;
using Sceelix.Core.Procedures;

namespace Sceelix.Core.Graphs.Expressions
{
    public class PropertyExpressionNode : ExpressionNode
    {
        public PropertyExpressionNode(string name)
            : base(name)
        {
        }



        public PropertyExpressionNode(XmlElement element)
            : base(element)
        {
        }



        public override object Clone()
        {
            return new PropertyExpressionNode(Name);
        }



        public override Expression GetExpressionTree(ParameterExpression inputDataExpression, ParameterExpression internalEntityExpression, Procedure masterProcedure, Procedure currentProcedure)
        {
            throw new NotImplementedException();
        }



        public override string GetOriginalExpression()
        {
            return Name;
        }
    }
}