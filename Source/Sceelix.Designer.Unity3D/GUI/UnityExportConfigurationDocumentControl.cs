using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Ionic.Zip;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Sceelix.Annotations;
using Sceelix.Core.Environments;
using Sceelix.Core.Graphs;
using Sceelix.Designer.Graphs.Environments;
using Sceelix.Designer.GUI.Controls;
using Sceelix.Designer.GUI.MenuControls;
using Sceelix.Designer.GUI.TreeViewControls;
using Sceelix.Designer.GUI.Windows;
using Sceelix.Designer.Interfaces;
using Sceelix.Designer.Messaging;
using Sceelix.Designer.ProjectExplorer.GUI;
using Sceelix.Designer.ProjectExplorer.Management;
using Sceelix.Designer.Services;
using Sceelix.Designer.Utils;
using Sceelix.Extensions;
using HorizontalAlignment = DigitalRune.Game.UI.HorizontalAlignment;
using Orientation = DigitalRune.Game.UI.Orientation;
using TextBox = DigitalRune.Game.UI.Controls.TextBox;


namespace Sceelix.Designer.Unity3D.GUI
{
    public class UnityExportConfigurationDocumentControl : DocumentControl, IServiceable
    {
        private UnityExportConfiguration _configuration;
        private IServiceLocator _services;
        private MessageManager _messageManager;


        public void Initialize(IServiceLocator services)
        {
            _services = services;
            _messageManager = services.Get<MessageManager>();
        }

        protected override void OnFirstLoad()
        {
            var verticalStackPanel = new FlexibleStackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };


            BarMenu barMenuContent = new BarMenu();

            var graphMenuItemControl = new MenuChild(Export) { Text = "Export" };
            barMenuContent.MenuChildren.Add(graphMenuItemControl);

            verticalStackPanel.Children.Add(barMenuContent);

            LayoutControl layoutControl = new LayoutControl
            {
                Margin = new Vector4F(15),
                InnerMargin = new Vector4F(3)
            };

            try
            {
                _configuration = JsonConvert.DeserializeObject<UnityExportConfiguration>(File.ReadAllText(FileItem.FullPath));
                if (_configuration == null)
                    _configuration = new UnityExportConfiguration();
            }
            catch (Exception)
            {
                _configuration = new UnityExportConfiguration();
            }


            layoutControl.Add("Unity Project 'Assets' Directory:", CreateFileSelectionTextBox(_configuration.UnityAssetsPath, newValue =>
            {
                _configuration.UnityAssetsPath = newValue;
                Save();
            }));


            layoutControl.Add("Package SubPath:", CreateTextBox(_configuration.PackageSubPath,
                (newValue) =>
                {
                    _configuration.PackageSubPath = newValue;
                    Save();
                }));


            verticalStackPanel.Children.Add(layoutControl);

            Content = verticalStackPanel;
        }


        public UIControl CreateFileSelectionTextBox(string initialText, Action<String> action)
        {
            var textbox = new ExtendedTextBox() {Text = initialText, HorizontalAlignment = HorizontalAlignment.Stretch};
            var gameProperty = textbox.Properties.Get<String>("Text");
            gameProperty.Changed += delegate(object sender, GamePropertyEventArgs<string> args) { action.Invoke(args.NewValue); };

            //button to select the folder
            var button = new TextButton() {Text = "...", ToolTip = "Select Location."};
            button.Click += delegate
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog() {SelectedPath = initialText};
                if (dialog.ShowCrossDialog() == DialogResult.OK)
                {
                    textbox.Text = dialog.SelectedPath;
                }
            };


