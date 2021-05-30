using Sceelix.Designer.GUI;
using Sceelix.Designer.Utils;
using System;
using System.IO;
using log4net;
using log4net.Config;
using Sceelix.Designer.Messaging;
#if MACOS
using MonoMac.AppKit;
using MonoMac.Foundation;
#else
using System.Windows.Forms;

#endif

namespace Sceelix.Designer
{
#if WINDOWS || LINUX

	/// <summary>
	/// The main class.
	/// </summary>
	public static class DesignerProgram
	{
		public static readonly ILog Log = LogManager.GetLogger(typeof(DesignerProgram));



		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			try
			{
				GlobalContext.Properties["LogsFolder"] = SceelixApplicationInfo.LogsFolder;
				XmlConfigurator.Configure(new FileInfo(Path.Combine(SceelixApplicationInfo.SceelixExeFolder, "log4net.config")));

				//log information about the software version and OS
				Log.Debug($"Started Sceelix Designer, version {SystemInfoManager.Version}.");

                #if WINDOWS
				Log.Debug($"OS is {System.Environment.OSVersion}, i.e. {SystemInfoManager.OS}.");
                #elif LINUX
					Log.Debug(String.Format("OS is {0}.", System.Environment.OSVersion.ToString()));
				#endif

                AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs eventArgs) { Log.Debug(String.Format("Application Crash: {0}.", (Exception) eventArgs.ExceptionObject)); };

			    using (var game = new SceelixGame())
					game.Run();
			}
			catch (Exception ex)
			{
				Log.Fatal("Sceelix Main Fatal Error.", ex);

				MessageBox.Show(ex.ToString(), "Sceelix Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}

#elif MACOS

	public class AppDelegate : NSApplicationDelegate
	{
		private SceelixGame game;

		public override void FinishedLaunching (MonoMac.Foundation.NSObject notification)
		{
			try
			{
				game = new SceelixGame();
				game.Run ();
			}
			catch (Exception ex)
			{
				DesignerProgram.Log.Fatal("Sceelix Main Fatal Error.", ex);

				var alert = new NSAlert();
				alert.MessageText = "Sceelix Fatal Error";
				alert.InformativeText = ex.ToString();
				alert.AlertStyle = NSAlertStyle.Critical;
				alert.RunModal();
			}
		}

		public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
		{
			DesignerProgram.Log.Debug("Application Closed.");
			game.MessageManager.Publish(new ProcessExiting());

			return true;
		}
		

		public override void OpenFiles(NSApplication sender, string[] filenames)
		{
			base.OpenFiles(sender, filenames);
		}

		public override bool OpenFile(NSApplication sender, string filename)
		{
			return base.OpenFile(sender, filename);
		}
	}


	public static class DesignerProgram
	{
		public static readonly ILog Log = LogManager.GetLogger(typeof(DesignerProgram));

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main (string [] args)
		{
			//try this!
			GlobalContext.Properties["LogsFolder"] = SceelixApplicationInfo.LogsFolder;
			XmlConfigurator.Configure(new FileInfo(Path.Combine(SceelixApplicationInfo.SceelixExeFolder, "log4net.config")));

			 //log information about the software version and OS
			Log.Debug(String.Format("Started Sceelix Designer, version {0}.", SystemInfoManager.Version));
				
			Log.Debug(String.Format("OS is {0}.", System.Environment.OSVersion.ToString()));

			AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs eventArgs)
			{
				Log.Debug(String.Format("Application Crash: {0}.", (Exception)eventArgs.ExceptionObject));
			};

			NSApplication.Init();

			using (var p = new NSAutoreleasePool())
			{
				NSApplication.SharedApplication.Delegate = new AppDelegate();
				NSApplication.Main(args);
			}
		}
	}
#endif
}