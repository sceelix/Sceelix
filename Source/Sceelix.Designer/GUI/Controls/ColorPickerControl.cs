using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.Extensions;
using Sceelix.Designer.Utils;
using Sceelix.Extensions;

namespace Sceelix.Designer.GUI.Controls
{
    public class ColorPickerPopupContent : ContentControl
    {
        public static readonly int SelectedColorPropertyId = CreateProperty(typeof(ColorPickerPopupContent), "SelectedColor", GamePropertyCategories.Appearance, null, Color.Transparent, UIPropertyOptions.AffectsRender);

        private readonly ContentControl _selectedColorControl;
        private readonly IntSpinControl _alphaSpinControl;

        private readonly Texture2D _baseColorGradient;
        private readonly IntSpinControl _blueSpinControl;
        private readonly Image _colorAim;
        private readonly Image _colorGradientFill;
        private readonly IntSpinControl _greenSpinControl;
        private bool _handlingColorGradientFill;
        private bool _handlingHueGradientFill;
        private readonly TextBox _hexCodeControl;
        private readonly Image _hueArrow;
        private readonly Image _hueGradientFill;
        private readonly IntSpinControl _hueSpinControl;
        private readonly IntSpinControl _redSpinControl;
        private readonly IntSpinControl _satSpinControl;

        private bool _updatingControls = false;
        private readonly IntSpinControl _valSpinControl;



        public ColorPickerPopupContent()
        {
            //the size is fixed
            Width = 415;
            Height = 190;

            _baseColorGradient = EmbeddedResources.Load<Texture2D>("Resources/ColorGradient.png");

            //use a canvas with absolute positioning
            var canvas = new Canvas();

            canvas.Children.Add(_selectedColorControl = new ContentControl() {Style = "ColorBox", X = 220, Y = 5, Width = 180, Height = 40});
            canvas.Children.Add(_colorGradientFill = new Image() {Texture = _baseColorGradient, X = 5, Y = 5, Width = 180, Height = 180});
            canvas.Children.Add(_hueGradientFill = new Image() {Texture = EmbeddedResources.Load<Texture2D>("Resources/HueGradient.png"), X = 195, Y = 5, Width = 12, Height = 180});
            canvas.Children.Add(_colorAim = new Image() {Texture = EmbeddedResources.Load<Texture2D>("Resources/ColorAim.png"), X = 105, Y = 105, Width = 9, Height = 9});
            canvas.Children.Add(_hueArrow = new Image() {Texture = EmbeddedResources.Load<Texture2D>("Resources/ColorHueArrow.png"), X = 194, Y = 3, Width = 14, Height = 5});

            canvas.Children.Add(new TextBlock() {Text = "Red:", X = 220, Y = 60});
            canvas.Children.Add(new TextBlock() {Text = "Green:", X = 220, Y = 85});
            canvas.Children.Add(new TextBlock() {Text = "Blue:", X = 220, Y = 110});

            canvas.Children.Add(_redSpinControl = CreateSpinControl(260, 55, 45, 255, OnRGBSpinChange));
            canvas.Children.Add(_greenSpinControl = CreateSpinControl(260, 80, 45, 255, OnRGBSpinChange));
            canvas.Children.Add(_blueSpinControl = CreateSpinControl(260, 105, 45, 255, OnRGBSpinChange));
            

            canvas.Children.Add(new TextBlock() {Text = "Hue:", X = 320, Y = 60});
            canvas.Children.Add(new TextBlock() {Text = "Sat:", X = 320, Y = 85});
            canvas.Children.Add(new TextBlock() {Text = "Val:", X = 320, Y = 110});

            canvas.Children.Add(_hueSpinControl = CreateSpinControl(345, 55, 45, 360, OnHSVSpinChange));
            canvas.Children.Add(_satSpinControl = CreateSpinControl(345, 80, 45, 100, OnHSVSpinChange));
            canvas.Children.Add(_valSpinControl = CreateSpinControl(345, 105, 45, 100, OnHSVSpinChange));
            canvas.Children.Add(new TextBlock() {Text = "º", X = 395, Y = 60});
            canvas.Children.Add(new TextBlock() {Text = "%", X = 395, Y = 85});
            canvas.Children.Add(new TextBlock() {Text = "%", X = 395, Y = 110});

            canvas.Children.Add(new TextBlock() {Text = "Hex Code:", X = 220, Y = 140});
            canvas.Children.Add(_hexCodeControl = new ExtendedTextBox() {Text = "FF0000", X = 330, Y = 135, Width = 60});
            var isFocusedProperty = _hexCodeControl.Properties.Get<bool>("IsFocused");
            isFocusedProperty.Changed += IsFocusedPropertyOnChanged;

            canvas.Children.Add(new TextBlock() {Text = "Transparency/Alpha:", X = 220, Y = 165});
            canvas.Children.Add(_alphaSpinControl = CreateSpinControl(330, 160, 60, 100, OnAlphaSpinChange));

            Content = canvas;
        }



