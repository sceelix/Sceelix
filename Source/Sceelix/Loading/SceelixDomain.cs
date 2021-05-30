using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Sceelix.Annotations;
using Sceelix.Extensions;
using Sceelix.Logging;

namespace Sceelix.Loading
{
    /// <summary>
    /// The Sceelix main class that loads and controls all reflection-based activities on assemblies identified as Sceelix libraries.
    /// </summary>
    public static class SceelixDomain
    {
        private static readonly HashSet<Assembly> _sceelixAssemblies = new HashSet<Assembly>();


        /// <summary>
        /// Gets or sets the logger. The default logger writes to the console.
        /// </summary>
        /// <value> The logger to be used. </value>
        public static ILogger Logger
        {
            get;
            set;
        } = new ConsoleLogger();


        /// <summary>
        /// Gets the loaded Sceelix assemblies.
        /// </summary>
        /// <value> The Sceelix assemblies. </value>
        public static ReadOnlyCollection<Assembly> SceelixAssemblies => new ReadOnlyCollection<Assembly>(_sceelixAssemblies.ToList());



        /// <summary>
        /// Gets the types that are defined in all the loaded Sceelix assemblies.
        /// </summary>
        /// <value> The types defined in all the loaded Sceelix assemblies </value>
        public static ReadOnlyCollection<Type> Types
        {
            get { return new ReadOnlyCollection<Type>(_sceelixAssemblies.SelectMany(x => x.GetTypes()).ToList()); }
        }



        /// <summary>
        /// Determines if the given assembly is a managed one.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static bool IsManagedAssembly(string fileName)
        {
            //Obtained from: 
            //http://stackoverflow.com/questions/367761/how-to-determine-whether-a-dll-is-a-managed-assembly-or-native-prevent-loading

            uint[] dataDictionaryRva = new uint[16];
            uint[] dataDictionarySize = new uint[16];

            Stream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(fs);

            //PE Header starts @ 0x3C (60). Its a 4 byte header.
            fs.Position = 0x3C;
            var peHeader = reader.ReadUInt32();

            //Moving to PE Header start location...
            fs.Position = peHeader;

            //read the peHeaderSignature
            reader.ReadUInt32();

            //We can also show all these value, but we will be       
            //limiting to the CLI header test.
            //read the machine
            reader.ReadUInt16();

            //read the sections
            reader.ReadUInt16();

            //read the timestamp
            reader.ReadUInt32();

            //read the pSymbolTable
            reader.ReadUInt32();

            //read the noOfSymbol
            reader.ReadUInt32();

            //read the optionalHeaderSize
            reader.ReadUInt16();

            //read the characteristics
            reader.ReadUInt16();

            // Now we are at the end of the PE Header and from here, the PE Optional Headers starts... To go directly to the datadictionary, we'll increase the stream’s current position to with 96 (0x60). 96 because, 28 for Standard fields 68 for NT-specific fields From here DataDictionary starts...and its of total 128 bytes. DataDictionay has 16 directories in total, doing simple maths 128/16 = 8. So each directory is of 8 bytes. In this 8 bytes, 4 bytes is of RVA and 4 bytes of Size. btw, the 15th directory consist of CLR header! if its 0, its not a CLR file :)
            var dataDictionaryStart = Convert.ToUInt16(Convert.ToUInt16(fs.Position) + 0x60);
            fs.Position = dataDictionaryStart;
            for (int i = 0; i < 15; i++)
            {
                dataDictionaryRva[i] = reader.ReadUInt32();
                dataDictionarySize[i] = reader.ReadUInt32();
            }

            fs.Close();

            if (dataDictionaryRva[14] == 0)
                return false;

            return true;
        }



        /// <summary>
        /// Loads assemblies located at the given directory. Only assemblies imbued with a SceelixLibraryAttribute will be loaded.
        /// Writes the progress to the defined logger.
        /// </summary>
        /// <param name="directory">The directory.</param>
        public static void LoadAssembliesFrom(string directory)
        {
            //assume the current directory if null or empty is passed
            directory = string.IsNullOrWhiteSpace(directory) ? Directory.GetCurrentDirectory() : directory;

            string[] fileNames = Directory.GetFiles(directory);

            HashSet<string> excludedFileNames = new HashSet<string>();

            //read all the files with the exclude extension and add its contents as filepaths to be excluded
            foreach (string fileName in fileNames.Where(fileName => Path.GetExtension(fileName) == ".exclude"))
            foreach (string line in File.ReadAllLines(fileName).Where(x => !string.IsNullOrWhiteSpace(x)))
                excludedFileNames.Add(Path.Combine(directory, line));

            //load only the files with the dll extension and that are NOT in the list of excluded files
            foreach (string fileName in fileNames.Where(fileName => Path.GetExtension(fileName) == ".dll" && !excludedFileNames.Contains(fileName)))
                //Check if the dll is a valid C# assembly
                if (IsManagedAssembly(fileName))
                    try
                    {
                        Logger.Log("Loading '" + fileName + "' assembly.");

                        //doing the unsafeloadfrom releases us from the need to do the "unblock" files 
                        //for portable versions coming from the web
                        var loadedAssembly = Assembly.UnsafeLoadFrom(fileName);
                        
                        if (_sceelixAssemblies.Any(x => x.FullName == loadedAssembly.FullName))
                            continue;

                        if (loadedAssembly.HasCustomAttribute<SceelixLibraryAttribute>())
                            _sceelixAssemblies.Add(loadedAssembly);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(new Exception("Could not load assembly.", ex));
                    }
        }



        public static void LoadAssembliesFromCurrentDirectory()
        {
            LoadAssembliesFrom("");
        }



        /// <summary>
        /// Loads assemblies from the AppDomain.CurrentDomain. Only assemblies imbued with a SceelixLibraryAttribute will be loaded.
        /// </summary>
        public static void LoadFromCurrentDomain()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(x => x.HasCustomAttribute<SceelixLibraryAttribute>()).ToList()) _sceelixAssemblies.Add(assembly);
        }
    }
}