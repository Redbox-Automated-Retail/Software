using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class NewCartConfirmViewController : BaseViewController
{
	private static NewCartConfirmviewParameters _parameters;

	private static NewCartConfirmViewModel _viewModel;

	private static Action _cancelAction;

	private static Action _continueAction;

	private static Action _timeoutAction;

	private static Action _removeOfferAction;

	private static CustomerOffer _selectedOffer;

	private static bool _isMultiNightOffer;

	private static bool _isPerksOffer;

	public static void ConfigureView(NewCartConfirmviewParameters parameters)
	{
		BaseViewController.StopIdleTimer();
		CreateAndStartTimer();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			NewCartConfirmViewModel newCartConfirmViewModel = new NewCartConfirmViewModel();
			_parameters = parameters;
			_viewModel = newCartConfirmViewModel;
			_cancelAction = parameters.CancelAction;
			_continueAction = parameters.ContinueAction;
			_timeoutAction = parameters.TimeoutAction;
			_removeOfferAction = parameters.RemoveOfferAction;
			_selectedOffer = parameters.SelectedOffer;
			_isMultiNightOffer = _selectedOffer.DiscountType == OfferDiscountType.MultiNightPrice;
			_isPerksOffer = _selectedOffer.CustomerSegmentType == CustomerSegmentType.NewMemberPerksSignup;
			ConfigureTextAndVisibility(newCartConfirmViewModel, parameters);
			ConfigureCommands(newCartConfirmViewModel, parameters);
			ConfigureShoppingCartDisplay(newCartConfirmViewModel, parameters);
			ConfigureTTS(newCartConfirmViewModel, parameters);
			viewUserControl.DataContext = newCartConfirmViewModel;
		}
	}

	private static void ConfigureTTS(NewCartConfirmViewModel viewModel, NewCartConfirmviewParameters parameters)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (service != null && service.TTSEnabled && viewUserControl != null)
		{
			viewModel.OnGetSpeechControl = () => NewCartConfirmViewTextToSpeech.GetSpeechControl(GetShoppingCartViewModelFromView, parameters);
		}
	}

	private static NewCartConfirmViewModel GetShoppingCartViewModelFromView()
	{
		FrameworkElement shoppingCartViewUserControl = BaseViewController.GetViewUserControl();
		if (shoppingCartViewUserControl != null)
		{
			return shoppingCartViewUserControl.Dispatcher.Invoke(() => shoppingCartViewUserControl?.DataContext as NewCartConfirmViewModel);
		}
		return null;
	}

	private static void ConfigureShoppingCartDisplay(NewCartConfirmViewModel viewModel, NewCartConfirmviewParameters parameters)
	{
		viewModel.DisplayProductModels = CreateDisplayProductModelsFromCartItems(viewModel, parameters);
	}

	private static List<DisplayCheckoutProductModel> CreateDisplayProductModelsFromCartItems(NewCartConfirmViewModel viewModel, NewCartConfirmviewParameters parameters)
	{
		List<DisplayCheckoutProductModel> list = new List<DisplayCheckoutProductModel>();
		IRentalShoppingCart newCart = parameters.NewCart;
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (newCart != null && service != null && newCart.TitleItems.Count > 0)
		{
			foreach (IRentalShoppingCartTitleItem titleItem in newCart.TitleItems)
			{
				IKioskProduct kioskProduct = null;
				service.GetProduct(titleItem.ProductId, excludeTitleRollup: true, out kioskProduct);
				if (kioskProduct != null)
				{
					DisplayCheckoutProductModel displayCheckoutProductModel = new DisplayCheckoutProductModel
					{
						Name = kioskProduct.ProductName,
						Data = titleItem,
						Image = OrchestrationService.GetProductBitmapImage(kioskProduct)
					};
					bool flag = titleItem.ProductId == parameters.promoProduct?.ProductId;
					PopulateDisplayCheckoutProductModel(displayCheckoutProductModel, flag);
					if (flag)
					{
						displayCheckoutProductModel.TopCancelButtonVisibility = (_isMultiNightOffer ? Visibility.Collapsed : (BaseViewController.IsADAMode ? Visibility.Hidden : Visibility.Visible));
						displayCheckoutProductModel.BottomCancelButtonVisibility = (_isMultiNightOffer ? Visibility.Collapsed : ((!BaseViewController.IsADAMode) ? Visibility.Collapsed : Visibility.Visible));
						list.Insert(0, displayCheckoutProductModel);
					}
					else
					{
						displayCheckoutProductModel.TopCancelButtonVisibility = (_isMultiNightOffer ? Visibility.Collapsed : (BaseViewController.IsADAMode ? Visibility.Hidden : Visibility.Hidden));
						displayCheckoutProductModel.BottomCancelButtonVisibility = (_isMultiNightOffer ? Visibility.Collapsed : (BaseViewController.IsADAMode ? Visibility.Hidden : Visibility.Collapsed));
						list.Add(displayCheckoutProductModel);
					}
				}
			}
		}
		return list;
	}

	private static void PopulateDisplayCheckoutProductModel(DisplayCheckoutProductModel displayCheckoutProductModel, bool isSelectedProduct)
	{
		IRentalShoppingCartTitleItem eachRentalShoppingCartItem = displayCheckoutProductModel?.Data as IRentalShoppingCartTitleItem;
		if (eachRentalShoppingCartItem == null)
		{
			return;
		}
		displayCheckoutProductModel.ConfigureForCheckout();
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		IKioskProduct kioskProduct = null;
		service.GetProduct(eachRentalShoppingCartItem.ProductId, excludeTitleRollup: false, out kioskProduct);
		ITitleProduct titleProduct = kioskProduct as ITitleProduct;
		displayCheckoutProductModel.ProductTitleText = BrowseFunctions.GetProductDisplayName(titleProduct);
		MDVProduct mDVProduct = titleProduct as MDVProduct;
		displayCheckoutProductModel.MDVFieldVisibility = ((mDVProduct == null) ? Visibility.Collapsed : Visibility.Visible);
		displayCheckoutProductModel.MDVNumberOfDiscsText = ((mDVProduct != null) ? $"({mDVProduct.TitleProducts.Count} {Resources.multi_disc_vend_discs_text_all_caps})**" : null);
		displayCheckoutProductModel.MDVDiscVendText = ((mDVProduct != null) ? $"{mDVProduct.TitleProducts.Count} {Resources.disc_vend}" : null);
		TitleTypeProperty value = null;
		TitleTypeProperties.Properties.TryGetValue(titleProduct.TitleType, out value);
		string productTypeText = $" - {value?.GetName(Resources.ResourceManager)}";
		displayCheckoutProductModel.ProductTypeText = productTypeText;
		string text = null;
		if (eachRentalShoppingCartItem.Action == RentalShoppingCartItemAction.purchase)
		{
			text = Resources.cart_purchase;
		}
		else
		{
			string arg = string.Format(Resources.night_rental, eachRentalShoppingCartItem?.PricingRecord?.InitialDays ?? 1);
			string arg2 = $"{Resources.extra_night} {eachRentalShoppingCartItem?.PricingRecord.ExtraNight:C}";
			text = $"{arg} | {arg2}";
		}
		displayCheckoutProductModel.ProductDescriptionLine2Text = text;
		IPromotionService service2 = ServiceLocator.Instance.GetService<IPromotionService>();
		bool flag = service2.GetPromotion()?.PromotionItems.Any((IPromotionItem i) => i.RentalShoppingCartItem.ProductId == eachRentalShoppingCartItem.ProductId) ?? false;
		decimal num = (service2.GetPromotion()?.PromotionItems.FirstOrDefault((IPromotionItem i) => i.RentalShoppingCartItem.ProductId == eachRentalShoppingCartItem.ProductId))?.PromotionAmount ?? 0m;
		if (isSelectedProduct && eachRentalShoppingCartItem.Action == RentalShoppingCartItemAction.purchase)
		{
			displayCheckoutProductModel.ProductDescriptionLine3Text = string.Format(Resources.discount_applied_text, eachRentalShoppingCartItem.PurchasePrice - eachRentalShoppingCartItem.Price);
		}
		else if (isSelectedProduct || (flag && !_isMultiNightOffer))
		{
			displayCheckoutProductModel.ProductDescriptionLine3Text = string.Format(Resources.discount_applied_text, num);
		}
		else
		{
			displayCheckoutProductModel.ProductDescriptionLine3Text = "";
		}
		displayCheckoutProductModel.ProductDescriptionLine3TextVisibility = (string.IsNullOrEmpty(displayCheckoutProductModel.ProductDescriptionLine3Text) ? Visibility.Collapsed : Visibility.Visible);
		displayCheckoutProductModel.RemoveButtonText = Resources.shopping_cart_remove_button_text;
		if (_isMultiNightOffer)
		{
			OfferProduct offerProduct = _selectedOffer.OfferProducts.FirstOrDefault((OfferProduct o) => o.ProductId == titleProduct.ProductId);
			bool num2 = offerProduct != null;
			bool flag2 = num2 && eachRentalShoppingCartItem.PricingRecord.InitialDays == offerProduct.NumberOfNights;
			decimal? num3 = ((!Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.MarketingGeneral.UseDiscountsForPmnpOffers) ? new decimal?(0m) : ((decimal?)(eachRentalShoppingCartItem.PricingRecord.InitialDays - 1) * (decimal?)eachRentalShoppingCartItem.PricingRecord.ExtraNight - (decimal?)(offerProduct?.DiscountedPrice).GetValueOrDefault()));
			displayCheckoutProductModel.OfferActionVisibility = Visibility.Visible;
			decimal price = eachRentalShoppingCartItem.Price;
			decimal? obj = (flag2 ? num3 : new decimal?(0m));
			displayCheckoutProductModel.ProductCartPriceText = $"{(decimal?)price - obj:C}";
			DisplayProductCheckoutSpecialOfferActionModel displayProductCheckoutSpecialOfferActionModel = new DisplayProductCheckoutSpecialOfferActionModel();
			if (num2)
			{
				if (flag2)
				{
					displayProductCheckoutSpecialOfferActionModel.TextLine1 = string.Format(Resources.cart_offer_applied_text_line_1, offerProduct.NumberOfNights, offerProduct.DiscountedPrice.Value.ToCurrencyString(useCentSign: true));
					displayProductCheckoutSpecialOfferActionModel.TextLine2 = string.Format(Resources.cart_offer_applied_text_line_2, num3);
					displayProductCheckoutSpecialOfferActionModel.ButtonText = Resources.cart_offer_remove_button_text;
					displayProductCheckoutSpecialOfferActionModel.ButtonVisibility = (BaseViewController.IsADAMode ? Visibility.Collapsed : Visibility.Visible);
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
					displayProductCheckoutSpecialOfferActionModel.ButtonVisibility = (BaseViewController.IsADAMode ? Visibility.Collapsed : Visibility.Visible);
					displayProductCheckoutSpecialOfferActionModel.ButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
					{
						AddMultiNightCommand_Execute(eachRentalShoppingCartItem, _selectedOffer);
					}, (Func<bool>)null);
				}
			}
			else
			{
				displayProductCheckoutSpecialOfferActionModel.TextLine1 = Resources.cart_offer_not_applicable;
				displayProductCheckoutSpecialOfferActionModel.TextLine2 = string.Empty;
				displayProductCheckoutSpecialOfferActionModel.ButtonText = string.Empty;
				displayProductCheckoutSpecialOfferActionModel.ButtonVisibility = Visibility.Collapsed;
				displayProductCheckoutSpecialOfferActionModel.ButtonCommand = null;
			}
			displayCheckoutProductModel.OfferActionModel = displayProductCheckoutSpecialOfferActionModel;
		}
		else
		{
			displayCheckoutProductModel.ProductCartPriceText = $"{eachRentalShoppingCartItem.Price - num:C}";
		}
	}

	private static void AddMultiNightCommand_Execute(IRentalShoppingCartTitleItem rentalShoppingCartItem, CustomerOffer selectedOffer)
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("NewCartConfirmAddPMNPOfferToProduct", $"Add offer to {rentalShoppingCartItem?.ProductId}");
		ServiceLocator.Instance.GetService<IRentalShoppingCartService>().AddMultiNightOfferToItem(rentalShoppingCartItem, selectedOffer);
		UpdateCartDisplay();
	}

	private static void RemoveMultiNightCommand_Execute(IRentalShoppingCartTitleItem rentalShoppingCartItem, long offerId)
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("NewCartConfirmRemovePMNPOfferToProduct", $"delete {rentalShoppingCartItem?.ProductId}");
		ServiceLocator.Instance.GetService<IRentalShoppingCartService>().RemoveMultiNightOfferFromItem(rentalShoppingCartItem);
		UpdateCartDisplay();
	}

	private static void ConfigureCommands(NewCartConfirmViewModel viewModel, NewCartConfirmviewParameters parameters)
	{
		if (_isMultiNightOffer)
		{
			viewModel.CancelButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				ModifyCartAction_Execute();
			}, (Func<bool>)null);
		}
		else
		{
			viewModel.CancelButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				CancelCommand_Execute(parameters.CancelAction);
			}, (Func<bool>)null);
		}
		viewModel.SubmitButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
		{
			SubmitCommand_Execute(parameters.ContinueAction);
		}, (Func<bool>)null);
		viewModel.OnCancelBrowseItemModel += ViewModel_OnCancelBrowseItemModel;
	}

	private static void ModifyCartAction_Execute()
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("NewCartConfirmModifyCart");
		OrchestrationService.ViewShow("new_cart_confirm_ada_action_update_view", _selectedOffer);
	}

	private static void ViewModel_OnCancelBrowseItemModel(DisplayProductModel model, object arg2)
	{
		BaseViewController.ProcessWPFHit();
		object obj = model?.Data;
		BaseViewController.AnalyticsService?.AddButtonPressEvent("NewCartConfirmRemoveProductOffer", $"delete {(obj as IRentalShoppingCartTitleItem)?.ProductId}");
		_removeOfferAction();
		UpdateCartDisplay();
	}

	private static void UpdateCartDisplay()
	{
		ConfigureTextAndVisibility(_viewModel, _parameters);
		ConfigureShoppingCartDisplay(_viewModel, _parameters);
	}

	private static void SubmitCommand_Execute(Action continueAction)
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("NewCartConfirmSubmit");
		OrchestrationService.ViewGoBackSafe("NewCartConfirm_Submit", popOnly: false);
		continueAction();
	}

	private static void CancelCommand_Execute(Action cancelAction)
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("NewCartConfirmSkipOffer");
		OrchestrationService.ViewGoBackSafe("NewCartConfirm_SkipOffer", popOnly: false);
		CancelWithConfirm(cancelAction);
	}

	private static void CancelWithConfirm(Action cancelAction)
	{
		BaseViewController.ProcessWPFHit();
		CustomerOffer selectedOffer = Redbox.Rental.Services.Session.Session.Instance.PromoOfferState.SelectedOffer;
		if (selectedOffer != null && selectedOffer.OfferScopeType == OfferScopeType.StoredRedeemable)
		{
			OrchestrationService.ViewShow("OfferAbandonConfirmView", new OfferAbandonConfirmInfo
			{
				IsInCart = false,
				ContinueAction = cancelAction
			});
		}
		else
		{
			cancelAction();
		}
	}

	private static void ConfigureTextAndVisibility(NewCartConfirmViewModel viewModel, NewCartConfirmviewParameters parameters)
	{
		if (viewModel != null)
		{
			OrchestrationService.SetCurrentUICulture();
			IRentalShoppingCart newCart = parameters.NewCart;
			_ = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession;
			viewModel.LabelTop = Resources.my_bag;
			viewModel.PriceHeaderText = Resources.summary;
			if (_isMultiNightOffer)
			{
				viewModel.BackButtonText = Resources.update_bag.ToUpper();
			}
			else
			{
				viewModel.BackButtonText = Resources.skip_offer_button_text.ToUpper();
			}
			TimeSpan value = Redbox.Rental.Services.Session.Session.Instance?.RentalReturnTime ?? TimeSpan.FromHours(21.0);
			DateTime dateTime = DateTime.Today.Add(value);
			string arg = ((value.Seconds == 0) ? dateTime.ToString("h:mm tt") : dateTime.ToString("h:mm:ss tt"));
			viewModel.PassiveSaleNote = string.Format(Resources.passive_sale_note, arg);
			viewModel.MDVFootnoteVisibility = ((!newCart.TitleItems.Any((IRentalShoppingCartTitleItem x) => x.MultiDisc)) ? Visibility.Collapsed : Visibility.Visible);
			viewModel.MDVFootnote = (newCart.TitleItems.Any((IRentalShoppingCartTitleItem x) => x.MultiDisc) ? Resources.multi_disc_vend_footnote : null);
			int num = (int)((newCart?.DefaultServiceFee ?? 0m) * 100m);
			viewModel.ServiceFeeText = string.Format(Resources.service_fee_text, num);
			viewModel.ServiceFeeTextVisibility = ((num <= 0) ? Visibility.Collapsed : Visibility.Visible);
			viewModel.PricingModel.DiscountLabelText = Resources.discount_label;
			viewModel.PricingModel.ServiceFeeLabelText = Resources.service_fee_label;
			viewModel.PricingModel.SubtotalLabelText = Resources.subtotal_label;
			viewModel.PricingModel.TaxLabelText = Resources.tax_label;
			viewModel.PricingModel.TotalLabelText = Resources.total_label;
			viewModel.PricingModel.SubtotalValueText = $"{newCart?.Totals?.SubTotal:C}";
			string text = (ServiceLocator.Instance.GetService<IDataService>()?.GetStore())?.State;
			viewModel.PricingModel.TaxLineVisibility = ((text == "SC" || text == "MD") ? Visibility.Collapsed : Visibility.Visible);
			viewModel.PricingModel.TaxValueText = $"{newCart?.Totals?.TaxAmount:C}";
			viewModel.PricingModel.TotalValueText = $"{newCart?.Totals?.GrandTotal:f2}";
			LogHelper.Instance.Log($"NewCartConfirmViewController: cart discount amount: {(newCart?.Totals?.DiscountAmountUsed).GetValueOrDefault()} ");
			string text2 = $"{((!((newCart?.Totals?.DiscountAmountUsed).GetValueOrDefault() != 0m)) ? ((decimal?)null) : newCart?.Totals?.DiscountAmountUsed):C}";
			if (!string.IsNullOrEmpty(text2))
			{
				text2 = $"-{text2}";
			}
			viewModel.PricingModel.DiscountValueText = text2;
			viewModel.PricingModel.DiscountLineVisibility = ((!((newCart?.Totals?.DiscountAmountUsed).GetValueOrDefault() > 0m)) ? Visibility.Collapsed : Visibility.Visible);
			viewModel.PricingModel.ServiceFeeValueText = $"{newCart?.Totals?.ServiceFee:C}";
			viewModel.PricingModel.ServiceFeeLineVisibility = ((!((newCart?.Totals?.ServiceFee).GetValueOrDefault() > 0m)) ? Visibility.Collapsed : Visibility.Visible);
			viewModel.SubmitButtonText = Resources.submit_button;
			viewModel.SkipOfferButtonVisibility = ((!BaseViewController.IsADAMode && (parameters.promoProduct != null || _isMultiNightOffer || _isPerksOffer)) ? Visibility.Collapsed : Visibility.Visible);
		}
	}

	private static void CreateAndStartTimer()
	{
		IIdleTimerService service = ServiceLocator.Instance.GetService<IIdleTimerService>();
		ITextToSpeechService service2 = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		int timeOut = ((service2 != null && service2.TTSEnabled) ? 90000 : OrchestrationService.TTSGetTimeout("medium", 15000));
		IIdleTimer obj = service?.CreateIdleTimer("NewCartConfirmViewTimer");
		obj?.Initialize(timeOut, delegate
		{
			BaseViewController.AnalyticsService?.AddTimeoutEvent(timeOut);
			MessagePopupViewController.ShowOfferTimeoutMessage(delegate
			{
				OrchestrationService.ViewGoBackSafe("NewCartConfirmViewController time out", popOnly: false);
				CancelWithConfirm(_timeoutAction);
			}, delegate
			{
				OrchestrationService.ViewGoBackSafe("NewCartConfirmViewController time out", popOnly: false);
				CancelWithConfirm(_cancelAction);
			}, delegate
			{
				OrchestrationService.ViewGoBackSafe("NewCartConfirmViewController time out", popOnly: false);
			});
		});
		obj?.Start();
	}

	private static void StopAndRemoveTimer()
	{
		IIdleTimerService service = ServiceLocator.Instance.GetService<IIdleTimerService>();
		(service?.GetIdleTimer("NewCartConfirmViewTimer"))?.Stop();
		service?.RemoveIdleTimer("PromoOfferBrowseViewTimer");
	}

	public static void LeaveView()
	{
		StopAndRemoveTimer();
	}
}
