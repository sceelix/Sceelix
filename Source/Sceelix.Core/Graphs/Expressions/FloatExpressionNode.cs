using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Xml;
using Sceelix.Core.Procedures;

namespace Sceelix.Core.Graphs.Expressions
{
    public class FloatExpressionNode : ExpressionNode
    {
        private readonly float _value;



        public FloatExpressionNode(float value)
            : base(value.GetType().Name)
        {
            _value = value;
        }



        public FloatExpressionNode(XmlElement element)
            : base(element)
        {
            _value = Convert.ToSingle(element.InnerText, CultureInfo.InvariantCulture);
        }



        public override Type ExpressionType => typeof(float);



        public override object Clone()
        {
            return new FloatExpressionNode(_value);
        }



        public override Expression GetExpressionTree(ParameterExpression inputDataExpression, ParameterExpression internalEntityExpression, Procedure masterProcedure, Procedure currentProcedure)
        {
            //return Expression.Constant(_value);
            return Expression.Convert(Expression.Constant(_value), typeof(object));
        }



        public override string GetOriginalExpression()
        {
            return _value.ToString(CultureInfo.InvariantCulture) + "f";
        }



        public override void PrintTree(int level)
        {
            base.PrintTree(level);
            Console.WriteLine(Name + "(" + _value + "f)");
        }



        public override void WriteSpecificXML(XmlWriter writer)
        {
            writer.WriteValue(_value);
        }
    }
}