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
using Redbox.Rental.Model.Personalization;

namespace Redbox.KioskEngine.PreferenceManager.ProxyService;

[PreferencePage(Name = "Recommended Titles", Path = "Kiosk Client Service/Proxy Service/Login Results/Recommended Titles", TreeOrder = 332)]
public partial class LoginResultRecommendedTitlesUserControl : UserControl, IPreferencePageHost, IComponentConnector
{
	private LoginResultsGeneralUserControl.ProxyServiceLoginResultsModel _model = new LoginResultsGeneralUserControl.ProxyServiceLoginResultsModel();

	private TestDataUtility _testDataUtility;

	private List<TestDataReadPropertyItem> _testDataReadPropertyItems = new List<TestDataReadPropertyItem>();

	private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

	public LoginResultRecommendedTitlesUserControl()
	{
		InitializeComponent();
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.TivoQueryId", delegate(ReadTestDataValueResult result)
		{
			_model.TivoQueryId = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.TivoQueryId = null;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.OfferCode", delegate(ReadTestDataValueResult result)
		{
			_model.OfferCode = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.OfferCode = null;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.RecommendedTitleIdType", delegate(ReadTestDataValueResult result)
		{
			RecommendedTitlesIdType recommendedTitleTypeId = RecommendedTitlesIdType.TitleID;
			if (Enum.TryParse<RecommendedTitlesIdType>(result.GetPropertyValue<string>(), out var result2))
			{
				recommendedTitleTypeId = result2;
			}
			_model.RecommendedTitleTypeId = recommendedTitleTypeId;
		}, delegate
		{
			_model.RecommendedTitleTypeId = RecommendedTitlesIdType.TitleID;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.RecommendedIds", delegate(ReadTestDataValueResult result)
		{
			_model.Ids = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.Ids = null;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("Login.RecommendedTags", delegate(ReadTestDataValueResult result)
		{
			string text = result.GetPropertyValue<string>();
			if (string.IsNullOrEmpty(text))
			{
				text = "[wishlist] [recommended,wishlist]";
			}
			_model.Tags = text;
		}, delegate
		{
			_model.Tags = "[wishlist] [recommended,wishlist]";
		}));
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
			await _testDataUtility.WriteKCSTestDataProperty("Login.TivoQueryId", _model.TivoQueryId);
			await _testDataUtility.WriteKCSTestDataProperty("Login.OfferCode", _model.OfferCode);
			await _testDataUtility.WriteKCSTestDataProperty("Login.RecommendedTitleIdType", _model.RecommendedTitleTypeId.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("Login.RecommendedIds", _model.Ids);
			await _testDataUtility.WriteKCSTestDataProperty("Login.RecommendedTags", (_model.Tags == "[wishlist] [recommended,wishlist]") ? null : _model.Tags);
			return true;
		}
		finally
		{
			semaphoreSlim.Release();
		}
	}
}
