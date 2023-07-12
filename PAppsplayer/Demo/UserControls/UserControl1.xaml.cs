using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
namespace Demo.UserControls
{
	public static class Globals
	{
		public static readonly String APP_ID = "your app id"; // Unmodifiable
		public static readonly String APP_ICON = "/Images/globe.png"; // Unmodifiable
		public static readonly String APP_FOLDER_NAME = "your app folder name"; // Unmodifiable
		public static readonly String APP_NAME = "your app name"; // Unmodifiable
		public static readonly String TENANT_ID = "your teant id"; // Unmodifiable
		public static readonly String APP_USERAGENT = "Your useragent here";
		public static readonly String URI_SCHEMA = "ms-mobile-apps:///providers/Microsoft.PowerApps/apps/";
		public static readonly String BASE_URL = "https://apps.powerapps.com/play/" + APP_ID + "?tenantId=" + TENANT_ID + "&source=iframe&hidenavbar=true"; // Unmodifiable
		public static readonly String APP_REQUEST_LANG = "en-AU";
		public static readonly String USER_DATA_FOLDER = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), APP_FOLDER_NAME);
	}
	/// <summary>
	/// Interaction logic for UserControl1.xaml
	/// </summary>
	public partial class UserControl1 : UserControl
	{

		public static string RemoveSpecialChars(string str)
		{

			str = str.Replace($"{Globals.URI_SCHEMA}", "");
			// Create  a string array and add the special characters you want to remove
			string[] chars = new string[] { "~", "`", "!", "@", "#", "$", "%", "^", "*", "(", ")", "_", "+", "}", "{", "]", "[", "|", "\"", ":", "'", ":", ">", "<", "/", ".", ",", "\\" };

			//Iterate the number of times based on the String array length.
			for (int i = 0; i < chars.Length; i++)
			{
				if (str.Contains(chars[i]))
				{
					str = str.Replace(chars[i], "");
				}
			}

			return str;
		}

		private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
		{
			if (e.Uri.Contains("apps.powerapps.com/play/e/"))
			{
				e.NewWindow = WebView.CoreWebView2;
			}
			else
			{
				e.Handled = false;
			}
		}
		private async void startBrowser()
		{
			var options = new CoreWebView2EnvironmentOptions
			{
				AllowSingleSignOnUsingOSPrimaryAccount = true,
				Language = $"{Globals.APP_REQUEST_LANG}"
			};


			if (!Directory.Exists(Globals.USER_DATA_FOLDER))
			{
				Directory.CreateDirectory(Globals.USER_DATA_FOLDER);
			}
			var environment = await CoreWebView2Environment.CreateAsync(null, Globals.USER_DATA_FOLDER, options).ConfigureAwait(true);


			await WebView.EnsureCoreWebView2Async(environment).ConfigureAwait(true);
			WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
			WebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
			WebView.CoreWebView2.Settings.IsStatusBarEnabled = false;
			WebView.CoreWebView2.Settings.UserAgent = $"{Globals.APP_USERAGENT}";
			WebView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
			WebView.CoreWebView2.Settings.AreHostObjectsAllowed = false;
			WebView.CoreWebView2.Settings.IsWebMessageEnabled = false;
			WebView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
			WebView.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
			WebView.CoreWebView2.Settings.IsPasswordAutosaveEnabled = false;
			WebView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
			WebView.CoreWebView2.ContextMenuRequested += delegate (object sender, CoreWebView2ContextMenuRequestedEventArgs args)
			{
				IList<CoreWebView2ContextMenuItem> menuList = args.MenuItems;
				CoreWebView2ContextMenuTargetKind context = args.ContextMenuTarget.Kind;
				if (context == CoreWebView2ContextMenuTargetKind.Audio)
				{
					for (int index = menuList.Count - 1; index >= 0; index--)
					{
						menuList.RemoveAt(index);
					}
				}
				if (context == CoreWebView2ContextMenuTargetKind.Image)
				{
					for (int index = menuList.Count - 1; index >= 0; index--)
					{
						menuList.RemoveAt(index);
					}
				}
				if (context == CoreWebView2ContextMenuTargetKind.Page)
				{
					for (int index = menuList.Count - 1; index >= 0; index--)
					{
						if (menuList[index].Name != "refresh") { menuList.RemoveAt(index); }
					}
				}
				if (context == CoreWebView2ContextMenuTargetKind.SelectedText)
				{
					for (int index = menuList.Count - 1; index >= 0; index--)
					{
						if (menuList[index].Name != "copy" && menuList[index].Name != "paste" && menuList[index].Name != "cut") { menuList.RemoveAt(index); }
					}
				}
				if (context == CoreWebView2ContextMenuTargetKind.Video)
				{
					for (int index = menuList.Count - 1; index >= 0; index--)
					{
						menuList.RemoveAt(index);
					}
				}
			};
			if (Environment.GetCommandLineArgs().Length > 1)
			{
				var outString = RemoveSpecialChars(Environment.GetCommandLineArgs()[1]);
				//Add code to check for gpu pram
				if (outString.StartsWith("gpu"))
				{
					WebView.Source = new System.Uri($"edge://gpu", System.UriKind.Absolute);
					return;
				}
				else
				{
					//	WebView.Source = new System.Uri($"{Globals.BASE_URL}{outString}&source=iframe&hidenavbar=true", System.UriKind.Absolute);
						WebView.Source = new System.Uri($"http://google.com", System.UriKind.Absolute);

					return;
				}
			}
			else
			{
				WebView.Source = new System.Uri($"http://google.com", System.UriKind.Absolute);

				//WebView.Source = new System.Uri($"{Globals.BASE_URL}", System.UriKind.Absolute);
			}
		}
		public UserControl1()
        {
            InitializeComponent();
			//   this.ID.Text = Guid.NewGuid().ToString();
			startBrowser();
		}
    }
}
