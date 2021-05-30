using System.Collections.Generic;
using System.Globalization;
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using Sceelix.Core.Graphs.Expressions;
using Sceelix.Designer.Graphs.ExpressionParsing;

namespace Sceelix.Core.ExpressionParsing
{
    public class ExpressionParser
    {
        private static readonly Dictionary<string, ITree> _cachedTrees = new Dictionary<string, ITree>();



        public static RecognitionException CanParse(string code)
        {
            try
            {
                // create a lexer that feeds off of input CharStream
                SceelixGrammarLexer lexer = new SceelixGrammarLexer(new ANTLRStringStream(code));

                // create a buffer of tokens pulled from the lexer
                CommonTokenStream tokens = new CommonTokenStream(lexer);

                // create a parser that feeds off the tokens buffer
                SceelixGrammarParser parser = new SceelixGrammarParser(tokens);

                // begin parsing at rule expr
                parser.expr();
            }
            catch (RecognitionException ex)
            {
                return ex;
            }

            return null;
        }



        private static List<ExpressionNode> GetChildExpressionNodeList(ITree tree)
        {
            if (tree == null)
                return new List<ExpressionNode>();

            List<ExpressionNode> expressionNodes = new List<ExpressionNode>(tree.ChildCount);
            for (int i = 0; i < tree.ChildCount; i++)
                expressionNodes.Add(ToSceelixParsedTree(tree.GetChild(i)));

            return expressionNodes;
        }



        public static ParsedExpression Parse(string code)
        {
            //if the string is empty, let's
            if (string.IsNullOrWhiteSpace(code))
                return ParsedExpression.Empty;

            ITree tree;
            if (_cachedTrees.TryGetValue(code, out tree))
                return new ParsedExpression(ToSceelixParsedTree(_cachedTrees[code]));

            try
            {
                // create a lexer that feeds off of input CharStream
                SceelixGrammarLexer lexer = new SceelixGrammarLexer(new ANTLRStringStream(code));

                // create a buffer of tokens pulled from the lexer
                CommonTokenStream tokens = new CommonTokenStream(lexer);

                // create a parser that feeds off the tokens buffer
                SceelixGrammarParser parser = new SceelixGrammarParser(tokens);

                // begin parsing at rule expr
                AstParserRuleReturnScope<CommonTree, IToken> astParserRuleReturnScope = parser.expr();

                //transform the tree into Sceelix structures
                var parsedExpression = new ParsedExpression(ToSceelixParsedTree(astParserRuleReturnScope.Tree));

                _cachedTrees[code] = astParserRuleReturnScope.Tree;

                return parsedExpression;
            }
            catch (RecognitionException ex)
            {
                return new ParsedExpression(new ErrorExpressionNode(code, ex.CharPositionInLine));
            }
        }



