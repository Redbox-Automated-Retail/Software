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
using Redbox.Rental.Model.Transaction;

namespace Redbox.KioskEngine.PreferenceManager.TransactionService;

[PreferencePage(Name = "Authorize Results", Path = "Kiosk Client Service/Transaction Service/Authorize Results", TreeOrder = 410)]
public partial class AuthorizeResultUserControl : UserControl, IPreferencePageHost, IComponentConnector
{
	public class TransactionServiceAuthorizeResultsModel : DependencyObject
	{
		public static readonly DependencyProperty LiveCallProperty = DependencyProperty.Register("LiveCall", typeof(bool), typeof(TransactionServiceAuthorizeResultsModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty SimulateCommunicationFailureProperty = DependencyProperty.Register("SimulateCommunicationFailure", typeof(bool), typeof(TransactionServiceAuthorizeResultsModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty AuthorizeResponseErrorProperty = DependencyProperty.Register("AuthorizeResponseError", typeof(AuthorizeResponseErrorItem), typeof(TransactionServiceAuthorizeResultsModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty HasCustomerProfileProperty = DependencyProperty.Register("HasCustomerProfile", typeof(bool), typeof(TransactionServiceAuthorizeResultsModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty AccountNumberProperty = DependencyProperty.Register("AccountNumber", typeof(string), typeof(TransactionServiceAuthorizeResultsModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty CustomerProfileNumberProperty = DependencyProperty.Register("CustomerProfileNumber", typeof(string), typeof(TransactionServiceAuthorizeResultsModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty CustomerProfileFirstNameProperty = DependencyProperty.Register("CustomerProfileFirstName", typeof(string), typeof(TransactionServiceAuthorizeResultsModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty CustomerProfileLastNameProperty = DependencyProperty.Register("CustomerProfileLastName", typeof(string), typeof(TransactionServiceAuthorizeResultsModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty EmailProperty = DependencyProperty.Register("Email", typeof(string), typeof(TransactionServiceAuthorizeResultsModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty ReferenceNumberProperty = DependencyProperty.Register("ReferenceNumber", typeof(string), typeof(TransactionServiceAuthorizeResultsModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty ConfirmationStatusItemProperty = DependencyProperty.Register("ConfirmationStatusItem", typeof(ConfirmationStatusItem), typeof(TransactionServiceAuthorizeResultsModel), new FrameworkPropertyMetadata(null));

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

		public AuthorizeResponseErrorItem AuthorizeResponseError
		{
			get
			{
				return base.Dispatcher.Invoke(() => (AuthorizeResponseErrorItem)GetValue(AuthorizeResponseErrorProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(AuthorizeResponseErrorProperty, value);
				});
			}
		}

		public bool HasCustomerProfile
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(HasCustomerProfileProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(HasCustomerProfileProperty, value);
				});
			}
		}

		public string AccountNumber
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(AccountNumberProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(AccountNumberProperty, value);
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

		public string CustomerProfileFirstName
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(CustomerProfileFirstNameProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(CustomerProfileFirstNameProperty, value);
				});
			}
		}

		public string CustomerProfileLastName
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(CustomerProfileLastNameProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(CustomerProfileLastNameProperty, value);
				});
			}
		}

		public string Email
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(EmailProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(EmailProperty, value);
				});
			}
		}

		public string ReferenceNumber
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(ReferenceNumberProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(ReferenceNumberProperty, value);
				});
			}
		}

		public ConfirmationStatusItem ConfirmationStatusItem
		{
			get
			{
				return base.Dispatcher.Invoke(() => (ConfirmationStatusItem)GetValue(ConfirmationStatusItemProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(ConfirmationStatusItemProperty, value);
				});
			}
		}
	}

	public class AuthorizeResponseErrorItem
	{
		public string Code { get; set; }

		public string ResponseName { get; set; }

		public override string ToString()
		{
			return ResponseName + " " + Code;
		}
	}

	public class ConfirmationStatusItem
	{
		public ConfirmationStatus ConfirmationStatus { get; set; }

		public override string ToString()
		{
			return ConfirmationStatus.ToString();
		}
	}

	private TransactionServiceAuthorizeResultsModel _model = new TransactionServiceAuthorizeResultsModel();

	private TestDataUtility _testDataUtility;

	private List<TestDataReadPropertyItem> _testDataReadPropertyItems = new List<TestDataReadPropertyItem>();

	private List<AuthorizeResponseErrorItem> _responseItems = new List<AuthorizeResponseErrorItem>();

	private List<ConfirmationStatusItem> _confirmationStatusItems = new List<ConfirmationStatusItem>();

	private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

	private AuthorizeResponseErrorItem ApprovedResponseErrorItem => _responseItems.FirstOrDefault((AuthorizeResponseErrorItem x) => x.Code == null);

	private ConfirmationStatusItem DoNotShowConfirmationStatusItem => _confirmationStatusItems.FirstOrDefault((ConfirmationStatusItem x) => x.ConfirmationStatus == ConfirmationStatus.DontShow);

	public AuthorizeResultUserControl()
	{
		InitializeComponent();
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Authorize.ErrorCode", delegate(ReadTestDataValueResult result)
		{
			_model.AuthorizeResponseError = _responseItems.FirstOrDefault((AuthorizeResponseErrorItem x) => x.Code == result.GetPropertyValue<string>()) ?? ApprovedResponseErrorItem;
		}, delegate
		{
			_model.AuthorizeResponseError = ApprovedResponseErrorItem;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Authorize.LiveCall", delegate(ReadTestDataValueResult result)
		{
			_model.LiveCall = result.GetPropertyValue<bool?>() == true || result.PropertyValue == null;
		}, delegate
		{
			_model.LiveCall = true;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Authorize.SimulateCommunicationFailure", delegate(ReadTestDataValueResult result)
		{
			_model.SimulateCommunicationFailure = result.GetPropertyValue<bool?>() == true;
		}, delegate
		{
			_model.SimulateCommunicationFailure = true;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Authorize.HasCustomerProfile", delegate(ReadTestDataValueResult result)
		{
			_model.HasCustomerProfile = result.GetPropertyValue<bool?>() == true;
		}, delegate
		{
			_model.HasCustomerProfile = false;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Authorize.AccountNumber", delegate(ReadTestDataValueResult result)
		{
			_model.AccountNumber = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.AccountNumber = null;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Authorize.CustomerProfileNumber", delegate(ReadTestDataValueResult result)
		{
			_model.CustomerProfileNumber = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.CustomerProfileNumber = null;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Authorize.CustomerProfileFirstName", delegate(ReadTestDataValueResult result)
		{
			_model.CustomerProfileFirstName = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.CustomerProfileFirstName = null;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Authorize.CustomerProfileLastName", delegate(ReadTestDataValueResult result)
		{
			_model.CustomerProfileLastName = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.CustomerProfileLastName = null;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Authorize.Email", delegate(ReadTestDataValueResult result)
		{
			_model.Email = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.Email = null;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Authorize.ReferenceNumber", delegate(ReadTestDataValueResult result)
		{
			_model.ReferenceNumber = result.GetPropertyValue<long?>()?.ToString();
		}, delegate
		{
			_model.ReferenceNumber = "0";
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Authorize.ConfirmationStatus", delegate(ReadTestDataValueResult result)
		{
			_model.ConfirmationStatusItem = _confirmationStatusItems.FirstOrDefault((ConfirmationStatusItem x) => x.ConfirmationStatus.ToString() == result.GetPropertyValue<string>()) ?? DoNotShowConfirmationStatusItem;
		}, delegate
		{
			_model.ConfirmationStatusItem = DoNotShowConfirmationStatusItem;
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
			AuthorizeResponseErrorItem authorizeResponseErrorItem = new AuthorizeResponseErrorItem
			{
				Code = code,
				ResponseName = responseName
			};
			_responseItems.Add(authorizeResponseErrorItem);
			AuthorizeResponseErrorComboBox.Items.Add(authorizeResponseErrorItem);
		}
		ConfirmationStatusComboBox.Items.Clear();
		_confirmationStatusItems.Clear();
		foreach (object eachConfirmationStatus in Enum.GetValues(typeof(ConfirmationStatus)))
		{
			if (_confirmationStatusItems.FirstOrDefault((ConfirmationStatusItem x) => x.ConfirmationStatus == (ConfirmationStatus)eachConfirmationStatus) == null)
			{
				ConfirmationStatusItem confirmationStatusItem = new ConfirmationStatusItem
				{
					ConfirmationStatus = (ConfirmationStatus)eachConfirmationStatus
				};
				_confirmationStatusItems.Add(confirmationStatusItem);
				ConfirmationStatusComboBox.Items.Add(confirmationStatusItem);
			}
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
			await _testDataUtility.WriteKCSTestDataProperty("Authorize.LiveCall", _model.LiveCall.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("Authorize.SimulateCommunicationFailure", _model.SimulateCommunicationFailure.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("Authorize.ErrorCode", _model?.AuthorizeResponseError?.Code?.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("Authorize.HasCustomerProfile", _model.HasCustomerProfile.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("Authorize.AccountNumber", _model.AccountNumber);
			await _testDataUtility.WriteKCSTestDataProperty("Authorize.CustomerProfileNumber", _model.CustomerProfileNumber);
			await _testDataUtility.WriteKCSTestDataProperty("Authorize.CustomerProfileFirstName", _model.CustomerProfileFirstName);
			await _testDataUtility.WriteKCSTestDataProperty("Authorize.CustomerProfileLastName", _model.CustomerProfileLastName);
			await _testDataUtility.WriteKCSTestDataProperty("Authorize.Email", _model.Email);
			await _testDataUtility.WriteKCSTestDataProperty("Authorize.ReferenceNumber", _model.ReferenceNumber);
			await _testDataUtility.WriteKCSTestDataProperty("Authorize.ConfirmationStatus", _model.ConfirmationStatusItem?.ConfirmationStatus.ToString());
			return true;
		}
		finally
		{
			_semaphoreSlim.Release();
		}
	}
}
