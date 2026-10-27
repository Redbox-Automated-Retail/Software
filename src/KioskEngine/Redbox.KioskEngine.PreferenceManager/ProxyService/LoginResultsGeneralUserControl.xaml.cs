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

[PreferencePage(Name = "General", Path = "Kiosk Client Service/Proxy Service/Login Results/General", TreeOrder = 330)]
public partial class LoginResultsGeneralUserControl : UserControl, IPreferencePageHost, IComponentConnector
{
	public class ProxyServiceLoginResultsModel : DependencyObject
	{
		public static readonly DependencyProperty ErrorMessageProperty = DependencyProperty.Register("ErrorMessage", typeof(string), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty LiveCallProperty = DependencyProperty.Register("LiveCall", typeof(bool), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty LoginResponseErrorProperty = DependencyProperty.Register("LoginResponseError", typeof(LoginResponseErrorItem), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty CustomerProfileNumberProperty = DependencyProperty.Register("CustomerProfileNumber", typeof(string), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty FirstNameProperty = DependencyProperty.Register("FirstName", typeof(string), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty LoginEmailProperty = DependencyProperty.Register("LoginEmail", typeof(string), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty MobilePhoneNumberProperty = DependencyProperty.Register("MobilePhoneNumber", typeof(string), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty LoyaltyPointBalanceProperty = DependencyProperty.Register("LoyaltyPointBalance", typeof(string), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata("0"));

		public static readonly DependencyProperty LoyaltyTierProperty = DependencyProperty.Register("LoyaltyTier", typeof(LoyaltyTiers), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(LoyaltyTiers.None));

		public static readonly DependencyProperty LoyaltyTierCounterProperty = DependencyProperty.Register("LoyaltyTierCounter", typeof(string), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty LoyaltySystemOnlineProperty = DependencyProperty.Register("LoyaltySystemOnline", typeof(bool), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty LoyaltyIsEnrolledProperty = DependencyProperty.Register("LoyaltyIsEnrolled", typeof(bool), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty PointsExpiringProperty = DependencyProperty.Register("PointsExpiring", typeof(string), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty PointsExpirationDateProperty = DependencyProperty.Register("PointsExpirationDate", typeof(DateTime?), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty PromptForPerksProperty = DependencyProperty.Register("PromptForPerks", typeof(bool), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty PromptForEarlyIdProperty = DependencyProperty.Register("PromptForEarlyId", typeof(bool), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty RedboxPlusFree1NightRentalAvailableProperty = DependencyProperty.Register("RedboxPlusFree1NightRentalAvailable", typeof(bool), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty TextClubEnabledProperty = DependencyProperty.Register("TextClubEnabled", typeof(bool), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty NumberOfSampleStoredPromosProperty = DependencyProperty.Register("NumberOfSampleStoredPromos", typeof(int), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(0));

		public static readonly DependencyProperty RedboxPlusSubscriptionTierProperty = DependencyProperty.Register("RedboxPlusSubscriptionTier", typeof(string), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty RedboxPlusSubscriptionStatusProperty = DependencyProperty.Register("RedboxPlusSubscriptionStatus", typeof(string), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty RedboxPlusSubscriptionPlanIdProperty = DependencyProperty.Register("RedboxPlusSubscriptionPlanId", typeof(string), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty RedboxPlusSubscriptionEndDateProperty = DependencyProperty.Register("RedboxPlusSubscriptionEndDate", typeof(string), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty RedboxPlusSubscriptionCampaignIdProperty = DependencyProperty.Register("RedboxPlusSubscriptionCampaign", typeof(string), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty EligibleSubscriptionPlanIdsProperty = DependencyProperty.Register("EligibleSubscriptionPlanIds", typeof(string), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty TivoQueryIdProperty = DependencyProperty.Register("TivoQueryId", typeof(string), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty OfferCodeProperty = DependencyProperty.Register("OfferCode", typeof(string), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty ProductGroupIdsCheckedProperty = DependencyProperty.Register("ProductGroupIdsChecked", typeof(bool), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty TitleIdsCheckedProperty = DependencyProperty.Register("TitleIdsChecked", typeof(bool), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(true));

		public static readonly DependencyProperty RecommendedTitleTypeIdProperty = DependencyProperty.Register("RecommendedTitleTypeId", typeof(RecommendedTitlesIdType), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(RecommendedTitlesIdType.TitleID));

		public static readonly DependencyProperty IdsProperty = DependencyProperty.Register("Ids", typeof(string), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty TagsProperty = DependencyProperty.Register("Tags", typeof(string), typeof(ProxyServiceLoginResultsModel), new FrameworkPropertyMetadata(null));

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

		public bool RedboxPlusFree1NightRentalAvailable
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(RedboxPlusFree1NightRentalAvailableProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(RedboxPlusFree1NightRentalAvailableProperty, value);
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

		public int NumberOfSampleStoredPromos
		{
			get
			{
				return base.Dispatcher.Invoke(() => (int)GetValue(NumberOfSampleStoredPromosProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(NumberOfSampleStoredPromosProperty, value);
				});
			}
		}

		public string RedboxPlusSubscriptionTier
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(RedboxPlusSubscriptionTierProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(RedboxPlusSubscriptionTierProperty, value);
				});
			}
		}

		public string RedboxPlusSubscriptionPlanId
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(RedboxPlusSubscriptionPlanIdProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(RedboxPlusSubscriptionPlanIdProperty, value);
				});
			}
		}

		public string RedboxPlusSubscriptionEndDate
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(RedboxPlusSubscriptionEndDateProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(RedboxPlusSubscriptionEndDateProperty, value);
				});
			}
		}

		public string RedboxPlusSubscriptionStatus
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(RedboxPlusSubscriptionStatusProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(RedboxPlusSubscriptionStatusProperty, value);
				});
			}
		}

		public string RedboxPlusSubscriptionCampaignId
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(RedboxPlusSubscriptionCampaignIdProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(RedboxPlusSubscriptionCampaignIdProperty, value);
				});
			}
		}

		public string EligibleSubscriptionPlanIds
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(EligibleSubscriptionPlanIdsProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(EligibleSubscriptionPlanIdsProperty, value);
				});
			}
		}

		public LoginResponseErrorItem LoginResponseError
		{
			get
			{
				return base.Dispatcher.Invoke(() => (LoginResponseErrorItem)GetValue(LoginResponseErrorProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(LoginResponseErrorProperty, value);
				});
			}
		}

		public string FirstName
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(FirstNameProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(FirstNameProperty, value);
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

		public string LoginEmail
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(LoginEmailProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(LoginEmailProperty, value);
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

		public string LoyaltyPointBalance
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(LoyaltyPointBalanceProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(LoyaltyPointBalanceProperty, value);
				});
			}
		}

		public LoyaltyTiers LoyaltyTier
		{
			get
			{
				return base.Dispatcher.Invoke(() => (LoyaltyTiers)GetValue(LoyaltyTierProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(LoyaltyTierProperty, value);
				});
			}
		}

		public string LoyaltyTierCounter
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(LoyaltyTierCounterProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(LoyaltyTierCounterProperty, value);
				});
			}
		}

		public bool LoyaltyIsEnrolled
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(LoyaltyIsEnrolledProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(LoyaltyIsEnrolledProperty, value);
				});
			}
		}

		public bool LoyaltySystemOnline
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(LoyaltySystemOnlineProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(LoyaltySystemOnlineProperty, value);
				});
			}
		}

