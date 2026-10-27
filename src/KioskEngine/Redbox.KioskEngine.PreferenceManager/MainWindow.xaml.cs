using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel.ApiService.EngineCore;
using Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient;
using Redbox.KioskEngine.PreferenceManager.Preferences;
using Redbox.Rental.Services.App;

namespace Redbox.KioskEngine.PreferenceManager;

public partial class MainWindow : Window, IComponentConnector
{
	private class PreferencePageTreeHeader
	{
		public IPreferencePage PreferencePage { get; set; }

		public override string ToString()
		{
			List<string> list = PreferencePage?.Path.Split('/').ToList();
			return list?[list.Count - 1];
		}
	}

	private string _environment;

	private TaskScheduler _taskScheduler;

	private bool _initialized;

	private Redbox.KioskEngine.PreferenceManager.Preferences.PreferenceManager _preferenceManager;

	private IPreferencePage _selectedPreferencePage;

	public MainWindow()
	{
		InitializeComponent();
	}

	private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		SavePreferencePageHostValues(_selectedPreferencePage);
		Close();
	}

	private void Window_Activated(object sender, EventArgs e)
	{
		if (!_initialized)
		{
			Initialize();
		}
	}

	private async Task<bool> ValidateApplicationEnvironment()
	{
		bool result = false;
		APIResponse<GetPreferenceValueResponse> aPIResponse = await KioskEngineApiEngineCoreClient.GetPreferenceValue("EngineCore_General", "Environment");
		if (aPIResponse.IsSuccessStatusCode)
		{
			_environment = aPIResponse.Response.Content;
			result = _environment == "Development" || _environment == "UAT" || _environment == "QA";
		}
		return result;
	}

	private void InitializeSafetyTimer()
	{
		if (1 == 0)
		{
			return;
		}
		_taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
		System.Timers.Timer timer = new System.Timers.Timer(TimeSpan.FromMinutes(15.0).TotalMilliseconds);
		timer.Elapsed += delegate
		{
			Task.Factory.StartNew(delegate
			{
				Close();
			}, CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
		};
		timer.Start();
	}

	private void Initialize()
	{
		_initialized = true;
		ServiceLocator.Instance.AddService(typeof(IHttpService), new HttpService(LogHelper.Instance, new HttpClient()));
		Task.Factory.StartNew((Func<Task>)async delegate
		{
			StatusLabel.Content = "Validating environment...";
			if (await ValidateApplicationEnvironment())
			{
				_preferenceManager = new Redbox.KioskEngine.PreferenceManager.Preferences.PreferenceManager();
				_preferenceManager.RegisterPreferencePages();
				PopulatePreferencesTree();
				StatusLabel.Content = null;
				InitializeSafetyTimer();
			}
			else
			{
				SimpleMessageWindow.Show(this, "Invalid Environment", "Error: Invalid Environment: " + _environment);
				Close();
			}
		}, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
	}

	private void PopOutPreferencePage()
	{
		if (_selectedPreferencePage?.PreferencePageHost != null && _selectedPreferencePage?.PreferencePageHost is IPreferencePageHost && _selectedPreferencePage?.PreferencePageHost is FrameworkElement preferencePageHostFrameworkElement)
		{
			PreferencePageHostGrid.Children.Clear();
			_selectedPreferencePage.PreferencePageHost = null;
			PopOutWindow popOutWindow = new PopOutWindow();
			popOutWindow.Owner = this;
			popOutWindow.Width = PanelHostGrid.ActualWidth + 10.0;
			popOutWindow.Height = PanelHostGrid.ActualHeight + 10.0;
			popOutWindow.SetPreferencePageHost(_selectedPreferencePage, preferencePageHostFrameworkElement);
			popOutWindow.Show();
		}
	}

	private void PopulatePreferencesTree()
	{
		if (_preferenceManager?.PreferencePages == null)
		{
			return;
		}
		foreach (IPreferencePage preferencePage in _preferenceManager.PreferencePages)
		{
			List<string> list = preferencePage.Path.Split('/').ToList();
			TreeViewItem treeViewItem = null;
			foreach (string item in list)
			{
				if (treeViewItem == null)
				{
					foreach (object item2 in (IEnumerable)NavigationTree.Items)
					{
						if (item2 is TreeViewItem { Header: var header } treeViewItem2 && header?.ToString() == item)
						{
							treeViewItem = treeViewItem2;
						}
					}
					if (treeViewItem == null)
					{
						treeViewItem = new TreeViewItem
						{
							Header = item
						};
						NavigationTree.Items.Add(treeViewItem);
					}
					continue;
				}
				TreeViewItem treeViewItem3 = null;
				foreach (object item3 in (IEnumerable)treeViewItem.Items)
				{
					if (item3 is TreeViewItem { Header: var header2 } treeViewItem4 && header2?.ToString() == item)
					{
						treeViewItem3 = treeViewItem4;
					}
				}
				if (treeViewItem3 == null)
				{
					treeViewItem3 = new TreeViewItem
					{
						Header = item
					};
					treeViewItem.Items.Add(treeViewItem3);
				}
				treeViewItem = treeViewItem3;
			}
			if (treeViewItem != null)
			{
				treeViewItem.Header = new PreferencePageTreeHeader
				{
					PreferencePage = preferencePage
				};
			}
		}
	}

	private void NavigationTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
	{
		SavePreferencePageHostValues(((e.OldValue as TreeViewItem)?.Header as PreferencePageTreeHeader)?.PreferencePage);
		IPreferencePage preferencePage = ((e.NewValue as TreeViewItem)?.Header as PreferencePageTreeHeader)?.PreferencePage;
		UpdatePreferencePageHostGrid(preferencePage);
	}

	private void SavePreferencePageHostValues(IPreferencePage preferencePage)
	{
		if (preferencePage != null)
		{
			(preferencePage.PreferencePageHost as IPreferencePageHost)?.SaveValues(preferencePage);
		}
	}

	private void UpdatePreferencePageHostGrid(IPreferencePage preferencePage)
	{
		if (preferencePage != null)
		{
			FrameworkElement frameworkElement = preferencePage.PreferencePageHost as FrameworkElement;
			IPreferencePageHost preferencePageHost = frameworkElement as IPreferencePageHost;
			if (frameworkElement == null)
			{
				frameworkElement = Activator.CreateInstance(preferencePage.PreferencePageHostType) as FrameworkElement;
				preferencePageHost = (IPreferencePageHost)(preferencePage.PreferencePageHost = frameworkElement as IPreferencePageHost);
			}
			PreferencePageHostGrid.Children.Clear();
			PreferencePageHostGrid.Children.Add(frameworkElement);
			preferencePageHost?.LoadValues(preferencePage);
			_selectedPreferencePage = preferencePage;
		}
		else
		{
			PreferencePageHostGrid.Children.Clear();
			_selectedPreferencePage = null;
		}
	}

	private void PopOutCommand_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		PopOutPreferencePage();
	}
}
