using System;
using System.Linq.Expressions;
using System.Xml;
using Sceelix.Core.IO;
using Sceelix.Core.Procedures;
using Sceelix.Extensions;

namespace Sceelix.Core.Graphs.Expressions
{
    public class ObjectDataExpressionNode : ExpressionNode
    {
        //private static readonly MethodInfo SceelixGetMethodInfo = typeof(Entity).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).First(val => !val.IsGenericMethod && val.Name == "Get");

        //private string _objectType;
        private readonly string _portName;
        private Type _expressionType;



        public ObjectDataExpressionNode( /*String objectType, */ string portName)
            : base( /*objectType + "." + */portName)
        {
            //_objectType = objectType;
            _portName = portName;
        }



        public ObjectDataExpressionNode(XmlElement element)
            : base(element)
        {
            //_objectType = element.GetAttributeOrDefault<String>("ObjectType");
            _portName = element.GetAttributeOrDefault<string>("PortName");
        }



        public override Type ExpressionType => _expressionType;



        public override object Clone()
        {
            return new ObjectDataExpressionNode(Name);
        }



        public override Expression GetExpressionTree(ParameterExpression inputDataExpression, ParameterExpression internalEntityExpression, Procedure masterProcedure, Procedure currentProcedure)
        {
            ConstantExpression portStringConstant = Expression.Constant(_portName);

            _expressionType = currentProcedure.Inputs[_portName].EntityType;

            MethodCallExpression indexProperty = Expression.Call(inputDataExpression, typeof(InputData).GetMethod("get_Item"), portStringConstant);

            UnaryExpression typedExpression = Expression.Convert(indexProperty, _expressionType);

            return typedExpression;
        }



        public override string GetOriginalExpression()
        {
            return "${" + _portName + "}"; // "{" + _objectType + "}";
        }



        public override void PrintTree(int level)
        {
            base.PrintTree(level);
            Console.WriteLine("ObjectData - " + Name);
        }



        public override void WriteSpecificXML(XmlWriter writer)
        {
            //writer.WriteAttributeString("ObjectType", _objectType);
            writer.WriteAttributeString("PortName", _portName);
        }
    }
}