        /// <summary>
        /// Sets color data without updating all the controls.
        /// </summary>
        private Color InternalSelectedColor
        {
            get { return GetValue<Color>(SelectedColorPropertyId); }
            set { SetValue(SelectedColorPropertyId, value); }
        }



        /// <summary>
        /// Sets color data and updates the corresponding controls.
        /// </summary>
        public Color SelectedColor
        {
            get { return GetValue<Color>(SelectedColorPropertyId); }
            set
            {
                SetValue(SelectedColorPropertyId, value);

                UpdateControls();
            }
        }



        private IntSpinControl CreateSpinControl(int x, int y, int width, int maxValue, EventHandler<GamePropertyEventArgs<int>> changedEventHandler)
        {
            var spinControl = new IntSpinControl() {X = x, Y = y, Width = width, MinValue = 0, MaxValue = maxValue};
            var property = spinControl.Properties.Get<int>("Value");
            property.Changed += changedEventHandler;

            return spinControl;
        }



        private void UpdateControls()
        {
            UpdateDemo();
            UpdateColorGradient();
            UpdateColorAim();
            UpdateHueSlider();
            UpdateRGBSpins();
            UpdateHSVSpins();
            UpdateHexCodeTextbox();
            UpdateAlphaSpin();
        }



        private void UpdateDemo()
        {
            _selectedColorControl.Background = SelectedColor;
        }



        private void UpdateColorGradient()
        {
            var gdiColor = SelectedColor.ToGDIColor();
            var hue = gdiColor.GetHue();

            _colorGradientFill.Texture = _baseColorGradient.ModifyHue((int) hue);
        }



        private void UpdateColorAim()
        {
            //update the hsv controls
            var gdiColor = SelectedColor.ToGDIColor();

            var saturation = gdiColor.GetSaturation();
            var value = gdiColor.GetBrightness();

            _colorAim.X = _colorGradientFill.X + saturation*_colorGradientFill.Width - _colorAim.Width/2;
            _colorAim.Y = _colorGradientFill.Y + (1 - value)*_colorGradientFill.Height - _colorAim.Height/2;
        }



        private void UpdateHueSlider()
        {
            //update the hsv controls
            var gdiColor = SelectedColor.ToGDIColor();

            var hue = gdiColor.GetHue();

            var huePercentage = hue/360f;

            _hueArrow.Y = _hueGradientFill.Y + (1 - huePercentage)*_hueGradientFill.Height;
        }



        void UpdateHSVSpins()
        {
            _updatingControls = true;

            var colorToHSV = SelectedColor.ColorToHSV();

            _hueSpinControl.Value = (int) colorToHSV[0];
            _satSpinControl.Value = (int) (colorToHSV[1]*100);
            _valSpinControl.Value = (int) (colorToHSV[2]*100);

            _updatingControls = false;
        }



        private void UpdateHSVSpins(double hue, double saturation, double value)
        {
            _updatingControls = true;

            _hueSpinControl.Value = (int) hue;
            _satSpinControl.Value = (int) saturation;
            _valSpinControl.Value = (int) value;

            _updatingControls = false;
        }



