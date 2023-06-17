using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using ChromeTabs;

namespace PAppsplayer.ViewModel
{
	public abstract class TabBase : ViewModelBase
	{
		private int _tabNumber;
		public int TabNumber
		{
			get => _tabNumber;
			set
			{
				if (_tabNumber != value)
				{
					Set(() => TabNumber, ref _tabNumber, value);
				}
			}
		}

		private string _tabName;
		public string TabName
		{
			get => _tabName;
			set
			{
				if (_tabName != value)
				{
					Set(() => TabName, ref _tabName, value);
				}
			}
		}


		private bool _isPinned;
		public bool IsPinned
		{
			get => _isPinned;
			set
			{
				if (_isPinned != value)
				{
					Set(() => IsPinned, ref _isPinned, value);
				}
			}
		}
	}
}

namespace PAppsplayer.ViewModel
{
	public class TabClass1 : TabBase
	{
		public string MyStringContent { get; set; }
	}
}
namespace PAppsplayer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		public ObservableCollection<MainWindow> ItemCollection { get; set; }
		//since we don't know what kind of objects are bound, so the sorting happens outside with the ReorderTabsCommand.
		public RelayCommand<TabReorder> ReorderTabsCommand { get; set; }
		public RelayCommand AddTabCommand { get; set; }
		public RelayCommand<MainWindow> CloseTabCommand { get; set; }



		//To close a tab, we simply remove the viewmodel from the source collection.
		private void CloseTabCommandAction(MainWindow vm)
		{
			ItemCollection.Remove(vm);
		}

		//Adds a random tab
		private void AddTabCommandAction()
		{
			//Code to create new tab and new instance of webview
			ItemCollection.Add(CreateTab1());
		}
		protected MainWindow CreateTab1()
		{

		//I think I create my webview here?

		//	var tab = new ViewModel { TabName = "Tab class 1", MyStringContent = "New webview2 content here"};
			return new MainWindow();
		}
		private object MainWindow1()
		{
			throw new NotImplementedException();
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
			ItemCollection = new ObservableCollection<MainWindow>();
			//	ItemCollection.CollectionChanged += ItemCollection_CollectionChanged;
			//	ReorderTabsCommand = new RelayCommand<TabReorder>(ReorderTabsCommandAction);
			AddTabCommand = new RelayCommand(AddTabCommandAction, () => true);
			CloseTabCommand = new RelayCommand<MainWindow>(CloseTabCommandAction);
			if (!Directory.Exists(PAppsplayer.MainWindow.Globals.USER_DATA_FOLDER))
			{
				Directory.CreateDirectory(PAppsplayer.MainWindow.Globals.USER_DATA_FOLDER);
			}
			InitializeComponent();
			AttachControlEventHandlers();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (Environment.GetCommandLineArgs().Length > 1)
			{
				var outString = RemoveSpecialChars(Environment.GetCommandLineArgs()[1]);
				//Add code to check for gpu pram
				if (outString.StartsWith("gpu"))
				{
					AddTab($"edge://gpu");
					return;
				}
				else
				{
					AddTab($"{Globals.BASE_URL}{outString}&source=iframe&hidenavbar=true");
					return;
				}
			}
			else
			{
				AddTab($"{Globals.BASE_URL}");
			}
		}


		private void AddTab(string url, string headerText = null)
		{
			AddTab(new Uri(url), headerText);
		}

		//This function creates a new webview2 instance when a tab is created in any means
		private void AddTab(Uri uri, string headerText = null)
		{
			//increment
			_tabCount++;

			if (headerText == null)
				headerText = $"Tab {_tabCount}";
			var options = new CoreWebView2EnvironmentOptions
			{
				AllowSingleSignOnUsingOSPrimaryAccount = true,
				Language = $"{Globals.APP_REQUEST_LANG}"
			};
			var userDataFolder = System.IO.Path.Combine(Globals.USER_DATA_FOLDER + "\\" + _tabCount);
			//if userDataFolder hasn't been specified, create a folder in the user's temp folder
			//each WebView2 instance will have it's own folder
			if (!Directory.Exists(Globals.USER_DATA_FOLDER))
			{
				Directory.CreateDirectory(Globals.USER_DATA_FOLDER);
			}
			var webView2Environment = CoreWebView2Environment.CreateAsync(null, userDataFolder, options).Result;

			//create new instance setting userDataFolder
			WebView2 wv = new WebView2();
			wv.EnsureCoreWebView2Async(webView2Environment);

			wv.CoreWebView2InitializationCompleted += WebView2_CoreWebView2InitializationCompleted;

			//add TabItem
			_webView2Tabs.Add(new TabItem {  Content = wv, Name = $"tab_{_tabCount}" });

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

		//This function hold what should be done when the tab is closed off
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
				wv.CoreWebView2.CallDevToolsProtocolMethodAsync("Network.clearBrowserCache", "{}");
				wv.CoreWebView2.Profile.ClearBrowsingDataAsync();
				//dispose
				wv.Dispose();

				//wait for WebView2 process to exit
				wvProcess.WaitForExit();

				DirectoryInfo directory = new DirectoryInfo(userDataFolder);

				foreach (FileInfo file in directory.EnumerateFiles())
				{
					try
					{
						file.Delete();
					}
					catch (IOException)
					{
						return;
					}
					catch (UnauthorizedAccessException)
					{
						return;
					}
				}

				foreach (DirectoryInfo dir in directory.EnumerateDirectories())
				{
					try
					{
						dir.Delete(true);
					}
					catch (IOException)
					{
						return;
					}
					catch (UnauthorizedAccessException)
					{
						return;
					}
				}


				TabItem item = _webView2Tabs[index];
				LogMsg($"Removing {_webView2Tabs[index].Name}");

				//remove
				_webView2Tabs.RemoveAt(index);
			}
			else
			{
				LogMsg($"Invalid index: {index}; _webView2Tabs.Count: {_webView2Tabs.Count}");
			}
		}

		//This is the function that is run when a new tab button is pressed (although may not even be needed)
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
				AddTab($"{Globals.BASE_URL}");
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

		private void WebView2_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
		{
			LogMsg("WebView2_CoreWebView2InitializationCompleted");
			if (!e.IsSuccess)
				LogMsg($"{e.InitializationException}");

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

		//Here i'm closing each tab one by one and clearing up temp data left behind
		private void Window_Closing(object sender, CancelEventArgs e)
		{
			MessageBoxResult result = MessageBox.Show("Do you really want to close " + Globals.APP_NAME + "?",
"Warning", MessageBoxButton.YesNo, MessageBoxImage.Question);
			if (result == MessageBoxResult.No)
			{
				e.Cancel = true;
			}
			if (result == MessageBoxResult.Yes)
			{
				if (_webView2Tabs != null && _webView2Tabs.Count > 0)
				{
					for (int i = 0; i < _webView2Tabs.Count - 1; i++)
					{
						MessageBox.Show($"{i}");
						//remove all tabs which will dispose of each WebView2
						RemoveTab(i);
					}
				}
				var milliseconds = 100;
				Thread.Sleep(milliseconds);
				DirectoryInfo directory = new DirectoryInfo(Globals.USER_DATA_FOLDER);

				foreach (FileInfo file in directory.EnumerateFiles())
				{
					try
					{
						file.Delete();
					}
					catch (IOException)
					{
						return;
					}
					catch (UnauthorizedAccessException)
					{
						return;
					}
				}

				foreach (DirectoryInfo dir in directory.EnumerateDirectories())
				{
					try
					{
						dir.Delete(true);
					}
					catch (IOException)
					{
						return;
					}
					catch (UnauthorizedAccessException)
					{
						return;
					}
				}
			}
		}

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

		public void OnChanged(Object sender, FileSystemEventArgs e)
		{
			Dispatcher.Invoke(() =>
			{
				this.Activate();
				var milliseconds = 100;
				Thread.Sleep(milliseconds);
				if (e.ChangeType != WatcherChangeTypes.Changed)
				{
					return;
				}

				string filePath = @MainWindow.Globals.USER_DATA_FOLDER + @"\temp.txt";
				using (StreamReader inputFile = new StreamReader(filePath))
				{
					var outString = Regex.Replace(inputFile.ReadToEnd(), @"^\s*$\n|\r", string.Empty, RegexOptions.Multiline).TrimEnd();
					var outString1 = MainWindow.RemoveSpecialChars(outString);
					//Add code to check for gpu pram
					if (outString1.StartsWith("gpu"))
					{
						AddTab($"edge://gpu");
					}
					else
					{
						AddTab($"{Globals.BASE_URL}{outString1}&source=iframe&hidenavbar=true");
					}
				}
			});
		}
		public void AttachControlEventHandlers()
		{
			if (!Directory.Exists(Globals.USER_DATA_FOLDER))
			{
				Directory.CreateDirectory(Globals.USER_DATA_FOLDER);
			}

			 var watcher = new FileSystemWatcher($"{Globals.USER_DATA_FOLDER}");

			//FileSystemWatcher fileSystemWatcher = new($"{Globals.USER_DATA_FOLDER}");
			//var watcher = fileSystemWatcher;
			watcher.NotifyFilter = NotifyFilters.LastWrite;
			watcher.Changed += OnChanged;
			watcher.Filter = "temp.txt";
			watcher.IncludeSubdirectories = false;
			watcher.EnableRaisingEvents = true;
		}
		//Only show required menu option in webview2
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
	}
}