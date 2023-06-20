using System.IO;
using System.Windows;
using System.Threading;
using Microsoft.Web.WebView2.Core;
using System;

namespace PAppsplayer
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		static Mutex mutex = new Mutex(true, @PAppsplayer.MainWindow.Globals.TENANT_ID);

		public App()
		{
			if (mutex.WaitOne(TimeSpan.Zero, true))
			{
				try
				{
					try
					{
						if (!Directory.Exists(@PAppsplayer.MainWindow.Globals.USER_DATA_FOLDER))
						{
							Directory.CreateDirectory(@PAppsplayer.MainWindow.Globals.USER_DATA_FOLDER);
						}
						var version = CoreWebView2Environment.GetAvailableBrowserVersionString();
						// Do something with `version` if needed.
					}
					catch (WebView2RuntimeNotFoundException exception)
					{
						// Handle the runtime not being installed.
						// `exception.Message` is very nicely specific: It (currently at least) says "Couldn't find a compatible Webview2 Runtime installation to host WebViews."
						MessageBox.Show(exception.Message);
						Environment.Exit(0);
					}
				}
				finally
				{
					mutex.ReleaseMutex();
				}
			}
			else
			{
				if (!Directory.Exists(@PAppsplayer.MainWindow.Globals.USER_DATA_FOLDER))
				{
					Directory.CreateDirectory(@PAppsplayer.MainWindow.Globals.USER_DATA_FOLDER);
				}
				string filePath = @PAppsplayer.MainWindow.Globals.USER_DATA_FOLDER + @"\temp.txt";
				//	var outString = PAppsplayer.MainWindow.RemoveSpecialChars(Environment.GetCommandLineArgs()[1]);
				var outString = "";
				using (StreamWriter outputFile = new StreamWriter(filePath))
				{
					outputFile.WriteLine(outString);
				}
				Environment.Exit(0);
			}

		}
	}
}