        void UpdateRGBSpins()
        {
            _updatingControls = true;

            //update the rgb controls
            _redSpinControl.Value = SelectedColor.R;
            _greenSpinControl.Value = SelectedColor.G;
            _blueSpinControl.Value = SelectedColor.B;

            _updatingControls = false;
        }



        void UpdateHexCodeTextbox()
        {
            _updatingControls = true;

            _hexCodeControl.Text = SelectedColor.R.ToString("X2") + SelectedColor.G.ToString("X2") +
                                   SelectedColor.B.ToString("X2");

            _updatingControls = false;
        }



        void UpdateAlphaSpin()
        {
            _updatingControls = true;

            _alphaSpinControl.Value = (int) ((SelectedColor.A/255f)*100f);

            _updatingControls = false;
        }



        private void IsFocusedPropertyOnChanged(object sender, GamePropertyEventArgs<bool> e)
        {
            //if it has lost focus
            if (!e.NewValue)
            {
                if (String.IsNullOrWhiteSpace(_hexCodeControl.Text) ||
                    !Regex.IsMatch(_hexCodeControl.Text, "^#?(?:[0-9a-fA-F]{3}){1,2}$"))
                {
                    _hexCodeControl.Text = "FFFFFF";
                }
                else
                {
                    var value = _hexCodeControl.Text;

                    if (value.StartsWith("#"))
                        value = value.Replace("#", "");

                    var components = value.SplitSize(2).ToArray();

                    var red = Int32.Parse(components[0], NumberStyles.HexNumber);
                    var green = Int32.Parse(components[1], NumberStyles.HexNumber);
                    var blue = Int32.Parse(components[2], NumberStyles.HexNumber);

                    InternalSelectedColor = new Color(red/255f, green/255f, blue/255f, _alphaSpinControl.Value/100f);

                    UpdateDemo();
                    UpdateHSVSpins();
                    UpdateColorGradient();
                    UpdateColorAim();
                    UpdateHueSlider();
                    UpdateRGBSpins();
                }
            }
        }



        private void OnAlphaSpinChange(object sender, GamePropertyEventArgs<int> e)
        {
            if (!_updatingControls)
            {
                InternalSelectedColor = new Color(InternalSelectedColor, _alphaSpinControl.Value/100f);

                UpdateDemo();
            }
        }



        private void OnHSVSpinChange(object sender, GamePropertyEventArgs<int> e)
        {
            if (!_updatingControls)
            {
                InternalSelectedColor = ColorExtension.HsvToRgb(_hueSpinControl.Value, _satSpinControl.Value/100d, _valSpinControl.Value/100d);

                UpdateDemo();
                UpdateColorAim();
                UpdateColorGradient();
                UpdateHueSlider();
                UpdateRGBSpins();
                UpdateHexCodeTextbox();
            }
        }



        private void OnRGBSpinChange(object sender, GamePropertyEventArgs<int> e)
        {
            if (!_updatingControls)
            {
                InternalSelectedColor = new Color(_redSpinControl.Value, _greenSpinControl.Value, _blueSpinControl.Value, (int)((_alphaSpinControl.Value / 100f) * 255));

                UpdateDemo();
                UpdateColorAim();
                UpdateColorGradient();
                UpdateHueSlider();
                UpdateHSVSpins();
                UpdateHexCodeTextbox();
            }
        }



