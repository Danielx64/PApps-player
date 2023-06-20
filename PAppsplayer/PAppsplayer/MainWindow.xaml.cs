using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using ActiproSoftware.Windows.Controls.Docking;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Web.WebView2.Core;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
namespace PAppsplayer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		private int browserIndex = 0;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes an instance of the <c>MainControl</c> class.
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();

			this.AddTab("http://www.bing.com");
		}
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
		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// NON-PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////
		private void AddTab(string url)
		{
			CreateBrowserWindow(new Uri(url));
		}
		/// <summary>
		/// Creates a new web browser <see cref="DocumentWindow"/>.
		/// </summary>
		/// <param name="url">The URL to load.</param>
		/// <returns>The <see cref="DocumentWindow"/> that was created.</returns>
		private DocumentWindow CreateBrowserWindow(Uri url)
		{
			var options = new CoreWebView2EnvironmentOptions
			{
				AllowSingleSignOnUsingOSPrimaryAccount = true,
				Language = $"{Globals.APP_REQUEST_LANG}"
			};
			var userDataFolder = System.IO.Path.Combine(Globals.USER_DATA_FOLDER);

			if (!Directory.Exists(Globals.USER_DATA_FOLDER))
			{
				Directory.CreateDirectory(Globals.USER_DATA_FOLDER);
			}
			var webView2Environment = CoreWebView2Environment.CreateAsync(null, userDataFolder, options).Result;

			//create new instance setting userDataFolder
			WebView2 browser = new WebView2();
			browser.EnsureCoreWebView2Async(webView2Environment);

			browser.CoreWebView2InitializationCompleted += WebView2_CoreWebView2InitializationCompleted;



			InteropFocusTracking.SetIsEnabled(browser, true);

			// Create the document
			var documentWindow = new DocumentWindow(dockSite, "Browser" + (++browserIndex), "New Tab", null, browser);

			// Activate the document
			documentWindow.Activate();

			// Navigate to a page
			browser.Source = url;

			return documentWindow;
		}
		private void WebView2_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
		{

			if (sender != null)
			{
				WebView2 wv = (WebView2)sender;
				wv.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
				wv.CoreWebView2.Settings.AreDevToolsEnabled = false;
				wv.CoreWebView2.Settings.IsStatusBarEnabled = false;
				wv.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
				wv.CoreWebView2.Settings.AreHostObjectsAllowed = false;
				wv.CoreWebView2.Settings.IsWebMessageEnabled = false;
				wv.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
				wv.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
				wv.CoreWebView2.Settings.IsPasswordAutosaveEnabled = false;
				wv.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
				wv.CoreWebView2.ContextMenuRequested += menurequested;
			}
		}
		/// <summary>
		/// Occurs when the browser completes page loading.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The <c>NavigationEventArgs</c> that contained data related to this event.</param>


		/// <summary>
		/// Occurs when the browser starts navigation.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The <c>NavigationEventArgs</c> that contained data related to this event.</param>


		/// <summary>
		/// Occurs when a new window is requested by the user.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The <c>RoutedEventArgs</c> that contained data related to this event.</param>
		private void OnDockSiteNewWindowRequested(object sender, RoutedEventArgs e)
		{
			this.AddTab("Https://google.com");
		}

		/// <summary>
		/// Occurs when a docking window is activated.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The <c>DockingWindowEventArgs</c> that contains data related to this event.</param>
		private void OnDockSiteWindowActivated(object sender, DockingWindowEventArgs e)
		{
			var browser = (e.Window != null ? e.Window.Content as WebView2 : null);
			if (browser != null) ;
			//	this.UpdateUrlAndTitle(browser);
		}

		/// <summary>
		/// Occurs when a docking window is registered.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The <c>DockingWindowEventArgs</c> that contains data related to this event.</param>
		private void OnDockSiteWindowRegistered(object sender, DockingWindowEventArgs e)
		{
			var browser = (WebView2)e.Window.Content;
			//browser.LoadCompleted += OnBrowserLoadCompleted;
			//browser.Navigated += OnBrowserNavigated;
		}

		/// <summary>
		/// Occurs when a docking window is registered.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The <c>DockingWindowEventArgs</c> that contains data related to this event.</param>
		private void OnDockSiteWindowUnregistered(object sender, DockingWindowEventArgs e)
		{
			var browser = (WebView2)e.Window.Content;
			//	browser.LoadCompleted -= OnBrowserLoadCompleted;
			//	browser.Navigated -= OnBrowserNavigated;

			// Ensure there is always at least one browser tab
			if (dockSite.DocumentWindows.Count == 0)
				this.AddTab("about:blank");
		}



		/// <summary>
		/// Updates the URL and title from the specified web browser.
		/// </summary>
		/// <param name="browser">The <see cref="WebBrowser"/> to examine.</param>
		private void UpdateUrlAndTitle(WebBrowser browser)
		{
			try
			{
				//	urlTextBox.Text = (browser.Source != null ? browser.Source.ToString() : string.Empty);
				//	var window = (DocumentWindow)browser.Parent;
				//	window.Title = urlTextBox.Text;
				//	window.FileName = urlTextBox.Text;
			}
			catch { }
		}
		private void menurequested(object sender, CoreWebView2ContextMenuRequestedEventArgs args)
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
					if (menuList[index].Name != "reload") { menuList.RemoveAt(index); }
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

		}

		private void CoreWebView2_NewWindowRequested(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs e)
		{
			//WebView2 wv = (WebView2)_webView2Tabs[SelectedIndex].Content;
			if (e.Uri.Contains("apps.powerapps.com/play/e/"))
			{
				e.NewWindow = (CoreWebView2)sender;
				e.Handled = true;
			}
			else if (e.Uri.Contains("edge://gpu"))
			{
				e.NewWindow = (CoreWebView2)sender;
				e.Handled = true;
			}
			else
			{
				AddTab(e.Uri);
				e.Handled = true;
			}
		}
	}
}