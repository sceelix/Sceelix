using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Sceelix.Designer.GUI.Dialogs
{
    public class OXDialogs
    {
        public static ProcessStartInfo CreatFilePrompt(string script, String prompt, String filename, String defaultPath)
        {
            ProcessStartInfo startInfo = CreateApplescriptStartInfo(string.Format(
                "return POSIX path of (choose file name with prompt \"{0}\" default name \"{1}\"{2})",
                prompt,
                filename,
                string.IsNullOrEmpty(defaultPath)
                    ? ""
                    : string.Format("default location POSIX file \"{0}\"", defaultPath)
            ));

            return startInfo;
        }



        public static ProcessStartInfo CreateApplescriptPrompt(string script, String prompt, String defaultPath)
        {
            return CreateApplescriptStartInfo(string.Format(
                "return POSIX path of (choose file with prompt \"{0}\"{1})",
                prompt,
                string.IsNullOrEmpty(defaultPath) ? "" : string.Format("default location POSIX file \"{0}\"", defaultPath)
            ));
        }



        public static ProcessStartInfo CreateApplescriptStartInfo(string script)
        {
            return new ProcessStartInfo()
            {
                FileName = "/usr/bin/osascript",
                Arguments = string.Format("-e '{0}'", script)
            };
        }



        public static void Test()
        {
            var info = CreateApplescriptStartInfo("display notification \"Lorem ipsum dolor sit amet\" with title \"Title\"");
            Process.Start(info);

            //var info = CreateApplescriptStartInfo("osascript -e 'tell app \"System Events\" to display dialog \"You are running Sceelix for the first time. In order to build the appropriate caches, Sceelix requires a few minutes.\"'");
            var info2 = CreateApplescriptStartInfo("display dialog \"\"");
            Process.Start(info2);
        }



        public static void ShowCacheLoadingMessage()
        {
            var info = CreateApplescriptStartInfo("display dialog \"You are running Sceelix for the first time. In order to build appropriate caches, Sceelix will perform a one-time cache process that may take a few minutes. \n \nThis is a standard procedure, during which the application will appear unresponsible, but it will resume once it is ready.\" buttons {\"OK\"} default button \"OK\" with icon note with title \"First Time Loading\"");
            Process process = new Process();
            process.StartInfo = info;
            process.Start();
            process.WaitForExit();
        }
    }
}