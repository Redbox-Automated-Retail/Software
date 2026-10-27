using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Redbox.Core;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.ShoppingCart;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class TitleRollupUtilities
{
	public class TitleDetailFormatRentalButtonTextValues : ITitleRollupFormatButtonTextValues
	{
		private string _titleRollupOverlayUnavailableText;

		private string _titleRollupOverlayCartText;

		private string _titleRollupOverlayOosText;

		private string _titleRollupOverlayAddText;

		private string _titleRollupOverlayBuyText;

		private string _currencySymbol;

		private string _outOfStockText;

		private string _inCartText;

		public string Unavailable
		{
			get
			{
				if (_titleRollupOverlayUnavailableText == null)
				{
					_titleRollupOverlayUnavailableText = Resources.title_rollup_format_selection_unavailable_text;
				}
				return _titleRollupOverlayUnavailableText;
			}
		}

		public string InCartFormat
		{
			get
			{
				if (_titleRollupOverlayCartText == null)
				{
					_titleRollupOverlayCartText = Resources.title_rollup_format_selection_cart_text;
				}
				return _titleRollupOverlayCartText;
			}
		}

		public string OutOfStockFormat
		{
			get
			{
				if (_titleRollupOverlayOosText == null)
				{
					_titleRollupOverlayOosText = Resources.title_rollup_format_selection_oos_text;
				}
				return _titleRollupOverlayOosText;
			}
		}

		public string Rent
		{
			get
			{
				if (_titleRollupOverlayAddText == null)
				{
					_titleRollupOverlayAddText = Resources.title_detail_format_rent_text;
				}
				return _titleRollupOverlayAddText;
			}
		}

		public string Buy
		{
			get
			{
				if (_titleRollupOverlayBuyText == null)
				{
					_titleRollupOverlayBuyText = Resources.title_detail_format_buy_text;
				}
				return _titleRollupOverlayBuyText;
			}
		}

		public string CurrencySymbol
		{
			get
			{
				if (_currencySymbol == null)
				{
					_currencySymbol = Resources.currency_symbol;
				}
				return _currencySymbol;
			}
		}

		public string OutOfStockText
		{
			get
			{
				if (_outOfStockText == null)
				{
					_outOfStockText = Resources.title_rollup_format_selection_oos_text;
				}
				return _outOfStockText;
			}
		}

		public string InCartText
		{
			get
			{
				if (_inCartText == null)
				{
					_inCartText = Resources.title_rollup_format_selection_cart_text;
				}
				return _inCartText;
			}
		}
	}

	public static void ConfigureTitleRollupOverlay(IBrowseItemModel browseItemModel, TitleRollupOverlayModel titleRollupOverlayModel, bool showDigitalCodeInfo, bool isPurchase)
	{
		if (browseItemModel?.Data is TitleRollupProduct titleRollupProduct)
		{
			titleRollupOverlayModel.BrowseItemModel = browseItemModel;
			TitleRollupOverlayButtonProperties titleRollupOverlayButtonProperties = GetTitleRollupOverlayButtonProperties(titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.DVD), isPurchase);
			titleRollupOverlayModel.DVDButtonText = titleRollupOverlayButtonProperties.ButtonText;
			titleRollupOverlayModel.DVDButtonLine2Text = titleRollupOverlayButtonProperties.ButtonTextLine2;
			titleRollupOverlayModel.DVDButtonVisibility = titleRollupOverlayButtonProperties.Visibility;
			titleRollupOverlayModel.DVDButtonEnabled = titleRollupOverlayButtonProperties.Enabled;
			titleRollupOverlayButtonProperties = GetTitleRollupOverlayButtonProperties(titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.Bluray), isPurchase);
			titleRollupOverlayModel.BlurayButtonText = titleRollupOverlayButtonProperties.ButtonText;
			titleRollupOverlayModel.BlurayButtonLine2Text = titleRollupOverlayButtonProperties.ButtonTextLine2;
			titleRollupOverlayModel.BlurayButtonVisibility = titleRollupOverlayButtonProperties.Visibility;
			titleRollupOverlayModel.BlurayButtonEnabled = titleRollupOverlayButtonProperties.Enabled;
			titleRollupOverlayButtonProperties = GetTitleRollupOverlayButtonProperties(titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType._4KUHD), isPurchase);
			titleRollupOverlayModel._4kUhdButtonText = titleRollupOverlayButtonProperties.ButtonText;
			titleRollupOverlayModel._4kUhdButtonLine2Text = titleRollupOverlayButtonProperties.ButtonTextLine2;
			titleRollupOverlayModel._4kUhdButtonVisibility = titleRollupOverlayButtonProperties.Visibility;
			titleRollupOverlayModel._4kUhdButtonEnabled = titleRollupOverlayButtonProperties.Enabled;
			if (titleRollupOverlayModel._4kUhdButtonVisibility != Visibility.Visible)
			{
				titleRollupOverlayModel.Grid4kUhdButtonGridRowHeight = new GridLength(0.0);
				titleRollupOverlayModel.Grid4kUhdButtonMarginGridRowHeight = new GridLength(0.0);
			}
			else
			{
				titleRollupOverlayModel.Grid4kUhdButtonGridRowHeight = new GridLength(20.0, GridUnitType.Star);
				titleRollupOverlayModel.Grid4kUhdButtonMarginGridRowHeight = new GridLength(18.0, GridUnitType.Star);
			}
			titleRollupOverlayModel.Visibility = Visibility.Visible;
		}
	}

	public static bool IsAdaDefaultTitle(ITitleProduct productToCheck, TitleRollupProduct rollupProduct)
	{
		return rollupProduct.KioskProducts.Any((ITitleProduct y) => y == productToCheck && y.TitleType == (from t in rollupProduct.KioskProducts
			where t.IsRentableAndInStock
			select t.TitleType).Min());
	}

	public static TitleRollupOverlayButtonProperties GetTitleRollupOverlayButtonProperties(ITitleProduct titleProduct, bool isPurchase)
	{
		return GetTitleRollupOverlayButtonProperties(titleProduct, isPurchase, new TitleRollupOverlayButtonTextValues());
	}

	public static TitleRollupOverlayButtonProperties GetTitleRollupOverlayButtonProperties(ITitleProduct titleProduct, bool isPurchase, ITitleRollupFormatButtonTextValues buttonTextValues)
	{
		TitleRollupOverlayButtonProperties titleRollupOverlayButtonProperties = new TitleRollupOverlayButtonProperties
		{
			Visibility = Visibility.Collapsed
		};
		if (titleProduct != null && buttonTextValues != null)
		{
			titleRollupOverlayButtonProperties.ButtonText = buttonTextValues.Unavailable;
			TitleRollupFormatButtonProperties rollupFormatButtonProperties = GetRollupFormatButtonProperties(titleProduct, isPurchase, buttonTextValues);
			titleRollupOverlayButtonProperties.ButtonText = rollupFormatButtonProperties.ButtonText;
			titleRollupOverlayButtonProperties.ButtonTextLine2 = rollupFormatButtonProperties.ButtonTextLine2;
			titleRollupOverlayButtonProperties.Enabled = rollupFormatButtonProperties.IsEnabled;
			titleRollupOverlayButtonProperties.Visibility = rollupFormatButtonProperties.Visibility;
		}
		return titleRollupOverlayButtonProperties;
	}

	public static TitleRollupFormatButtonProperties GetRollupFormatButtonProperties(ITitleProduct titleProduct, bool isPurchase, ITitleRollupFormatButtonTextValues buttonTextValues, bool showOutOfStock = true, Func<decimal, decimal> discountedPriceCalc = null, bool showPricing = true, int? redboxPlusBenefitId = null)
	{
		TitleRollupFormatButtonProperties titleRollupFormatButtonProperties = new TitleRollupFormatButtonProperties();
		titleRollupFormatButtonProperties.IsEnabled = false;
		titleRollupFormatButtonProperties.TitleType = titleProduct.TitleType;
		titleRollupFormatButtonProperties.ButtonText = buttonTextValues.Unavailable;
		titleRollupFormatButtonProperties.StyleName = "button_light_large_primary_style";
		string titleTypeText = GetTitleTypeText(titleProduct.TitleType);
		decimal purchasePrice = titleProduct.PurchasePrice;
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = service?.CurrentRentalShoppingCart?.GetTitleItemsByCriteria(new RentalShoppingCartFilterCriteria
		{
			IncludeProducts = new List<long> { titleProduct.ProductId }
		}).FirstOrDefault();
		if (isPurchase && (purchasePrice <= 0m || !titleProduct.IsSellable))
		{
			titleRollupFormatButtonProperties.HideAndDisableButton();
		}
		else if (service?.CurrentRentalShoppingCart?.HasTitleItem(titleProduct.ProductId) == true)
		{
			titleRollupFormatButtonProperties.ButtonText = titleTypeText;
			titleRollupFormatButtonProperties.ButtonTextLine2 = (string.IsNullOrEmpty(buttonTextValues.InCartText) ? buttonTextValues.InCartFormat : buttonTextValues.InCartText);
			titleRollupFormatButtonProperties.Visibility = Visibility.Visible;
			titleRollupFormatButtonProperties.IsEnabled = redboxPlusBenefitId == 2 && !(rentalShoppingCartTitleItem?.IsRedboxPlusFreeOneNightRental ?? false);
		}
		else if (!titleProduct.InStock & showOutOfStock)
		{
			if (titleProduct.TitleType == TitleType._4KUHD && !titleProduct.HasBeenInStock)
			{
				titleRollupFormatButtonProperties.HideAndDisableButton();
			}
			else
			{
				titleRollupFormatButtonProperties.ButtonText = titleTypeText;
				titleRollupFormatButtonProperties.ButtonTextLine2 = (string.IsNullOrEmpty(buttonTextValues.OutOfStockText) ? buttonTextValues.OutOfStockFormat : buttonTextValues.OutOfStockText);
				titleRollupFormatButtonProperties.Visibility = Visibility.Visible;
			}
		}
		else if (isPurchase)
		{
			titleRollupFormatButtonProperties.ButtonText = string.Format(buttonTextValues.Buy, titleTypeText);
			if (showPricing)
			{
				if (discountedPriceCalc == null)
				{
					titleRollupFormatButtonProperties.ButtonTextLine2 = $"{buttonTextValues.CurrencySymbol}{$"{purchasePrice:N2}"}";
				}
				else
				{
					decimal num = discountedPriceCalc(purchasePrice);
					if (num > 0m)
					{
						titleRollupFormatButtonProperties.ButtonTextLine2 = $"{buttonTextValues.CurrencySymbol}{$"{num:N2}"}";
					}
					else
					{
						titleRollupFormatButtonProperties.ButtonTextLine2 = $"{buttonTextValues.CurrencySymbol}{$"{Resources.offer_info_free:N2}"}";
					}
				}
			}
			titleRollupFormatButtonProperties.Visibility = Visibility.Visible;
			titleRollupFormatButtonProperties.IsEnabled = true;
			titleRollupFormatButtonProperties.StyleName = "button_large_purchase_style";
		}
		else if (titleProduct.IsAvailable && (titleProduct.InStock || !showOutOfStock))
		{
			titleRollupFormatButtonProperties.ButtonText = ((redboxPlusBenefitId == 2) ? titleTypeText : string.Format(buttonTextValues.Rent, titleTypeText));
			if (showPricing)
			{
				IPricingRecord pricingRecord = ServiceLocator.Instance.GetService<IPricingService>()?.GetOneDayProductPricingRecord(titleProduct.ProductId);
				if (pricingRecord != null)
				{
					decimal initialNight = pricingRecord.InitialNight;
					if (discountedPriceCalc == null)
					{
						titleRollupFormatButtonProperties.ButtonTextLine2 = $"{initialNight:C}";
					}
					else
					{
						decimal num2 = discountedPriceCalc(initialNight);
						titleRollupFormatButtonProperties.ButtonTextLine2 = ((num2 > 0m) ? $"{num2:C}" : Resources.offer_info_free);
					}
				}
			}
			titleRollupFormatButtonProperties.Visibility = Visibility.Visible;
			titleRollupFormatButtonProperties.IsEnabled = true;
		}
		if (titleProduct.TitleType == TitleType.DigitalCode && !isPurchase)
		{
			titleRollupFormatButtonProperties.HideAndDisableButton();
		}
		if (titleProduct.TitleFamily == TitleFamily.Games)
		{
			titleRollupFormatButtonProperties.HideAndDisableButton();
		}
		return titleRollupFormatButtonProperties;
	}

	private static string GetTitleTypeText(TitleType type)
	{
		string result = string.Empty;
		switch (type)
		{
		case TitleType.DVD:
			result = Resources.title_type_name_dvd.ToUpper();
			break;
		case TitleType.Bluray:
			result = Resources.title_type_name_bluray.ToUpper();
			break;
		case TitleType._4KUHD:
			result = Resources.title_type_name_4kuhd.ToUpper();
			break;
		case TitleType.DigitalCode:
			result = Resources.title_type_name_digital_code.ToUpper();
			break;
		}
		return result;
	}
}
