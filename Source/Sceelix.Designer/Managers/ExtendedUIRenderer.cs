using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Extensions;

namespace Sceelix.Designer.Managers
{
    public class ExtendedUIRenderer : UIRenderer
    {
        public ExtendedUIRenderer(Game game, Theme theme)
            : base(game, theme)
        {
            RenderCallbacks.Add("SyntaxHighlightingTextBox", RenderExpressionTextBox);
        }

        public ExtendedUIRenderer(GraphicsDevice graphicsDevice, Theme theme)
            : base(graphicsDevice, theme)
        {
        }


        /// <summary>
        /// Gets the content bounds (= actual bounds - padding) snapped to pixels.
        /// </summary>
        private static RectangleF GetContentBoundsRounded(UIControl control)
        {
            Vector4F padding = control.Padding;
            return new RectangleF(
                (int)(control.ActualX + padding.X + 0.5f),
                (int)(control.ActualY + padding.Y + 0.5f),
                (int)(control.ActualWidth - padding.X - padding.Z + 0.5f),
                (int)(control.ActualHeight - padding.Y - padding.W + 0.5f));
        }


        private void RenderExpressionTextBox(UIControl control, UIRenderContext context)
        {
            // Textbox content is always clipped using the scissors rectangle.
            // TODO: TextBox should determine if clipping is necessary and set a VisualClip flag.

            // Background images.
            RenderImages(control, context, false);

            var textBox = control as SyntaxHighlightingTextBox;
            if (textBox != null)
            {
                RectangleF contentBounds = GetContentBoundsRounded(textBox);
                Rectangle originalScissorRectangle = GraphicsDevice.ScissorRectangle;

                EndBatch();
                Rectangle scissorRectangle = context.RenderTransform.Transform(textBox.VisualClip).ToRectangle(true);
                GraphicsDevice.ScissorRectangle = Rectangle.Intersect(scissorRectangle, originalScissorRectangle);

                BeginBatch();

                bool hasSelection = (textBox.VisualSelectionBounds.Count > 0);
                bool hasFocus = textBox.IsFocused;
                if (hasSelection && hasFocus)
                {
                    // Render selection.
                    Color selectionColor = textBox.SelectionColor;
                    selectionColor = Color.FromNonPremultiplied(selectionColor.ToVector4() * new Vector4(1, 1, 1, context.Opacity));

                    foreach (RectangleF selection in textBox.VisualSelectionBounds)
                    {
                        // The selection rectangle of an empty line has zero width.
                        // Show a small rectangle to indicate that the selection covers the line. 
                        RectangleF rectangle = selection;
                        rectangle.Width = Math.Max(rectangle.Width, 4);

                        // Draw rectangle using TextBox.SelectionColor.
                        context.RenderTransform.Draw(SpriteBatch, WhiteTexture, rectangle, null, selectionColor);
                    }
                }


                // Render text.
                Vector2F position = new Vector2F(contentBounds.X, contentBounds.Y);
                if (!textBox.IsMultiline)
                    position.X -= textBox.VisualOffset;
                else
                    position.Y -= textBox.VisualOffset;

                var font = GetFont(textBox.Font);
                var text = textBox.Text;
                var visualText = textBox.VisualText.ToString();
                

                //Render the background bracket matching
                if (!hasSelection
                    && hasFocus
                    && !textBox.VisualCaret.IsNaN
                    //&& IsCaretVisible(textBox.VisualCaret)
                )
                {
                    foreach (BracketStyle textBoxBracketStyle in textBox.BracketStyles)
                    {
                        //check the openchar
                        if (textBox.CaretIndex < textBox.Text.Length)
                        {
                            char highlightedChar = textBox.Text[textBox.CaretIndex];
                            if (highlightedChar == textBoxBracketStyle.OpenChar)
                            {
                                var rectangleColor = textBox.BracketMissingColor;

                                String subPart = text.Substring(textBox.CaretIndex+1);

                                Stack<char> startingStack = new Stack<char>(new[] { textBoxBracketStyle.OpenChar });
                                var index = Do(subPart, startingStack, textBox.BracketStyles.ToDictionary(x => x.OpenChar, y => y.CloseChar));//, textBox.BracketStyles.ToDictionary(x => x.OpenChar, y => y.CloseChar)
                                if (index >= 0)
                                {
                                    if (startingStack.Count == 0)
                                        rectangleColor = textBox.BracketMatchColor;

                                    var charPosition = position + GetPositionByIndex(visualText, font, textBox.CaretIndex + index + 1);
                                    var char2Size = font.MeasureString(highlightedChar.ToString());
                                    var draw2Rectangle = new RectangleF(charPosition.X, charPosition.Y, char2Size.X, char2Size.Y);

                                    context.RenderTransform.Draw(SpriteBatch, WhiteTexture, draw2Rectangle, null, rectangleColor);
                                }

                                var charSize = font.MeasureString(highlightedChar.ToString());
                                var drawRectangle = new RectangleF(textBox.VisualCaret.X, textBox.VisualCaret.Y, charSize.X, charSize.Y);
                                context.RenderTransform.Draw(SpriteBatch, WhiteTexture, drawRectangle, null, rectangleColor);
                            }
                        }
                        //check the closechar
                        if (textBox.CaretIndex > 0 && textBox.CaretIndex <= textBox.Text.Length)
                        {
                            char highlightedChar = textBox.Text[textBox.CaretIndex - 1];

                            if (highlightedChar == textBoxBracketStyle.CloseChar)
                            {
                                //by default, assume we don't find the match
                                var rectangleColor = textBox.BracketMissingColor;

                                //in this case, all is inverted, starting with the string
                                String subPart = text.Substring(0, textBox.CaretIndex - 1).Reverse();
                                
                                //the definition of "open" and "close" chars are also inverted
                                Stack<char> startingStack = new Stack<char>(new [] {textBoxBracketStyle.CloseChar });
                                var index = Do(subPart, startingStack, textBox.BracketStyles.ToDictionary(x => x.CloseChar, y => y.OpenChar));//, textBox.BracketStyles.ToDictionary(x => x.OpenChar, y => y.CloseChar)
                                if (index >= 0)
                                {
                                    //invert back the index, too
                                    index = subPart.Length - index-1;

                                    if (startingStack.Count == 0)
                                        rectangleColor = textBox.BracketMatchColor;

                                    var charPosition = position + GetPositionByIndex(visualText, font, index);
                                    var char2Size = font.MeasureString(highlightedChar.ToString());
                                    var draw2Rectangle = new RectangleF(charPosition.X, charPosition.Y, char2Size.X, char2Size.Y);

                                    context.RenderTransform.Draw(SpriteBatch, WhiteTexture, draw2Rectangle, null, rectangleColor);
                                }

                                var charSize = font.MeasureString(highlightedChar.ToString());
                                var drawRectangle = new RectangleF(textBox.VisualCaret.X - charSize.X, textBox.VisualCaret.Y, charSize.X, charSize.Y);
                                context.RenderTransform.Draw(SpriteBatch, WhiteTexture, drawRectangle, null, rectangleColor);


                            }
                        }
                        
                    }
                }

                
                Color foreground = GetForeground(control, GetState(context), context.Opacity);
                //context.RenderTransform.DrawString(SpriteBatch, font, textBox.VisualText, position, foreground);

                //render the highlighted text
                var lineBreakPositions = FindAllNewLines(visualText).ToList();
                int currentStringIndex = 0;
                Vector2F offset = Vector2F.Zero;
                for (int i = 0; i < textBox.ExpressionPieces.Count; i++)
                {
                    var token = textBox.ExpressionPieces[i].Key;
                    var color = textBox.ExpressionPieces[i].Value;
                    
                    var splitStrings = SplitAtLocations(token, lineBreakPositions.Select(x => x - currentStringIndex)).ToList();
                    for (int j = 0; j < splitStrings.Count; j++)
                    {
                        var stringSegment = splitStrings[j];

                        if (j > 0)
                        {
                            offset.X = 0;
                            offset.Y += font.MeasureString(token).Y;
                        }

                        context.RenderTransform.DrawString(SpriteBatch, font, stringSegment, position + offset, color);
                        offset.X += font.MeasureString(stringSegment).X;
                    }

                    currentStringIndex += textBox.ExpressionPieces[i].Key.Length;
                }

                if (!hasSelection
                    && hasFocus
                    && !textBox.VisualCaret.IsNaN
                    //&& IsCaretVisible(textBox.VisualCaret)
                )
                {
                    // Render caret.
                    RectangleF caret = new RectangleF(textBox.VisualCaret.X, textBox.VisualCaret.Y, 2, font.LineSpacing);
                    context.RenderTransform.Draw(SpriteBatch, WhiteTexture, caret, null, foreground);
                }

                EndBatch();
                GraphicsDevice.ScissorRectangle = originalScissorRectangle;
                BeginBatch();
            }

            // Visual children.
            foreach (var child in control.VisualChildren)
                child.Render(context);

            // Overlay images.
            RenderImages(control, context, true);
        }

        



