using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Sceelix.Designer.Utils
{
    public static class FileDialogExtender
    {
        public static DialogResult ShowCrossDialog(this CommonDialog commonDialog)
        {
#if LINUX

            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            DialogResult result = DialogResult.None;

            //start a new thread to handle the result
            var thr = new Thread(() =>
            {
                //OpenFileDialog openFileDialog = new OpenFileDialog();
                result = commonDialog.ShowDialog();

                //run this so the Dialog is correctly closed
                //Application.Run();
                Application.DoEvents();

                manualResetEvent.Set();
            });
            thr.SetApartmentState(ApartmentState.STA);
            thr.IsBackground = true;
            thr.Name = "FileDialogExtender";
            thr.Start();

            //we have to block everything until we are done
            manualResetEvent.WaitOne();

            return result;

            #else

            return commonDialog.ShowDialog();

#endif
        }
    }
}