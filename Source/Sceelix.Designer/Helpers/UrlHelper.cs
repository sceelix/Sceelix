using System;
using System.Diagnostics;
using DigitalRune.Text;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.Helpers
{
    public class UrlHelper
    {
        public static void OpenUrlInBrowser(string url)
        {
            if (BuildPlatform.IsWindows)
            {
                Process.Start(url);
            }
            else
            {
                String command = BuildPlatform.IsMacOS ? "open" : "xdg-open";

                Process.Start(new ProcessStartInfo
                {
                    FileName = command,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Arguments = "\"" + url + "\""
                    
                });
            }   
        }



        public static void OpenFolderInExplorer(String path)
        {
            //turns out the process is the same...until something changes again
            OpenUrlInBrowser(path);
        }
    }
}
