using System;
using Sceelix.Designer.GUI;

namespace Sceelix.Designer.Login
{
    internal interface ILoginManager
    {
        LoadingWindow LoadingWindow
        {
            get;
        }

        event EventHandler<EventArgs> Finished;


        void Initialize();
    }
}