using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Sceelix.Designer.Utils
{
    public class SceelixApplicationInfo
    {
        private static readonly bool _isPortable;

        ///private static readonly string _userApplicationFolder;
        //private static readonly string _userDocumentsFolder;
        private static readonly string _configurationFolder;

        private static readonly string _sceelixExeFolder;
        private static readonly string _sceelixMainFolder;
        private static readonly string _settingsFolder;
        private static readonly string _layoutsFolder;
        private static readonly string _logsFolder;
        private static readonly string _extrasFolder;
        private static readonly string _contentFolder;
        private static readonly string _documentsFolder;
        private static readonly string _pluginsFolder;



        static SceelixApplicationInfo()
        {
            //first, load the definition that indicates if the application is portable or not
            _isPortable = Convert.ToBoolean(ConfigurationManager.AppSettings["IsPortable"]);

            //depending on what has been read, the following will be changed
            var userApplicationFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sceelix");
            //var userDocumentsFolder = Path.Combine(, "Sceelix");

            _sceelixExeFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            _sceelixMainFolder = Path.GetDirectoryName(SceelixExeFolder) ?? Directory.GetCurrentDirectory();

            _configurationFolder = _isPortable ? _sceelixMainFolder : userApplicationFolder;
            _settingsFolder = Path.Combine(_configurationFolder, "Settings");
            _logsFolder = Path.Combine(_configurationFolder, "Logs");
            _layoutsFolder = Path.Combine(_settingsFolder, "Layouts");

            _documentsFolder = _isPortable ? _sceelixMainFolder : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Sceelix");
            _extrasFolder = _isPortable ? Path.Combine(_sceelixMainFolder, "Extras") : Path.Combine(_documentsFolder, "Extras", CurrentVersion.ToString());
            _pluginsFolder = _isPortable ? Path.Combine(_sceelixMainFolder, "Plugins") : Path.Combine(_documentsFolder, "Plugins");

#if MACOS
            _contentFolder = Path.Combine(_sceelixMainFolder, "Resources", "Content");
            #else
            _contentFolder = Path.Combine(SceelixExeFolder, "Content");
#endif

            //create the directories. If an exception occurs, then the application would not be able to run properly
            //anyway, to it is better to crash at this point
            Directory.CreateDirectory(userApplicationFolder);
            Directory.CreateDirectory(_configurationFolder);
            Directory.CreateDirectory(_settingsFolder);
            Directory.CreateDirectory(_logsFolder);
            Directory.CreateDirectory(_layoutsFolder);
            Directory.CreateDirectory(_extrasFolder);
            Directory.CreateDirectory(_pluginsFolder);
        }



        /// <summary>
        /// Indicates if we are running the portable version or not.
        /// </summary>
        public static bool IsPortable
        {
            get { return _isPortable; }
        }



        /// <summary>
        /// Folder where the configurations are stored (typically in the Application Data folder)
        /// </summary>
        public static string ConfigurationFolder
        {
            get { return _configurationFolder; }
        }



        /// <summary>
        /// Folder where the Sceelix.Designer.exe is located. (typically Bin folder)
        /// </summary>
        public static string SceelixExeFolder
        {
            get { return _sceelixExeFolder; }
        }



        /// <summary>
        /// Folder where the Bin folder and the Sceelix.exe shortcut is located.
        /// </summary>
        public static string SceelixMainFolder
        {
            get { return _sceelixMainFolder; }
        }



        /// <summary>
        /// Folder where the settings are stored (typically in the Config folder)
        /// </summary>
        public static string SettingsFolder
        {
            get { return _settingsFolder; }
        }



        /// <summary>
        /// Folder where the layouts are stored (typically in the Config folder)
        /// </summary>
        public static string LayoutsFolder
        {
            get { return _layoutsFolder; }
        }



        /// <summary>
        /// Folder where the logs are stored (typically in the Config folder)
        /// </summary>
        public static string LogsFolder
        {
            get { return _logsFolder; }
        }



        /// <summary>
        /// Folder where the extras (api) are stored (typically in the Documents/Sceelix folder)
        /// </summary>
        public static string ExtrasFolder
        {
            get { return _extrasFolder; }
        }



        /// <summary>
        /// Folder where the assembly content is stored (typically inside the Bin folder)
        /// </summary>
        public static string ContentFolder
        {
            get { return _contentFolder; }
        }



        /// <summary>
        /// Folder inside the documents folder.
        /// </summary>
        public static string DocumentsFolder
        {
            get { return _documentsFolder; }
        }



        /// <summary>
        /// Folder where the plugins should be deployed.
        /// </summary>
        public static string PluginsFolder
        {
            get { return _pluginsFolder; }
        }



        /// <summary>
        /// Currently running version of Sceelix.
        /// </summary>
        public static Version CurrentVersion
        {
            get
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                return new Version(fileVersionInfo.ProductVersion);
            }
        }
    }
}