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
using Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient.ShoppingCart;
using Redbox.KioskEngine.PreferenceManager.Preferences;
using Redbox.Rental.Model.Loyalty;

namespace Redbox.KioskEngine.PreferenceManager.ProxyService;

[PreferencePage(Name = "Loyalty Service", Path = "Kiosk Client Service/Proxy Service/Loyalty Service", TreeOrder = 340)]
public partial class LoyaltyServiceUserControl : UserControl, IPreferencePageHost, IComponentConnector
{
	private class ProxyServiceLoyaltyServiceModel : DependencyObject
	{
		public static readonly DependencyProperty LiveCallProperty = DependencyProperty.Register("LiveCall", typeof(bool), typeof(ProxyServiceLoyaltyServiceModel), new FrameworkPropertyMetadata(true));

		public static readonly DependencyProperty SimulateAccrualSuccessProperty = DependencyProperty.Register("SimulateAccrualSuccess", typeof(bool), typeof(ProxyServiceLoyaltyServiceModel), new FrameworkPropertyMetadata(true));

		public static readonly DependencyProperty SimulateRedemptionSuccessProperty = DependencyProperty.Register("SimulateRedemptionSuccess", typeof(bool), typeof(ProxyServiceLoyaltyServiceModel), new FrameworkPropertyMetadata(true));