        protected override void OnHandleInput(InputContext context)
        {
            if (!InputService.IsMouseOrTouchHandled)
            {
                base.OnHandleInput(context);

                if (_colorGradientFill.IsMouseOver && InputService.IsPressed(MouseButtons.Left, false))
                    _handlingColorGradientFill = true;

                if (_hueGradientFill.IsMouseOver && InputService.IsPressed(MouseButtons.Left, false))
                    _handlingHueGradientFill = true;


                if (_handlingColorGradientFill)
                {
                    if (InputService.IsReleased(MouseButtons.Left))
                        _handlingColorGradientFill = false;
                    else
                    {
                        var xValue = InputService.MousePosition.X - _colorGradientFill.ActualX;
                        var yValue = InputService.MousePosition.Y - _colorGradientFill.ActualY;

                        xValue = Math.Max(0, Math.Min(xValue, _colorGradientFill.ActualWidth));
                        yValue = Math.Max(0, Math.Min(yValue, _colorGradientFill.ActualHeight));

                        _colorAim.X = _colorGradientFill.X + xValue - _colorAim.ActualWidth/2;
                        _colorAim.Y = _colorGradientFill.Y + yValue - _colorAim.ActualHeight/2;

                        var saturation = (xValue/_colorGradientFill.ActualWidth);
                        var value = (1 - (yValue/_colorGradientFill.ActualHeight));

                        InternalSelectedColor = new Color(ColorExtension.HsvToRgb(_hueSpinControl.Value, saturation, value), _alphaSpinControl.Value/100f);

                        UpdateDemo();
                        UpdateHSVSpins(_hueSpinControl.Value, saturation*100, value*100);
                        UpdateRGBSpins();
                        UpdateHexCodeTextbox();
                    }
                }
                else if (_handlingHueGradientFill)
                {
                    if (InputService.IsReleased(MouseButtons.Left))
                        _handlingHueGradientFill = false;
                    else
                    {
                        var yValue = InputService.MousePosition.Y - _hueGradientFill.ActualY;
                        yValue = Math.Max(0, Math.Min(yValue, _hueGradientFill.ActualHeight));

                        //reposition the arrow
                        _hueArrow.Y = _hueGradientFill.Y + yValue - _hueArrow.ActualHeight/2;

                        var hue = ((1 - (yValue/_hueGradientFill.ActualHeight))*360);

                        InternalSelectedColor = new Color(ColorExtension.HsvToRgb(_hueSpinControl.Value, _satSpinControl.Value/100d, _valSpinControl.Value/100d), _alphaSpinControl.Value/100f);

                        UpdateDemo();
                        UpdateColorGradient();
                        UpdateHSVSpins(hue, _satSpinControl.Value, _valSpinControl.Value);
                        UpdateRGBSpins();
                        UpdateHexCodeTextbox();
                    }
                }
            }
        }
    }


    public class ColorPickerControl : ContentControl
    {
        public static readonly int SelectedColorPropertyId = CreateProperty(typeof(ColorPickerControl), "SelectedColor", GamePropertyCategories.Appearance, null, Color.Transparent, UIPropertyOptions.AffectsRender);

        //private Color _selectedColor = Color.Red;
        private readonly ContentControl _colorFillControl;

        private readonly PopupWindow _window;
        private readonly ColorPickerPopupContent colorPickerPopupContent;



        public ColorPickerControl()
        {
            Content = _colorFillControl = new ContentControl()
            {
                Style = "ColorBox",
                Margin = new Vector4F(0),
                Background = Color.Transparent,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            Height = 20;

            colorPickerPopupContent = new ColorPickerPopupContent();
            var colorProperty = colorPickerPopupContent.Properties.Get<Color>("SelectedColor");
            colorProperty.Changed += (sender, args) =>
            {
                SetValue(SelectedColorPropertyId, args.NewValue);

                _colorFillControl.Background = args.NewValue;
            };

            _window = new PopupWindow(colorPickerPopupContent);
            //colorPickerPopupContent.SelectedColor = Color.Green;
        }



        public Color SelectedColor
        {
            get { return GetValue<Color>(SelectedColorPropertyId); }
            set
            {
                SetValue(SelectedColorPropertyId, value);

                //also, update the popup data
                colorPickerPopupContent.SelectedColor = value;
            }
        }



        protected override void OnHandleInput(InputContext context)
        {
            base.OnHandleInput(context);

            if (!InputService.IsMouseOrTouchHandled)
            {
                if (IsMouseOver && InputService.IsPressed(MouseButtons.Left, false))
                {
                    InputService.IsMouseOrTouchHandled = true;

                    _window.Open(this);
                }
            }
        }



        /*public Color SelectedColor
        {
            get { return _selectedColor; }
            set
            {
                _selectedColor = value;
                _colorFillControl.Background = _selectedColor;
            }
        }*/
    }
}