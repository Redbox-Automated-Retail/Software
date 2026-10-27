using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Redbox.Core;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Services.RedboxPlus;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class RedboxPlusOfferDetailViewController : BaseViewController
{
	public static void ConfigureRedboxPlusOfferDetailView(IRedboxPlusSubscriptionProduct redboxPlusSubscriptionProduct)
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			BaseViewController.StopIdleTimer();
			RedboxPlusOfferDetailViewModel redboxPlusOfferDetailViewModel = new RedboxPlusOfferDetailViewModel
			{
				RedboxPlusSubscriptionProduct = redboxPlusSubscriptionProduct
			};
			ConfigureTextAndButtons(redboxPlusOfferDetailViewModel);
			ConfigureActions(redboxPlusOfferDetailViewModel);
			viewUserControl.DataContext = redboxPlusOfferDetailViewModel;
			BaseViewController.StartIdleTimer();
		}
	}

	private static void ConfigureActions(RedboxPlusOfferDetailViewModel redboxPlusOfferDetailViewModel)
	{
		if (redboxPlusOfferDetailViewModel != null)
		{
			redboxPlusOfferDetailViewModel.BackButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), BackButtonCommand_Execute);
			redboxPlusOfferDetailViewModel.TermsButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), TermsButtonCommand_Execute);
			redboxPlusOfferDetailViewModel.AcceptButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), AcceptButtonCommand_Execute);
		}
	}

	private static void AcceptButtonCommand_Execute()
	{
		RedboxPlusOfferDetailViewModel viewModel = BaseViewController.GetViewModel<RedboxPlusOfferDetailViewModel>();
		if (ProductAlreadyInTheCart(viewModel))
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("RedboxPlusOfferDetailViewInBagButton");
			OrchestrationService.ViewGoBackSafe("RedboxPlusOfferDetailView View In Bag Button", popOnly: false);
			return;
		}
		BaseViewController.AnalyticsService?.AddButtonPressEvent("RedboxPlusOfferDetailAcceptButton");
		if (viewModel?.RedboxPlusSubscriptionProduct != null)
		{
			if (Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.IsLoggedIn == true)
			{
				RedboxPlusSignupService.StartRedboxPlusSignup(viewModel?.RedboxPlusSubscriptionProduct);
			}
			else
			{
				PersonalizationFunctions.ShowRedboxPlusSignInSignUpDialog(viewModel?.RedboxPlusSubscriptionProduct);
			}
		}
		else
		{
			LogHelper.Instance.Log("Error in RedboxPlusOfferDetailViewController.AcceptButtonCommand_Execute.  Missiing RedboxPlusSubscriptionProduct");
		}
	}

	private static void TermsButtonCommand_Execute()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("RedboxPlusOfferDetailTermsButton");
		HelpViewController.ShowRedboxPlus();
	}

	private static void BackButtonCommand_Execute()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("RedboxPlusOfferDetailBackButton");
		OrchestrationService.ViewGoBack("RedboxPlusOfferDetailView Back Button", popOnly: false);
	}

	private static bool ProductAlreadyInTheCart(RedboxPlusOfferDetailViewModel redboxPlusOfferDetailViewModel)
	{
		return (BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart)?.HasSubscriptionItem(redboxPlusOfferDetailViewModel?.RedboxPlusSubscriptionProduct) ?? false;
	}

	private static void ConfigureTextAndButtons(RedboxPlusOfferDetailViewModel redboxPlusOfferDetailViewModel)
	{
		if (redboxPlusOfferDetailViewModel == null)
		{
			return;
		}
		redboxPlusOfferDetailViewModel.BackButtonText = Resources.back_uppercase_button;
		redboxPlusOfferDetailViewModel.BackButtonVisibility = (ProductAlreadyInTheCart(redboxPlusOfferDetailViewModel) ? Visibility.Collapsed : Visibility.Visible);
		redboxPlusOfferDetailViewModel.TermsButtonText = Resources.redbox_plus_offer_detail_view_terms_button_text;
		redboxPlusOfferDetailViewModel.AcceptButtonText = (ProductAlreadyInTheCart(redboxPlusOfferDetailViewModel) ? Resources.redbox_plus_offer_accept_button_text_already_in_cart : Resources.redbox_plus_offer_detail_view_accept_button_text);
		redboxPlusOfferDetailViewModel.AcceptButtonLegalText = (ProductAlreadyInTheCart(redboxPlusOfferDetailViewModel) ? Resources.redbox_plus_offer_detail_view_accept_button_legal_text_already_in_cart : Resources.redbox_plus_offer_detail_view_accept_button_legal_text);
		redboxPlusOfferDetailViewModel.BoxArtHeaderText = Resources.redbox_plus_offer_detail_view_boxart_title_text;
		int num = 7;
		for (int i = 1; i <= num; i++)
		{
			redboxPlusOfferDetailViewModel.GetType().GetProperty($"BoxArt{i}Visibility")?.SetValue(redboxPlusOfferDetailViewModel, Visibility.Collapsed);
		}
		List<IKioskProduct> products = null;
		ServiceLocator.Instance.GetService<IDataService>()?.GetProducts(BrowseViewFilter.RedboxPlus, TitleFamily.Movies, TitleType.All, BrowseSort.SortDate, null, null, null, null, isADAView: false, null, null, out products);
		if (products != null)
		{
			int num2 = 1;
			foreach (IKioskProduct item in products.Take(num))
			{
				redboxPlusOfferDetailViewModel.GetType().GetProperty($"BoxArt{num2}")?.SetValue(redboxPlusOfferDetailViewModel, OrchestrationService.GetProductBitmapImage(item));
				redboxPlusOfferDetailViewModel.GetType().GetProperty($"BoxArt{num2}Visibility")?.SetValue(redboxPlusOfferDetailViewModel, Visibility.Visible);
				num2++;
			}
		}
		if (redboxPlusOfferDetailViewModel.RedboxPlusSubscriptionProduct != null)
		{
			redboxPlusOfferDetailViewModel.MainStackPanelMargin = redboxPlusOfferDetailViewModel.RedboxPlusSubscriptionProduct.MainStackPanelMargin;
			redboxPlusOfferDetailViewModel.TitleText = redboxPlusOfferDetailViewModel.RedboxPlusSubscriptionProduct.TitleText;
			redboxPlusOfferDetailViewModel.SubtitleText = redboxPlusOfferDetailViewModel.RedboxPlusSubscriptionProduct.SubtitleText;
			redboxPlusOfferDetailViewModel.LegalText = redboxPlusOfferDetailViewModel.RedboxPlusSubscriptionProduct.LegalText;
			redboxPlusOfferDetailViewModel.Benefit1HeaderText = redboxPlusOfferDetailViewModel.RedboxPlusSubscriptionProduct.Benefit1Header;
			redboxPlusOfferDetailViewModel.Benefit1DetailText = redboxPlusOfferDetailViewModel.RedboxPlusSubscriptionProduct.Benefit1Detail;
			redboxPlusOfferDetailViewModel.Benefit2HeaderText = redboxPlusOfferDetailViewModel.RedboxPlusSubscriptionProduct.Benefit2Header;
			redboxPlusOfferDetailViewModel.Benefit2DetailText = redboxPlusOfferDetailViewModel.RedboxPlusSubscriptionProduct.Benefit2Detail;
			redboxPlusOfferDetailViewModel.Benefit3Visibility = (string.IsNullOrEmpty(redboxPlusOfferDetailViewModel.RedboxPlusSubscriptionProduct.Benefit3Header) ? Visibility.Collapsed : Visibility.Visible);
			redboxPlusOfferDetailViewModel.Benefit3HeaderText = redboxPlusOfferDetailViewModel.RedboxPlusSubscriptionProduct.Benefit3Header;
			redboxPlusOfferDetailViewModel.Benefit3DetailText = redboxPlusOfferDetailViewModel.RedboxPlusSubscriptionProduct.Benefit3Detail;
			redboxPlusOfferDetailViewModel.BottomBoxArtDisplayVisibility = ((!redboxPlusOfferDetailViewModel.RedboxPlusSubscriptionProduct.ShowBottomBoxArtDisplay) ? Visibility.Collapsed : Visibility.Visible);
			redboxPlusOfferDetailViewModel.TopBoxArtDisplayVisibility = ((!redboxPlusOfferDetailViewModel.RedboxPlusSubscriptionProduct.ShowTopBoxArtDisplay) ? Visibility.Collapsed : Visibility.Visible);
		}
	}
}
