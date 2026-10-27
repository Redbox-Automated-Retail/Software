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
using Redbox.Rental.Model.Personalization;

namespace Redbox.KioskEngine.PreferenceManager.ProxyService;

[PreferencePage(Name = "Marketing Service", Path = "Kiosk Client Service/Proxy Service/Marketinng Service", TreeOrder = 360)]
public partial class MarketingServiceUserControl : UserControl, IPreferencePageHost, IComponentConnector
{
	public class MerchantServiceModel : DependencyObject
	{
		public static readonly DependencyProperty LiveCallProperty = DependencyProperty.Register("LiveCall", typeof(bool), typeof(MerchantServiceModel), new FrameworkPropertyMetadata(true));

		public static readonly DependencyProperty CommunicationFailureProperty = DependencyProperty.Register("CommunicationFailure", typeof(bool), typeof(MerchantServiceModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty ConfirmationStatusProperty = DependencyProperty.Register("ConfirmationStatus", typeof(ConfirmationStatus), typeof(MerchantServiceModel), new FrameworkPropertyMetadata(ConfirmationStatus.DontShow));

		public static readonly DependencyProperty CustomerProfileNumberProperty = DependencyProperty.Register("CustomerProfileNumber", typeof(string), typeof(MerchantServiceModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty MobilePhoneNumberProperty = DependencyProperty.Register("MobilePhoneNumber", typeof(string), typeof(MerchantServiceModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty EmailVerifiedProperty = DependencyProperty.Register("EmailVerified", typeof(bool?), typeof(MerchantServiceModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty PromptForPerksProperty = DependencyProperty.Register("PromptForPerks", typeof(bool), typeof(MerchantServiceModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty PromptForEarlyIdProperty = DependencyProperty.Register("PromptForEarlyId", typeof(bool), typeof(MerchantServiceModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty TextClubEnabledProperty = DependencyProperty.Register("TextClubEnabled", typeof(bool), typeof(MerchantServiceModel), new FrameworkPropertyMetadata(false));

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

		public bool CommunicationFailure
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(CommunicationFailureProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(CommunicationFailureProperty, value);
				});
			}
		}

		public ConfirmationStatus ConfirmationStatus
		{
			get
			{
				return base.Dispatcher.Invoke(() => (ConfirmationStatus)GetValue(ConfirmationStatusProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(ConfirmationStatusProperty, value);
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

		public bool? EmailVerified
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool?)GetValue(EmailVerifiedProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(EmailVerifiedProperty, value);
				});
			}
		}

		public bool PromptForPerks
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(PromptForPerksProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(PromptForPerksProperty, value);
				});
			}
		}

		public bool PromptForEarlyId
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(PromptForEarlyIdProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(PromptForEarlyIdProperty, value);
				});
			}
		}

		public bool TextClubEnabled
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(TextClubEnabledProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(TextClubEnabledProperty, value);
				});
			}
		}
	}

	private MerchantServiceModel _model = new MerchantServiceModel();

	private TestDataUtility _testDataUtility;

	private List<TestDataReadPropertyItem> _testDataReadPropertyItems = new List<TestDataReadPropertyItem>();

	private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

	public MarketingServiceUserControl()
	{
		InitializeComponent();
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("MarketingService.LiveCall", delegate(ReadTestDataValueResult result)
		{
			_model.LiveCall = result.GetPropertyValue<bool?>() == true || result.PropertyValue == null;
		}, delegate
		{
			_model.LiveCall = true;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("MarketingService.CommunicationFailure", delegate(ReadTestDataValueResult result)
		{
			_model.CommunicationFailure = result.GetPropertyValue<bool?>() == true;
		}, delegate
		{
			_model.CommunicationFailure = false;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("MarketingService.ConfirmationStatus", delegate(ReadTestDataValueResult result)
		{
			ConfirmationStatus result2 = ConfirmationStatus.DontShow;
			Enum.TryParse<ConfirmationStatus>(result.GetPropertyValue<string>(), out result2);
			_model.ConfirmationStatus = result2;
		}, delegate
		{
			_model.ConfirmationStatus = ConfirmationStatus.DontShow;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("MarketingService.CustomerProfileNumber", delegate(ReadTestDataValueResult result)
		{
			_model.CustomerProfileNumber = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.CustomerProfileNumber = null;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("MarketingService.MobilePhoneNumber", delegate(ReadTestDataValueResult result)
		{
			_model.MobilePhoneNumber = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.MobilePhoneNumber = null;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("MarketingService.EmailVerified", delegate(ReadTestDataValueResult result)
		{
			_model.EmailVerified = result.GetPropertyValue<bool?>();
		}, delegate
		{
			_model.EmailVerified = null;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("MarketingService.PromptForPerks", delegate(ReadTestDataValueResult result)
		{
			_model.PromptForPerks = result.GetPropertyValue<bool?>() == true;
		}, delegate
		{
			_model.PromptForPerks = false;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("MarketingService.PromptForEarlyId", delegate(ReadTestDataValueResult result)
		{
			_model.PromptForEarlyId = result.GetPropertyValue<bool?>() == true;
		}, delegate
		{
			_model.PromptForEarlyId = false;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("MarketingService.TextClubEnabled", delegate(ReadTestDataValueResult result)
		{
			_model.TextClubEnabled = result.GetPropertyValue<bool?>() == true;
		}, delegate
		{
			_model.TextClubEnabled = false;
		}));
	}

	private bool PopulateComboBoxes()
	{
		List<ConfirmationStatus> list = new List<ConfirmationStatus>();
		foreach (object value in Enum.GetValues(typeof(ConfirmationStatus)))
		{
			if (!list.Contains((ConfirmationStatus)value))
			{
				list.Add((ConfirmationStatus)value);
			}
		}
		list.ForEach(delegate(ConfirmationStatus x)
		{
			ConfirmationStatusComboBox.Items.Add(x);
		});
		return true;
	}

	private void InitializeTestDataUtility()
	{
		_testDataUtility = new TestDataUtility
		{
			CategoryName = "ProxyServiceMarketingService"
		};
		_testDataUtility.UpdateUIErrorMessage = delegate(string errorMessage)
		{
			PreferencePageHeaderUserControl.SetStatusText(errorMessage);
		};
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
			await _testDataUtility.WriteKCSTestDataProperty("MarketingService.LiveCall", _model.LiveCall.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("MarketingService.CommunicationFailure", _model.CommunicationFailure.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("MarketingService.ConfirmationStatus", _model.ConfirmationStatus.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("MarketingService.CustomerProfileNumber", _model.CustomerProfileNumber);
			await _testDataUtility.WriteKCSTestDataProperty("MarketingService.MobilePhoneNumber", _model.MobilePhoneNumber);
			await _testDataUtility.WriteKCSTestDataProperty("MarketingService.EmailVerified", _model.EmailVerified?.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("MarketingService.PromptForPerks", _model.PromptForPerks.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("MarketingService.PromptForEarlyId", _model.PromptForEarlyId.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("MarketingService.TextClubEnabled", _model.TextClubEnabled.ToString());
			return true;
		}
		finally
		{
			_semaphoreSlim.Release();
		}
	}
}
