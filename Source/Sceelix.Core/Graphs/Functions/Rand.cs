using System;
using System.Collections.Generic;
using Sceelix.Core.Environments;

namespace Sceelix.Core.Graphs.Functions
{
    /// <summary>
    /// Problem with this approach: It is not extensible as the randomprocedure is...
    /// </summary>
    public class Rand
    {
        private static Random _randomGenerator;
        private static Dictionary<string, string[]> _fileFolderCache = new Dictionary<string, string[]>();

        private static IProcedureEnvironment _loadEnvironment;
        //private static int _seed;



        static Rand()
        {
            //_seed = Guid.NewGuid().GetHashCode();
            //_randomGenerator = new System.Random(1000);
            _randomGenerator = new Random();
        }



        public static double Double()
        {
            return Double(0, Double(double.MaxValue));
        }



        public static double Double(double max)
        {
            return Double(0, max);
        }



        public static double Double(double min, double max)
        {
            return min + _randomGenerator.NextDouble() * (max - min);
        }



        /*public float Float()
        {
            return Convert.ToSingle(_randomGenerator.NextDouble());
        }*/



        public static float Float()
        {
            return Float(0, float.MaxValue);
        }



        public static float Float(float max)
        {
            return Float(0, max);
        }



        public static float Float(float min, float max)
        {
            return Convert.ToSingle(min + _randomGenerator.NextDouble() * (max - min));
        }



        public static int Int()
        {
            return Int(0, int.MaxValue);
        }



        public static int Int(int max)
        {
            return Int(0, max);
        }



        public static int Int(int min, int max)
        {
            return _randomGenerator.Next(min, max);
        }



        public static object RandD(dynamic min, dynamic max)
        {
            return Double(min, max);
        }



        public static object RandF(dynamic min, dynamic max)
        {
            return Float(min, max);
        }



        /*public static String File(String folder)
        {
            string pathToFolder = Path.Combine(_loadEnvironment.ProjectFolder, folder);

            String[] fileNames;
            if (!_fileFolderCache.TryGetValue(pathToFolder, out fileNames))
            {
                fileNames = _loadEnvironment.GetFilePaths(pathToFolder);
                _fileFolderCache.Add(pathToFolder, fileNames);
            }

            String selectedItem = Path.GetFileName(fileNames[Int(0, fileNames.Length)]);

            return Path.Combine(folder, selectedItem);
        }*/



        public static object RandI(dynamic min, dynamic max)
        {
            return Int(min, max);
        }



        public static void Reset(IProcedureEnvironment loadEnvironment, int seed)
        {
            _loadEnvironment = loadEnvironment;
            _randomGenerator = new Random(seed);
            _fileFolderCache = new Dictionary<string, string[]>();
        }
    }
}