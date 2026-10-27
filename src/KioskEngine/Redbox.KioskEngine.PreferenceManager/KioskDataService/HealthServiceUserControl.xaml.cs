using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.KioskEngine.PreferenceManager.KioskClientServiceClient.TestData;
using Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient;
using Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient.Health;
using Redbox.KioskEngine.PreferenceManager.Preferences;

namespace Redbox.KioskEngine.PreferenceManager.KioskDataService;

[PreferencePage(Name = "Health Service", Path = "Kiosk Data Service/Health Service", TreeOrder = 510)]
public partial class HealthServiceUserControl : UserControl, IPreferencePageHost, IComponentConnector
{
	private class KioskDataServiceHealthServiceModel : DependencyObject
	{
		public static readonly DependencyProperty LiveCallProperty = DependencyProperty.Register("LiveCall", typeof(bool), typeof(KioskDataServiceHealthServiceModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty PingPeriodProperty = DependencyProperty.Register("PingPeriod", typeof(int), typeof(KioskDataServiceHealthServiceModel), new FrameworkPropertyMetadata(300000));

		public static readonly DependencyProperty SmtpServerProperty = DependencyProperty.Register("SmtpServer", typeof(string), typeof(KioskDataServiceHealthServiceModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty RecipientsProperty = DependencyProperty.Register("Recipients", typeof(string), typeof(KioskDataServiceHealthServiceModel), new FrameworkPropertyMetadata(null));

		public bool LiveCall
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(LiveCallProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(LiveCallProperty, value);
				});
			}
		}

		public int PingPeriod
		{
			get
			{
				return base.Dispatcher.Invoke(() => (int)GetValue(PingPeriodProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(PingPeriodProperty, value);
				});
			}
		}

		public string SmtpServer
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(SmtpServerProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(SmtpServerProperty, value);
				});
			}
		}

		public string Recipients
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(RecipientsProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(RecipientsProperty, value);
				});
			}
		}
	}

	private KioskDataServiceHealthServiceModel _model = new KioskDataServiceHealthServiceModel();

	private List<TestDataReadPropertyItem> _testDataReadPropertyItems = new List<TestDataReadPropertyItem>();

	private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

	public HealthServiceUserControl()
	{
		InitializeComponent();
	}

	public async void LoadValues(IPreferencePage preferencePage)
	{
		await _semaphoreSlim.WaitAsync();
		try
		{
			PreferencePageHeaderUserControl.SetStatusText("Loading...");
			base.DataContext = _model;
			KioskDataServiceHealthServiceModel model = _model;
			model.LiveCall = await KioskEngineApiEngineCoreClient.GetMachineSettingValue("Remote Services\\Health Services", "LiveCall", defaultValue: true);
			model = _model;
			model.PingPeriod = await KioskEngineApiEngineCoreClient.GetMachineSettingValue("Remote Services\\Health Services", "PingPeriod", 300000);
			model = _model;
			model.SmtpServer = await KioskEngineApiEngineCoreClient.GetMachineSettingValue<string>("Remote Services\\Health Services", "SmtpServer", null);
			model = _model;
			model.Recipients = await KioskEngineApiEngineCoreClient.GetMachineSettingValue<string>("Remote Services\\Health Services", "Recipients", null);
			PreferencePageHeaderUserControl.SetStatusText(null);
		}
		finally
		{
			_semaphoreSlim.Release();
		}
	}

	public async Task<bool> SaveValues(IPreferencePage preferencePage)
	{
		await _semaphoreSlim.WaitAsync();
		try
		{
			PreferencePageHeaderUserControl.SetStatusText("Saving...");
			await KioskEngineApiEngineCoreClient.SetMachineSettingValue("Remote Services\\Health Services", "LiveCall", _model.LiveCall.ToString());
			await KioskEngineApiEngineCoreClient.SetMachineSettingValue("Remote Services\\Health Services", "PingPeriod", _model.PingPeriod.ToString());
			await KioskEngineApiEngineCoreClient.SetMachineSettingValue("Remote Services\\Health Services", "SmtpServer", _model.SmtpServer);
			await KioskEngineApiEngineCoreClient.SetMachineSettingValue("Remote Services\\Health Services", "Recipients", _model.Recipients);
			return true;
		}
		finally
		{
			PreferencePageHeaderUserControl.SetStatusText(null);
			_semaphoreSlim.Release();
		}
	}

	private async void PingButton_Click(object sender, RoutedEventArgs e)
	{
		await KioskEngineApiHealthClient.SendPing();
	}

	private async void TestKioskAlertButton_Click(object sender, RoutedEventArgs e)
	{
		await KioskEngineApiHealthClient.SendAlert();
	}
}
