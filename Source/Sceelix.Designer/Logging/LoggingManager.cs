using System;
using System.IO;
using System.Linq;
using Sceelix.Designer.Settings;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.Logging
{
    internal class LoggingManager
    {
        public LoggingManager(DesignerSettings designerSettings)
        {
            CleanLogFiles(designerSettings);
        }


        private void CleanLogFiles(DesignerSettings designerSettings)
        {
            DesignerProgram.Log.Debug("Deleting Log Files.");

            if (designerSettings.MaxLogFiles.Value > 0)
            {
                //skip the last 'MaxLogFiles' files and delete the rest
                foreach (string fileName in Directory.GetFiles(SceelixApplicationInfo.LogsFolder).OrderByDescending(x => x).Skip(designerSettings.MaxLogFiles.Value).ToList())
                {
                    try
                    {
                        File.Delete(fileName);
                    }
                    catch (Exception)
                    {
                        // ignored
                        // we don't want this to cause errors if we can't delete the log file
                        // it could be in use, so we can delete the next time
                    }
                }
            }
        }
    }
}