        private Vector2F GetPositionByIndex(String str, SpriteFont spriteFont, int targetIndex)
        {
            Vector2F position = new Vector2F();

            if (targetIndex < 0)
                return position;

            int currentIndex = 0;
            foreach (char c in str)
            {
                if (currentIndex == targetIndex)
                    break;
                if (c == '\n')
                {
                    position.X = 0;
                    position.Y += spriteFont.LineSpacing;
                }
                else
                {
                    position.X += spriteFont.MeasureString(c.ToString()).X;
                    currentIndex++;
                }
            }

            /*for (int i = 0; i < str.Length; i++)
            {
                if (i == targetIndex)
                    break;
                if (str[i] == '\n')
                {
                    position.X = 0;
                    position.Y += spriteFont.LineSpacing;
                }
                else
                {
                    position.X += spriteFont.MeasureString(str[i].ToString()).X;
                    i++;
                }
            }*/

            return position;
        }



        private int Do(string str, Stack<char> stack, Dictionary<char, char> bracketStyles)//
        {
            for (int i = 0; i < str.Length; i++)
            {
                foreach (var keyValue in bracketStyles)
                {
                    //example: if the current char is an opening char, add it to the stack
                    if (str[i] == keyValue.Key)
                        stack.Push(keyValue.Key);
                    //if it is a closing char
                    else if (str[i] == keyValue.Value)
                    {
                        //either it is closing the element at the top of the stack
                        if (keyValue.Key == stack.Peek())
                        {
                            stack.Pop();

                            if (stack.Count == 0)
                                return i;
                        }
                        //or not, in which case it is a mismatch
                        else
                        {
                            return i;
                        }
                    }
                }
            }

            return -1;
        }