		public string PointsExpiring
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(PointsExpiringProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(PointsExpiringProperty, value);
				});
			}
		}

		public DateTime? PointsExpirationDate
		{
			get
			{
				return base.Dispatcher.Invoke(() => (DateTime?)GetValue(PointsExpirationDateProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(PointsExpirationDateProperty, value);
				});
			}
		}

		public string TivoQueryId
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(TivoQueryIdProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(TivoQueryIdProperty, value);
				});
			}
		}

		public string OfferCode
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(OfferCodeProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(OfferCodeProperty, value);
				});
			}
		}

		public bool ProductGroupIdsChecked
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(ProductGroupIdsCheckedProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(ProductGroupIdsCheckedProperty, value);
				});
			}
		}

		public bool TitleIdsChecked
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(TitleIdsCheckedProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(TitleIdsCheckedProperty, value);
				});
			}
		}

		public RecommendedTitlesIdType RecommendedTitleTypeId
		{
			get
			{
				return base.Dispatcher.Invoke(() => (RecommendedTitlesIdType)GetValue(RecommendedTitleTypeIdProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(RecommendedTitleTypeIdProperty, value);
				});
			}
		}

		public string Ids
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(IdsProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(IdsProperty, value);
				});
			}
		}

		public string Tags
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(TagsProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(TagsProperty, value);
				});
			}
		}
	}

	public class LoginResponseErrorItem
	{
		public string Code { get; set; }

		public string ResponseName { get; set; }

		public override string ToString()
		{
			return ResponseName + " " + Code;
		}
	}

	private ProxyServiceLoginResultsModel _model = new ProxyServiceLoginResultsModel();

	private TestDataUtility _testDataUtility;

	private List<TestDataReadPropertyItem> _testDataReadPropertyItems = new List<TestDataReadPropertyItem>();

	private List<LoginResponseErrorItem> _responseItems = new List<LoginResponseErrorItem>();

	private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

	public LoginResultsGeneralUserControl()
	{
		InitializeComponent();
		LoginResponseErrorItem noneResponseErrorItem = _responseItems.FirstOrDefault((LoginResponseErrorItem x) => x.Code == null);
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.ErrorCode", delegate(ReadTestDataValueResult result)
		{
			_model.LoginResponseError = _responseItems.FirstOrDefault((LoginResponseErrorItem x) => x.Code == result.GetPropertyValue<string>()) ?? noneResponseErrorItem;
		}, delegate
		{
			_model.LoginResponseError = noneResponseErrorItem;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.LiveCall", delegate(ReadTestDataValueResult result)
		{
			_model.LiveCall = result.GetPropertyValue<bool?>() == true || result.PropertyValue == null;
		}, delegate
		{
			_model.LiveCall = true;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.CustomerProfileNumber", delegate(ReadTestDataValueResult result)
		{
			_model.CustomerProfileNumber = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.CustomerProfileNumber = null;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.FirstName", delegate(ReadTestDataValueResult result)
		{
			_model.FirstName = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.FirstName = null;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.LoginEmail", delegate(ReadTestDataValueResult result)
		{
			_model.LoginEmail = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.LoginEmail = null;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.MobilePhoneNumber", delegate(ReadTestDataValueResult result)
		{
			_model.MobilePhoneNumber = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.MobilePhoneNumber = null;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.LoyaltyPointBalance", delegate(ReadTestDataValueResult result)
		{
			int result2 = 0;
			int.TryParse(result.GetPropertyValue<string>(), out result2);
			_model.LoyaltyPointBalance = result2.ToString();
		}, delegate
		{
			_model.LoyaltyPointBalance = "0";
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.LoyaltyTier", delegate(ReadTestDataValueResult result)
		{
			LoyaltyTiers result2 = LoyaltyTiers.None;
			Enum.TryParse<LoyaltyTiers>(result.GetPropertyValue<string>(), out result2);
			_model.LoyaltyTier = result2;
		}, delegate
		{
			_model.LoyaltyTier = LoyaltyTiers.None;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.LoyaltyTierCounter", delegate(ReadTestDataValueResult result)
		{
			int result2 = 0;
			int.TryParse(result.GetPropertyValue<string>(), out result2);
			_model.LoyaltyTierCounter = result2.ToString();
		}, delegate
		{
			_model.LoyaltyTierCounter = "0";
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.LoyaltyIsEnrolled", delegate(ReadTestDataValueResult result)
		{
			_model.LoyaltyIsEnrolled = result.GetPropertyValue<bool?>() == true || result.PropertyValue == null;
		}, delegate
		{
			_model.LoyaltyIsEnrolled = true;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.LoyaltySystemOnline", delegate(ReadTestDataValueResult result)
		{
			_model.LoyaltySystemOnline = result.GetPropertyValue<bool?>() == true || result.PropertyValue == null;
		}, delegate
		{
			_model.LoyaltySystemOnline = true;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.PointsExpiring", delegate(ReadTestDataValueResult result)
		{
			int result2 = 0;
			int.TryParse(result.GetPropertyValue<string>(), out result2);
			_model.PointsExpiring = result2.ToString();
		}, delegate
		{
			_model.PointsExpiring = "0";
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.PointsExpirationDate", delegate(ReadTestDataValueResult result)
		{
			DateTime? pointsExpirationDate = null;
			if (DateTime.TryParse(result.GetPropertyValue<string>(), out var result2))
			{
				pointsExpirationDate = result2;
			}
			_model.PointsExpirationDate = pointsExpirationDate;
		}, delegate
		{
			_model.PointsExpirationDate = null;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.PromptForPerks", delegate(ReadTestDataValueResult result)
		{
			bool result2 = false;
			bool.TryParse(result.GetPropertyValue<string>(), out result2);
			_model.PromptForPerks = result2;
		}, delegate
		{
			_model.PromptForPerks = false;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.PromptForEarlyId", delegate(ReadTestDataValueResult result)
		{
			bool result2 = false;
			bool.TryParse(result.GetPropertyValue<string>(), out result2);
			_model.PromptForEarlyId = result2;
		}, delegate
		{
			_model.PromptForEarlyId = false;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.TextClubEnabled", delegate(ReadTestDataValueResult result)
		{
			bool result2 = false;
			bool.TryParse(result.GetPropertyValue<string>(), out result2);
			_model.TextClubEnabled = result2;
		}, delegate
		{
			_model.TextClubEnabled = false;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.NumberOfSampleStoredPromos", delegate(ReadTestDataValueResult result)
		{
			int result2 = 0;
			int.TryParse(result.GetPropertyValue<string>(), out result2);
			_model.NumberOfSampleStoredPromos = result2;
		}, delegate
		{
			_model.NumberOfSampleStoredPromos = 0;
		}));
	}

	private bool PopulateComboBoxes()
	{
		foreach (object value in Enum.GetValues(typeof(KioskLoginResponseErrors)))
		{
			if ((KioskLoginResponseErrors)value != KioskLoginResponseErrors.LoyaltyAccountNotFound && (KioskLoginResponseErrors)value != KioskLoginResponseErrors.LoyaltyIsDown)
			{
				Type enumType = typeof(KioskLoginResponseErrors);
				object[] customAttributes = enumType.GetMember(value.ToString()).FirstOrDefault((MemberInfo m) => m.DeclaringType == enumType).GetCustomAttributes(typeof(ResponseCodeAttribute), inherit: false);
				string responseName = value.ToString();
				string code = ((customAttributes.Count() <= 0) ? null : (customAttributes[0] as ResponseCodeAttribute)?.Name);
				LoginResponseErrorItem loginResponseErrorItem = new LoginResponseErrorItem
				{
					Code = code,
					ResponseName = responseName
				};
				_responseItems.Add(loginResponseErrorItem);
				LoginResponseErrorComboBox.Items.Add(loginResponseErrorItem);
			}
		}
		foreach (object value2 in Enum.GetValues(typeof(LoyaltyTiers)))
		{
			LoyaltyTierComboBox.Items.Add(value2);
		}
		return true;
	}

	private void InitializeTestDataUtility()
	{
		_testDataUtility = new TestDataUtility
		{
			CategoryName = "ProxyServiceCustomerProfile"
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
			await _testDataUtility.WriteKCSTestDataProperty("Login.LiveCall", _model.LiveCall.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("Login.ErrorCode", _model?.LoginResponseError?.Code?.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("Login.CustomerProfileNumber", _model.CustomerProfileNumber);
			await _testDataUtility.WriteKCSTestDataProperty("Login.FirstName", _model.FirstName);
			await _testDataUtility.WriteKCSTestDataProperty("Login.LoginEmail", _model.LoginEmail);
			await _testDataUtility.WriteKCSTestDataProperty("Login.MobilePhoneNumber", _model.MobilePhoneNumber);
			int.TryParse(_model.LoyaltyPointBalance, out var _);
			await _testDataUtility.WriteKCSTestDataProperty("Login.LoyaltyPointBalance", _model.LoyaltyPointBalance?.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("Login.LoyaltyTier", _model.LoyaltyTier.ToString());
			int.TryParse(_model.LoyaltyTierCounter, out var _);
			await _testDataUtility.WriteKCSTestDataProperty("Login.LoyaltyTierCounter", _model.LoyaltyTierCounter?.ToString());
			int.TryParse(_model.PointsExpiring, out var _);
			await _testDataUtility.WriteKCSTestDataProperty("Login.LoyaltyIsEnrolled", _model.LoyaltyIsEnrolled.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("Login.LoyaltySystemOnline", _model.LoyaltySystemOnline.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("Login.PointsExpiring", _model.PointsExpiring?.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("Login.PointsExpirationDate", _model.PointsExpirationDate?.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("Login.PromptForPerks", _model.PromptForPerks.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("Login.PromptForEarlyId", _model.PromptForEarlyId.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("Login.TextClubEnabled", _model.TextClubEnabled.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("Login.NumberOfSampleStoredPromos", _model.NumberOfSampleStoredPromos.ToString());
			return true;
		}
		finally
		{
			_semaphoreSlim.Release();
		}
	}
}
