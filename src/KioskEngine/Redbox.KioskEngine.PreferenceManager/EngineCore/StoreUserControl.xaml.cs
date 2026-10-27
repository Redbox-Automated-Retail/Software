using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient;
using Redbox.KioskEngine.PreferenceManager.Preferences;

namespace Redbox.KioskEngine.PreferenceManager.EngineCore;

[PreferencePage(Name = "Store", Path = "Engine Core/Store", TreeOrder = 100)]
public partial class StoreUserControl : UserControl, IPreferencePageHost, IComponentConnector
{
	private class StoreServicesModel : DependencyObject
	{
		public static readonly DependencyProperty StoreNumberProperty = DependencyProperty.Register("StoreNumber", typeof(string), typeof(StoreServicesModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty RunInitialSyncProperty = DependencyProperty.Register("RunInitialSync", typeof(bool), typeof(StoreServicesModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty InitialSyncTypeProperty = DependencyProperty.Register("InitialSyncType", typeof(string), typeof(StoreServicesModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty IsStandAloneProperty = DependencyProperty.Register("IsStandAlone", typeof(bool), typeof(StoreServicesModel), new FrameworkPropertyMetadata(false));

		public string StoreNumber
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(StoreNumberProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(StoreNumberProperty, value);
				});
			}
		}

		public bool RunInitialSync
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(RunInitialSyncProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(RunInitialSyncProperty, value);
				});
			}
		}

		public string InitialSyncType
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(InitialSyncTypeProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(InitialSyncTypeProperty, value);
				});
			}
		}

		public bool IsStandAlone
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(IsStandAloneProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(IsStandAloneProperty, value);
				});
			}
		}
	}

	private StoreServicesModel _model = new StoreServicesModel();

	private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

	public StoreUserControl()
	{
		InitializeComponent();
	}

	public async void LoadValues(IPreferencePage preferencePage)
	{
		await semaphoreSlim.WaitAsync();
		try
		{
			PreferencePageHeaderUserControl.SetStatusText("Loading...");
			base.DataContext = _model;
			StoreServicesModel model = _model;
			model.StoreNumber = await KioskEngineApiEngineCoreClient.GetMachineSettingValue<string>("Store", "ID", null);
			model = _model;
			model.RunInitialSync = await KioskEngineApiEngineCoreClient.GetMachineSettingValue("Store", "RunInitialSync", defaultValue: false);
			model = _model;
			model.InitialSyncType = await KioskEngineApiEngineCoreClient.GetMachineSettingValue<string>("Store", "InitialSyncType", null);
			model = _model;
			model.IsStandAlone = await KioskEngineApiEngineCoreClient.GetApplicationStateValue("IsStandAlone", defaultValue: false);
			PreferencePageHeaderUserControl.SetStatusText(null);
		}
		finally
		{
			semaphoreSlim.Release();
		}
	}

	public async Task<bool> SaveValues(IPreferencePage preferencePage)
	{
		await semaphoreSlim.WaitAsync();
		try
		{
			PreferencePageHeaderUserControl.SetStatusText("Saving...");
			int storeNumberFromString = GetStoreNumberFromString(_model.StoreNumber);
			if (storeNumberFromString != 0)
			{
				await KioskEngineApiEngineCoreClient.SetMachineSettingValue("Store", "ID", storeNumberFromString.ToString());
			}
			await KioskEngineApiEngineCoreClient.SetMachineSettingValue("Store", "RunInitialSync", _model.RunInitialSync.ToString());
			await KioskEngineApiEngineCoreClient.SetMachineSettingValue("Store", "InitialSyncType", _model.InitialSyncType);
			await KioskEngineApiEngineCoreClient.SetApplicationStateValue("IsStandAlone", _model.IsStandAlone.ToString());
			return true;
		}
		finally
		{
			PreferencePageHeaderUserControl.SetStatusText(null);
			semaphoreSlim.Release();
		}
	}

	public int GetStoreNumberFromString(string storeNumberString)
	{
		int result = 0;
		int.TryParse(storeNumberString, out result);
		return result;
	}
}
