using System;
using Microsoft.Xna.Framework;
using System.Windows.Forms;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.Plugins;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.Managers
{
    public class ExitManager
    {
        private readonly PluginManager _pluginManager;
        private readonly MessageManager _messageManager;
        private readonly Game _game;



        public ExitManager(Game game, PluginManager pluginManager, MessageManager messageManager)
        {
            _game = game;

            game.Exiting += OnExit;

            _pluginManager = pluginManager;
            _messageManager = messageManager;
#if WINDOWS
            var form = (Form) Control.FromHandle(game.Window.Handle);
            form.Closed += delegate(object sender, EventArgs args)
            {
                OnExit(this,EventArgs.Empty);
                Environment.Exit(0);
            };
#endif
        }


        private void OnExit(object sender, EventArgs e)
        {
            _pluginManager.OnClose();

            //notify all interested parties, so that they may dispose of things of notify any external services
            //In MacOS this is performed somewhere else, so we should call it twice
            if(!BuildPlatform.IsMacOS)
                _messageManager.Publish(new ProcessExiting());

            //In Linux, Sceelix won't exit properly.
            //It seems to be linked to the InputManager class (inside the ExtendedInputManager)
            //which can't be disposed, but keeps a hold on the application for some reason
            if (BuildPlatform.IsLinux)
            {
#if LINUX
                DesignerProgram.Log.Debug("Disposing OpenTK Window.");
                //var window = Sceelix.Designer.Helpers.WindowHelper.GetForm(_game.Window);
                Environment.Exit(0);
#endif
            }
        }



        public void Exit()
        {
            #if WINDOWS
                OnExit(this,EventArgs.Empty);
                Environment.Exit(0);
            #else
                _game.Exit();
            #endif
        }
    }
}
