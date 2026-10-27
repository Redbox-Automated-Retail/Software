using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.PreferenceManager.KioskClientServiceClient.TestData;
using Redbox.KioskEngine.PreferenceManager.Preferences;
using Redbox.Rental.Model.Personalization;

namespace Redbox.KioskEngine.PreferenceManager.ProxyService;

[PreferencePage(Name = "Personalization Service", Path = "Kiosk Client Service/Proxy Service/Personalization Service", TreeOrder = 350)]
public partial class PersonalizationServiceUserControl : UserControl, IPreferencePageHost, IComponentConnector
{
	private class ProxyServicePersonalizationServiceModel : DependencyObject
	{
		public static readonly DependencyProperty LiveCallProperty = DependencyProperty.Register("LiveCall", typeof(bool), typeof(ProxyServicePersonalizationServiceModel), new FrameworkPropertyMetadata(true));

		public static readonly DependencyProperty UpdateAccountErrorProperty = DependencyProperty.Register("UpdateAccountError", typeof(UpdateAccountErrorItem), typeof(ProxyServicePersonalizationServiceModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty CustomerProfileNumberProperty = DependencyProperty.Register("CustomerProfileNumber", typeof(string), typeof(ProxyServicePersonalizationServiceModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty MobilePhoneNumberProperty = DependencyProperty.Register("MobilePhoneNumber", typeof(string), typeof(ProxyServicePersonalizationServiceModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty TempPasswordProperty = DependencyProperty.Register("TempPassword", typeof(string), typeof(ProxyServicePersonalizationServiceModel), new FrameworkPropertyMetadata(null));

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

		public UpdateAccountErrorItem UpdateAccountError
		{
			get
			{
				return base.Dispatcher.Invoke(() => (UpdateAccountErrorItem)GetValue(UpdateAccountErrorProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(UpdateAccountErrorProperty, value);
				});
			}
		}

		public string CustomerProfileNumber
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(CustomerProfileNumberProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(CustomerProfileNumberProperty, value);
				});
			}
		}

		public string MobilePhoneNumber
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(MobilePhoneNumberProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(MobilePhoneNumberProperty, value);
				});
			}
		}

		public string TempPassword
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(TempPasswordProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(TempPasswordProperty, value);
				});
			}
		}
	}

	public class UpdateAccountErrorItem
	{
		public string Code { get; set; }

		public string ResponseName { get; set; }

		public override string ToString()
		{
			return ResponseName + " " + Code;
		}
	}

	private ProxyServicePersonalizationServiceModel _model = new ProxyServicePersonalizationServiceModel();

	private TestDataUtility _testDataUtility;

	private List<TestDataReadPropertyItem> _testDataReadPropertyItems = new List<TestDataReadPropertyItem>();

	private List<UpdateAccountErrorItem> _updateAccountErrorItems = new List<UpdateAccountErrorItem>();

	private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

	private UpdateAccountErrorItem DefaultUpdateAccountErrorItem => _updateAccountErrorItems.FirstOrDefault((UpdateAccountErrorItem x) => x.ResponseName == UpdateAccountResponseErrors.None.ToString());

	public PersonalizationServiceUserControl()
	{
		InitializeComponent();
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("PersonalizationService.LiveCall", delegate(ReadTestDataValueResult result)
		{
			_model.LiveCall = result.GetPropertyValue<bool?>() == true || result.PropertyValue == null;
		}, delegate
		{
			_model.LiveCall = true;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("PersonalizationService.UpdateAccountError", delegate(ReadTestDataValueResult result)
		{
			_model.UpdateAccountError = _updateAccountErrorItems.FirstOrDefault((UpdateAccountErrorItem x) => x.Code == result.GetPropertyValue<string>()) ?? DefaultUpdateAccountErrorItem;
		}, delegate
		{
			_model.UpdateAccountError = DefaultUpdateAccountErrorItem;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("PersonalizationService.CustomerProfileNumber", delegate(ReadTestDataValueResult result)
		{
			_model.CustomerProfileNumber = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.CustomerProfileNumber = null;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("PersonalizationService.MobilePhoneNumber", delegate(ReadTestDataValueResult result)
		{
			_model.MobilePhoneNumber = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.MobilePhoneNumber = null;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("PersonalizationService.TempPassword", delegate(ReadTestDataValueResult result)
		{
			_model.TempPassword = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.TempPassword = null;
		}));
	}

	private void InitializeTestDataUtility()
	{
		_testDataUtility = new TestDataUtility
		{
			CategoryName = "ProxyServicePersonalizationService"
		};
		_testDataUtility.UpdateUIErrorMessage = delegate(string errorMessage)
		{
			PreferencePageHeaderUserControl.SetStatusText(errorMessage);
		};
	}

	private bool PopulateComboBoxes()
	{
		foreach (object value in Enum.GetValues(typeof(UpdateAccountResponseErrors)))
		{
			Type enumType = typeof(UpdateAccountResponseErrors);
			object[] customAttributes = enumType.GetMember(value.ToString()).FirstOrDefault((MemberInfo m) => m.DeclaringType == enumType).GetCustomAttributes(typeof(ResponseCodeAttribute), inherit: false);
			string responseName = value.ToString();
			string code = ((customAttributes.Count() > 0) ? (customAttributes[0] as ResponseCodeAttribute)?.Name : null);
			UpdateAccountErrorItem updateAccountErrorItem = new UpdateAccountErrorItem
			{
				Code = code,
				ResponseName = responseName
			};
			_updateAccountErrorItems.Add(updateAccountErrorItem);
			UpdateAccountErrorComboBox.Items.Add(updateAccountErrorItem);
		}
		return true;
	}

	public async void LoadValues(IPreferencePage preferencePage)
	{
		await _semaphoreSlim.WaitAsync();
		try
		{
			PreferencePageHeaderUserControl.SetStatusText("Loading...");
			if (_testDataUtility == null)
			{
				InitializeTestDataUtility();
			}
			if (PopulateComboBoxes())
			{
				_testDataUtility.ReadDataSucceeded = false;
				base.DataContext = _model;
				await _testDataUtility.ReadKCSTestDataProperties(_testDataReadPropertyItems);
				_testDataUtility.ProcessReadActions(_testDataReadPropertyItems);
				if (_testDataUtility.ReadDataSucceeded)
				{
					PreferencePageHeaderUserControl.SetStatusText(null);
				}
			}
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
			await _testDataUtility.WriteKCSTestDataProperty("PersonalizationService.LiveCall", _model.LiveCall.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("PersonalizationService.UpdateAccountError", _model.UpdateAccountError?.Code);
			await _testDataUtility.WriteKCSTestDataProperty("PersonalizationService.CustomerProfileNumber", _model.CustomerProfileNumber);
			await _testDataUtility.WriteKCSTestDataProperty("PersonalizationService.MobilePhoneNumber", _model.MobilePhoneNumber);
			await _testDataUtility.WriteKCSTestDataProperty("PersonalizationService.TempPassword", _model.TempPassword);
			return true;
		}
		finally
		{
			_semaphoreSlim.Release();
		}
	}
}
