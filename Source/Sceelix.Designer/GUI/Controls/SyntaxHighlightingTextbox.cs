using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DigitalRune.Game;
using DigitalRune.Game.UI.Controls;
using Microsoft.Xna.Framework;

namespace Sceelix.Designer.GUI.Controls
{
    public class SyntaxHighlightingStyle
    {
        public SyntaxHighlightingStyle(string name, string regex, Color color)
        {
            Name = name;
            Regex = regex;
            Color = color;
        }



        public String Name
        {
            get;
            set;
        }



        public String Regex
        {
            get;
            set;
        }



        public Color Color
        {
            get;
            set;
        }
    }

    public class BracketStyle
    {
        private readonly char _openChar;
        private readonly char _closeChar;



        public BracketStyle(char openChar, char closeChar)
        {
            _openChar = openChar;
            _closeChar = closeChar;
        }



        public char OpenChar
        {
            get { return _openChar; }
        }



        public char CloseChar
        {
            get { return _closeChar; }
        }
    }

    public class SyntaxHighlightingTextBox : ExtendedTextBox
    {
        List<SyntaxHighlightingStyle> _syntaxHighlightingStyles = new List<SyntaxHighlightingStyle>();
        List<BracketStyle> _bracketStyles = new List<BracketStyle>();
        private readonly List<KeyValuePair<string, Color>> _expressionPieces = new List<KeyValuePair<string, Color>>();



        public SyntaxHighlightingTextBox()
        {
            Style = "SyntaxHighlightingTextBox";
        }



        public List<KeyValuePair<String, Color>> ExpressionPieces
        {
            get { return _expressionPieces; }
        }



        public List<SyntaxHighlightingStyle> SyntaxHighlightingStyles
        {
            get { return _syntaxHighlightingStyles; }
            set { _syntaxHighlightingStyles = value; }
        }



        public List<BracketStyle> BracketStyles
        {
            get { return _bracketStyles; }
            set { _bracketStyles = value; }
        }



        public Color BracketMatchColor
        {
            get;
            set;
        }



        public Color BracketMissingColor
        {
            get;
            set;
        }



        protected override void OnLoad()
        {
            base.OnLoad();
            var textChangedProperty = this.Properties.Get<String>("Text");
            textChangedProperty.Changed += TextChangedPropertyOnChanged;
            CalculateExpressionPieces();
        }



        private void TextChangedPropertyOnChanged(object sender, GamePropertyEventArgs<string> gamePropertyEventArgs)
        {
            CalculateExpressionPieces();
        }


        public void InsertTextInCaret(String stringToInsert, char startingChar = ' ', char endingChar = ' ')
        {
            var placementIndex = CaretIndex;

            //place an empty space before, if there isn't one already
            if (placementIndex > 0 && Text[placementIndex - 1] != startingChar)
                stringToInsert = startingChar + stringToInsert;

            //place an empty space before, if there isn't one already
            if (placementIndex <= Text.Length - 1 && Text[placementIndex] != endingChar)
                stringToInsert = stringToInsert + endingChar;

            Text = Text.Insert(placementIndex, stringToInsert);
            CaretIndex = placementIndex + stringToInsert.Length;
            Focus();
        }


        private void CalculateExpressionPieces()
        {
            var regexExpress = String.Join("|", _syntaxHighlightingStyles.Select(x => "(?<" + x.Name + ">" + x.Regex + ")").Concat(new [] { "(.)" } ));
            

            Regex regex = new Regex(regexExpress);
            var matches = regex.Matches(Text);

            _expressionPieces.Clear();
            foreach (Match match in matches)
            {
                var piece = new KeyValuePair<string, Color>(match.Value, Foreground);

                foreach (SyntaxHighlightingStyle style in _syntaxHighlightingStyles)
                {
                    if (IsMatchGroup(match, style.Name))
                    {
                        piece = new KeyValuePair<string, Color>(match.Value, style.Color);
                        break;
                    }
                }

                _expressionPieces.Add(piece);
            }
        }



        private bool IsMatchGroup(Match match, string groupName)
        {
            return !String.IsNullOrWhiteSpace(match.Groups[groupName].Value);
        }
    }
}