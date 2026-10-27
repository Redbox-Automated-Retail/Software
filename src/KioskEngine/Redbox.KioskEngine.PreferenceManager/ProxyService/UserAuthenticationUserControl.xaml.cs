using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Newtonsoft.Json;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.PreferenceManager.KioskClientServiceClient.TestData;
using Redbox.KioskEngine.PreferenceManager.Preferences;
using Redbox.Rental.Model.KioskClientService.Authentication;

namespace Redbox.KioskEngine.PreferenceManager.ProxyService;

[PreferencePage(Name = "User Authentication", Path = "Kiosk Client Service/Proxy Service/User Authentication", TreeOrder = 320)]
public partial class UserAuthenticationUserControl : UserControl, IPreferencePageHost, IComponentConnector
{
	public class ProxyServiceAuthenticationResultsModel : DependencyObject
	{
		public static readonly DependencyProperty LiveCallProperty = DependencyProperty.Register("LiveCall", typeof(bool), typeof(ProxyServiceAuthenticationResultsModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty SimulateCommunicationsFailureProperty = DependencyProperty.Register("SimulateCommunicationsFailure", typeof(bool), typeof(ProxyServiceAuthenticationResultsModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty UserAuthenticationResponsesProperty = DependencyProperty.Register("UserAuthenticationResponses", typeof(UserAuthenticationResponseCollection), typeof(ProxyServiceAuthenticationResultsModel), new FrameworkPropertyMetadata(null));

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

		public bool SimulateCommunicationsFailure
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(SimulateCommunicationsFailureProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(SimulateCommunicationsFailureProperty, value);
				});
			}
		}

		public UserAuthenticationResponseCollection UserAuthenticationResponses
		{
			get
			{
				return base.Dispatcher.Invoke(() => (UserAuthenticationResponseCollection)GetValue(UserAuthenticationResponsesProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(UserAuthenticationResponsesProperty, value);
				});
			}
		}
	}

	public class AuthenticationResponseItem
	{
		public string Code { get; set; }

		public string ResponseName { get; set; }

		public override string ToString()
		{
			return ResponseName + " " + Code;
		}
	}

	public class UserAuthenticationResponseCollection : ObservableCollection<UserAuthenticationResponse>
	{
	}

	public class UserAuthenticationResponse
	{
		public string UserName { get; set; }

		public string AuthenticationResponse { get; set; }
	}

	private ProxyServiceAuthenticationResultsModel _model = new ProxyServiceAuthenticationResultsModel();

	private TestDataUtility _testDataUtility;

	private List<AuthenticationResponseItem> _responseItems = new List<AuthenticationResponseItem>();

	private List<TestDataReadPropertyItem> _testDataReadPropertyItems = new List<TestDataReadPropertyItem>();

	private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

	public UserAuthenticationUserControl()
	{
		InitializeComponent();
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Authentication.LiveCall", delegate(ReadTestDataValueResult result)
		{
			_model.LiveCall = result.GetPropertyValue<bool?>() == true || result.PropertyValue == null;
		}, delegate
		{
			_model.LiveCall = true;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Authentication.SimulateCommunicationsFailure", delegate(ReadTestDataValueResult result)
		{
			_model.SimulateCommunicationsFailure = result.GetPropertyValue<bool?>() == true;
		}, delegate
		{
			_model.SimulateCommunicationsFailure = false;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Authentication.UserAuthenticationResponses", delegate(ReadTestDataValueResult result)
		{
			_model.UserAuthenticationResponses = new UserAuthenticationResponseCollection();
			string propertyValue = result.GetPropertyValue<string>();
			if (!string.IsNullOrEmpty(propertyValue))
			{
				try
				{
					_model.UserAuthenticationResponses = JsonConvert.DeserializeObject<UserAuthenticationResponseCollection>(propertyValue);
				}
				catch
				{
				}
			}
		}, delegate
		{
			_model.UserAuthenticationResponses = new UserAuthenticationResponseCollection();
		}));
	}

	private void InitializeTestDataUtility()
	{
		_testDataUtility = new TestDataUtility
		{
			CategoryName = "ProxyServiceAuthentication"
		};
		_testDataUtility.UpdateUIErrorMessage = delegate(string errorMessage)
		{
			PreferencePageHeaderUserControl.SetStatusText(errorMessage);
		};
	}

	private bool PopulateComboBoxes()
	{
		foreach (object value in Enum.GetValues(typeof(AuthenticationResponse)))
		{
			Type enumType = typeof(AuthenticationResponse);
			object[] customAttributes = enumType.GetMember(value.ToString()).FirstOrDefault((MemberInfo m) => m.DeclaringType == enumType).GetCustomAttributes(typeof(ResponseCodeAttribute), inherit: false);
			string responseName = value.ToString();
			string code = ((customAttributes.Count() > 0) ? (customAttributes[0] as ResponseCodeAttribute)?.Name : null);
			AuthenticationResponseItem authenticationResponseItem = new AuthenticationResponseItem
			{
				Code = code,
				ResponseName = responseName
			};
			_responseItems.Add(authenticationResponseItem);
			ResponseComboBox.Items.Add(authenticationResponseItem);
		}
		return true;
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
			semaphoreSlim.Release();
		}
	}

	public async Task<bool> SaveValues(IPreferencePage preferencePage)
	{
		await semaphoreSlim.WaitAsync();
		try
		{
			PreferencePageHeaderUserControl.SetStatusText("Saving...");
			await _testDataUtility.WriteKCSTestDataProperty("Authentication.LiveCall", _model.LiveCall.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("Authentication.SimulateCommunicationsFailure", _model.SimulateCommunicationsFailure.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("Authentication.UserAuthenticationResponses", JsonConvert.SerializeObject(_model.UserAuthenticationResponses));
			return true;
		}
		finally
		{
			semaphoreSlim.Release();
		}
	}

	private void AddButton_Click(object sender, RoutedEventArgs e)
	{
		AuthenticationResponseItem authenticationResponseItem = ResponseComboBox.SelectedItem as AuthenticationResponseItem;
		if (!string.IsNullOrEmpty(UserNameTextBox.Text) && authenticationResponseItem != null)
		{
			_model.UserAuthenticationResponses.Add(new UserAuthenticationResponse
			{
				UserName = UserNameTextBox.Text,
				AuthenticationResponse = authenticationResponseItem.ResponseName
			});
			UserNameTextBox.Text = null;
			ResponseComboBox.SelectedItem = null;
		}
		else
		{
			MessageBox.Show("Enter a User Name and select a Response to add a User to the list.");
		}
	}

	private void DeleteButton_Click(object sender, RoutedEventArgs e)
	{
		if (UserAuthenticationResponsesGrid.SelectedItem is UserAuthenticationResponse item)
		{
			_model.UserAuthenticationResponses.Remove(item);
		}
		else
		{
			MessageBox.Show("Select a user from the list to delete.");
		}
	}
}