        private static ExpressionNode ToSceelixParsedTree(ITree tree)
        {
            switch (tree.Text)
            {
                case "||":
                case "&&":
                case "==":
                case "!=":
                case ">":
                case "<":
                case ">=":
                case "<=":
                case "+":
                case "*":
                case "/":
                case "%":
                    return new BinaryExpressionNode(tree.Text, ToSceelixParsedTree(tree.GetChild(0)), ToSceelixParsedTree(tree.GetChild(1)));
                case "-":
                    if (tree.ChildCount == 1)
                        return new UnaryExpressionNode(tree.Text, ToSceelixParsedTree(tree.GetChild(0)));

                    return new BinaryExpressionNode(tree.Text, ToSceelixParsedTree(tree.GetChild(0)), ToSceelixParsedTree(tree.GetChild(1)));
                case "ObjectMember":
                    return new MemberExpressionNode(GetChildExpressionNodeList(tree));
                case "[":
                    return new IndexExpressionNode(tree.Text, ToSceelixParsedTree(tree.GetChild(0)), ToSceelixParsedTree(tree.GetChild(1)));
                case "Property":
                    return new PropertyExpressionNode(tree.GetChild(0).Text);
                case "Function":
                    return new FunctionExpressionNode(tree.GetChild(0).Text, GetChildExpressionNodeList(tree.GetChild(1)));
                case "Array":
                    return new ArrayExpressionNode(GetChildExpressionNodeList(tree.GetChild(0)));
                case ":":
                    return new KeyValueExpressionNode(ToSceelixParsedTree(tree.GetChild(0)), ToSceelixParsedTree(tree.GetChild(1)));
                case "DirectFunction":
                    return new DirectFunctionExpressionNode(tree.GetChild(0).Text, GetChildExpressionNodeList(tree.GetChild(1)));
                case "()":
                    return new ParenthesisExpressionNode(ToSceelixParsedTree(tree.GetChild(0)));
                case "Parameter":
                    return new ParameterExpressionNode(tree.GetChild(0).Text); //.TrimStart('{').TrimEnd('}')
                //case "PortAttribute":
                //    return new PortVariableExpressionNode(tree.GetChild(0).Text.TrimStart('{').TrimEnd('}'), tree.GetChild(1).Text.TrimStart('{').TrimEnd('}'));
                case "Attribute":
                    return new AttributeExpressionNode(tree.GetChild(0).Text, tree.GetChild(1) != null); //.TrimStart('{').TrimEnd('}'));
                case "InternalAttribute":
                    return new InternalAttributeExpressionNode(tree.GetChild(0).Text, tree.GetChild(1) != null); //TrimStart('{').TrimEnd('}')
                //case "LocalAttribute":
                //    return new LocalAttributeExpressionNode(tree.GetChild(0).Text.TrimStart('{').TrimEnd('}'));
                case "ObjectData":
                    return new ObjectDataExpressionNode(tree.GetChild(0).Text.TrimStart('{').TrimEnd('}') /*, tree.GetChild(1).Text.TrimStart('{').TrimEnd('}')*/);
                case "!":
                    return new UnaryExpressionNode(tree.Text, ToSceelixParsedTree(tree.GetChild(0)));
                case "Int":
                    return new ConstantExpressionNode(int.Parse(tree.GetChild(0).Text));
                case "Float":
                    return new FloatExpressionNode(float.Parse(tree.GetChild(0).Text.Trim('f'), CultureInfo.InvariantCulture));
                case "Double":
                    return new DoubleExpressionNode(double.Parse(tree.GetChild(0).Text, CultureInfo.InvariantCulture));
                case "Boolean":
                    return new ConstantExpressionNode(bool.Parse(tree.GetChild(0).Text));
                case "Char":
                    return new ConstantExpressionNode(char.Parse(tree.GetChild(0).Text));
                case "String":
                    return new StringExpressionNode(tree.GetChild(0).Text.Trim('"'));
            }

            return null;
        }



        /*private Expression ProcessExpressionTree(ITree tree)
        {
            switch (tree.Text)
            {
                
                case "!":
                    return Expression.Not(ProcessExpressionTree(tree.GetChild(0)));
                case "Int":
                    return Expression.Constant(Int32.Parse(tree.GetChild(0).Text));
                case "Float":
                    return Expression.Constant(Single.Parse(tree.GetChild(0).Text));
                case "Boolean":
                    return Expression.Constant(Boolean.Parse(tree.GetChild(0).Text));
                case "Char":
                    return Expression.Constant(Char.Parse(tree.GetChild(0).Text));
                case "String":
                    return Expression.Constant(tree.GetChild(0).Text);
            }

            return null;
        }*/

        /*private void PrintTree(ITree tree, int level)
        {
            for (int i = 0; i < level; i++)
                Console.Write("\t");

            Console.WriteLine(tree.Text);

            for (int i = 0; i < tree.ChildCount; i++)
                PrintTree(tree.GetChild(i), level + 1);
        }*/
    }
}