using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class NewCartConfirmADAActionUpdateViewController : BaseViewController
{
	private static NewCartConfirmADAActionUpdateViewModel _viewModel;

	private static CustomerOffer _selectedOffer;

	public static void ConfigureView(CustomerOffer selectedOffer)
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			OrchestrationService.StopIdleTimer("idle_timer");
			_viewModel = new NewCartConfirmADAActionUpdateViewModel();
			_selectedOffer = selectedOffer;
			ConfigureTextAndVisibility(_viewModel);
			ConfigureCommands(_viewModel);
			ConfigureShoppingCartDisplay(_viewModel);
			ConfigureTTS(_viewModel);
			viewUserControl.DataContext = _viewModel;
			CommandManager.InvalidateRequerySuggested();
			OrchestrationService.StartIdleTimer(OrchestrationService.GetIdleTimerTimeout(), IdleTimeOut, "shopping_cart_update_ada_idle_timer");
		}
	}

	private static void ConfigureTTS(NewCartConfirmADAActionUpdateViewModel viewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (service != null && service.TTSEnabled && viewUserControl != null)
		{
			viewModel.ProcessGetSpeechControl = () => NewCartConfirmADAActionUpdateViewTextToSpeech.GetSpeechControl(GetShoppingCartViewModelFromView);
		}
	}

	private static NewCartConfirmADAActionUpdateViewModel GetShoppingCartViewModelFromView()
	{
		FrameworkElement control = BaseViewController.GetViewUserControl();
		if (control != null)
		{
			return control.Dispatcher.Invoke(() => control?.DataContext as NewCartConfirmADAActionUpdateViewModel);
		}
		return null;
	}

	private static void ConfigureShoppingCartDisplay(NewCartConfirmADAActionUpdateViewModel viewModel)
	{
		viewModel.DisplayProductModels = CreateDisplayProductModelsFromCartItems();
	}

	private static List<DisplayCheckoutProductModel> CreateDisplayProductModelsFromCartItems()
	{
		List<DisplayCheckoutProductModel> list = new List<DisplayCheckoutProductModel>();
		IRentalShoppingCart rentalShoppingCart = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart;
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (rentalShoppingCart != null && service != null)
		{
			foreach (IRentalShoppingCartTitleItem eachRentalShoppingCartItem in rentalShoppingCart.TitleItems)
			{
				IKioskProduct kioskProduct = null;
				service.GetProduct(eachRentalShoppingCartItem.ProductId, excludeTitleRollup: true, out kioskProduct);
				if (kioskProduct == null)
				{
					continue;
				}
				DisplayCheckoutProductModel displayCheckoutProductModel = new DisplayCheckoutProductModel
				{
					Name = kioskProduct.ProductName,
					Data = eachRentalShoppingCartItem,
					Image = OrchestrationService.GetProductBitmapImage(kioskProduct)
				};
				displayCheckoutProductModel.ConfigureForUpdateADACart();
				ITitleProduct titleProduct = kioskProduct as ITitleProduct;
				OfferProduct offerProduct = _selectedOffer.OfferProducts.FirstOrDefault((OfferProduct o) => o.ProductId == titleProduct.ProductId);
				bool num = offerProduct != null;
				bool flag = num && eachRentalShoppingCartItem.PricingRecord.InitialDays == offerProduct.NumberOfNights;
				decimal? num2 = (decimal?)(eachRentalShoppingCartItem.PricingRecord.InitialDays - 1) * (decimal?)eachRentalShoppingCartItem.PricingRecord.ExtraNight;
				decimal? num3 = offerProduct?.DiscountedPrice;
				if (num2.HasValue & num3.HasValue)
				{
					_ = num2.GetValueOrDefault() - num3.GetValueOrDefault();
				}
				displayCheckoutProductModel.OfferActionVisibility = Visibility.Visible;
				DisplayProductCheckoutSpecialOfferActionModel displayProductCheckoutSpecialOfferActionModel = new DisplayProductCheckoutSpecialOfferActionModel();
				if (num)
				{
					if (flag)
					{
						displayProductCheckoutSpecialOfferActionModel.TextLine1 = string.Format(Resources.cart_offer_applied_text_line_1, offerProduct.NumberOfNights, offerProduct.DiscountedPrice.Value.ToCurrencyString(useCentSign: true));
						displayProductCheckoutSpecialOfferActionModel.TextLine2 = string.Empty;
						displayProductCheckoutSpecialOfferActionModel.ButtonText = Resources.cart_offer_remove_button_text;
						displayProductCheckoutSpecialOfferActionModel.ButtonVisibility = Visibility.Visible;
						displayProductCheckoutSpecialOfferActionModel.ButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
						{
							RemoveMultiNightCommand_Execute(eachRentalShoppingCartItem, _selectedOffer.OfferId);
						}, (Func<bool>)null);
					}
					else
					{
						displayProductCheckoutSpecialOfferActionModel.TextLine1 = string.Format(Resources.cart_offer_removed_text_line_1, _selectedOffer.Amount);
						displayProductCheckoutSpecialOfferActionModel.TextLine2 = string.Empty;
						displayProductCheckoutSpecialOfferActionModel.ButtonText = Resources.cart_offer_add_button_text;
						displayProductCheckoutSpecialOfferActionModel.ButtonVisibility = Visibility.Visible;
						displayProductCheckoutSpecialOfferActionModel.ButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
						{
							AddMultiNightCommand_Execute(eachRentalShoppingCartItem, _selectedOffer);
						}, (Func<bool>)null);
					}
				}
				else
				{
					displayCheckoutProductModel.ImageOpacity = 0.4;
					displayProductCheckoutSpecialOfferActionModel.TextLine1 = Resources.cart_offer_not_applicable;
					displayProductCheckoutSpecialOfferActionModel.TextLine2 = string.Empty;
					displayProductCheckoutSpecialOfferActionModel.ButtonText = string.Empty;
					displayProductCheckoutSpecialOfferActionModel.ButtonVisibility = Visibility.Collapsed;
					displayProductCheckoutSpecialOfferActionModel.ButtonCommand = null;
				}
				displayCheckoutProductModel.OfferActionModel = displayProductCheckoutSpecialOfferActionModel;
				list.Add(displayCheckoutProductModel);
			}
		}
		return list;
	}

	private static void ConfigureTextAndVisibility(NewCartConfirmADAActionUpdateViewModel viewModel)
	{
		OrchestrationService.SetCurrentUICulture();
		viewModel.Title = Resources.update_your_bag;
		viewModel.OkButtonText = Resources.okay;
	}

	private static void IdleTimeOut()
	{
		OrchestrationService.ViewGoBack("shopping_cart_update_ada_view", popOnly: false);
	}

	public static void LeaveView()
	{
		OrchestrationService.RemoveIdleTimer("shopping_cart_update_ada_idle_timer");
	}

	private static void AddMultiNightCommand_Execute(IRentalShoppingCartTitleItem rentalShoppingCartItem, CustomerOffer selectedOffer)
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("NewCartConfirmADAAddProductOffer", $"Add offer To {rentalShoppingCartItem?.ProductId}");
		ServiceLocator.Instance.GetService<IRentalShoppingCartService>().AddMultiNightOfferToItem(rentalShoppingCartItem, selectedOffer);
		UpdateCartDisplay();
	}

	private static void UpdateCartDisplay()
	{
		ConfigureTextAndVisibility(_viewModel);
		ConfigureShoppingCartDisplay(_viewModel);
	}

	private static void RemoveMultiNightCommand_Execute(IRentalShoppingCartTitleItem rentalShoppingCartItem, long offerId)
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("NewCartConfirmADARemoveProductOffer", $"delete {rentalShoppingCartItem?.ProductId}");
		ServiceLocator.Instance.GetService<IRentalShoppingCartService>().RemoveMultiNightOfferFromItem(rentalShoppingCartItem);
		UpdateCartDisplay();
	}

	private static void ConfigureCommands(NewCartConfirmADAActionUpdateViewModel viewModel)
	{
		viewModel.OkButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
		{
			OkCommand_Execute();
		}, (Func<bool>)null);
	}

	private static void OkCommand_Execute()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("NewCartConfirmADAOK");
		OrchestrationService.ViewGoBackSafe("OK Pressed", popOnly: false);
	}
}
