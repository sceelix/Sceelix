using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml;
using Sceelix.Core.Procedures;
using Sceelix.Extensions;

namespace Sceelix.Core.Graphs.Expressions
{
    public abstract class ExpressionNode : ICloneable
    {
        protected ExpressionNode(string name)
        {
            Name = name;
        }



        protected ExpressionNode(XmlElement element)
        {
            Name = element.GetAttributeOrDefault<string>("Name");
        }



        public virtual Type ExpressionType => typeof(object);


        public string Name
        {
            get;
            protected set;
        }


        public abstract object Clone();


        public abstract Expression GetExpressionTree(ParameterExpression inputDataExpression, ParameterExpression internalEntityExpression, Procedure masterProcedure, Procedure currentProcedure);


        public abstract string GetOriginalExpression();



        public virtual IEnumerable<string> GetReferencedAttributes()
        {
            yield break;
        }



        public virtual IEnumerable<ExpressionNode> GetSubtree()
        {
            yield break;
        }



        public virtual bool HasReferenceToParameter(string label)
        {
            return false;
        }



        public virtual bool HasReferenceToVariable(string label)
        {
            return false;
        }



        public static ExpressionNode InstanciateFromXml(XmlElement element)
        {
            string expressionNodeType = element.GetAttributeOrDefault<string>("Type");
            return (ExpressionNode) Activator.CreateInstance(Type.GetType("Sceelix.Core.Graphs.Expressions." + expressionNodeType), element);
        }



        public virtual void PrintTree(int level)
        {
            for (int i = 0; i < level; i++)
                Console.Write("\t");
        }



        public virtual void RefactorGlobalParameters(string oldLabel, string newLabel)
        {
            //does nothing by default
        }



        public virtual void RefactorPortVariables(string oldLabel, string newLabel)
        {
            //does nothing by default
        }



        public virtual void WriteSpecificXML(XmlWriter writer)
        {
        }



        public void WriteXML(XmlWriter writer)
        {
            writer.WriteStartElement("ExprNode");
            {
                writer.WriteAttributeString("Name", Name);
                writer.WriteAttributeString("Type", GetType().Name);
                WriteSpecificXML(writer);
            }
            writer.WriteEndElement();
        }
    }
}