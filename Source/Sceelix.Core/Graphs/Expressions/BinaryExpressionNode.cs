using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml;
using Sceelix.Core.Graphs.Functions;
using Sceelix.Core.Procedures;

namespace Sceelix.Core.Graphs.Expressions
{
    public class BinaryExpressionNode : ExpressionNode
    {
        private readonly ExpressionNode _operand1;
        private readonly ExpressionNode _operand2;
        private Type _expressionType;



        public BinaryExpressionNode(XmlElement element)
            : base(element)
        {
            _operand1 = InstanciateFromXml((XmlElement) element.ChildNodes[0]);
            _operand2 = InstanciateFromXml((XmlElement) element.ChildNodes[1]);
        }



        public BinaryExpressionNode(string name, ExpressionNode operand1, ExpressionNode operand2)
            : base(name)
        {
            _operand1 = operand1;
            _operand2 = operand2;
        }



        public override Type ExpressionType => _expressionType;



        public override object Clone()
        {
            return new BinaryExpressionNode(Name, (ExpressionNode) _operand1.Clone(), (ExpressionNode) _operand2.Clone());
        }



        private Expression DoSwitch(ParameterExpression parameterExpression, ParameterExpression internalEntityExpression, Procedure masterProcedure, Procedure currentProcedure)
        {
            Expression operand1Expression = _operand1.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure);
            Expression operand2Expression = _operand2.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure);

