using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sceelix.Core.Utils
{
    public class FileExtensionInfo
    {
        public FileExtensionInfo(string unparsedString)
        {
            try
            {
                string extensionsPart;

                if (unparsedString.Contains("|"))
                {
                    var splitParts = unparsedString.Split('|');
                    Title = splitParts[0].Trim();
                    extensionsPart = splitParts[1];
                }
                else
                {
                    extensionsPart = unparsedString;
                }

                //for the second part, if it has several commas, split them
                if (extensionsPart.Contains(","))
                    Extensions = extensionsPart.Split(',');
                else
                    Extensions = new[] {extensionsPart};

                //clean up the extensions
                for (int i = 0; i < Extensions.Length; i++)
                {
                    //remove the *, the . and surrounding spaces
                    var extension = Extensions[i].Trim();

                    if (extension.StartsWith("*."))
                        Extensions[i] = extension;
                    else if (extension.StartsWith("."))
                        Extensions[i] = "*" + extension;
                    else
                        Extensions[i] = "*." + extension;
                }

                if (string.IsNullOrEmpty(Title)) Title = Extensions.First().ToUpper().Replace("*.", "") + " File";
            }
            catch (Exception)
            {
                throw new FormatException("The given '" + unparsedString + "' string was not in the correct format.");
            }
        }



        public string[] Extensions
        {
            get;
        }


        public string Title
        {
            get;
        }



        public bool FitsExtension(string fileName)
        {
            return Extensions.Any(x => FitsMask(fileName, x));
        }



        private bool FitsMask(string fileName, string fileMask)
        {
            string pattern =
                '^' +
                Regex.Escape(fileMask.Replace(".", "__DOT__")
                        .Replace("*", "__STAR__")
                        .Replace("?", "__QM__"))
                    .Replace("__DOT__", "[.]")
                    .Replace("__STAR__", ".*")
                    .Replace("__QM__", ".")
                + '$';
            return new Regex(pattern, RegexOptions.IgnoreCase).IsMatch(fileName);
        }



        /*public static IEnumerable<FileExtensionInfo> Parse(string[] infoExtensionFilter)
        {
            
        }*/
    }
}