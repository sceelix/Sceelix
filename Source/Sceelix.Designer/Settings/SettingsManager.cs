using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Game.UI.Controls;
using Sceelix.Annotations;
using Sceelix.Designer.Annotations;
using Sceelix.Designer.Managers;
using Sceelix.Designer.Services;

namespace Sceelix.Designer.Settings
{
    public class SettingsManager
    {
        private readonly WindowAnimator _windowAnimator;
        private readonly List<SettingsRegistry> _settingsRegistries = new List<SettingsRegistry>();



        public SettingsManager(WindowAnimator windowAnimator)
        {
            _windowAnimator = windowAnimator;
        }



        internal void Initialize()
        {
            var settings = AttributeReader.OfStringKeyAttribute<ApplicationSettingsAttribute>().GetInstancesOfType<ApplicationSettings>();
            foreach (KeyValuePair<string, ApplicationSettings> keyValuePair in settings)
            {
                Register(keyValuePair.Key, keyValuePair.Value);
                //_settingsRegistries.Add(new SettingsRegistry(keyValuePair.Key, keyValuePair.Value));
            }
        }


        internal void Register(String name, ApplicationSettings settings)
        {
            if (_settingsRegistries.All(x => x.RegisteredName != name))
                _settingsRegistries.Add(new SettingsRegistry(name, settings));
            else
            {
                throw new Exception("A settings definition with that name already exists.");
            }
        }


        public void OpenSettingsWindow(string sectionToFocus)
        {
            SettingsWindow settingsWindow = new SettingsWindow(this, sectionToFocus);
            settingsWindow.Show(_windowAnimator);
        }





        internal IEnumerable<SettingsRegistry> SettingsRegistries
        {
            get { return _settingsRegistries; }
        }



        public ApplicationSettings Get(Type type)
        {
            return _settingsRegistries.First(val => val.Settings.GetType() == type).Settings;
        }



        public T Get<T>() where T : ApplicationSettings
        {
            return (T)Get(typeof(T));
        }
    }
}