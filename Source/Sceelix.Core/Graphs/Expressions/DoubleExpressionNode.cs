using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Xml;
using Sceelix.Core.Procedures;

namespace Sceelix.Core.Graphs.Expressions
{
    public class DoubleExpressionNode : ExpressionNode
    {
        private readonly double _value;



        public DoubleExpressionNode(double value)
            : base(value.GetType().Name)
        {
            _value = value;
        }



        public DoubleExpressionNode(XmlElement element)
            : base(element)
        {
            _value = Convert.ToDouble(element.InnerText, CultureInfo.InvariantCulture);
        }



        public override Type ExpressionType => typeof(double);



        public override object Clone()
        {
            return new DoubleExpressionNode(_value);
        }



        public override Expression GetExpressionTree(ParameterExpression inputDataExpression, ParameterExpression internalEntityExpression, Procedure masterProcedure, Procedure currentProcedure)
        {
            //return Expression.Constant(_value);
            return Expression.Convert(Expression.Constant(_value), typeof(object));
        }



        public override string GetOriginalExpression()
        {
            return _value.ToString(CultureInfo.InvariantCulture);
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