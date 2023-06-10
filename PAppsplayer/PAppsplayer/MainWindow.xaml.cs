using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace PAppsplayer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
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

		public event PropertyChangedEventHandler PropertyChanged;

		private int _tabCount = 0;
		private int _selectedIndex = 0;

		private ObservableCollection<TabItem> _webView2Tabs = new ObservableCollection<TabItem>();
		public int SelectedIndex
		{
			get { return _selectedIndex; }
			set
			{
				if (_selectedIndex == value)
					return;

				//set value
				_selectedIndex = value;

				OnPropertyChanged(nameof(SelectedIndex));
			}
		}

		public ObservableCollection<TabItem> WebView2Tabs
		{
			get { return _webView2Tabs; }
			set
			{
				if (_webView2Tabs == value)
					return;

				//set value
				_webView2Tabs = value;

				OnPropertyChanged(nameof(WebView2Tabs));
			}
		}
		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			AddTab("https://www.microsoft.com");
		}


		private void AddTab(string url, string headerText = null, string userDataFolder = null)
		{
			AddTab(new Uri(url), headerText, userDataFolder);
		}
		private void AddTab(Uri uri, string headerText = null, string userDataFolder = null)
		{
			//increment
			_tabCount++;

			if (headerText == null)
				headerText = $"Tab {_tabCount}";

			//if userDataFolder hasn't been specified, create a folder in the user's temp folder
			//each WebView2 instance will have it's own folder
			if (String.IsNullOrEmpty(userDataFolder))
				userDataFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location) + _tabCount);

			//create new instance setting userDataFolder
			WebView2 wv = new WebView2() { CreationProperties = new CoreWebView2CreationProperties() { UserDataFolder = userDataFolder } };
			wv.CoreWebView2InitializationCompleted += WebView2_CoreWebView2InitializationCompleted;

			//create TextBlock
			TextBlock textBlock = new TextBlock();

			//add new Run to TextBlock
			textBlock.Inlines.Add(new Run(headerText));

			//add new Run to TextBlock
			textBlock.Inlines.Add(new Run("   "));

			//create Run
			Run runHyperlink = new Run("X");
			runHyperlink.FontFamily = new FontFamily("Monotype Corsiva");
			runHyperlink.FontWeight = FontWeights.Bold;
			runHyperlink.Foreground = new SolidColorBrush(Colors.Red);

			//add Run to HyperLink
			Hyperlink hyperlink = new Hyperlink(runHyperlink) { Name = $"hyperlink_{_tabCount}" };
			hyperlink.Click += Hyperlink_Click;

			//add Hyperlink to TextBlock
			textBlock.Inlines.Add(hyperlink);

			//create new instance and set Content
			HeaderedContentControl hcc = new HeaderedContentControl() { Content = textBlock };

			//add TabItem
			_webView2Tabs.Add(new TabItem { Header = hcc, Content = wv, Name = $"tab_{_tabCount}" });

			//navigate
			wv.Source = uri;

			//set selected index
			tabControl1.SelectedIndex = _webView2Tabs.Count - 1;
		}

		private void LogMsg(string msg, bool includeTimestamp = true)
		{
			if (includeTimestamp)
				msg = $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff")} - {msg}";

			Debug.WriteLine(msg);
		}

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void RemoveTab(int index)
		{
			if (index >= 0 && index < _webView2Tabs.Count)
			{
				WebView2 wv = (WebView2)_webView2Tabs[index].Content;

				//get userDataFolder location
				//string userDataFolder = wv.CreationProperties.UserDataFolder;
				string userDataFolder = wv.CoreWebView2.Environment.UserDataFolder;

				//unsubscribe from event(s)
				wv.CoreWebView2InitializationCompleted -= WebView2_CoreWebView2InitializationCompleted;
				wv.CoreWebView2.NewWindowRequested -= CoreWebView2_NewWindowRequested;

				//get process
				var wvProcess = Process.GetProcessById((int)wv.CoreWebView2.BrowserProcessId);

				//dispose
				wv.Dispose();

				//wait for WebView2 process to exit
				wvProcess.WaitForExit();

				//for security purposes, delete userDataFolder
				if (!String.IsNullOrEmpty(userDataFolder) && System.IO.Directory.Exists(userDataFolder))
				{
					System.IO.Directory.Delete(userDataFolder, true);
					LogMsg($"UserDataFolder '{userDataFolder}' deleted.");
				}

				//TabItem item = _webView2Tabs[index];
				LogMsg($"Removing {_webView2Tabs[index].Name}");

				//remove
				_webView2Tabs.RemoveAt(index);
			}
			else
			{
				LogMsg($"Invalid index: {index}; _webView2Tabs.Count: {_webView2Tabs.Count}");
			}
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			if (_webView2Tabs.Count > 0)
			{
				//get instance of WebView2 from last tab
				WebView2 wv = (WebView2)_webView2Tabs[_webView2Tabs.Count - 1].Content;

				//if CoreWebView2 hasn't finished initializing, it will be null
				if (wv.CoreWebView2?.BrowserProcessId > 0)
				{
					await wv.ExecuteScriptAsync($@"window.open('https://www.google.com/', '_blank');");
				}
			}
			else
			{
				AddTab("https://www.microsoft.com");
			}
		}

		private void CoreWebView2_NewWindowRequested(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs e)
		{
			e.Handled = true;

			AddTab(e.Uri);
		}

		private void Hyperlink_Click(object sender, RoutedEventArgs e)
		{
			Hyperlink hyperlink = (Hyperlink)sender;

			LogMsg($"Hyperlink_Click - name: {hyperlink.Name}");

			string hyperLinkNumStr = hyperlink.Name.Substring(hyperlink.Name.IndexOf("_") + 1);
			int hyperLinkNum = 0;

			//try to convert to int
			Int32.TryParse(hyperLinkNumStr, out hyperLinkNum);

			int index = 0;

			//it's possible that an 'X' was clicked on a tab that wasn't selected
			//since both the tab name and hyperlink name end with the same number,
			//get the number from the hyperlink name and use that to find the matching 
			//tab name
			for (int i = 0; i < _webView2Tabs.Count; i++)
			{
				TabItem item = _webView2Tabs[i];

				if (item.Name == $"tab_{hyperLinkNum}")
				{
					index = i;
					break;
				}
			}

			//set selected index
			tabControl1.SelectedIndex = index;

			RemoveTab(index);
		}
		private void WebView2_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
		{
			LogMsg("WebView2_CoreWebView2InitializationCompleted");
			if (!e.IsSuccess)
				LogMsg($"{e.InitializationException}");

			if (sender != null)
			{
				WebView2 wv = (WebView2)sender;
				wv.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
			}
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			if (_webView2Tabs != null && _webView2Tabs.Count > 0)
			{
				for (int i = 0; i < _webView2Tabs.Count - 1; i++)
				{
					//remove all tabs which will dispose of each WebView2
					RemoveTab(i);
				}
			}
		}
	}
}