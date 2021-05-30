using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml;
using Sceelix.Core.Procedures;

namespace Sceelix.Core.Graphs.Expressions
{
    public class KeyValueExpressionNode : ExpressionNode
    {
        private readonly ExpressionNode _key;
        private readonly ExpressionNode _value;



        public KeyValueExpressionNode(ExpressionNode key, ExpressionNode value)
            : base(":")
        {
            _key = key;
            _value = value;
        }



        public KeyValueExpressionNode(XmlElement element)
            : base(element)
        {
            _key = InstanciateFromXml((XmlElement) element.ChildNodes[0]);
            _value = InstanciateFromXml((XmlElement) element.ChildNodes[1]);
        }



        public override object Clone()
        {
            return new KeyValueExpressionNode((ExpressionNode) _key.Clone(), (ExpressionNode) _value.Clone());
        }



        public override Expression GetExpressionTree(ParameterExpression inputDataExpression, ParameterExpression internalEntityExpression, Procedure masterProcedure, Procedure currentProcedure)
        {
            return Expression.Call(typeof(ExpressionHelper).GetMethod("CreateKeyValue"), _key.GetExpressionTree(inputDataExpression, internalEntityExpression, masterProcedure, currentProcedure), _value.GetExpressionTree(inputDataExpression, internalEntityExpression, masterProcedure, currentProcedure));
        }



        public override string GetOriginalExpression()
        {
            return _key.GetOriginalExpression() + " : " + _value.GetOriginalExpression();
        }



        public override IEnumerable<ExpressionNode> GetSubtree()
        {
            yield return _key;
            foreach (var expressionNode in _key.GetSubtree())
                yield return expressionNode;

            yield return _value;
            foreach (var expressionNode in _value.GetSubtree())
                yield return expressionNode;
        }



        public override void WriteSpecificXML(XmlWriter writer)
        {
            _key.WriteXML(writer);
            _value.WriteXML(writer);
        }
    }
}