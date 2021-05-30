using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Xml;
using Sceelix.Core.Attributes;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Extensions;

namespace Sceelix.Core.Graphs.Expressions
{
    public class InternalAttributeExpressionNode : ExpressionNode
    {
        private readonly bool _allowNull;



        public InternalAttributeExpressionNode(string name, bool allowNull)
            : base(name)
        {
            _allowNull = allowNull;
        }



        public InternalAttributeExpressionNode(XmlElement element)
            : base(element)
        {
            _allowNull = element.GetAttributeOrDefault("AllowNull", false);
        }



        public override object Clone()
        {
            return new InternalAttributeExpressionNode(Name, _allowNull);
        }



        public static object GetAttribute(AttributeCollection collection, object name)
        {
            try
            {
                return collection[name];
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException(string.Format("An attribute with the name '{0}' does not exist in one of more input entities. Use the ? operator after the attribute name to bypass this verification and use the default 'null' value.", name));
            }
        }



        public override Expression GetExpressionTree(ParameterExpression inputDataExpression, ParameterExpression internalEntityExpression, Procedure masterProcedure, Procedure currentProcedure)
        {
            var key = AttributeParameter.GetKey(Name, masterProcedure);
            //String encodedName = AttributeParameter.Encode(Name, masterProcedure);
            //String encodedName = AttributeCollection.Encode(Name, masterProcedure);

            ConstantExpression portStringConstant = Expression.Constant(key);
            //ConstantExpression procedureConstant = Expression.Constant(masterProcedure);

            var attributeCollection = Expression.Property(internalEntityExpression, "Attributes");

            //MethodCallExpression getAttributeCallFunction = Expression.Call(attributeCollection, typeof(InputData).GetMethod("Get"), portStringConstant, Expression.Constant(_allowNull));

            //var methods= typeof (AttributeCollection).GetMethods();

            if (_allowNull)
                return Expression.Call(attributeCollection, typeof(AttributeCollection).GetMethod("TryGet"), portStringConstant);
            return Expression.Call(typeof(InternalAttributeExpressionNode).GetMethod("GetAttribute"), attributeCollection, portStringConstant);
            //return Expression.Call(attributeCollection, typeof(AttributeCollection).GetMethod("get_Item"), portStringConstant);
            //return Expression.Call(attributeCollection, typeof(AttributeCollection).GetMethods().First(x => x.Name == "Get" && !x.IsGenericMethod), portStringConstant);
            //currentDataBlock.Inputs.Count
            //return getAttributeCallFunction;
        }



        public override string GetOriginalExpression()
        {
            return "@@" + Name + (_allowNull ? "?" : string.Empty); //"@{" + Name + "}"
        }



        public override IEnumerable<string> GetReferencedAttributes()
        {
            yield return Name;
        }



        public override bool HasReferenceToVariable(string label)
        {
            return Name == label;
        }



        public override void RefactorPortVariables(string oldLabel, string newLabel)
        {
            if (oldLabel == Name)
                Name = newLabel;
        }



        public override void WriteSpecificXML(XmlWriter writer)
        {
            writer.WriteAttributeString("AllowNull", _allowNull.ToString(CultureInfo.InvariantCulture));
        }



        //public static 
    }
}