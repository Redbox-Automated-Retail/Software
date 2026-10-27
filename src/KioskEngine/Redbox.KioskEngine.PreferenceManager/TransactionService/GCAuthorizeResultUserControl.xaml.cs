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
using Redbox.Rental.Model.Transaction;

namespace Redbox.KioskEngine.PreferenceManager.TransactionService;

[PreferencePage(Name = "Gift Card Authorize Results", Path = "Kiosk Client Service/Transaction Service/Gift Card Authorize Results", TreeOrder = 415)]
public partial class GCAuthorizeResultUserControl : UserControl, IPreferencePageHost, IComponentConnector
{
	public class TransactionServiceGiftCardAuthorizeResultsModel : DependencyObject
	{
		public static readonly DependencyProperty LiveCallProperty = DependencyProperty.Register("LiveCall", typeof(bool), typeof(TransactionServiceGiftCardAuthorizeResultsModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty SimulateCommunicationFailureProperty = DependencyProperty.Register("SimulateCommunicationFailure", typeof(bool), typeof(TransactionServiceGiftCardAuthorizeResultsModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty AuthorizeResponseErrorProperty = DependencyProperty.Register("AuthorizeResponseError", typeof(AuthorizeResultUserControl.AuthorizeResponseErrorItem), typeof(TransactionServiceGiftCardAuthorizeResultsModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty ExistsProperty = DependencyProperty.Register("Exists", typeof(bool), typeof(TransactionServiceGiftCardAuthorizeResultsModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty GiftCardStatusItemProperty = DependencyProperty.Register("GiftCardStatusItem", typeof(GiftCardStatusItem), typeof(TransactionServiceGiftCardAuthorizeResultsModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty BalanceProperty = DependencyProperty.Register("Balance", typeof(string), typeof(TransactionServiceGiftCardAuthorizeResultsModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty MaxDiscsProperty = DependencyProperty.Register("MaxDiscs", typeof(string), typeof(TransactionServiceGiftCardAuthorizeResultsModel), new FrameworkPropertyMetadata(null));

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

		public bool SimulateCommunicationFailure
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(SimulateCommunicationFailureProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(SimulateCommunicationFailureProperty, value);
				});
			}
		}

		public AuthorizeResultUserControl.AuthorizeResponseErrorItem AuthorizeResponseError
		{
			get
			{
				return base.Dispatcher.Invoke(() => (AuthorizeResultUserControl.AuthorizeResponseErrorItem)GetValue(AuthorizeResponseErrorProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(AuthorizeResponseErrorProperty, value);
				});
			}
		}

		public bool Exists
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(ExistsProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(ExistsProperty, value);
				});
			}
		}

		public GiftCardStatusItem GiftCardStatusItem
		{
			get
			{
				return base.Dispatcher.Invoke(() => (GiftCardStatusItem)GetValue(GiftCardStatusItemProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(GiftCardStatusItemProperty, value);
				});
			}
		}

		public string Balance
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(BalanceProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(BalanceProperty, value);
				});
			}
		}

		public string MaxDiscs
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(MaxDiscsProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(MaxDiscsProperty, value);
				});
			}
		}
	}

	public class GiftCardStatusItem
	{
		public GiftCardStatus GiftCardStatus { get; set; }

		public override string ToString()
		{
			return GiftCardStatus.ToString();
		}
	}

	private TransactionServiceGiftCardAuthorizeResultsModel _model = new TransactionServiceGiftCardAuthorizeResultsModel();

	private TestDataUtility _testDataUtility;

	private List<TestDataReadPropertyItem> _testDataReadPropertyItems = new List<TestDataReadPropertyItem>();

	private List<AuthorizeResultUserControl.AuthorizeResponseErrorItem> _responseItems = new List<AuthorizeResultUserControl.AuthorizeResponseErrorItem>();

	private List<GiftCardStatusItem> _giftCardStatusItems = new List<GiftCardStatusItem>();

	private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

	private AuthorizeResultUserControl.AuthorizeResponseErrorItem ApprovedResponseErrorItem => _responseItems.FirstOrDefault((AuthorizeResultUserControl.AuthorizeResponseErrorItem x) => x.Code == null);

	private GiftCardStatusItem RegisteredGiftCardStatusItem => _giftCardStatusItems.FirstOrDefault((GiftCardStatusItem x) => x.GiftCardStatus == GiftCardStatus.Registered);

