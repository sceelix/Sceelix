using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DigitalRune.Animation;
using DigitalRune.Animation.Easing;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Ionic.Zip;
using Sceelix.Designer.Annotations;
using Sceelix.Designer.GUI.MenuControls;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Managers;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.Plugins;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.Layouts
{
    public class LayoutManager
    {
        public const string DefaultLayoutName = "ApplicationLayout";
        private readonly List<Layout> _availableLayouts = new List<Layout>();
        
        private readonly PluginManager _pluginManager;
        private readonly IServiceLocator _services;
        private UIScreen _uiScreen;

        private Layout _defaultLayout = Layout.FromFile(DefaultLayoutName);
        private MenuChild _layoutsMenuItem;

        private readonly DelayedEventRunner _delayedEventRunner = new DelayedEventRunner();
        private MessageManager _messageManager;
        private WindowAnimator _windowAnimator;


        public LayoutManager(IServiceLocator services, UIScreen uiScreen)
        {
            _services = services;
            
            _pluginManager = services.Get<PluginManager>();
            _messageManager = services.Get<MessageManager>();
            _windowAnimator = services.Get<WindowAnimator>();

            _uiScreen = uiScreen;

            try
            {
                RefreshAvailableLayouts();
            }
            catch (Exception ex)
            {
                DesignerProgram.Log.Error("Error initializing LayoutManager.", ex);
            }


            //To load a specific, we could sue (x => x.Name == "Complete")
            if (_defaultLayout == null)
                _defaultLayout = new Layout(DefaultLayoutName, _availableLayouts.First());
        }


        /// <summary>
        /// Looks up the layouts folder for available layout files.
        /// </summary>
        private void RefreshAvailableLayouts()
        {
            _availableLayouts.Clear();

            RefreshLayoutsFromEmbeddedZip();
            
            RefreshLayoutsFromUserFolder();
        }


        private void RefreshLayoutsFromEmbeddedZip()
        {
            ZipFile zipFile = ZipFile.Read(EmbeddedResources.Load<Stream>("Resources/Layouts.zip",false));
            foreach (ZipEntry zipEntry in zipFile)
            {
                var name = Path.GetFileNameWithoutExtension(zipEntry.FileName);

                using (MemoryStream stream = new MemoryStream())
                {
                    zipEntry.Extract(stream);
                    stream.Seek(0, SeekOrigin.Begin);

                    _availableLayouts.Add(Layout.FromStream(name, stream));
                }
            }
        }

        
        private void RefreshLayoutsFromUserFolder()
        {
            foreach (string file in Directory.EnumerateFiles(SceelixApplicationInfo.LayoutsFolder, "*" + Layout.LayoutExtension, SearchOption.AllDirectories))
            {
                if (Path.GetFileNameWithoutExtension(file) != DefaultLayoutName)
                {
                    var userLayout = Layout.FromFile(Path.GetFileNameWithoutExtension(file));
                    userLayout.IsUserLayout = true;
                    _availableLayouts.Add(userLayout);
                }
            }
        }

        
        /// <summary>
        /// Loads the default layout
        /// </summary>
        public void LoadDefaultLayout()
        {
            LoadLayout(_defaultLayout, ScreenManager.GetWindowArea(_uiScreen));
        }



        private void LoadLayout(Layout layout, RectangleF screenBounds)
        {
            //before doing anything else, close windows that are not indicated in the layout
            HashSet<String> windowsInLayout = new HashSet<string>(layout.Locations.Select(val => val.WindowType));
            foreach (var openedWindow in _uiScreen.Children.OfType<AnimatedWindow>().ToList())
            {
                if (!windowsInLayout.Contains(openedWindow.GetType().FullName))
                    openedWindow.Close();
            }

            //now, open the windows that are indicated!
            foreach (var location in layout.Locations)
            {
                DesignerWindowAttribute attribute = _pluginManager.PluginWindows.FirstOrDefault(val => val.WindowType.ToString() == location.WindowType);
                if (attribute != null)
                {
                    var window = CreateOrGetWindow(attribute.WindowType);

                    //DesignerWindow window = CreateWindow(attribute.WindowType, location.Bounds.X, location.Bounds.Y);
                    var previousBounds = window.ActualBounds;

                    if (previousBounds.Width <= 0 || previousBounds.Height <= 0)
                    {
                        previousBounds.X = _uiScreen.ActualWidth/2;
                        previousBounds.Y = _uiScreen.ActualHeight/2;
                    }

                    window.X = screenBounds.X + screenBounds.Width*location.Bounds.X;
                    window.Y = screenBounds.Y + screenBounds.Height*location.Bounds.Y;
                    window.Width = screenBounds.Width*location.Bounds.Width;
                    window.Height = screenBounds.Height*location.Bounds.Height;

                    AnimateMovementAndScaling(window, previousBounds);
                }
            }
        }


        public void AnimateMovementAndScaling(AnimatedWindow window, RectangleF oldbounds)
        {
            var animationService = _services.Get<IAnimationService>();

            var renderTransform = window.LoadingAnimation = new TimelineGroup
            {
                new Vector2FFromToByAnimation
                {
                    TargetProperty = "RenderTranslation",
                    From = new Vector2F(oldbounds.X - window.X, oldbounds.Y - window.Y),
                    To = new Vector2F(0, 0),
                    Duration = TimeSpan.FromSeconds(0.8),
                    EasingFunction = new LogarithmicEase() {Mode = EasingMode.EaseOut},
                },
                new Vector2FFromToByAnimation
                {
                    TargetProperty = "RenderScale",
                    From = new Vector2F(oldbounds.Width/window.Width, oldbounds.Height/window.Height),
                    To = new Vector2F(1, 1),
                    Duration = TimeSpan.FromSeconds(0.8),
                    EasingFunction = new LogarithmicEase {Mode = EasingMode.EaseOut},
                }
            };

            var animationController = animationService.StartAnimation(renderTransform, window);
            animationController.UpdateAndApply();
            animationController.AutoRecycle();
        }

        public AnimatedWindow CreateOrGetWindow(Type windowType)
        {
            var foundWindow = GetWindow(windowType);
            if (foundWindow == null)
            {
                var sceelixWindow = InstantiateWindow(windowType);
                sceelixWindow.Show(_windowAnimator);

                return sceelixWindow;
            }

            return foundWindow;
        }



        public AnimatedWindow GetWindow(Type type)
        {
            return _uiScreen.Children.OfType<AnimatedWindow>().FirstOrDefault(val => val.GetType() == type);
        }


        public void SaveDefaultLayout()
        {
            //since DigitalRune does not provide window drag events and because
            //window property change events will call this function several times in a row
            //we should have this delayed control system that will aggregate all the calls
            //within a time period and make the actual call in the end.
            _delayedEventRunner.Run(SaveInstantDefaultLayout, 2000);
        }
        

        private void SaveInstantDefaultLayout()
        {
            _defaultLayout = new Layout(DefaultLayoutName, _uiScreen.Children.OfType<AnimatedWindow>().Select(w => new WindowLocation(w)).ToList());
            _defaultLayout.Save();
        }



        public void SetupLayoutMenu(MenuChild windowMenuItem)
        {
            _layoutsMenuItem = new MenuChild() {Text = "Layouts"};
            windowMenuItem.MenuChildren.Add(_layoutsMenuItem);

            UpdateLayoutMenuList();

            foreach (DesignerWindowAttribute pluginWindowAttribute in _pluginManager.PluginWindows.OrderBy(x => x.WindowName))
            {
                windowMenuItem.MenuChildren.Add(new CheckMenuChild(WindowChildOnClick)
                {
                    Text = pluginWindowAttribute.WindowName,
                    UserData = pluginWindowAttribute,
                    //IsChecked = GetWindow(pluginWindowAttribute.WindowType) != null
                });
            }
        }



        private void UpdateLayoutMenuList()
        {
            _layoutsMenuItem.MenuChildren.Clear();

            bool foundUserLayout = false;

            foreach (Layout availableLayout in _availableLayouts.OrderBy(x => x.IsUserLayout).ThenBy(x => x.Name))
            {
                _layoutsMenuItem.MenuChildren.Add(new CheckMenuChild(LayoutChildClick) {Text = availableLayout.Name, UserData = availableLayout, BeginGroup = (!foundUserLayout && availableLayout.IsUserLayout)});

                foundUserLayout = availableLayout.IsUserLayout;
            }

            _layoutsMenuItem.MenuChildren.Add(new MenuChild(OnSaveLayout) {Text = "Save Layout", BeginGroup = true});
        }



        private void LayoutChildClick(MenuChild obj)
        {
            var layout = (Layout) obj.UserData;

            //load the new layout
            LoadLayout(layout, ScreenManager.GetWindowArea(_uiScreen));

            //make this layout configuration the new default
            _defaultLayout = new Layout(DefaultLayoutName, layout);
            _defaultLayout.Save();

            ClearLayoutChecksExcept(obj);
            //_defaultLayout.Locations = layout.Locations.ToList();
            //_defaultLayout.Save();
        }



        private void ClearLayoutChecksExcept(MenuChild obj = null)
        {
            foreach (CheckMenuChild menuChild in _layoutsMenuItem.MenuChildren.OfType<CheckMenuChild>())
            {
                if (menuChild != obj)
                    menuChild.IsChecked = false;
            }
        }



        private void OnSaveLayout(MenuChild obj)
        {
            InputWindow window = new InputWindow()
            {
                Title = "Save Layout",
                LabelText = "Layout Name:",
                InputText = "My New Layout"
            };
            window.Accepted += delegate
            {
                Layout layout = new Layout(window.InputText, _defaultLayout);
                layout.Save();

                RefreshAvailableLayouts();
                UpdateLayoutMenuList();
                //SetupContextMenu();
            };

            window.Show(_uiScreen);
        }



        private void WindowChildOnClick(MenuChild menuChild)
        {
            DesignerWindowAttribute windowAttribute = (DesignerWindowAttribute) menuChild.UserData;

            var foundWindow = GetWindow(windowAttribute.WindowType);
            if (foundWindow != null)
            {
                //activate this window
                //foundWindow.Activate();
                foundWindow.Close();
            }
            else
            {
                //if there's no window yet, load a new one
                var sceelixWindow = InstantiateWindow(windowAttribute.WindowType);

                var contextMenu = (UIControl) menuChild.MenuParent;
                sceelixWindow.X = contextMenu.ActualX;
                sceelixWindow.Y = contextMenu.ActualY;

                sceelixWindow.Show(_windowAnimator);
            }

            ((CheckMenuChild) menuChild).IsChecked = GetWindow(windowAttribute.WindowType) != null;
        }


        private AnimatedWindow InstantiateWindow(Type type)
        {
            var sceelixWindow = (AnimatedWindow)Activator.CreateInstance(type);
            if (sceelixWindow is IServiceable)
                ((IServiceable)sceelixWindow).Initialize(_services);
            
            sceelixWindow.LoadingAnimation = new SingleFromToByAnimation
            {
                TargetProperty = "Opacity", // Transition the property UIControl.Opacity 
                From = 0, // from 0 to its actual value
                Duration = TimeSpan.FromSeconds(0.2), // over a duration of 0.2 seconds.
            };

            sceelixWindow.ClosingAnimation = new SingleFromToByAnimation
            {
                TargetProperty = "Opacity", // Transition the property UIControl.Opacity
                To = 0, // from its current value to 0
                Duration = TimeSpan.FromSeconds(0.2), // over a duration 0.2 seconds.
            };

            //define the property here, so that we can use the uiScreen parameter
            sceelixWindow.PropertyChanged += delegate (object sender, GamePropertyEventArgs e)
            {
                if (sceelixWindow.ActualIsVisible)
                {

                    if (e.Property.Name == "X" || e.Property.Name == "Y" || e.Property.Name == "Width" || e.Property.Name == "Height")
                        SaveDefaultLayout();
                }
            };

            //define the property here, so that we can use the uiScreen parameter
            sceelixWindow.Closed += delegate
            {
                SaveDefaultLayout();

                //unregister all the messages
                _messageManager.Unregister(sceelixWindow);

                //let the top menu know that we are closed
                SetMenuItemCheck(type, false);
            };

            SetMenuItemCheck(type, true);


            return sceelixWindow;
        }


        public void Scale(RectangleF oldRectangle, RectangleF newRectangle)
        {
            foreach (AnimatedWindow sceelixWindow in _uiScreen.Children.OfType<AnimatedWindow>())
            {
                var relativeX = (sceelixWindow.X - oldRectangle.X)/oldRectangle.Width;
                var relativeY = (sceelixWindow.Y - oldRectangle.Y)/oldRectangle.Height;
                var relativeWidth = (sceelixWindow.Width/oldRectangle.Width);
                var relativeHeight = (sceelixWindow.Height/oldRectangle.Height);

                sceelixWindow.X = newRectangle.X + (relativeX*newRectangle.Width);
                sceelixWindow.Y = newRectangle.Y + (relativeY*newRectangle.Height);
                sceelixWindow.Width = relativeWidth*newRectangle.Width;
                sceelixWindow.Height = relativeHeight*newRectangle.Height;
            }
        }



        public void SetMenuItemCheck(Type windowType, bool state)
        {
            var menuChild = _layoutsMenuItem.MenuParent.MenuChildren.OfType<CheckMenuChild>().
                FirstOrDefault(x => ((DesignerWindowAttribute) x.UserData).WindowType == windowType);

            if (menuChild != null)
                menuChild.IsChecked = state;
        }



        public void StartDragMode(Window window)
        {
            _uiScreen.Children.Add(new DockHandle(DockHandleSection.Up, ScreenManager.GetWindowArea(_uiScreen), window));
            _uiScreen.Children.Add(new DockHandle(DockHandleSection.Left, ScreenManager.GetWindowArea(_uiScreen), window));
            _uiScreen.Children.Add(new DockHandle(DockHandleSection.Right, ScreenManager.GetWindowArea(_uiScreen), window));
            _uiScreen.Children.Add(new DockHandle(DockHandleSection.Down, ScreenManager.GetWindowArea(_uiScreen), window));
        }



        public void EndDragMode()
        {
            foreach (DockHandle dockHandle in _uiScreen.Children.OfType<DockHandle>().ToList())
            {
                _uiScreen.Children.Remove(dockHandle);
            }
        }
    }

    public enum DockHandleSection
    {
        Left,
        Up,
        Right,
        Down
    }

    public class DockHandle : UIControl
    {
        const int DockHandleSize = 40;
        private readonly RectangleF _bounds;

        private readonly DockHandleSection _section;
        private readonly Window _window;
        private DockHighlight _dockHighlight;



        public DockHandle(DockHandleSection section, RectangleF bounds, Window window)
        {
            _section = section;
            _bounds = bounds;
            _window = window;

            Width = Height = DockHandleSize;

            Style = "DockHandle" + section.ToString();
            switch (section)
            {
                case DockHandleSection.Left:
                    X = bounds.X;
                    Y = bounds.Y + bounds.Height/2f - DockHandleSize/2f;
                    break;
                case DockHandleSection.Up:
                    X = bounds.X + bounds.Width/2f - DockHandleSize/2f;
                    Y = bounds.Y;
                    break;
                case DockHandleSection.Right:
                    X = bounds.X + bounds.Width - DockHandleSize;
                    Y = bounds.Y + bounds.Height/2f - DockHandleSize/2f;
                    break;
                case DockHandleSection.Down:
                    X = bounds.X + bounds.Width/2f - DockHandleSize/2f;
                    Y = bounds.Y + bounds.Height - DockHandleSize;
                    break;
            }
        }



        protected override void OnHandleInput(InputContext context)
        {
            base.OnHandleInput(context);

            if (IsMouseOver)
            {
                if (_dockHighlight == null)
                {
                    switch (_section)
                    {
                        case DockHandleSection.Left:
                            _dockHighlight = new DockHighlight() {X = _bounds.X, Y = _bounds.Y, Width = Math.Min(_window.Width, _bounds.Width/2f), Height = _bounds.Height};
                            break;
                        case DockHandleSection.Up:
                            _dockHighlight = new DockHighlight() {X = _bounds.X, Y = _bounds.Y, Width = _bounds.Width, Height = Math.Min(_window.Height, _bounds.Height/2f)};
                            break;
                        case DockHandleSection.Right:
                            var width = Math.Min(_window.Width, _bounds.Width/2f);
                            _dockHighlight = new DockHighlight() {X = _bounds.X + _bounds.Width - width, Y = _bounds.Y, Width = width, Height = _bounds.Height};
                            break;
                        case DockHandleSection.Down:
                            var height = Math.Min(_window.Height, _bounds.Height/2f);
                            _dockHighlight = new DockHighlight() {X = _bounds.X, Y = _bounds.Y + _bounds.Height - height, Width = _bounds.Width, Height = height};
                            break;
                    }

                    Screen.Children.Add(_dockHighlight);
                }
                else
                {
                    if (InputService.IsReleased(MouseButtons.Left))
                    {
                        _window.X = _dockHighlight.X;
                        _window.Y = _dockHighlight.Y;
                        _window.Width = _dockHighlight.Width;
                        _window.Height = _dockHighlight.Height;

                        Screen.Children.Remove(_dockHighlight);
                        _dockHighlight = null;
                    }
                }
            }
            else
            {
                if (_dockHighlight != null)
                {
                    Screen.Children.Remove(_dockHighlight);
                    _dockHighlight = null;
                }
            }
        }
    }

    public class DockHighlight : UIControl
    {
        public DockHighlight()
        {
            Style = "DockHighlight";
        }
    }
}