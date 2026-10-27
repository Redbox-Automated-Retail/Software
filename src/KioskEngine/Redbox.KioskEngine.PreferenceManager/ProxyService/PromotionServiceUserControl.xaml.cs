using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Newtonsoft.Json;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.PreferenceManager.KioskClientServiceClient.TestData;
using Redbox.KioskEngine.PreferenceManager.Preferences;

namespace Redbox.KioskEngine.PreferenceManager.ProxyService;

[PreferencePage(Name = "Promotion Service", Path = "Kiosk Client Service/Proxy Service/Promotion Service", TreeOrder = 370)]
public partial class PromotionServiceUserControl : UserControl, IPreferencePageHost, IComponentConnector
{
	public class ProxyServicePromotionServiceModel : DependencyObject
	{
		public static readonly DependencyProperty LiveCallProperty = DependencyProperty.Register("LiveCall", typeof(bool), typeof(ProxyServicePromotionServiceModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty CommunicationFailureProperty = DependencyProperty.Register("CommunicationFailure", typeof(bool), typeof(ProxyServicePromotionServiceModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty PromoCodeProperty = DependencyProperty.Register("PromoCode", typeof(string), typeof(ProxyServicePromotionServiceModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty AmountProperty = DependencyProperty.Register("Amount", typeof(decimal), typeof(ProxyServicePromotionServiceModel), new FrameworkPropertyMetadata(0m));

		public static readonly DependencyProperty PromotionIntentCodeProperty = DependencyProperty.Register("PromotionIntentCode", typeof(PromotionIntentCode), typeof(ProxyServicePromotionServiceModel), new FrameworkPropertyMetadata(PromotionIntentCode.FixedValue));

		public static readonly DependencyProperty PromotionValidationResponseProperty = DependencyProperty.Register("PromotionValidationResponse", typeof(PromotionValidationResponse), typeof(ProxyServicePromotionServiceModel), new FrameworkPropertyMetadata(PromotionValidationResponse.Valid));

		public static readonly DependencyProperty TestPromoCodesProperty = DependencyProperty.Register("TestPromoCodes", typeof(ObservableCollection<TestPromoCode>), typeof(ProxyServicePromotionServiceModel), new FrameworkPropertyMetadata(null));

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

		public string PromoCode
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(PromoCodeProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(PromoCodeProperty, value);
				});
			}
		}

		public decimal Amount
		{
			get
			{
				return base.Dispatcher.Invoke(() => (decimal)GetValue(AmountProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(AmountProperty, value);
				});
			}
		}

		public PromotionIntentCode PromotionIntentCode
		{
			get
			{
				return base.Dispatcher.Invoke(() => (PromotionIntentCode)GetValue(PromotionIntentCodeProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(PromotionIntentCodeProperty, value);
				});
			}
		}

		public PromotionValidationResponse PromotionValidationResponse
		{
			get
			{
				return base.Dispatcher.Invoke(() => (PromotionValidationResponse)GetValue(PromotionValidationResponseProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(PromotionValidationResponseProperty, value);
				});
			}
		}

		public ObservableCollection<TestPromoCode> TestPromoCodes
		{
			get
			{
				return base.Dispatcher.Invoke(() => (ObservableCollection<TestPromoCode>)GetValue(TestPromoCodesProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(TestPromoCodesProperty, value);
				});
			}
		}
	}

	public class TestPromoCode
	{
		public string PromoCode { get; set; }

		public decimal Amount { get; set; }

		public PromotionIntentCode PromotionIntentCode { get; set; }

		public PromotionValidationResponse PromotionValidationResponse { get; set; }
	}

	private ProxyServicePromotionServiceModel _model = new ProxyServicePromotionServiceModel();

	private TestDataUtility _testDataUtility;

	private List<TestDataReadPropertyItem> _testDataReadPropertyItems = new List<TestDataReadPropertyItem>();

	private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

	public PromotionServiceUserControl()
	{
		InitializeComponent();
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("PromotionService.LiveCall", delegate(ReadTestDataValueResult result)
		{
			_model.LiveCall = result.GetPropertyValue<bool?>() == true || result.PropertyValue == null;
		}, delegate
		{
			_model.LiveCall = true;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("PromotionService.CommunicationFailure", delegate(ReadTestDataValueResult result)
		{
			_model.CommunicationFailure = result.GetPropertyValue<bool?>() == true;
		}, delegate
		{
			_model.CommunicationFailure = false;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("PromotionService.TestPromoCodes", delegate(ReadTestDataValueResult result)
		{
			PopulateTestPromoCodeModels(result);
		}, delegate
		{
			_model.TestPromoCodes = new ObservableCollection<TestPromoCode>();
		}));
	}

	private void PopulateTestPromoCodeModels(ReadTestDataValueResult result)
	{
		new ObservableCollection<TestPromoCode>();
		ObservableCollection<TestPromoCode> testPromoCodes = new ObservableCollection<TestPromoCode>();
		string propertyValue = result.GetPropertyValue<string>();
		if (!string.IsNullOrEmpty(propertyValue))
		{
			try
			{
				testPromoCodes = JsonConvert.DeserializeObject<ObservableCollection<TestPromoCode>>(propertyValue);
			}
			catch
			{
			}
		}
		_model.TestPromoCodes = testPromoCodes;
	}

	private bool PopulateComboBoxes()
	{
		foreach (object value in Enum.GetValues(typeof(PromotionIntentCode)))
		{
			PromotionIntentCodeComboBox.Items.Add(value);
		}
		foreach (object value2 in Enum.GetValues(typeof(PromotionValidationResponse)))
		{
			PromotionValidationResponseComboBox.Items.Add(value2);
		}
		return true;
	}

	private void InitializeTestDataUtility()
	{
		_testDataUtility = new TestDataUtility
		{
			CategoryName = "ProxyServicePromotionService"
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
			await _testDataUtility.WriteKCSTestDataProperty("PromotionService.LiveCall", _model.LiveCall.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("PromotionService.CommunicationFailure", _model.CommunicationFailure.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("PromotionService.TestPromoCodes", GetJsonTestPromoCodes());
			return true;
		}
		finally
		{
			_semaphoreSlim.Release();
		}
	}

	private string GetJsonTestPromoCodes()
	{
		string result = null;
		if (_model.TestPromoCodes != null)
		{
			result = JsonConvert.SerializeObject(_model.TestPromoCodes);
		}
		return result;
	}

	private void AddTestPromoCodeButton_Click(object sender, RoutedEventArgs e)
	{
		if (string.IsNullOrEmpty(_model.PromoCode))
		{
			MessageBox.Show("Enter a Promo Code");
			return;
		}
		if (_model.TestPromoCodes.Any((TestPromoCode x) => x.PromoCode == _model.PromoCode))
		{
			MessageBox.Show("Test Promo Code " + _model.PromoCode + " already exists.");
			return;
		}
		TestPromoCode item = new TestPromoCode
		{
			PromoCode = _model.PromoCode.Trim(),
			Amount = _model.Amount,
			PromotionIntentCode = _model.PromotionIntentCode,
			PromotionValidationResponse = _model.PromotionValidationResponse
		};
		_model.TestPromoCodes.Add(item);
	}

	private void DeleteTestPromoCodeButton_Click(object sender, RoutedEventArgs e)
	{
		if (TestPromoCodesDataGrid.SelectedItem is TestPromoCode item)
		{
			_model.TestPromoCodes.Remove(item);
		}
		else
		{
			MessageBox.Show("Select a Test Promo Code to delete");
		}
	}
}
