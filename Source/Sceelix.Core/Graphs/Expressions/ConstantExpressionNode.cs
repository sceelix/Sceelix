using System;
using System.Linq.Expressions;
using System.Xml;
using Sceelix.Conversion;
using Sceelix.Core.Procedures;
using Sceelix.Extensions;

namespace Sceelix.Core.Graphs.Expressions
{
    public class ConstantExpressionNode : ExpressionNode
    {
        private readonly object _value;



        public ConstantExpressionNode(object value)
            : base(value.GetType().Name)
        {
            _value = value;
        }



        public ConstantExpressionNode(XmlElement element)
            : base(element)
        {
            Type type = Type.GetType("System." + Name);

            _value = ConvertHelper.Convert(element.InnerText, type);
        }



        public override Type ExpressionType => _value.GetType();



        public override object Clone()
        {
            return new ConstantExpressionNode(_value.Clone());
        }



        public override Expression GetExpressionTree(ParameterExpression inputDataExpression, ParameterExpression internalEntityExpression, Procedure masterProcedure, Procedure currentProcedure)
        {
            return Expression.Convert(Expression.Constant(_value), typeof(object));
        }



        public override string GetOriginalExpression()
        {
            if (_value is bool)
                return _value.ToString().ToLower();

            return _value.ToString();
        }



        public override void PrintTree(int level)
        {
            base.PrintTree(level);

            Console.WriteLine(Name + "(" + _value + ")");
        }



        public override void WriteSpecificXML(XmlWriter writer)
        {
            writer.WriteValue(_value);
        }
    }
}