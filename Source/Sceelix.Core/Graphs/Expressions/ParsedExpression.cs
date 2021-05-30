using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml;
using Sceelix.Conversion;
using Sceelix.Core.Data;
using Sceelix.Core.IO;
using Sceelix.Core.Procedures;

namespace Sceelix.Core.Graphs.Expressions
{
    public class ParsedExpression : ICloneable
    {
        private readonly ExpressionNode _rootNode;



        public ParsedExpression(ExpressionNode rootNode)
        {
            _rootNode = rootNode;
        }



        public ParsedExpression(XmlElement element)
        {
            _rootNode = ExpressionNode.InstanciateFromXml((XmlElement) element.FirstChild);
        }



        public static ParsedExpression Empty => new ParsedExpression(new ConstantExpressionNode(""));


        public string OriginalExpression => _rootNode.GetOriginalExpression();



        public static void CastCollection(List<Expression> argExpressions, List<Type> typeList)
        {
            for (int i = 0; i < argExpressions.Count; i++)
                argExpressions[i] = Expression.Convert(argExpressions[i], typeList[i]);
        }



        public static void CastCollection(Expression[] argExpressions, Type[] typeList)
        {
            for (int i = 0; i < argExpressions.Length; i++)
                argExpressions[i] = Expression.Convert(argExpressions[i], typeList[i]);
        }



        public object Clone()
        {
            return new ParsedExpression((ExpressionNode) _rootNode.Clone());
        }



        public Func<InputData, IEntity, object> GetCompiledExpressionTree(Procedure masterProcedure, Procedure currentProcedure, Type argumentType)
        {
            //the "inputdata => " part of th expression
            ParameterExpression parameterExpression = Expression.Parameter(typeof(InputData), "inputData");

            ParameterExpression entityExpression = Expression.Parameter(typeof(IEntity), "specificEntity");

            //get the expression and convert the result to object
            Expression expressionTree = GetExpressionTree(parameterExpression, entityExpression, masterProcedure, currentProcedure, argumentType);

            return Expression.Lambda<Func<InputData, IEntity, object>>(expressionTree, parameterExpression, entityExpression).Compile();
        }



        private Expression GetExpressionTree(ParameterExpression parameterExpression, ParameterExpression internalEntityExpression, Procedure masterProcedure, Procedure currentProcedure, Type argumentType)
        {
            Expression expressionTree = _rootNode.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure);
            Expression objectExpressionTree = Expression.Convert(expressionTree, typeof(object));
            MethodCallExpression callOfSceelixConvert = Expression.Call(typeof(ConvertHelper).GetMethod("Convert", new[] {typeof(object), typeof(Type)}), objectExpressionTree, Expression.Constant(argumentType));

            return callOfSceelixConvert;
        }



        public IEnumerable<string> GetReferencedAttributes()
        {
            return _rootNode.GetReferencedAttributes();
        }



        public IEnumerable<ExpressionNode> GetSubtree()
        {
            yield return _rootNode;

            foreach (var expressionNode in _rootNode.GetSubtree())
                yield return expressionNode;
        }



        public bool HasReferenceToParameter(string label)
        {
            return _rootNode.HasReferenceToParameter(label);
        }



        public bool HasReferenceToVariable(string label)
        {
            return _rootNode.HasReferenceToVariable(label);
        }



        public void PrintTree()
        {
            _rootNode.PrintTree(0);
        }



        public void RefactorGlobalParameters(string oldLabel, string newLabel)
        {
            _rootNode.RefactorGlobalParameters(oldLabel, newLabel);
        }



        public void RefactorPortVariables(string oldLabel, string newLabel)
        {
            _rootNode.RefactorPortVariables(oldLabel, newLabel);
        }



        public void WriteXML(XmlWriter writer)
        {
            writer.WriteStartElement("ParsedExpression");
            {
                _rootNode.WriteXML(writer);
            }
            writer.WriteEndElement();
        }
    }
}