	public GCAuthorizeResultUserControl()
	{
		InitializeComponent();
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("GCAuth.ErrorCode", delegate(ReadTestDataValueResult result)
		{
			_model.AuthorizeResponseError = _responseItems.FirstOrDefault((AuthorizeResultUserControl.AuthorizeResponseErrorItem x) => x.Code == result.GetPropertyValue<string>()) ?? ApprovedResponseErrorItem;
		}, delegate
		{
			_model.AuthorizeResponseError = ApprovedResponseErrorItem;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("GCAuth.LiveCall", delegate(ReadTestDataValueResult result)
		{
			_model.LiveCall = result.GetPropertyValue<bool?>() == true || result.PropertyValue == null;
		}, delegate
		{
			_model.LiveCall = true;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("GCAuth.SimulateCommunicationFailure", delegate(ReadTestDataValueResult result)
		{
			_model.SimulateCommunicationFailure = result.GetPropertyValue<bool?>() == true;
		}, delegate
		{
			_model.SimulateCommunicationFailure = true;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("GCAuth.Exists", delegate(ReadTestDataValueResult result)
		{
			_model.Exists = result.GetPropertyValue<bool?>() == true || result.PropertyValue == null;
		}, delegate
		{
			_model.Exists = true;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("GCAuth.GCStatusType", delegate(ReadTestDataValueResult result)
		{
			_model.GiftCardStatusItem = _giftCardStatusItems.FirstOrDefault((GiftCardStatusItem x) => x.GiftCardStatus.ToString() == result.GetPropertyValue<string>()) ?? RegisteredGiftCardStatusItem;
		}, delegate
		{
			_model.GiftCardStatusItem = RegisteredGiftCardStatusItem;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("GCAuth.Balance", delegate(ReadTestDataValueResult result)
		{
			_model.Balance = result.GetPropertyValue<long?>()?.ToString() ?? "20";
		}, delegate
		{
			_model.Balance = "20";
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("GCAuth.MaxDiscs", delegate(ReadTestDataValueResult result)
		{
			_model.MaxDiscs = result.GetPropertyValue<long?>()?.ToString() ?? "2";
		}, delegate
		{
			_model.MaxDiscs = "2";
		}));
	}

	private bool PopulateComboBoxes()
	{
		foreach (object value in Enum.GetValues(typeof(AuthorizeResponse)))
		{
			Type enumType = typeof(AuthorizeResponse);
			object[] customAttributes = enumType.GetMember(value.ToString()).FirstOrDefault((MemberInfo m) => m.DeclaringType == enumType).GetCustomAttributes(typeof(ResponseCodeAttribute), inherit: false);
			string responseName = value.ToString();
			string code = ((customAttributes.Count() > 0) ? (customAttributes[0] as ResponseCodeAttribute)?.Name : null);
			AuthorizeResultUserControl.AuthorizeResponseErrorItem authorizeResponseErrorItem = new AuthorizeResultUserControl.AuthorizeResponseErrorItem
			{
				Code = code,
				ResponseName = responseName
			};
			_responseItems.Add(authorizeResponseErrorItem);
			AuthorizeResponseErrorComboBox.Items.Add(authorizeResponseErrorItem);
		}
		GiftCardStatusComboBox.Items.Clear();
		_giftCardStatusItems.Clear();
		foreach (object value2 in Enum.GetValues(typeof(GiftCardStatus)))
		{
			GiftCardStatusItem giftCardStatusItem = new GiftCardStatusItem
			{
				GiftCardStatus = (GiftCardStatus)value2
			};
			_giftCardStatusItems.Add(giftCardStatusItem);
			GiftCardStatusComboBox.Items.Add(giftCardStatusItem);
		}
		return true;
	}

	private void InitializeTestDataUtility()
	{
		_testDataUtility = new TestDataUtility
		{
			CategoryName = "TransactionService"
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
				_model = new TransactionServiceGiftCardAuthorizeResultsModel();
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
			await _testDataUtility.WriteKCSTestDataProperty("GCAuth.LiveCall", _model.LiveCall.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("GCAuth.SimulateCommunicationFailure", _model.SimulateCommunicationFailure.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("GCAuth.ErrorCode", _model?.AuthorizeResponseError?.Code?.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("GCAuth.Exists", _model?.Exists.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("GCAuth.GCStatusType", _model?.GiftCardStatusItem?.GiftCardStatus.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("GCAuth.Balance", _model?.Balance);
			await _testDataUtility.WriteKCSTestDataProperty("GCAuth.MaxDiscs", _model?.MaxDiscs);
			return true;
		}
		finally
		{
			_semaphoreSlim.Release();
		}
	}
}
