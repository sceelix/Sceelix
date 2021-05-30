using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml;
using Sceelix.Core.IO;
using Sceelix.Core.Procedures;

namespace Sceelix.Core.Graphs.Expressions
{
    /// <summary>
    /// Not used
    /// </summary>
    [Obsolete("To be removed once we confirm that we don't need it.")]
    public class LocalAttributeExpressionNode : ExpressionNode
    {
        public LocalAttributeExpressionNode(string name)
            : base(name)
        {
        }



        public LocalAttributeExpressionNode(XmlElement element)
            : base(element)
        {
        }



        public override object Clone()
        {
            return new LocalAttributeExpressionNode(Name);
        }



        public override Expression GetExpressionTree(ParameterExpression inputDataExpression, ParameterExpression internalEntityExpression, Procedure masterProcedure, Procedure currentProcedure)
        {
            string encodedName = "local-" + masterProcedure.GetHashCode() + ":" + Name; //AttributeParameter.Encode(Name, masterProcedure);
            //String encodedName = AttributeCollection.Encode(Name, masterProcedure);

            ConstantExpression portStringConstant = Expression.Constant(encodedName);
            ConstantExpression procedureConstant = Expression.Constant(masterProcedure);

            MethodCallExpression getAttributeCallFunction = Expression.Call(inputDataExpression, typeof(InputData).GetMethod("Get"), portStringConstant);

            //currentDataBlock.Inputs.Count
            return getAttributeCallFunction;
        }



        public override string GetOriginalExpression()
        {
            return "@local:" + Name; //"@{" + Name + "}"
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



        //public static 
    }
}