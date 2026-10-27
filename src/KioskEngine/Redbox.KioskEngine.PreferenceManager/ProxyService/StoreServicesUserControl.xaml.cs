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
using Redbox.KioskEngine.PreferenceManager.Preferences;

namespace Redbox.KioskEngine.PreferenceManager.ProxyService;

[PreferencePage(Name = "Store Services", Path = "Kiosk Client Service/Proxy Service/Store Services", TreeOrder = 310)]
public partial class StoreServicesUserControl : UserControl, IPreferencePageHost, IComponentConnector
{
	private class ProxyServiceStoreServicesModel : DependencyObject
	{
		public static readonly DependencyProperty LiveCallProperty = DependencyProperty.Register("LiveCall", typeof(bool), typeof(ProxyServiceStoreServicesModel), new FrameworkPropertyMetadata(true));

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
	}

	private ProxyServiceStoreServicesModel _model = new ProxyServiceStoreServicesModel();

	private TestDataUtility _testDataUtility;

	private List<TestDataReadPropertyItem> _testDataReadPropertyItems = new List<TestDataReadPropertyItem>();

	private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

	public StoreServicesUserControl()
	{
		InitializeComponent();
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("StoreServices.LiveCall", delegate(ReadTestDataValueResult result)
		{
			_model.LiveCall = result.GetPropertyValue<bool?>() == true || result.PropertyValue == null;
		}, delegate
		{
			_model.LiveCall = true;
		}));
	}

	private void InitializeTestDataUtility()
	{
		_testDataUtility = new TestDataUtility
		{
			CategoryName = "ProxyServiceStoreServices"
		};
		_testDataUtility.UpdateUIErrorMessage = delegate(string errorMessage)
		{
			PreferencePageHeaderUserControl.SetStatusText(errorMessage);
		};
	}

	public async void LoadValues(IPreferencePage preferencePage)
	{
		await semaphoreSlim.WaitAsync();
		try
		{
			PreferencePageHeaderUserControl.SetStatusText("Loading...");
			if (_testDataUtility == null)
			{
				InitializeTestDataUtility();
			}
			_testDataUtility.ReadDataSucceeded = false;
			base.DataContext = _model;
			await _testDataUtility.ReadKCSTestDataProperties(_testDataReadPropertyItems);
			_testDataUtility.ProcessReadActions(_testDataReadPropertyItems);
			if (_testDataUtility.ReadDataSucceeded)
			{
				PreferencePageHeaderUserControl.SetStatusText(null);
			}
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
			await _testDataUtility.WriteKCSTestDataProperty("StoreServices.LiveCall", _model.LiveCall.ToString());
			return true;
		}
		finally
		{
			semaphoreSlim.Release();
		}
	}
}
