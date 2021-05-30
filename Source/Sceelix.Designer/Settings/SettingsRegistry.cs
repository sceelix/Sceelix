using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Loading;

namespace Sceelix.Designer.Settings
{
    internal class SettingsRegistry
    {
        private readonly Dictionary<string, string> _fieldComments = new Dictionary<string, string>();
        private string _registeredName;
        private ApplicationSettings _settings;



        public SettingsRegistry(string registeredName, ApplicationSettings settings)
        {
            RegisteredName = registeredName;
            Settings = settings;

            foreach (IVisualApplicationField field in settings.OfType<IVisualApplicationField>())
            {
                var fieldInfo = settings.GetType().GetField(field.Label);
                if (fieldInfo != null)
                    _fieldComments[field.Label] = CommentLoader.GetComment(fieldInfo).Summary;
            }
        }



        internal String RegisteredName
        {
            get { return _registeredName; }
            set { _registeredName = value; }
        }



        internal ApplicationSettings Settings
        {
            get { return _settings; }
            set { _settings = value; }
        }



        internal String GetComment(String label)
        {
            String comment;
            if (_fieldComments.TryGetValue(label, out comment))
                return comment;

            return null;
        }
    }
}