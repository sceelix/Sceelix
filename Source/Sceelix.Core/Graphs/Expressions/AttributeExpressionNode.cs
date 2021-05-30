using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Xml;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Extensions;

namespace Sceelix.Core.Graphs.Expressions
{
    public class AttributeExpressionNode : ExpressionNode
    {
        private readonly bool _allowNull;



        public AttributeExpressionNode(string name, bool allowNull)
            : base(name)
        {
            _allowNull = allowNull;
        }



        public AttributeExpressionNode(XmlElement element)
            : base(element)
        {
            _allowNull = element.GetAttributeOrDefault("AllowNull", false);
        }



        public override object Clone()
        {
            return new AttributeExpressionNode(Name, _allowNull);
        }



        public override Expression GetExpressionTree(ParameterExpression inputDataExpression, ParameterExpression internalEntityExpression, Procedure masterProcedure, Procedure currentProcedure)
        {
            var key = AttributeParameter.GetKey(Name, masterProcedure);
            //String encodedName = AttributeParameter.Encode(Name, masterProcedure);
            //String encodedName = AttributeCollection.Encode(Name, masterProcedure);

            ConstantExpression portStringConstant = Expression.Constant(key);
            //ConstantExpression procedureConstant = Expression.Constant(masterProcedure);

            MethodCallExpression getAttributeCallFunction = Expression.Call(inputDataExpression, typeof(InputData).GetMethod("Get"), portStringConstant, Expression.Constant(_allowNull));

            //currentDataBlock.Inputs.Count
            return getAttributeCallFunction;
        }



        public override string GetOriginalExpression()
        {
            return "@" + Name + (_allowNull ? "?" : string.Empty); //"@{" + Name + "}"
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