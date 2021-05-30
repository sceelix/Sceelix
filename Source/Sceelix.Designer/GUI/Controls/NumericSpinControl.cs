using System;
using System.Globalization;
using System.Text.RegularExpressions;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Schema;
using Sceelix.Conversion;

namespace Sceelix.Designer.GUI.Controls
{
    public abstract class NumericSpinControl<T> : ContentControl where T : IConvertible, IComparable
    {
        //public event EventHandler<EventArgs> ValueChanged = delegate { };

        private readonly ExtendedTextBox _textBox;

        //private T _value;
        
        private T _minValue;
        private T _maxValue;
        private T _increment;
        
        //these should be set by the subclasses
        private int _minDecimalDigits = 2;
        private int _maxDecimalDigits = 2;

        private int? _decimalDigits;



        protected NumericSpinControl()
        {
            FlexibleStackPanel stackPanel = new FlexibleStackPanel() {Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Stretch,};
            stackPanel.Children.Add(_textBox = new ExtendedTextBox()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                //VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Vector4F(0),
                Text = "0"
            });

            var gameProperty = _textBox.Properties.Get<String>("Text");
            gameProperty.Changing += delegate(object sender, GamePropertyEventArgs<string> gamePropertyEventArgs)
            {
                if (!String.IsNullOrWhiteSpace(gamePropertyEventArgs.NewValue) && !Regex.IsMatch(gamePropertyEventArgs.NewValue, "^[0-9[.-]+$"))
                    gamePropertyEventArgs.CoercedValue = gamePropertyEventArgs.OldValue;
            };

            var focusedProperty = _textBox.Properties.Get<bool>("IsFocused");
            focusedProperty.Changed += delegate(object sender, GamePropertyEventArgs<bool> args)
            {
                //if it has lost the focus
                if (!_textBox.IsFocused)
                {
                    Value = ValidateValue();
                }
            };


            Button buttonUp, buttonDown;

            EqualStackPanel buttonPanel = new EqualStackPanel() {Orientation = Orientation.Vertical};
            buttonPanel.Children.Add(buttonUp = new Button() {Margin = new Vector4F(0), VerticalAlignment = VerticalAlignment.Stretch, Focusable = false, IsRepeatButton = true});
            buttonPanel.Children.Add(buttonDown = new Button() {Margin = new Vector4F(0), VerticalAlignment = VerticalAlignment.Stretch, Focusable = false, IsRepeatButton = true});
            buttonPanel.Width = 15;
            buttonPanel.VerticalAlignment = VerticalAlignment.Stretch;
            stackPanel.Children.Add(buttonPanel);

            buttonUp.Click += ButtonUpOnClick;
            buttonDown.Click += ButtonDownOnClick;

            stackPanel.Height = 20;

            Content = stackPanel;
        }



