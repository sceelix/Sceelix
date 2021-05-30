using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Sceelix.Designer.GUI.MenuHandling;
using Sceelix.Designer.Helpers;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Plugins;
using Sceelix.Designer.ProjectExplorer.FileHandlers;
using Sceelix.Designer.Renderer3D.Annotations;
using Sceelix.Designer.Renderer3D.GUI;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;

namespace Sceelix.Designer.Renderer3D.Services
{
    [Renderer3DService]
    public class ScreenshotManager : IServiceable
    {
        private BarMenuService _barMenuService;
        private Renderer3DControl _render3DControl;


        public void Initialize(IServiceLocator services)
        {
            _barMenuService = services.Get<BarMenuService>();
            _render3DControl = services.Get<Renderer3DControl>();

            Directory.CreateDirectory(ScreenshotsFolder);

            _barMenuService.RegisterMenuEntry("Camera/Take Screenshot", TakeScreenShot, EmbeddedResources.Load<Texture2D>("Resources/Photo_16x16.png"), true);
            _barMenuService.RegisterMenuEntry("Camera/Open Screenshots Folder", OpenScreenShotsFolder, EmbeddedResources.Load<Texture2D>("Resources/Folder_16x16.png"));
        }



        private void OpenScreenShotsFolder()
        {
            UrlHelper.OpenUrlInBrowser(ScreenshotsFolder);
        }



        private String ScreenshotsFolder
        {
            get { return Path.Combine(SceelixApplicationInfo.ConfigurationFolder, "Screenshots"); }
        }


        private void TakeScreenShot()
        {
            String suggestedFileName = Path.Combine(ScreenshotsFolder, "Renderer3D Screenshot - " + DateTime.Now.ToString("yyyy-dd-MM HH-mm-ss-ffff")) + ".png";

            _render3DControl.PrintScreen(suggestedFileName);
        }
    }
}