		public static readonly DependencyProperty AccrualErrorProperty = DependencyProperty.Register("AccrualError", typeof(AccrualErrorItem), typeof(ProxyServiceLoyaltyServiceModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty RedemptionErrorProperty = DependencyProperty.Register("RedemptionError", typeof(RedemptionErrorItem), typeof(ProxyServiceLoyaltyServiceModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty DefaultAccrualValueProperty = DependencyProperty.Register("DefaultAccrualValue", typeof(int?), typeof(ProxyServiceLoyaltyServiceModel), new FrameworkPropertyMetadata(1000));

		public static readonly DependencyProperty DefaultRedemptionValueProperty = DependencyProperty.Register("DefaultRedemptionValue", typeof(int?), typeof(ProxyServiceLoyaltyServiceModel), new FrameworkPropertyMetadata(2000));

		public static readonly DependencyProperty ItemModelsProperty = DependencyProperty.Register("ItemModels", typeof(ObservableCollection<LoyaltyTestAccrualRedemptionModel>), typeof(ProxyServiceLoyaltyServiceModel), new FrameworkPropertyMetadata(null));

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

		public bool SimulateAccrualSuccess
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(SimulateAccrualSuccessProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(SimulateAccrualSuccessProperty, value);
				});
			}
		}

		public bool SimulateRedemptionSuccess
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(SimulateRedemptionSuccessProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(SimulateRedemptionSuccessProperty, value);
				});
			}
		}

		public AccrualErrorItem AccrualError
		{
			get
			{
				return base.Dispatcher.Invoke(() => (AccrualErrorItem)GetValue(AccrualErrorProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(AccrualErrorProperty, value);
				});
			}
		}

		public RedemptionErrorItem RedemptionError
		{
			get
			{
				return base.Dispatcher.Invoke(() => (RedemptionErrorItem)GetValue(RedemptionErrorProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(RedemptionErrorProperty, value);
				});
			}
		}

		public int? DefaultAccrualValue
		{
			get
			{
				return base.Dispatcher.Invoke(() => (int?)GetValue(DefaultAccrualValueProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(DefaultAccrualValueProperty, value);
				});
			}
		}

		public int? DefaultRedemptionValue
		{
			get
			{
				return base.Dispatcher.Invoke(() => (int?)GetValue(DefaultRedemptionValueProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(DefaultRedemptionValueProperty, value);
				});
			}
		}

		public ObservableCollection<LoyaltyTestAccrualRedemptionModel> ItemModels
		{
			get
			{
				return base.Dispatcher.Invoke(() => (ObservableCollection<LoyaltyTestAccrualRedemptionModel>)GetValue(ItemModelsProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(ItemModelsProperty, value);
				});
			}
		}
	}

	public class LoyaltyTestAccrualRedemptionModel : DependencyObject
	{
		public static readonly DependencyProperty ProductIdProperty = DependencyProperty.Register("ProductId", typeof(long), typeof(LoyaltyTestAccrualRedemptionModel), new FrameworkPropertyMetadata(0L));

		public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(LoyaltyTestAccrualRedemptionModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty AccrualPointsProperty = DependencyProperty.Register("AccrualPoints", typeof(int), typeof(LoyaltyTestAccrualRedemptionModel), new FrameworkPropertyMetadata(0));

		public static readonly DependencyProperty RedemptionPointsProperty = DependencyProperty.Register("RedemptionPoints", typeof(int), typeof(LoyaltyTestAccrualRedemptionModel), new FrameworkPropertyMetadata(0));

		public long ProductId
		{
			get
			{
				return base.Dispatcher.Invoke(() => (long)GetValue(ProductIdProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(ProductIdProperty, value);
				});
			}
		}

		public string Name
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(NameProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(NameProperty, value);
				});
			}
		}

		public int AccrualPoints
		{
			get
			{
				return base.Dispatcher.Invoke(() => (int)GetValue(AccrualPointsProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(AccrualPointsProperty, value);
				});
			}
		}

		public int RedemptionPoints
		{
			get
			{
				return base.Dispatcher.Invoke(() => (int)GetValue(RedemptionPointsProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(RedemptionPointsProperty, value);
				});
			}
		}
	}

	public class LoyaltyTestAccrualRedemption
	{
		public long ProductId { get; set; }

		public string Name { get; set; }

		public int AccrualPoints { get; set; }

		public int RedemptionPoints { get; set; }
	}

	public class AccrualErrorItem
	{
		public string Code { get; set; }

		public string ResponseName { get; set; }

		public override string ToString()
		{
			return ResponseName + " " + Code;
		}
	}

	public class RedemptionErrorItem
	{
		public string Code { get; set; }

		public string ResponseName { get; set; }

		public override string ToString()
		{
			return ResponseName + " " + Code;
		}
	}

	private ProxyServiceLoyaltyServiceModel _model = new ProxyServiceLoyaltyServiceModel();

	private TestDataUtility _testDataUtility;

	private List<TestDataReadPropertyItem> _testDataReadPropertyItems = new List<TestDataReadPropertyItem>();

	private List<AccrualErrorItem> _accrualErrorItems = new List<AccrualErrorItem>();

	private List<RedemptionErrorItem> _redemptionErrorItems = new List<RedemptionErrorItem>();

	private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

	private AccrualErrorItem DefaultAccrualErrorItem => _accrualErrorItems.FirstOrDefault((AccrualErrorItem x) => x.Code == EstimateAccrualErrors.Error.ToString());

	private RedemptionErrorItem DefaultRedemptionErrorItem => _redemptionErrorItems.FirstOrDefault((RedemptionErrorItem x) => x.Code == EstimateRedemptionErrors.Error.ToString());

	public LoyaltyServiceUserControl()
	{
		InitializeComponent();
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("LoyaltyService.LiveCall", delegate(ReadTestDataValueResult result)
		{
			_model.LiveCall = result.GetPropertyValue<bool?>() == true || result.PropertyValue == null;
		}, delegate
		{
			_model.LiveCall = true;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("LoyaltyService.SimulateAccrualSuccess", delegate(ReadTestDataValueResult result)
		{
			_model.SimulateAccrualSuccess = result.GetPropertyValue<bool?>() == true || result.PropertyValue == null;
		}, delegate
		{
			_model.SimulateAccrualSuccess = true;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("LoyaltyService.AccrualError", delegate(ReadTestDataValueResult result)
		{
			_model.AccrualError = _accrualErrorItems.FirstOrDefault((AccrualErrorItem x) => x.Code == result.GetPropertyValue<string>()) ?? DefaultAccrualErrorItem;
		}, delegate
		{
			_model.AccrualError = DefaultAccrualErrorItem;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("LoyaltyService.SimulateRedemptionSuccess", delegate(ReadTestDataValueResult result)
		{
			_model.SimulateRedemptionSuccess = result.GetPropertyValue<bool?>() == true || result.PropertyValue == null;
		}, delegate
		{
			_model.SimulateRedemptionSuccess = true;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("LoyaltyService.RedemptionError", delegate(ReadTestDataValueResult result)
		{
			_model.RedemptionError = _redemptionErrorItems.FirstOrDefault((RedemptionErrorItem x) => x.Code == result.GetPropertyValue<string>()) ?? DefaultRedemptionErrorItem;
		}, delegate
		{
			_model.RedemptionError = DefaultRedemptionErrorItem;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("LoyaltyService.DefaultAccrualValue", delegate(ReadTestDataValueResult result)
		{
			_model.DefaultAccrualValue = result.GetPropertyValue<int?>() ?? 1000;
		}, delegate
		{
			_model.DefaultAccrualValue = 1000;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("LoyaltyService.DefaultRedemptionValue", delegate(ReadTestDataValueResult result)
		{
			_model.DefaultRedemptionValue = result.GetPropertyValue<int?>() ?? 1000;
		}, delegate
		{
			_model.DefaultRedemptionValue = 2000;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("LoyaltyService.ShoppingCartItems", async delegate(ReadTestDataValueResult result)
		{
			await PopulateItemModels(result);
		}, delegate
		{
			_model.ItemModels = new ObservableCollection<LoyaltyTestAccrualRedemptionModel>();
		}));
	}

	private async Task PopulateItemModels(ReadTestDataValueResult result)
	{
		ObservableCollection<LoyaltyTestAccrualRedemptionModel> itemModels = new ObservableCollection<LoyaltyTestAccrualRedemptionModel>();
		List<LoyaltyTestAccrualRedemption> savedLoyaltyTestAccrualRedemptions = new List<LoyaltyTestAccrualRedemption>();
		string propertyValue = result.GetPropertyValue<string>();
		if (!string.IsNullOrEmpty(propertyValue))
		{
			try
			{
				savedLoyaltyTestAccrualRedemptions = JsonConvert.DeserializeObject<List<LoyaltyTestAccrualRedemption>>(propertyValue);
			}
			catch
			{
			}
		}
		List<LoyaltyTestShoppingCartItem> list = await KioskEngineApiShoppingCartClient.GetLoyaltyTestShoppingCartItems();
		if (list != null)
		{
			int num = _model.DefaultAccrualValue ?? 1000;
			int num2 = _model.DefaultRedemptionValue ?? 1000;
			foreach (LoyaltyTestShoppingCartItem item in list)
			{
				LoyaltyTestAccrualRedemptionModel loyaltyTestAccrualRedemptionModel = new LoyaltyTestAccrualRedemptionModel
				{
					ProductId = item.ProductId,
					Name = item.Name
				};
				LoyaltyTestAccrualRedemption loyaltyTestAccrualRedemption = savedLoyaltyTestAccrualRedemptions?.FirstOrDefault((LoyaltyTestAccrualRedemption x) => x.ProductId == loyaltyTestAccrualRedemptionModel.ProductId);
				loyaltyTestAccrualRedemptionModel.AccrualPoints = loyaltyTestAccrualRedemption?.AccrualPoints ?? num;
				loyaltyTestAccrualRedemptionModel.RedemptionPoints = loyaltyTestAccrualRedemption?.RedemptionPoints ?? num2;
				itemModels.Add(loyaltyTestAccrualRedemptionModel);
			}
		}
		_model.ItemModels = itemModels;
	}

	private bool PopulateComboBoxes()
	{
		foreach (object value in Enum.GetValues(typeof(EstimateAccrualErrors)))
		{
			Type enumType = typeof(EstimateAccrualErrors);
			object[] customAttributes = enumType.GetMember(value.ToString()).FirstOrDefault((MemberInfo m) => m.DeclaringType == enumType).GetCustomAttributes(typeof(ResponseCodeAttribute), inherit: false);
			string responseName = value.ToString();
			string code = ((customAttributes.Count() > 0) ? (customAttributes[0] as ResponseCodeAttribute)?.Name : null);
			AccrualErrorItem accrualErrorItem = new AccrualErrorItem
			{
				Code = code,
				ResponseName = responseName
			};
			_accrualErrorItems.Add(accrualErrorItem);
			AccrualErrorComboBox.Items.Add(accrualErrorItem);
		}
		foreach (object value2 in Enum.GetValues(typeof(EstimateRedemptionErrors)))
		{
			Type enumType2 = typeof(EstimateRedemptionErrors);
			object[] customAttributes2 = enumType2.GetMember(value2.ToString()).FirstOrDefault((MemberInfo m) => m.DeclaringType == enumType2).GetCustomAttributes(typeof(ResponseCodeAttribute), inherit: false);
			string responseName2 = value2.ToString();
			string code2 = ((customAttributes2.Count() > 0) ? (customAttributes2[0] as ResponseCodeAttribute)?.Name : null);
			RedemptionErrorItem redemptionErrorItem = new RedemptionErrorItem
			{
				Code = code2,
				ResponseName = responseName2
			};
			_redemptionErrorItems.Add(redemptionErrorItem);
			RedemptionErrorComboBox.Items.Add(redemptionErrorItem);
		}
		return true;
	}

	private void InitializeTestDataUtility()
	{
		_testDataUtility = new TestDataUtility
		{
			CategoryName = "ProxyServiceLoyalytService"
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
			await _testDataUtility.WriteKCSTestDataProperty("LoyaltyService.LiveCall", _model.LiveCall.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("LoyaltyService.SimulateAccrualSuccess", _model.SimulateAccrualSuccess.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("LoyaltyService.AccrualError", _model.AccrualError.Code);
			await _testDataUtility.WriteKCSTestDataProperty("LoyaltyService.SimulateRedemptionSuccess", _model.SimulateRedemptionSuccess.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("LoyaltyService.RedemptionError", _model.RedemptionError.Code);
			await _testDataUtility.WriteKCSTestDataProperty("LoyaltyService.DefaultAccrualValue", _model.DefaultAccrualValue.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("LoyaltyService.DefaultRedemptionValue", _model.DefaultRedemptionValue.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("LoyaltyService.ShoppingCartItems", GetJsonLoyaltyTestAccrualRedemptions());
			return true;
		}
		finally
		{
			semaphoreSlim.Release();
		}
	}

	private string GetJsonLoyaltyTestAccrualRedemptions()
	{
		string result = null;
		List<LoyaltyTestAccrualRedemption> loyaltyTestAccrualRedemptions = GetLoyaltyTestAccrualRedemptions();
		if (loyaltyTestAccrualRedemptions != null)
		{
			result = JsonConvert.SerializeObject(loyaltyTestAccrualRedemptions);
		}
		return result;
	}

	private List<LoyaltyTestAccrualRedemption> GetLoyaltyTestAccrualRedemptions()
	{
		List<LoyaltyTestAccrualRedemption> list = null;
		if (_model?.ItemModels?.Any() == true)
		{
			list = new List<LoyaltyTestAccrualRedemption>();
			foreach (LoyaltyTestAccrualRedemptionModel itemModel in _model.ItemModels)
			{
				list.Add(new LoyaltyTestAccrualRedemption
				{
					ProductId = itemModel.ProductId,
					Name = itemModel.Name,
					AccrualPoints = itemModel.AccrualPoints,
					RedemptionPoints = itemModel.RedemptionPoints
				});
			}
		}
		return list;
	}
}