            return new FlexibleStackPanel() {Children = {textbox, button}, Orientation = Orientation.Horizontal};
        }



        public UIControl CreateTextBox(string initialText, Action<String> action)
        {
            var textbox = new ExtendedTextBox() {Text = initialText, HorizontalAlignment = HorizontalAlignment.Stretch};
            var gameProperty = textbox.Properties.Get<String>("Text");
            gameProperty.Changed += delegate(object sender, GamePropertyEventArgs<string> args) { action.Invoke(args.NewValue); };

            return textbox;
        }



        private void Save()
        {
            File.WriteAllText(FileItem.FullPath, JsonConvert.SerializeObject(_configuration));
        }



        private void Export(MenuChild obj)
        {
            try
            {
                if (!String.IsNullOrEmpty(_configuration.UnityAssetsPath) && !String.IsNullOrEmpty(_configuration.PackageSubPath))
                {
                    ProgressWindow progressWindow = new ProgressWindow(PerformExport)
                    {
                        Title = "Exporting...",
                        LargeIcon = EmbeddedResources.Load<Texture2D>("Resources/Game_48x48.png")
                    };
                    progressWindow.Show(this.Screen);
                }
                else
                {
                    MessageWindow messageWindow = new MessageWindow()
                    {
                        MessageIcon = MessageWindowIcon.Error,
                        Text = "Please fill in all the fields.",
                        Title = "Export Error.",
                        Buttons = new[] {"OK"}
                    };
                    messageWindow.Show(Screen);
                }
            }
            catch (Exception ex)
            {
                _messageManager.Publish(new ExceptionThrown(ex));
            }
        }



        private void PerformExport(ProgressWindow.ProgressHandler progressHandler)
        {
            var innerZipName = "Data";
            var outerZipName = Path.GetFileNameWithoutExtension(_configuration.PackageSubPath) + ".bytes";

            progressHandler.SetText("Packaging Assets...");

            //create a zip with the whole folder structure
            using (ZipFile zip = new ZipFile())
            {
                zip.AddDirectory(FileItem.Project.BaseFolder.FullPath);
                zip.Save(innerZipName);
            }


            //store the key inside the project file
            String meta = String.Empty;


            //create a zip with the zipped project inside and 
            using (ZipFile outerZip = new ZipFile())
            {
                //outerZip.Encryption = EncryptionAlgorithm.WinZipAes256;
                //outerZip.Password = @"R6gPSgMa9LQtgZ{~j<:%{@8IEge5]e.']K,pPSao<\({ZhCa\h";

                outerZip.AddFile(innerZipName);
                outerZip.AddEntry("Meta", meta);

                outerZip.Save(outerZipName);
            }

            if (progressHandler.ShouldCancel)
                return;

            progressHandler.SetText("Copying Package...");

            var packageDestinationRelativeFolder = Path.GetDirectoryName(_configuration.PackageSubPath);
            var packageDestinationRelativePath = Path.Combine(packageDestinationRelativeFolder, outerZipName);

            Directory.CreateDirectory(Path.Combine(_configuration.UnityAssetsPath, packageDestinationRelativeFolder));
            File.Copy(outerZipName, Path.Combine(_configuration.UnityAssetsPath, packageDestinationRelativePath), true);
            File.Delete(innerZipName);
            File.Delete(outerZipName);

            
            var streamingAssetsPath = Path.Combine(_configuration.UnityAssetsPath, "StreamingAssets", "Sceelix", BuildPlatform.Enum.ToString());
            Directory.CreateDirectory(streamingAssetsPath);

            if (progressHandler.ShouldCancel)
                return;

            progressHandler.SetText("Deploying Runtime and Assemblies...");

            //now, copy all the dlls referenced by the nodes
            var designerEnvironment = new ProcedureEnvironment(new DesignerResourceManager(FileItem.Project, _services));
            var graphFiles = FileItem.Project.BaseFolder.Descendants.OfType<FileItem>().Where(x => x.Extension == ".slxg");
            var systemNodesTypes = graphFiles.SelectMany(graphFile => GraphLoad.LoadNodeTypes(graphFile.FullPath, designerEnvironment));

            var assemblyReferences = GetTypeReferences(systemNodesTypes);
            foreach (Assembly assemblyReference in assemblyReferences)
            {
                var absolutePath = new Uri(assemblyReference.CodeBase).LocalPath;

                //copy the dll file
                File.Copy(absolutePath, Path.Combine(streamingAssetsPath, Path.GetFileNameWithoutExtension(absolutePath) + ".dll"), true);

                //and copy any native dlls (for example, the Assimp libraries) that may have been defined at the library properties
                var nativeReferenceAttributes = assemblyReference.GetCustomAttributes<AssemblyNativeReferences>();
                foreach (String reference in nativeReferenceAttributes.SelectMany(x => x.RelativePaths))
                    File.Copy(Path.Combine(SceelixApplicationInfo.SceelixExeFolder, reference), Path.Combine(streamingAssetsPath, reference), true);
            }

            //copy the standalone executable, too
            var mainExecutableName = "Sceelix.Runtime.exe";
            var dependency = "DotNetZip.dll";
            File.Copy(Path.Combine(SceelixApplicationInfo.SceelixExeFolder, mainExecutableName), Path.Combine(streamingAssetsPath, mainExecutableName), true);
            File.Copy(Path.Combine(SceelixApplicationInfo.SceelixExeFolder, dependency), Path.Combine(streamingAssetsPath, dependency), true);
        }



        private HashSet<Assembly> GetTypeReferences(IEnumerable<Type> systemNodeTypes)
        {
            //var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            //var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            //var referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll").ToList();

            var domainAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic && !x.GlobalAssemblyCache).ToDictionary(y => y.FullName);


            //var dependentAssemblyNames = new HashSet<String>();
            var dependentAssemblyNames = new Dictionary<String, AssemblyName>();
            foreach (Type type in systemNodeTypes.Distinct())
            {
                //add the assembly itself
                var assemblyName = type.Assembly.GetName();
                dependentAssemblyNames[assemblyName.FullName] = assemblyName;
                //dependentAssemblyNames.Add(type.Assembly.GetName().FullName);

                //now add the assemblies that this one depends on
                var referencedAssemblies = type.Assembly.GetReferencedAssemblies();
                foreach (var source in referencedAssemblies)
                {
                    dependentAssemblyNames[source.FullName] = source;
                    //dependentAssemblyNames.Add(source.FullName);
                }
            }

            HashSet<Assembly> dependentAssemblies = new HashSet<Assembly>();
            //foreach (String fullAssemblyName in dependentAssemblyNames)
            foreach (KeyValuePair<string, AssemblyName> dependentAssemblyName in dependentAssemblyNames)
            {
                Assembly actualAssembly;
                if (!domainAssemblies.TryGetValue(dependentAssemblyName.Key, out actualAssembly))
                    actualAssembly = Assembly.Load(dependentAssemblyName.Value);

                if (!actualAssembly.IsDynamic && !actualAssembly.GlobalAssemblyCache)
                    dependentAssemblies.Add(actualAssembly);
                //var assembly = domainAssemblies.FirstOrDefault(ass => ass.FullName == fullAssemblyName);
                //if (assembly != null)
                //dependentAssemblies.Add(assembly);
            }


            return dependentAssemblies;
        }
    }
}