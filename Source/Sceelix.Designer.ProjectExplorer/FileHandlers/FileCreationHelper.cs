using System;
using System.IO;
using System.Text.RegularExpressions;
using Sceelix.Designer.ProjectExplorer.Management;

namespace Sceelix.Designer.ProjectExplorer.FileHandlers
{
    public static class FileCreationHelper
    {
        public static readonly String InvalidFileNameChars;
        public static readonly Regex InvalidFileNameCharsRegex;



        static FileCreationHelper()
        {
            InvalidFileNameChars = new string(Path.GetInvalidFileNameChars());
            InvalidFileNameCharsRegex = new Regex("[" + Regex.Escape(InvalidFileNameChars) + "]");
        }



        public static string PerformChecks(string text, IFileCreator creator, FolderItem folderItem)
        {
            if (String.IsNullOrWhiteSpace(text))
                return "The file name cannot be empty. Please introduce a valid name.";
            if (InvalidFileNameCharsRegex.IsMatch(text))
                return "A file cannot contain any of the following characters:" + InvalidFileNameChars;
            if (folderItem.ContainsFileWithName(text + creator.Extension))
                return "A file with that name already exists. Please choose another.";

            return String.Empty;
        }
    }
}