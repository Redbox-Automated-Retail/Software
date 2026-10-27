using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.KioskEngine.PreferenceManager.KioskClientServiceClient.TestData;
using Redbox.KioskEngine.PreferenceManager.Preferences;
using Redbox.Rental.Model.KioskClientService.CustomerProfile;
using Redbox.Rental.Model.KioskClientService.Subscriptions;

namespace Redbox.KioskEngine.PreferenceManager.ProxyService;

[PreferencePage(Name = "Subscriptions", Path = "Kiosk Client Service/Proxy Service/Login Results/Subscriptions", TreeOrder = 334)]
public partial class LoginResultsSubscriptionUserControl : UserControl, IPreferencePageHost, IComponentConnector
{
	private LoginResultsGeneralUserControl.ProxyServiceLoginResultsModel _model = new LoginResultsGeneralUserControl.ProxyServiceLoginResultsModel();

	private TestDataUtility _testDataUtility;

	private List<TestDataReadPropertyItem> _testDataReadPropertyItems = new List<TestDataReadPropertyItem>();

	private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

	public LoginResultsSubscriptionUserControl()
	{
		InitializeComponent();
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.EligibleSubscriptionPlanIds", delegate(ReadTestDataValueResult result)
		{
			_model.EligibleSubscriptionPlanIds = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.EligibleSubscriptionPlanIds = null;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.RedboxPlusSubscriptionPlanId", delegate(ReadTestDataValueResult result)
		{
			_model.RedboxPlusSubscriptionPlanId = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.RedboxPlusSubscriptionPlanId = null;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.RedboxPlusSubscriptionCampaignId", delegate(ReadTestDataValueResult result)
		{
			_model.RedboxPlusSubscriptionCampaignId = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.RedboxPlusSubscriptionCampaignId = null;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.RedboxPlusSubscriptionTier", delegate(ReadTestDataValueResult result)
		{
			_model.RedboxPlusSubscriptionTier = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.RedboxPlusSubscriptionTier = null;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.RedboxPlusSubscriptionStatus", delegate(ReadTestDataValueResult result)
		{
			_model.RedboxPlusSubscriptionStatus = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.RedboxPlusSubscriptionStatus = null;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.RedboxPlusSubscriptionEndDate", delegate(ReadTestDataValueResult result)
		{
			_model.RedboxPlusSubscriptionEndDate = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.RedboxPlusSubscriptionEndDate = null;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.RedboxPlusFree1NightRentalAvailable", delegate(ReadTestDataValueResult result)
		{
			_model.RedboxPlusFree1NightRentalAvailable = result.GetPropertyValue<bool?>() == true;
		}, delegate
		{
			_model.RedboxPlusFree1NightRentalAvailable = false;
		}));
	}

	private bool PopulateComboBoxes()
	{
		foreach (object value in Enum.GetValues(typeof(RedboxPlusSubscriptionTier)))
		{
			RedboxPlusSubscriptionTierComboBox.Items.Add(value?.ToString());
		}
		foreach (object value2 in Enum.GetValues(typeof(CustomerSubscriptionPlanStatus)))
		{
			RedboxPlusSubscriptionStatusComboBox.Items.Add(value2?.ToString());
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
			await _testDataUtility.WriteKCSTestDataProperty("Login.EligibleSubscriptionPlanIds", _model.EligibleSubscriptionPlanIds);
			await _testDataUtility.WriteKCSTestDataProperty("Login.RedboxPlusSubscriptionPlanId", _model.RedboxPlusSubscriptionPlanId);
			await _testDataUtility.WriteKCSTestDataProperty("Login.RedboxPlusSubscriptionCampaignId", _model.RedboxPlusSubscriptionCampaignId);
			await _testDataUtility.WriteKCSTestDataProperty("Login.RedboxPlusSubscriptionTier", _model.RedboxPlusSubscriptionTier);
			await _testDataUtility.WriteKCSTestDataProperty("Login.RedboxPlusSubscriptionStatus", _model.RedboxPlusSubscriptionStatus);
			string propertyValue = null;
			if (DateTime.TryParse(_model.RedboxPlusSubscriptionEndDate, out var result))
			{
				propertyValue = result.ToString("MM/dd/yyyy");
			}
			await _testDataUtility.WriteKCSTestDataProperty("Login.RedboxPlusSubscriptionEndDate", propertyValue);
			await _testDataUtility.WriteKCSTestDataProperty("Login.RedboxPlusFree1NightRentalAvailable", _model.RedboxPlusFree1NightRentalAvailable.ToString());
			return true;
		}
		finally
		{
			semaphoreSlim.Release();
		}
	}
}