        private void ButtonDownOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                dynamic a = Value;
                Value = CheckBounds(Round(a - _increment, MaxDecimalDigits));
            }
            catch (OverflowException)
            {
                //ignore the button press
            }
        }



        private void ButtonUpOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                dynamic a = Value;
                Value = CheckBounds(Round(a + _increment, MaxDecimalDigits));
            }
            catch (OverflowException)
            {
                //ignore the button press
            }
        }

        



        private dynamic CheckBounds(dynamic value)
        {
            //TODO: complete here
            if (value.CompareTo(_minValue) < 0)
                return _minValue;
            if (value.CompareTo(_maxValue) > 0)
                return _maxValue;

            return value;
        }



        protected abstract T Round(T value, int digits);

        protected abstract string ToString(T value, int getDigits);



        /// <summary>
        /// This function cleans up, validates and converts the input text into the required type.
        /// </summary>
        /// <returns></returns>
        private T ValidateValue()
        {
            var text = _textBox.Text;
            String acceptedText = String.Empty;

            //if the separator is not allow, we consider any occurrence as if it would have been found already
            bool hasFoundSeparator = !AllowsSeparator;

            //first step - go char by char analyzing the text, clearing mistakes and trying to
            foreach (var c in text)
            {
                if (c == '-' && String.IsNullOrEmpty(acceptedText))
                    acceptedText += c;
                else if (char.IsDigit(c))
                    acceptedText += c;
                //(char.IsDigit(c) && currentDecimalDigits < MinDecimalDigits))

                else if (c == '.') // && MinDecimalDigits > 0 && currentDecimalDigits == -1)
                {
                    if (!hasFoundSeparator)
                    {
                        hasFoundSeparator = true;
                        acceptedText += c;
                    }
                    else
                        break;
                }
            }


            if(string.IsNullOrWhiteSpace(acceptedText))
                return DefaultValue();

            try
            {
                T value = ConvertHelper.Convert<T>(acceptedText);

                dynamic dvalue = CheckBounds(value);

                if(_decimalDigits.HasValue)
                    dvalue = Round(dvalue, _decimalDigits.Value);

                return dvalue;
            }
            catch (FormatException)
            {
                return DefaultValue();
            }
            catch (OverflowException)
            {
                return DefaultValue();
            }
        }



        private bool AllowsSeparator
        {
            get { return MaxDecimalDigits > 0; }
        }



        private T DefaultValue()
        {
            return (T) Convert.ChangeType("0", typeof(T), CultureInfo.InvariantCulture);
        }

        