            //TODO: Add support for shift operations and logical ^,& and |
            if (IsPrimitive(operand1Expression.Type) && IsPrimitive(operand2Expression.Type))
                //get the methodinfo and do the same casting as done in others
                switch (Name)
                {
                    case "||":
                        return GetPrimitiveOperator("op_ConditionalOr", operand1Expression, operand2Expression);
                    case "&&":
                        return GetPrimitiveOperator("op_ConditionalAnd", operand1Expression, operand2Expression);
                    case "==":
                        return GetPrimitiveOperator("op_Equal", operand1Expression, operand2Expression);
                    case "!=":
                        return GetPrimitiveOperator("op_NotEqual", operand1Expression, operand2Expression);
                    case ">":
                        return GetPrimitiveOperator("op_GreaterThan", operand1Expression, operand2Expression);
                    case "<":
                        return GetPrimitiveOperator("op_LesserThan", operand1Expression, operand2Expression);
                    case ">=":
                        return GetPrimitiveOperator("op_GreaterOrEqualThan", operand1Expression, operand2Expression);
                    case "<=":
                        return GetPrimitiveOperator("op_LesserOrEqualThan", operand1Expression, operand2Expression);
                    case "+":
                        return GetPrimitiveOperator("op_Addition", operand1Expression, operand2Expression);
                    case "-":
                        return GetPrimitiveOperator("op_Subtraction", operand1Expression, operand2Expression);
                    case "*":
                        return GetPrimitiveOperator("op_Multiplication", operand1Expression, operand2Expression);
                    case "/":
                        return GetPrimitiveOperator("op_Division", operand1Expression, operand2Expression);
                    case "%":
                        return GetPrimitiveOperator("op_Remainder", operand1Expression, operand2Expression);
                }
            else
                switch (Name)
                {
                    case "||":
                        return Expression.Call(typeof(ExpressionHelper).GetMethod("Or"), _operand1.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure), _operand2.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure));
                    case "&&":
                        return Expression.Call(typeof(ExpressionHelper).GetMethod("And"), _operand1.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure), _operand2.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure));
                    case "==":
                        return Expression.Call(typeof(ExpressionHelper).GetMethod("Equal"), _operand1.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure), _operand2.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure));
                    case "!=":
                        return Expression.Call(typeof(ExpressionHelper).GetMethod("NotEqual"), _operand1.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure), _operand2.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure));
                    case ">":
                        return Expression.Call(typeof(ExpressionHelper).GetMethod("GreaterThan"), _operand1.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure), _operand2.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure));
                    case "<":
                        return Expression.Call(typeof(ExpressionHelper).GetMethod("LessThan"), _operand1.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure), _operand2.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure));
                    case ">=":
                        return Expression.Call(typeof(ExpressionHelper).GetMethod("GreaterThanOrEqual"), _operand1.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure), _operand2.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure));
                    case "<=":
                        return Expression.Call(typeof(ExpressionHelper).GetMethod("LessThanOrEqual"), _operand1.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure), _operand2.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure));
                    case "+":
                        return Expression.Call(typeof(ExpressionHelper).GetMethod("Add"), _operand1.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure), _operand2.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure));
                    case "-":
                        return Expression.Call(typeof(ExpressionHelper).GetMethod("Subtract"), _operand1.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure), _operand2.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure));
                    case "*":
                        return Expression.Call(typeof(ExpressionHelper).GetMethod("Multiply"), _operand1.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure), _operand2.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure));
                    case "/":
                        return Expression.Call(typeof(ExpressionHelper).GetMethod("Divide"), _operand1.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure), _operand2.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure));
                    case "%":
                        return Expression.Call(typeof(ExpressionHelper).GetMethod("Modulo"), _operand1.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure), _operand2.GetExpressionTree(parameterExpression, internalEntityExpression, masterProcedure, currentProcedure));
                }


            return null;
        }



        public override Expression GetExpressionTree(ParameterExpression inputDataExpression, ParameterExpression internalEntityExpression, Procedure masterProcedure, Procedure currentProcedure)
        {
            Expression expression = DoSwitch(inputDataExpression, internalEntityExpression, masterProcedure, currentProcedure);
            _expressionType = expression.Type;

            return expression;
        }



        public override string GetOriginalExpression()
        {
            return _operand1.GetOriginalExpression() + " " + Name + " " + _operand2.GetOriginalExpression();
        }



        private Expression GetPrimitiveOperator(string opName, Expression operand1Expression, Expression operand2Expression)
        {
            MethodInfo methodInfo = typeof(PrimitiveOperators).GetMethod(opName, new[] {operand1Expression.Type, operand2Expression.Type});

            Expression[] argExpressions = {operand1Expression, operand2Expression};

            ParsedExpression.CastCollection(argExpressions, methodInfo.GetParameters().Select(val => val.ParameterType).ToArray());

            return Expression.Call(methodInfo, argExpressions);
        }



        public override IEnumerable<ExpressionNode> GetSubtree()
        {
            yield return _operand1;
            foreach (var expressionNode in _operand1.GetSubtree())
                yield return expressionNode;

            yield return _operand2;
            foreach (var expressionNode in _operand2.GetSubtree())
                yield return expressionNode;
        }



        public override bool HasReferenceToParameter(string label)
        {
            return _operand1.HasReferenceToParameter(label) || _operand2.HasReferenceToParameter(label);
        }



        public override bool HasReferenceToVariable(string label)
        {
            return _operand1.HasReferenceToVariable(label) || _operand2.HasReferenceToVariable(label);
        }



        public bool IsPrimitive(Type type)
        {
            return type.IsPrimitive || type == typeof(string);
        }



        public override void PrintTree(int level)
        {
            base.PrintTree(level);
            Console.WriteLine(Name);

            _operand1.PrintTree(level + 1);
            _operand2.PrintTree(level + 1);
        }



        public override void RefactorGlobalParameters(string oldLabel, string newLabel)
        {
            _operand1.RefactorGlobalParameters(oldLabel, newLabel);
            _operand2.RefactorGlobalParameters(oldLabel, newLabel);
        }



        public override void RefactorPortVariables(string oldLabel, string newLabel)
        {
            _operand1.RefactorPortVariables(oldLabel, newLabel);
            _operand2.RefactorPortVariables(oldLabel, newLabel);
        }



        public override void WriteSpecificXML(XmlWriter writer)
        {
            _operand1.WriteXML(writer);
            _operand2.WriteXML(writer);
        }
    }
}