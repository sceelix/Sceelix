using System;
using System.Linq.Expressions;
using System.Xml;
using Sceelix.Core.Procedures;
using Sceelix.Extensions;

namespace Sceelix.Core.Graphs.Expressions
{
    public class StringExpressionNode : ExpressionNode
    {
        private readonly string _value;



        public StringExpressionNode(string value)
            : base(value.GetType().Name)
        {
            _value = value;
        }



        public StringExpressionNode(XmlElement element)
            : base(element)
        {
            //the xmlreader ignores the string if the element value is empty
            //so we need this hack to save empty values
            var isEmpty = element.GetAttributeOrDefault("IsEmpty", false);

            _value = isEmpty ? string.Empty.PadLeft(int.Parse(element.InnerText), ' ') : element.InnerText;
        }



        public override Type ExpressionType => typeof(string);



        public override object Clone()
        {
            return new StringExpressionNode(_value);
        }



        public override Expression GetExpressionTree(ParameterExpression inputDataExpression, ParameterExpression internalEntityExpression, Procedure masterProcedure, Procedure currentProcedure)
        {
            return Expression.Constant(_value);
        }



        public override string GetOriginalExpression()
        {
            return "\"" + _value + "\"";
        }



        public override void PrintTree(int level)
        {
            base.PrintTree(level);
            Console.WriteLine(Name + "(\"" + _value + "\")");
        }



        public override void WriteSpecificXML(XmlWriter writer)
        {
            //the xmlreader ignores the string if the element value is empty
            //so we need this hack to save empty values
            if (string.IsNullOrWhiteSpace(_value))
            {
                writer.WriteAttributeString("IsEmpty", "true");
                writer.WriteString(_value.Length.ToString());
            }
            else
            {
                writer.WriteString(_value);
            }
        }
    }
}