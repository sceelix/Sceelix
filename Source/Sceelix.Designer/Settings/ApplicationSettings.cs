using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Sceelix.Designer.Utils;
using Sceelix.Serialization;

namespace Sceelix.Designer.Settings
{
    public abstract class ApplicationSettings : IEnumerable<ApplicationField>
    {
        private readonly List<ApplicationField> _applicationFields = new List<ApplicationField>();
        private readonly string _fileName;

        public event ApplicationFieldChanged Changed = delegate { };

        public event ApplicationFieldChanging Changing = delegate { return true; };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName">Name or path of the file. If left as relative path, it will be placed within the Settings folder. If left without extension, it will be added the default settings extension.</param>
        protected ApplicationSettings(String fileName)
        {
            if (!Path.IsPathRooted(fileName))
                fileName = Path.Combine(SceelixApplicationInfo.SettingsFolder, fileName);

            if (!Path.HasExtension(fileName))
                fileName = Path.ChangeExtension(fileName, ".slxset");

            _fileName = fileName;

            //read existing fields via reflection
            ReadFields();

            //load the data from file, if exists
            LoadData();
        }



        public string FileName
        {
            get { return _fileName; }
        }



        public IEnumerator<ApplicationField> GetEnumerator()
        {
            return _applicationFields.GetEnumerator();
        }



        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }



        private void ReadFields()
        {
            //tries to load the file, otherwise
            Type currentType = GetType();

            while (currentType != null && currentType != typeof(ApplicationSettings))
            {
                FieldInfo[] infos = currentType.GetFields(BindingFlags.Instance | BindingFlags.Public);

                foreach (FieldInfo fieldInfo in infos)
                {
                    if (typeof(ApplicationField).IsAssignableFrom(fieldInfo.FieldType))
                    {
                        ApplicationField applicationField = (ApplicationField) fieldInfo.GetValue(this);

                        applicationField.ApplicationSettings = this;
                        applicationField.Label = fieldInfo.Name;

                        //we would think we could just link the events, but it seems that they
                        //are not called, so we need the extra call
                        applicationField.Changed += CallSettingsChanged;
                        applicationField.Changing += CallSettingsChanging;

                        _applicationFields.Add(applicationField);
                    }
                }

                currentType = currentType.BaseType;
            }
        }

        private void CallSettingsChanged(ApplicationField field, object oldValue, object newValue)
        {
            Changed(field, oldValue, newValue);
        }

        private bool CallSettingsChanging(ApplicationField field, object oldValue, object newValue)
        {
            return Changing(field, oldValue, newValue);
        }


        internal void LoadData()
        {
            if (File.Exists(_fileName))
            {
                Dictionary<String, Object> data = JsonSerialization.LoadFromFile<Dictionary<String, Object>>(_fileName);

                //if the file load was successful
                if (data != null)
                {
                    //go over SettingsFields and load
                    foreach (ApplicationField applicationField in _applicationFields)
                    {
                        Object value;

                        //if the value is defined, load it, otherwise use the default value
                        if (data.TryGetValue(applicationField.Label, out value))
                            applicationField.Set(data[applicationField.Label]);
                    }
                }
            }
        }



        internal void SaveData()
        {
            Dictionary<String, Object> data = new Dictionary<String, Object>();

            foreach (ApplicationField applicationField in _applicationFields)
                data.Add(applicationField.Label, applicationField.Get());

            JsonSerialization.SaveToFile(_fileName, data);

            //save to file here
        }
    }
}