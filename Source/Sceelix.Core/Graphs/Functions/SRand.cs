using System;
using System.Collections.Generic;
using Sceelix.Core.Environments;

namespace Sceelix.Core.Graphs.Functions
{
    public class SRand
    {
        private static readonly Dictionary<string, string[]> _fileFolderCache = new Dictionary<string, string[]>();
        private static IProcedureEnvironment _loadEnvironment;



        public static double Double(object seed)
        {
            return Noise(Hash(seed)) * int.MaxValue;
        }



        public static double Double(object seed, double max)
        {
            return Noise(Hash(seed)) * max;
        }



        public static double Double(object seed, double min, double max)
        {
            return min + Noise(Hash(seed)) * (max - min);
        }



        public static float Float(object seed)
        {
            return Convert.ToSingle(Noise(Hash(seed)) * int.MaxValue);
        }



        public static float Float(object seed, float max)
        {
            return Convert.ToSingle(Noise(Hash(seed)) * max);
        }



        public static float Float(object seed, float min, float max)
        {
            return Convert.ToSingle(min + Noise(Hash(seed)) * (max - min));
        }



        private static int Hash(object seed)
        {
            string text = seed.ToString();

            //string text = null;

            //if(seed != null)
            //    text = seed.ToString();

            //if (String.IsNullOrEmpty(text))
            //    text = DateTime.Now.ToString(CultureInfo.InvariantCulture);

            unchecked
            {
                int hash = 23;
                foreach (char c in text) hash = hash * 36969 + c;

                return Math.Abs(hash);
            }
        }



        public static int Int(object seed)
        {
            return (int) (Noise(Hash(seed)) * int.MaxValue);
        }



        public static int Int(object seed, int max)
        {
            return (int) Noise(Hash(seed)) * max;
        }



        public static int Int(object seed, int min, int max)
        {
            return (int) (min + Noise(Hash(seed)) * (max - min));
        }



        private static double Noise(int x)
        {
            x = (x << 13) ^ x;

            return ((x * (x * x * 15731 + 789221) + 1376312589) & 0x7fffffff) / 2147483648.0;
        }



        public static void SetEnvironment(IProcedureEnvironment loadEnvironment)
        {
            _loadEnvironment = loadEnvironment;
        }



        public static object SRandD(object seed, dynamic min, dynamic max)
        {
            return Double(seed, min, max);
        }



        public static object SRandF(object seed, dynamic min, dynamic max)
        {
            return Float(seed, min, max);
        }



        /*public static String File(object seed, String folder)
        {
            string pathToFolder = Path.Combine(_loadEnvironment.ProjectFolder, folder);

            String[] fileNames;
            if (!_fileFolderCache.TryGetValue(pathToFolder, out fileNames))
            {
                fileNames = _loadEnvironment.GetFilePaths(pathToFolder);
                _fileFolderCache.Add(pathToFolder, fileNames);
            }

            String selectedItem = Path.GetFileName(fileNames[Int(seed, 0, fileNames.Length)]);

            return Path.Combine(folder, selectedItem);
        }*/



        public static object SRandI(object seed, dynamic min, dynamic max)
        {
            return Int(seed, min, max);
        }
    }
}