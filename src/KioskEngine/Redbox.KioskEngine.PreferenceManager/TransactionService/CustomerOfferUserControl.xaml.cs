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
using Redbox.Rental.Model.KioskClientService.Transactions;

namespace Redbox.KioskEngine.PreferenceManager.TransactionService;

[PreferencePage(Name = "After Swipe Customer Offers", Path = "Kiosk Client Service/Transaction Service/After Swipe Customer Offers", TreeOrder = 420)]
public partial class CustomerOfferUserControl : UserControl, IPreferencePageHost, IComponentConnector
{
	public class TransactionServiceAfterSwipeCustomerOffersModel : DependencyObject
	{
		public static readonly DependencyProperty LiveCallProperty = DependencyProperty.Register("LiveCall", typeof(bool), typeof(TransactionServiceAfterSwipeCustomerOffersModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty SuccessProperty = DependencyProperty.Register("Success", typeof(bool), typeof(TransactionServiceAfterSwipeCustomerOffersModel), new FrameworkPropertyMetadata(true));

		public static readonly DependencyProperty SelectedOfferProperty = DependencyProperty.Register("SelectedOffer", typeof(KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers), typeof(TransactionServiceAfterSwipeCustomerOffersModel), new FrameworkPropertyMetadata(KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers.NoOffer));

		public static readonly DependencyProperty IncludeMovieInCartOfferProperty = DependencyProperty.Register("IncludeMovieInCartOffer", typeof(bool), typeof(TransactionServiceAfterSwipeCustomerOffersModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty IncludeFreeGameInCartOfferProperty = DependencyProperty.Register("IncludeFreeGameInCartOffer", typeof(bool), typeof(TransactionServiceAfterSwipeCustomerOffersModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty IncludeAmountOffPurchasaeInCartOfferProperty = DependencyProperty.Register("IncludeAmountOffPurchaseInCartOffer", typeof(bool), typeof(TransactionServiceAfterSwipeCustomerOffersModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty IncludeMultiNightPriceInCartOfferProperty = DependencyProperty.Register("IncludeMultiNightPriceInCartOffer", typeof(bool), typeof(TransactionServiceAfterSwipeCustomerOffersModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty MultiNightPriceProductIdsProperty = DependencyProperty.Register("MultiNightPriceProductIds", typeof(string), typeof(TransactionServiceAfterSwipeCustomerOffersModel), new FrameworkPropertyMetadata(null));

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

		public bool Success
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(SuccessProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(SuccessProperty, value);
				});
			}
		}

		public KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers SelectedOffer
		{
			get
			{
				return base.Dispatcher.Invoke(() => (KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers)GetValue(SelectedOfferProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(SelectedOfferProperty, value);
				});
			}
		}

		public bool IncludeMovieInCartOffer
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(IncludeMovieInCartOfferProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(IncludeMovieInCartOfferProperty, value);
				});
			}
		}

		public bool IncludeFreeGameInCartOffer
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(IncludeFreeGameInCartOfferProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(IncludeFreeGameInCartOfferProperty, value);
				});
			}
		}

		public bool IncludeAmountOffPurchaseInCartOffer
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(IncludeAmountOffPurchasaeInCartOfferProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(IncludeAmountOffPurchasaeInCartOfferProperty, value);
				});
			}
		}

		public bool IncludeMultiNightPriceInCartOffer
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(IncludeMultiNightPriceInCartOfferProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(IncludeMultiNightPriceInCartOfferProperty, value);
				});
			}
		}

		public string MultiNightPriceProductIds
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(MultiNightPriceProductIdsProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(MultiNightPriceProductIdsProperty, value);
				});
			}
		}
	}

	private TransactionServiceAfterSwipeCustomerOffersModel _model = new TransactionServiceAfterSwipeCustomerOffersModel();

	private TestDataUtility _testDataUtility;

	private List<TestDataReadPropertyItem> _testDataReadPropertyItems = new List<TestDataReadPropertyItem>();

	private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

	public CustomerOfferUserControl()
	{
		InitializeComponent();
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("AfterSwipeCustomerOffers.LiveCall", delegate(ReadTestDataValueResult result)
		{
			_model.LiveCall = result.GetPropertyValue<bool?>() == true || result.PropertyValue == null;
		}, delegate
		{
			_model.LiveCall = true;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("AfterSwipeCustomerOffers.Success", delegate(ReadTestDataValueResult result)
		{
			_model.Success = result.GetPropertyValue<bool?>() == true || result.PropertyValue == null;
		}, delegate
		{
			_model.Success = true;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("AfterSwipeCustomerOffers.SelectedOffer", delegate(ReadTestDataValueResult result)
		{
			_model.SelectedOffer = RadioOffersToEnumConverter.ToOffer(result.PropertyValue);
		}, delegate
		{
			_model.SelectedOffer = KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers.NoOffer;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("AfterSwipeCustomerOffers.IncludeMovieInCartOffer", delegate(ReadTestDataValueResult result)
		{
			_model.IncludeMovieInCartOffer = result.GetPropertyValue<bool?>() == true;
		}, delegate
		{
			_model.IncludeMovieInCartOffer = false;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("AfterSwipeCustomerOffers.IncludeFreeGameInCartOffer", delegate(ReadTestDataValueResult result)
		{
			_model.IncludeFreeGameInCartOffer = result.GetPropertyValue<bool?>() == true;
		}, delegate
		{
			_model.IncludeFreeGameInCartOffer = false;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("AfterSwipeCustomerOffers.IncludeAmountOffPurchaseInCartOffer", delegate(ReadTestDataValueResult result)
		{
			_model.IncludeAmountOffPurchaseInCartOffer = result.GetPropertyValue<bool?>() == true;
		}, delegate
		{
			_model.IncludeAmountOffPurchaseInCartOffer = false;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("AfterSwipeCustomerOffers.IncludeMultiNightPriceInCartOffer", delegate(ReadTestDataValueResult result)
		{
			_model.IncludeMultiNightPriceInCartOffer = result.GetPropertyValue<bool?>() == true;
		}, delegate
		{
			_model.IncludeMultiNightPriceInCartOffer = false;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("AfterSwipeCustomerOffers.MultiNightPriceProductIds", delegate(ReadTestDataValueResult result)
		{
			_model.MultiNightPriceProductIds = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.MultiNightPriceProductIds = null;
		}));
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
			_testDataUtility.ReadDataSucceeded = false;
			base.DataContext = _model;
			await _testDataUtility.ReadKCSTestDataProperties(_testDataReadPropertyItems);
			_testDataUtility.ProcessReadActions(_testDataReadPropertyItems);
			PopulateRadioButtons();
			if (_testDataUtility.ReadDataSucceeded)
			{
				PreferencePageHeaderUserControl.SetStatusText(null);
			}
		}
		finally
		{
			_semaphoreSlim.Release();
		}
	}

	private void PopulateRadioButtons()
	{
		NoOfferRadioButton.IsChecked = IsOfferValueSet(KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers.NoOffer);
		PresentInCartOffersRadioButton.IsChecked = IsOfferValueSet(KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers.InCartOffers);
		PresentFracFreeMovieFirstVisitRadioButton.IsChecked = IsOfferValueSet(KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers.FracFreeMovieFirstVisit);
		PresentFracFreeMovieSecondVisitRadioButton.IsChecked = IsOfferValueSet(KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers.FracFreeMovieSecondVisit);
		PresentFracDiscountedDiscFirstVisitRadioButton.IsChecked = IsOfferValueSet(KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers.FracDiscountedDiscFirstVisit);
		PresentFracDiscountedDiscSecondVisitRadioButton.IsChecked = IsOfferValueSet(KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers.FracDiscountedDiscSecondVisit);
		PresentFreeItemPerksEquivalentRadioButton.IsChecked = IsOfferValueSet(KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers.FreeItemPerksEquivalent);
		PresentAmountOffPerksEquivalentRadioButton.IsChecked = IsOfferValueSet(KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers.AmountOffPerksEquivalent);
		PresentExtraPointsPerksEquivalentRadioButton.IsChecked = IsOfferValueSet(KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers.ExtraPointsPerksEquivalent);
	}

	public async Task<bool> SaveValues(IPreferencePage preferencePage)
	{
		await _semaphoreSlim.WaitAsync();
		try
		{
			PreferencePageHeaderUserControl.SetStatusText("Saving...");
			await _testDataUtility.WriteKCSTestDataProperty("AfterSwipeCustomerOffers.LiveCall", _model.LiveCall.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("AfterSwipeCustomerOffers.Success", _model.Success.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("AfterSwipeCustomerOffers.SelectedOffer", _model.SelectedOffer.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("AfterSwipeCustomerOffers.IncludeMovieInCartOffer", _model.IncludeMovieInCartOffer.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("AfterSwipeCustomerOffers.IncludeFreeGameInCartOffer", _model.IncludeFreeGameInCartOffer.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("AfterSwipeCustomerOffers.IncludeAmountOffPurchaseInCartOffer", _model.IncludeAmountOffPurchaseInCartOffer.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("AfterSwipeCustomerOffers.IncludeMultiNightPriceInCartOffer", _model.IncludeMultiNightPriceInCartOffer.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("AfterSwipeCustomerOffers.MultiNightPriceProductIds", _model.MultiNightPriceProductIds);
			return true;
		}
		finally
		{
			_semaphoreSlim.Release();
		}
	}

	private void SetOfferValue(KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers value)
	{
		if (_model != null)
		{
			_model.SelectedOffer = value;
		}
	}

	private bool IsOfferValueSet(KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers value)
	{
		TransactionServiceAfterSwipeCustomerOffersModel model = _model;
		if (model == null)
		{
			return false;
		}
		return model.SelectedOffer == value;
	}

	private void NoOfferRadioButton_Checked(object sender, RoutedEventArgs e)
	{
		SetOfferValue(KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers.NoOffer);
	}

	private void PresentInCartOffersRadioButton_Checked(object sender, RoutedEventArgs e)
	{
		SetOfferValue(KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers.InCartOffers);
	}

	private void PresentFracFreeMovieFirstVisitRadioButton_Checked(object sender, RoutedEventArgs e)
	{
		SetOfferValue(KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers.FracFreeMovieFirstVisit);
	}

	private void PresentFracFreeMovieSecondVisitRadioButton_Checked(object sender, RoutedEventArgs e)
	{
		SetOfferValue(KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers.FracFreeMovieSecondVisit);
	}

	private void PresentFracDiscountedDiscFirstVisitRadioButton_Checked(object sender, RoutedEventArgs e)
	{
		SetOfferValue(KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers.FracDiscountedDiscFirstVisit);
	}

	private void PresentFracDiscountedDiscSecondVisitRadioButton_Checked(object sender, RoutedEventArgs e)
	{
		SetOfferValue(KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers.FracDiscountedDiscSecondVisit);
	}

	private void PresentFreeItemPerksEquivalentRadioButton_Checked(object sender, RoutedEventArgs e)
	{
		SetOfferValue(KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers.FreeItemPerksEquivalent);
	}

	private void PresentAmountOffPerksEquivalentRadioButton_Checked(object sender, RoutedEventArgs e)
	{
		SetOfferValue(KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers.AmountOffPerksEquivalent);
	}

	private void PresentExtraPointsPerksEquivalentRadioButton_Checked(object sender, RoutedEventArgs e)
	{
		SetOfferValue(KioskClientServiceTransactionConstants.TestData.AfterSwipeCustomerOffers.Offers.ExtraPointsPerksEquivalent);
	}
}
