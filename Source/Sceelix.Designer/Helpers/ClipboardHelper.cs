using System;
using System.Diagnostics;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.Helpers
{
    public class ClipboardHelper
    {
        /// <summary>
        /// Cross-platform operation for clipboard copy.
        /// </summary>
        /// <param name="textToCopy">Text to copy to the clipboard.</param>
        public static void Copy(string textToCopy)
        {
            if (BuildPlatform.IsMacOS)
            {
                // Copy on MAC OS X using pbcopy commandline
                try
                {
                    using (var p = new Process())
                    {
                        p.StartInfo = new ProcessStartInfo("pbcopy", "-pboard general -Prefer txt");
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.RedirectStandardOutput = false;
                        p.StartInfo.RedirectStandardInput = true;
                        p.Start();
                        p.StandardInput.Write(textToCopy);
                        p.StandardInput.Close();
                        p.WaitForExit();
                    }
                }
                catch (Exception ex)
                {
                    DesignerProgram.Log.Error(ex.Message);
                }
            }
            else
            {
                System.Windows.Forms.Clipboard.SetText(textToCopy);
            }
        }



        /// <summary>
        ///  Cross-platform operation for clipboard paste.
        /// </summary>
        /// <returns>Pasted text.</returns>
        public static string Paste()
        {
            string pasteText;

            if (BuildPlatform.IsMacOS)
            {
                try
                {
                    using (var p = new Process())
                    {

                        p.StartInfo = new ProcessStartInfo("pbpaste", "-pboard general");
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.RedirectStandardOutput = true;
                        p.Start();
                        pasteText = p.StandardOutput.ReadToEnd();
                        p.WaitForExit();
                    }
                }
                catch (Exception ex)
                {
                    DesignerProgram.Log.Error(ex.Message);
                    return null;
                }
            }
            else
            {
                pasteText = System.Windows.Forms.Clipboard.GetText();
            }

            return pasteText;
        }



        /// <summary>
        ///  Cross-platform operation to verify if there is text at the clipboard
        /// </summary>
        /// <returns>True if there is text at the clipboard, false otherwise.</returns>
        public static bool ContainsText()
        {

            if(BuildPlatform.IsMacOS)
                return !String.IsNullOrEmpty(Paste());

            return System.Windows.Forms.Clipboard.ContainsText();
        }
    }
}