        private IEnumerable<String> SplitAtLocations(String str, IEnumerable<int> indices)
        {
            foreach (int index in indices)
            {
                //if the next index exceeds the length of the string,
                //the other will too, so we can stop here

                if (index >= str.Length)
                    break;
                    
                //if the index is smaller or equal 0, we can skip
                //otherwise we take the index to split the string
                else if(index >= 0)
                {
                    yield return str.Substring(0, index);
                    str = str.Substring(index);
                }
            }

            yield return str;
        }



        private IEnumerable<int> FindAllNewLines(string str)
        {
            //since we are using this to compare with a string that
            //has no newlines, we need to subtract the number of newlines
            //that came before each one
            int newLinesFound = 0;

            for (int i = 0; i < str.Length; i++)
                if (str[i] == '\n')
                    yield return i - newLinesFound++;
        }



        private static ThemeState GetState(UIRenderContext context)
        {
            object state;
            context.Data.TryGetValue("State", out state);
            return state as ThemeState;
        }

        /// <summary>
        /// Gets the foreground with pre-multiplied alpha for the given opacity.
        /// </summary>
        private static Color GetForeground(UIControl control, ThemeState state, float opacity)
        {
            Color foreground = (state != null && state.Foreground.HasValue) ? state.Foreground.Value : control.Foreground;

            return Color.FromNonPremultiplied(foreground.ToVector4() * new Vector4(1, 1, 1, opacity));
        }
    }
}