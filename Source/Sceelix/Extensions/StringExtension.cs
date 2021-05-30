using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sceelix.Extensions
{
    public static class StringExtension
    {
        public static string FindNonConflict(string formatSting, List<string> otherStrings, int startingIndex = 1)
        {
            int index = startingIndex;

            while (otherStrings.Contains(string.Format(formatSting, index)))
                index++;

            return string.Format(formatSting, index);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="validationFunction">Function that must be evaluated to false for the string to be accepted.</param>
        /// <param name="formatFunction"></param>
        /// <param name="startingIndex"></param>
        /// <returns></returns>
        public static string FindNonConflict(Func<string, bool> validationFunction, Func<int, string> formatFunction, int startingIndex = 1)
        {
            int index = startingIndex;
            string chosenName;
            while (validationFunction(chosenName = formatFunction(index)))
                index++;

            return chosenName;
        }



        public static string FirstLetterToLower(this string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToLower(str[0]) + str.Substring(1);

            return str.ToLower();
        }



        public static string FirstLetterToUpper(this string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }



        public static int LevenshteinDistanceTo(this string a, string b)
        {
            if (string.IsNullOrEmpty(a))
            {
                if (!string.IsNullOrEmpty(b)) return b.Length;
                return 0;
            }

            if (string.IsNullOrEmpty(b))
            {
                if (!string.IsNullOrEmpty(a)) return a.Length;
                return 0;
            }

            int cost;
            int[,] d = new int[a.Length + 1, b.Length + 1];
            int min1;
            int min2;
            int min3;

            for (int i = 0; i <= d.GetUpperBound(0); i += 1) d[i, 0] = i;

            for (int i = 0; i <= d.GetUpperBound(1); i += 1) d[0, i] = i;

            for (int i = 1; i <= d.GetUpperBound(0); i += 1)
            for (int j = 1; j <= d.GetUpperBound(1); j += 1)
            {
                cost = a[i - 1] != b[j - 1] ? 1 : 0;

                min1 = d[i - 1, j] + 1;
                min2 = d[i, j - 1] + 1;
                min3 = d[i - 1, j - 1] + cost;
                d[i, j] = Math.Min(Math.Min(min1, min2), min3);
            }

            return d[d.GetUpperBound(0), d.GetUpperBound(1)];
        }



        public static string Quote(this string str)
        {
            return "\"" + str + "\"";
        }



        public static string Reverse(this string str)
        {
            char[] array = str.ToCharArray();
            Array.Reverse(array);
            return new string(array);
        }



        public static string[] SplitInTwo(this string str, char separator)
        {
            return str.Split(new[] {separator}, 2);
        }



        /// <summary>
        /// Splits the string into substrings of the given size.
        ///  </summary>
        /// <param name="str">String to be split.</param>
        /// <param name="chunkSize">Size of the chunk. For this function, it must be multiple of the str length.</param>
        /// <returns></returns>
        public static IEnumerable<string> SplitSize(this string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }



        public static bool StartsWithLowerCase(this string str)
        {
            return !string.IsNullOrWhiteSpace(str) && char.IsLower(str[0]);
        }



        public static T ToEnum<T>(this string value, string spaceReplacer = "")
        {
            return (T) Enum.Parse(typeof(T), value.Replace(" ", spaceReplacer));
        }



        public static string ToSplitCamelCase(this string str)
        {
            return Regex.Replace(
                Regex.Replace(
                    str,
                    @"(\P{Ll})(\P{Ll}\p{Ll})",
                    "$1 $2"
                ),
                @"(\p{Ll})(\P{Ll})",
                "$1 $2"
            );
        }



        public static Stream ToStream(this string str)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}