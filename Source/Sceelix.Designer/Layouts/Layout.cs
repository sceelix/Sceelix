using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Utils;
using Sceelix.Serialization;

namespace Sceelix.Designer.Layouts
{
    public class Layout
    {
        //public const String LayoutLocation = "Settings/Layouts/";
        public const String LayoutExtension = ".slxly";

        //public readonly ApplicationField<Rectangle> WindowBounds = new ApplicationField<Rectangle>(new Rectangle(0,0,1024,768));
        public List<WindowLocation> Locations = new List<WindowLocation>();



        public Layout()
        {
        }



        public Layout(string name, Layout baseLayout)
        {
            Name = name;
            Locations = baseLayout.Locations.Select(x => (WindowLocation) x.Clone()).ToList();
        }



        public Layout(string name, List<WindowLocation> windowLocations)
        {
            Name = name;
            Locations = windowLocations;
        }



        public string Name
        {
            get;
            set;
        }



        public bool IsUserLayout
        {
            get;
            set;
        }



        public static Layout FromFile(String name)
        {
            var filePath = Path.Combine(SceelixApplicationInfo.LayoutsFolder, Path.ChangeExtension(name, LayoutExtension));

            if (!File.Exists(filePath))
                return null;

            var layout = JsonSerialization.LoadFromFile<Layout>(filePath);
            if (layout == null)
                return null;

            layout.Name = name;

            return layout;
        }



        public void Save()
        {
            JsonSerialization.SaveToFile(Path.Combine(SceelixApplicationInfo.LayoutsFolder, Path.ChangeExtension(Name, LayoutExtension)), this);
        }



        public static Layout FromStream(String name, Stream stream)
        {
            var layout = JsonSerialization.LoadFromStream<Layout>(stream);
            layout.Name = name;
            return layout;
        }



        /*public void CopyTo(Layout layout)
        {
            layout.Locations = this.Locations.Select(x => (WindowLocation)x.Clone()).ToList();
            layout.Save();
        }*/
    }
}