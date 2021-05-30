using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Sceelix.Designer.Settings;

namespace Sceelix.Designer.ProjectExplorer.Management
{
    public class ProjectHistorySettings : ApplicationSettings
    {
        public const int MaxProjectLocations = 10;
        public readonly ApplicationField<String> LastLoadedVersion = new ApplicationField<String>(String.Empty);
        public readonly ApplicationField<List<String>> ProjectsLocations = new ApplicationField<List<String>>(new List<String>());



        public ProjectHistorySettings()
            : base("ProjectHistory")
        {
        }



        public IEnumerable<String> ProjectNames
        {
            get { return ProjectsLocations.Value; }
        }



        /// <summary>
        /// Checks (and updates) the version of the last Sceelix version loaded. 
        /// </summary>
        public bool IsNewVersion
        {
            get
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                var currentVersion = new Version(fileVersionInfo.ProductVersion);

                if (String.IsNullOrWhiteSpace(LastLoadedVersion.Value)
                    || new Version(LastLoadedVersion.Value) < currentVersion)
                {
                    LastLoadedVersion.Value = currentVersion.ToString();
                    return true;
                }

                return false;
            }
        }



        public String GetLastProject()
        {
            if (ProjectsLocations.Value.Count > 0)
                return ProjectsLocations.Value[0];

            return String.Empty;
        }



        public void ReorganizeProjects(String lastProjectOpened)
        {
            var locations = new List<string>(ProjectsLocations.Value);

            //remove from middle of the list, if it is there
            locations.Remove(lastProjectOpened);

            //and put it in the beginning (meaning that it is the last loaded project)
            locations.Insert(0, lastProjectOpened);

            //restrict the number of projects to "MaxProjectFiles"
            if (locations.Count > MaxProjectLocations)
                locations.RemoveRange(MaxProjectLocations, locations.Count - MaxProjectLocations);

            ProjectsLocations.Value = locations;
        }
    }
}