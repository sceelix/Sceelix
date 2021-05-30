using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DigitalRune.Game.UI.Controls;
using Sceelix.Annotations;
using Sceelix.Designer.Annotations;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Logging;
using Sceelix.Designer.Services;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Utils;
using Sceelix.Extensions;
using Sceelix.Loading;

namespace Sceelix.Designer.Plugins
{
    public class PluginManager
    {
        private readonly List<DesignerWindowAttribute> _pluginWindows = new List<DesignerWindowAttribute>();
        //public static readonly List<Assembly> LoadedAssemblies;

        private readonly IServiceLocator _services;
        private List<Object> _designerComponents = new List<Object>();
        private ServiceManager _subServices;



        public PluginManager(IServiceLocator services)
        {
            _services = services;
        }



        internal void Initialize()
        {
            DesignerProgram.Log.Debug("Loading Assemblies.");

            SceelixDomain.Logger = new DesignerLogger();

            //loads assemblies from the sceelix bin folder
            SceelixDomain.LoadAssembliesFrom(SceelixApplicationInfo.SceelixExeFolder);

            //loads assemblies from the plugins folder
            SceelixDomain.LoadAssembliesFrom(SceelixApplicationInfo.PluginsFolder);

            _subServices = LoadPlugins(_services);

            LoadPluginWindows();

            //InitializeLayouts(subServices, uiScreen);
        }
        



        private ServiceManager LoadPlugins(IServiceLocator services)
        {
            DesignerProgram.Log.Debug("Loading Custom Attributes.");

            services.Get<SettingsManager>().Initialize();

            DesignerProgram.Log.Debug("Initializing Plugins...");

            ServiceManager subServices = new ServiceManager(services);
            //now that they have been loaded, initialize them
            //if this would have been done within the previous process, problems could occur
            _designerComponents = AttributeReader.OfAttribute<DesignerServiceAttribute>().GetInstancesOfType<Object>();
            foreach (IServiceable designerService in _designerComponents)
            {
                subServices.Register(designerService.GetType(), designerService);
            }

            foreach (IServiceable designerComponent in _designerComponents.OfType<IServiceable>())
            {
                try
                {
                    DesignerProgram.Log.Debug("Initializing '" + designerComponent.GetType().Name + "' component.");
                    designerComponent.Initialize(subServices);
                }
                catch (TargetInvocationException ex)
                {
                    Exception innerException = ex.GetRealException();

                    DesignerProgram.Log.Error("Error while initializing plugin.", ex);

                    throw innerException;
                }
                catch (Exception ex)
                {
                    DesignerProgram.Log.Error("Error while loading plugin attribute.", ex);
                }
            }

            return subServices;
        }



        private void LoadPluginWindows()
        {
            foreach (Type type in SceelixDomain.Types.Where(x => !x.IsAbstract && typeof(AnimatedWindow).IsAssignableFrom(x)))
            {
                var pluginWindow = type.GetCustomAttribute<DesignerWindowAttribute>();
                if (pluginWindow != null)
                {
                    pluginWindow.WindowType = type;
                    _pluginWindows.Add(pluginWindow);
                }
            }
        }


        public void Update(TimeSpan deltaTime)
        {
            _designerComponents.OfType<IUpdateableElement>().ForEach(x => x.Update(deltaTime));
        }

        

        public void OnClose()
        {
            foreach (IDisposable designerComponent in _designerComponents.OfType<IDisposable>())
            {
                designerComponent.Dispose();
            }
        }



        public List<DesignerWindowAttribute> PluginWindows
        {
            get { return _pluginWindows; }
        }



        public ServiceManager SubServices
        {
            get { return _subServices; }
        }
    }
}