/*public T Value
                {
                    get { return _value; }
                    set
                    {
                        _value = value;
                        _textBox.Text = ToString(_value);
                        ValueChanged.Invoke(this,EventArgs.Empty);
                    }
                }*/

        //public T Value { get; set; };

        public virtual T Value
        {
            get { return default(T); }
            set { _textBox.Text = ToString(value, GetDigits(value)); }
        }





        private int GetDigits(T value)
        {
            //try first to figure out how many decimal cases are in the string, up to the maximum number of digits allowed
            var stringValue = ToString(value, MaxDecimalDigits);
            
            //get the actial significant digits in the string, if possible
            string[] res = stringValue.Split('.');
            var significantDigits = res.Length > 1 ? GetSignificant(res[1]) : 0;

            //make sure the value is higher than the minimum, lower than the maximum, but 
            return Math.Min(Math.Max(significantDigits, MinDecimalDigits), MaxDecimalDigits);

            //return GetActualMax(Math.Min(Math.Max(0, MinDecimalDigits), MaxDecimalDigits));
        }



        private int GetActualMax(int value)
        {
            if (_decimalDigits.HasValue)
                return Math.Max(value, _decimalDigits.Value);

            return value;
        }



        private int GetSignificant(string stringValue)
        {
            for (int i = 0; i < stringValue.Length; i++)
            {
                if (stringValue[stringValue.Length - i - 1] != '0')
                    return stringValue.Length - i;
            }

            return 0;
        }



        /*protected abstract int GenericValueChangedPropertyId
                        {
                            get;
                        }*/




        public int? DecimalDigits
        {
            get
            {
                return _decimalDigits;
            }
            set
            {
                _decimalDigits = value;
                //_maxDecimalDigits = Math.Min(value,MaxDecimalDigits);
            }
        }


        protected int MinDecimalDigits
        {
            get { return _decimalDigits.HasValue ? Math.Max(_decimalDigits.Value,_minDecimalDigits) : _minDecimalDigits; }
            set { _minDecimalDigits = value; }
        }



        protected int MaxDecimalDigits
        {
            get { return _decimalDigits.HasValue ? Math.Min(_decimalDigits.Value, _maxDecimalDigits) : _maxDecimalDigits; }
            set { _maxDecimalDigits = value; }
        }

        public T MinValue
        {
            get { return _minValue; }
            set { _minValue = value; }
        }



        public T MaxValue
        {
            get { return _maxValue; }
            set { _maxValue = value; }
        }



        public T Increment
        {
            get { return _increment; }
            set { _increment = value; }
        }
    }


    public class IntSpinControl : NumericSpinControl<int>
    {
        public static readonly int ValueChangedPropertyId = CreateProperty<int>(typeof(IntSpinControl), "Value", GamePropertyCategories.Appearance, null, 0, UIPropertyOptions.AffectsRender);



        public IntSpinControl()
        {
            MinDecimalDigits = 0;
            MaxDecimalDigits = 0;
            MinValue = Int32.MinValue;
            MaxValue = Int32.MaxValue;
            Increment = 1;
        }



        public override int Value
        {
            get { return GetValue<int>(ValueChangedPropertyId); }
            set
            {
                SetValue(ValueChangedPropertyId, value);
                base.Value = value;
            }
        }



        protected override int Round(int value, int digits)
        {
            return value;
        }



        protected override string ToString(int value, int getDigits)
        {
            return value.ToString();
        }
    }

    public class FloatSpinControl : NumericSpinControl<float>
    {
        public static readonly int ValueChangedPropertyId = CreateProperty<float>(typeof(FloatSpinControl), "Value", GamePropertyCategories.Appearance, null, 0, UIPropertyOptions.AffectsRender);



        public FloatSpinControl()
        {
            MinDecimalDigits = 2;
            MaxDecimalDigits = 6;
            MinValue = Single.MinValue;
            MaxValue = Single.MaxValue;
            Increment = 1;
        }



        public override float Value
        {
            get { return GetValue<float>(ValueChangedPropertyId); }
            set
            {
                SetValue(ValueChangedPropertyId, value);
                base.Value = value;
            }
        }



        protected override float Round(float value, int digits)
        {
            //return value;
            return (float) Math.Round(value, digits);
        }
        


        protected override string ToString(float value, int digits)
        {
            return value.ToString("F" + digits, CultureInfo.InvariantCulture);
        }
    }

    public class DoubleSpinControl : NumericSpinControl<double>
    {
        public static readonly int ValueChangedPropertyId = CreateProperty<double>(typeof(DoubleSpinControl), "Value", GamePropertyCategories.Appearance, null, 0, UIPropertyOptions.AffectsRender);



        public DoubleSpinControl()
        {
            MinDecimalDigits = 3;
            MaxDecimalDigits = 15;
            MinValue = Single.MinValue;
            MaxValue = Single.MaxValue;
            Increment = 1;
        }



        public override double Value
        {
            get { return GetValue<double>(ValueChangedPropertyId); }
            set
            {
                SetValue(ValueChangedPropertyId, value);
                base.Value = value;
            }
        }



        protected override double Round(double value, int digits)
        {
            return Math.Round(value, digits);
        }



        protected override string ToString(double value, int digits)
        {
            return value.ToString("F" + digits, CultureInfo.InvariantCulture);
        }
    }

    public class DecimalSpinControl : NumericSpinControl<decimal>
    {
        public static readonly int ValueChangedPropertyId = CreateProperty<decimal>(typeof(DecimalSpinControl), "Value", GamePropertyCategories.Appearance, null, 0, UIPropertyOptions.AffectsRender);



        public DecimalSpinControl()
        {
            MinDecimalDigits = 4;
            MaxDecimalDigits = 28;
            MinValue = Decimal.MinValue;
            MaxValue = Decimal.MaxValue;
            Increment = 1;
        }



        public override decimal Value
        {
            get { return GetValue<decimal>(ValueChangedPropertyId); }
            set
            {
                SetValue(ValueChangedPropertyId, value);
                base.Value = value;
            }
        }



        protected override decimal Round(decimal value, int digits)
        {
            return Math.Round(value, digits);
        }



        protected override string ToString(decimal value, int digits)
        {
            return value.ToString("F" + digits, CultureInfo.InvariantCulture);
        }
    }
}