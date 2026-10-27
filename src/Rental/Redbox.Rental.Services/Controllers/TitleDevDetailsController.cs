using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Cache;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

public class TitleDevDetailsController : BaseViewController
{
	private static TitleDetailDevModel _model;

	public static void ConfigureView()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			BaseViewController.StopIdleTimer();
			_model = GetModel();
			viewUserControl.DataContext = _model;
		}
	}

	public static TitleDetailDevModel GetModel()
	{
		IPricingService service = ServiceLocator.Instance.GetService<IPricingService>();
		ServiceLocator.Instance.GetService<IStoreManager>();
		TitleDetailDevModel titleDetailDevModel = new TitleDetailDevModel();
		ITitleProduct titleProductObj = (ITitleProduct)Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectionSelectedProduct;
		titleDetailDevModel.OnCloseButtonClicked = (Action)Delegate.Combine(titleDetailDevModel.OnCloseButtonClicked, new Action(GoBackAction));
		titleDetailDevModel.OnBlurayButtonClicked = (Action)Delegate.Combine(titleDetailDevModel.OnBlurayButtonClicked, new Action(BluRayAction));
		titleDetailDevModel.OnDvdButtonClicked = (Action)Delegate.Combine(titleDetailDevModel.OnDvdButtonClicked, new Action(DVDAction));
		titleDetailDevModel.OnTitleRollupButtonClicked = (Action)Delegate.Combine(titleDetailDevModel.OnTitleRollupButtonClicked, new Action(TitleRollUpAction));
		titleDetailDevModel.OnDigitalButtonClicked = (Action)Delegate.Combine(titleDetailDevModel.OnDigitalButtonClicked, new Action(DigitalCodeAction));
		titleDetailDevModel.On4KUhdButtonClicked = (Action)Delegate.Combine(titleDetailDevModel.On4KUhdButtonClicked, new Action(FourKUhdAction));
		SetTitleProductDetails(titleDetailDevModel, titleProductObj, service);
		titleDetailDevModel.OnAddProductWithCustomPriceButtonClicked = OnAddProductWithCustomPriceButtonClicked;
		return titleDetailDevModel;
	}

	private static void OnAddProductWithCustomPriceButtonClicked(long productId, decimal amount)
	{
		ServiceLocator.Instance.GetService<IDataService>().GetProduct(productId, excludeTitleRollup: true, out var kioskProduct);
		ITitleProduct titleProduct = kioskProduct as ITitleProduct;
		IPricingRecord pricingRecord = ServiceLocator.Instance.GetService<IPricingService>().GetOneDayProductPricingRecord(titleProduct.ProductId).Clone();
		pricingRecord.InitialNight = amount;
		BrowseFunctions.AddProductToCart(titleProduct, isPurchase: false, pricingRecord);
	}

	private static void SetTitleProductDetails(TitleDetailDevModel viewModel, ITitleProduct TitleProductObj, IPricingService pricingService)
	{
		TitleProductDetailDevModel titleProductDetailDevModel = CreateTitleProductDetailDevModel(TitleProductObj, pricingService);
		viewModel.Products.Add((TitleProductObj.KioskProductType != KioskProductType.TitleRollup) ? TitleProductObj.TitleType : TitleType.All, titleProductDetailDevModel);
		viewModel.CurrentProduct = titleProductDetailDevModel;
		viewModel.CurrentProduct.HasRollup = false;
		if (TitleProductObj is TitleRollupProduct)
		{
			TitleRollupProduct titleRollupProduct = (TitleRollupProduct)TitleProductObj;
			titleProductDetailDevModel.IsDvd = titleRollupProduct.KioskProducts.Where((ITitleProduct b) => b.TitleType == TitleType.DVD).Any();
			titleProductDetailDevModel.IsBluRay = titleRollupProduct.KioskProducts.Where((ITitleProduct b) => b.TitleType == TitleType.Bluray).Any();
			titleProductDetailDevModel.IsDigitalTitle = titleRollupProduct.KioskProducts.Where((ITitleProduct b) => b.TitleType == TitleType.DigitalCode).Any();
			titleProductDetailDevModel.Is4KUhdTitle = titleRollupProduct.KioskProducts.Where((ITitleProduct b) => b.TitleType == TitleType._4KUHD).Any();
			titleProductDetailDevModel.HasRollup = true;
			titleProductDetailDevModel.TitleRollupProductGroupId = titleRollupProduct.ProductGroupId;
			{
				foreach (TitleProduct kioskProduct in titleRollupProduct.KioskProducts)
				{
					TitleProductDetailDevModel titleProductDetailDevModel2 = CreateTitleProductDetailDevModel(kioskProduct, pricingService);
					titleProductDetailDevModel2.HasRollup = true;
					titleProductDetailDevModel2.IsDvd = titleRollupProduct.KioskProducts.Where((ITitleProduct b) => b.TitleType == TitleType.DVD).Any();
					titleProductDetailDevModel2.IsBluRay = titleRollupProduct.KioskProducts.Where((ITitleProduct b) => b.TitleType == TitleType.Bluray).Any();
					titleProductDetailDevModel2.IsDigitalTitle = titleRollupProduct.KioskProducts.Where((ITitleProduct b) => b.TitleType == TitleType.DigitalCode).Any();
					titleProductDetailDevModel2.Is4KUhdTitle = titleRollupProduct.KioskProducts.Where((ITitleProduct b) => b.TitleType == TitleType._4KUHD).Any();
					viewModel.Products.Add(kioskProduct.TitleType, titleProductDetailDevModel2);
				}
				return;
			}
		}
		titleProductDetailDevModel.IsDvd = false;
		titleProductDetailDevModel.IsBluRay = false;
		titleProductDetailDevModel.IsDigitalTitle = false;
	}

	private static TitleProductDetailDevModel CreateTitleProductDetailDevModel(ITitleProduct TitleProductObj, IPricingService pricingService)
	{
		string text = TitleProductObj.ProductId.ToString();
		string productName = TitleProductObj.ProductName;
		ObservableCollection<BarcodeProduct> barCodes = new ObservableCollection<BarcodeProduct>(TitleProductObj.BarCodeProducts.Values.ToList());
		IPricingRecord pricingRecord = pricingService?.GetOneDayProductPricingRecord(TitleProductObj.ProductId);
		TitleProductDetailDevModel titleProductDetailDevModel = new TitleProductDetailDevModel();
		titleProductDetailDevModel.ProductId = TitleProductObj.ProductId;
		titleProductDetailDevModel.DevTitle = text + " - " + productName;
		titleProductDetailDevModel.ComingSoonDays = TitleProductObj.ComingSoonDays;
		DateTime dateTime = (TitleProductObj.NationalStreetDate.HasValue ? TitleProductObj.NationalStreetDate.Value : DateTime.MinValue);
		titleProductDetailDevModel.NationalStreetDate = ((dateTime == DateTime.MinValue) ? string.Empty : dateTime.ToShortDateString());
		titleProductDetailDevModel.BoxOfficeGross = TitleProductObj.BoxOfficeGross;
		titleProductDetailDevModel.ClosedCaptioned = TitleProductObj.ClosedCaptioned;
		titleProductDetailDevModel.SellThruNew = TitleProductObj.SellThruNew;
		titleProductDetailDevModel.ProductTypeId = TitleProductObj.ProductTypeId;
		titleProductDetailDevModel.MerchandiseDate = TitleProductObj.MerchandiseDate.ToShortDateString();
		titleProductDetailDevModel.SortDate = TitleProductObj.SortDate.ToShortDateString();
		dateTime = (TitleProductObj.ReleaseDate.HasValue ? TitleProductObj.ReleaseDate.Value : DateTime.MinValue);
		titleProductDetailDevModel.ReleaseDate = ((dateTime == DateTime.MinValue) ? string.Empty : dateTime.ToShortDateString());
		titleProductDetailDevModel.SellThru = TitleProductObj.SellThru;
		titleProductDetailDevModel.DiscountRestriction = TitleProductObj.DiscountRestriction.ToString();
		if (pricingRecord != null)
		{
			titleProductDetailDevModel.PromoValue = pricingRecord.PromoValue;
			titleProductDetailDevModel.ExtraNight = pricingRecord.ExtraNight;
			titleProductDetailDevModel.ExpirationPrice = pricingRecord.ExpirationPrice;
			titleProductDetailDevModel.InitialNight = pricingRecord.InitialNight;
			titleProductDetailDevModel.PriceSetId = pricingRecord.ProductPricingId.ToString();
			titleProductDetailDevModel.Purchase = pricingRecord.Purchase;
			titleProductDetailDevModel.NonReturn = pricingRecord.NonReturn;
			titleProductDetailDevModel.NonReturnDays = pricingRecord.NonReturnDays;
		}
		titleProductDetailDevModel.GenreIds = TitleProductObj.Genres;
		foreach (long genre in TitleProductObj.Genres)
		{
			titleProductDetailDevModel.GenreList.Add((genre + " " + Enum.Parse(typeof(Genres), genre.ToString())).ToUpper());
		}
		titleProductDetailDevModel.IsComingSoon = TitleProductObj.ComingSoon;
		titleProductDetailDevModel.InStock = TitleProductObj.InStock;
		titleProductDetailDevModel.CanBeSold = TitleProductObj.IsSellable;
		titleProductDetailDevModel.ProductFamilyName = TitleProductObj.TitleFamily.ToString();
		titleProductDetailDevModel.ProductTypeName = TitleProductObj.TitleType.ToString();
		titleProductDetailDevModel.IsAvailable = TitleProductObj.IsAvailable;
		titleProductDetailDevModel.ShowOutOfStock = TitleProductObj.ShowOutOfStock;
		titleProductDetailDevModel.Type = TitleProductObj.KioskProductType.ToString();
		titleProductDetailDevModel.BarCodes = barCodes;
		titleProductDetailDevModel.TitleRollupProductGroupId = TitleProductObj.TitleRollupProductGroupId;
		titleProductDetailDevModel.DisplayAddProductWithCustomPriceButtonVisibility = ((!titleProductDetailDevModel.InStock) ? Visibility.Collapsed : Visibility.Visible);
		titleProductDetailDevModel.AddProductWithCustomPriceButtonVisibility = Visibility.Collapsed;
		IBitmapCacheService service = ServiceLocator.Instance.GetService<IBitmapCacheService>();
		titleProductDetailDevModel.ImgFile = service.GetBitmapImage(TitleProductObj.ImageFile);
		return titleProductDetailDevModel;
	}

	private static void GoBackAction()
	{
		BaseViewController.StartIdleTimer();
		OrchestrationService.ViewGoBack("go back", popOnly: false);
	}

	private static void DVDAction()
	{
		BaseViewController.StopIdleTimer();
		TitleDetailDevModel titleDetailDevModel = BaseViewController.GetViewUserControl().DataContext as TitleDetailDevModel;
		if (titleDetailDevModel.Products.ContainsKey(TitleType.DVD))
		{
			titleDetailDevModel.CurrentProduct = titleDetailDevModel.Products[TitleType.DVD];
		}
	}

	private static void TitleRollUpAction()
	{
		BaseViewController.StopIdleTimer();
		TitleDetailDevModel titleDetailDevModel = BaseViewController.GetViewUserControl().DataContext as TitleDetailDevModel;
		if (titleDetailDevModel.Products.ContainsKey(TitleType.All))
		{
			titleDetailDevModel.CurrentProduct = titleDetailDevModel.Products[TitleType.All];
		}
	}

	private static void DigitalCodeAction()
	{
		BaseViewController.StopIdleTimer();
		TitleDetailDevModel titleDetailDevModel = BaseViewController.GetViewUserControl().DataContext as TitleDetailDevModel;
		if (titleDetailDevModel.Products.ContainsKey(TitleType.DigitalCode))
		{
			titleDetailDevModel.CurrentProduct = titleDetailDevModel.Products[TitleType.DigitalCode];
		}
	}

	private static void BluRayAction()
	{
		BaseViewController.StopIdleTimer();
		TitleDetailDevModel titleDetailDevModel = BaseViewController.GetViewUserControl().DataContext as TitleDetailDevModel;
		if (titleDetailDevModel.Products.ContainsKey(TitleType.Bluray))
		{
			titleDetailDevModel.CurrentProduct = titleDetailDevModel.Products[TitleType.Bluray];
		}
	}

	private static void FourKUhdAction()
	{
		BaseViewController.StopIdleTimer();
		TitleDetailDevModel titleDetailDevModel = BaseViewController.GetViewUserControl().DataContext as TitleDetailDevModel;
		if (titleDetailDevModel.Products.ContainsKey(TitleType._4KUHD))
		{
			titleDetailDevModel.CurrentProduct = titleDetailDevModel.Products[TitleType._4KUHD];
		}
	}
}
