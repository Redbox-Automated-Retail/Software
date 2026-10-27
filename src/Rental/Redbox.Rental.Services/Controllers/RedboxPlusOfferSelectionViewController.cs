using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Redbox.Core;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class RedboxPlusOfferSelectionViewController : BaseViewController
{
	public static void ConfigureView()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			BaseViewController.StopIdleTimer();
			RedboxPlusOfferSelectionViewModel redboxPlusOfferSelectionViewModel = new RedboxPlusOfferSelectionViewModel();
			ConfigureTextAndButtons(redboxPlusOfferSelectionViewModel);
			ConfigureActions(redboxPlusOfferSelectionViewModel);
			ConfigureTTS(redboxPlusOfferSelectionViewModel);
			viewUserControl.DataContext = redboxPlusOfferSelectionViewModel;
			BaseViewController.StartIdleTimer();
		}
	}

	private static void ConfigureTTS(RedboxPlusOfferSelectionViewModel redboxPlusOfferSelectionViewModel)
	{
	}

	private static void ConfigureActions(RedboxPlusOfferSelectionViewModel redboxPlusOfferSelectionViewModel)
	{
		if (redboxPlusOfferSelectionViewModel != null)
		{
			redboxPlusOfferSelectionViewModel.BackButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), BackButtonCommand_Execute);
			redboxPlusOfferSelectionViewModel.TermsButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), TermsButtonCommand_Execute);
			redboxPlusOfferSelectionViewModel.Tier1AcceptButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), Tier1AcceptButtonCommand_Execute);
			redboxPlusOfferSelectionViewModel.Tier2AcceptButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), Tier2AcceptButtonCommand_Execute);
			redboxPlusOfferSelectionViewModel.ScanQRCodeButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), ScanQRCodeButtonCommand_Execute);
		}
	}

	private static void ScanQRCodeButtonCommand_Execute()
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("RedboxPlusOfferSelectionScanQRCodeButton");
		RedboxPlusFunctions.ShowRedboxPlusQRCode();
	}

	private static void Tier1AcceptButtonCommand_Execute()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("RedboxPlusOfferSelectionTier1AcceptButton");
		ShowRedboxPlusDetailView("RBPLUS_OFFER_1");
	}

	private static void Tier2AcceptButtonCommand_Execute()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("RedboxPlusOfferSelectionTier2AcceptButton");
		ShowRedboxPlusDetailView("RBPLUS_OFFER_2");
	}

	private static void ShowRedboxPlusDetailView(string redboxPlusSubscriptionId)
	{
		List<ISubscriptionProduct> subscriptionProducts = null;
		ServiceLocator.Instance?.GetService<IDataService>()?.GetSubscriptionProducts(out subscriptionProducts);
		ISubscriptionProduct subscriptionProduct = subscriptionProducts.FirstOrDefault((ISubscriptionProduct x) => x.SubscriptionType == SubscriptionType.RedboxPlus && x.SubscriptionId == redboxPlusSubscriptionId);
		if (subscriptionProduct != null)
		{
			OrchestrationService.ViewShow("redbox_plus_offer_detail_view", subscriptionProduct);
		}
		else
		{
			LogHelper.Instance.Log("Error: unable to find Redbox+ product with id = " + redboxPlusSubscriptionId);
		}
	}

	private static void TermsButtonCommand_Execute()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("RedboxPlusOfferSelectionTermsButton");
		HelpViewController.ShowRedboxPlus();
	}

	private static void BackButtonCommand_Execute()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("RedboxPlusOfferSelectionBackButton");
		OrchestrationService.ViewGoBack("RedboxPlusOfferSelectionView Back Button", popOnly: false);
	}

	private static void ConfigureTextAndButtons(RedboxPlusOfferSelectionViewModel redboxPlusOfferSelectionViewModel)
	{
		if (redboxPlusOfferSelectionViewModel == null)
		{
			return;
		}
		OrchestrationService.SetCurrentUICulture();
		redboxPlusOfferSelectionViewModel.BackButtonText = Resources.back_button.ToUpper();
		redboxPlusOfferSelectionViewModel.TermsButtonText = Resources.redbox_plus_offer_selection_terms_button;
		redboxPlusOfferSelectionViewModel.LegalText = Resources.redbox_plus_offer_selection_legal_text;
		redboxPlusOfferSelectionViewModel.Tier1TitleText = Resources.redbox_plus_offer_selection_tier1_title;
		redboxPlusOfferSelectionViewModel.Tier1SubtitleText = Resources.redbox_plus_offer_selection_tier1_subtitle;
		redboxPlusOfferSelectionViewModel.Tier1Bullet1Text = Resources.redbox_plus_offer_selection_tier1_bullet1;
		redboxPlusOfferSelectionViewModel.Tier1Bullet2Text = Resources.redbox_plus_offer_selection_tier1_bullet2;
		redboxPlusOfferSelectionViewModel.Tier2TitleText = Resources.redbox_plus_offer_selection_tier2_title;
		redboxPlusOfferSelectionViewModel.Tier2SubtitleText = Resources.redbox_plus_offer_selection_tier2_subtitle;
		redboxPlusOfferSelectionViewModel.Tier2Bullet1Text = Resources.redbox_plus_offer_selection_tier2_bullet1;
		redboxPlusOfferSelectionViewModel.Tier2Bullet2Text = Resources.redbox_plus_offer_selection_tier2_bullet2;
		redboxPlusOfferSelectionViewModel.Tier2Bullet3Text = Resources.redbox_plus_offer_selection_tier2_bullet3;
		redboxPlusOfferSelectionViewModel.Tier1AcceptButtonText = Resources.redbox_plus_offer_selection_view_details_button_text;
		redboxPlusOfferSelectionViewModel.Tier2AcceptButtonText = Resources.redbox_plus_offer_selection_view_details_button_text;
		redboxPlusOfferSelectionViewModel.NotReadyYetText = string.Format(Resources.redbox_plus_offer_selection_not_ready_yet_text, Resources.redbox_plus_qr_code_disiplay_url);
		redboxPlusOfferSelectionViewModel.ScanQRCodeButtonText = Resources.redbox_plus_scan_qr_code_text;
		List<IKioskProduct> products = null;
		ServiceLocator.Instance.GetService<IDataService>()?.GetProducts(BrowseViewFilter.RedboxPlus, TitleFamily.Movies, TitleType.All, BrowseSort.SortDate, null, null, null, null, isADAView: false, null, null, out products);
		if (products == null)
		{
			return;
		}
		int num = 1;
		foreach (IKioskProduct item in products.Take(9))
		{
			redboxPlusOfferSelectionViewModel.GetType().GetProperty($"BoxArt{num++}")?.SetValue(redboxPlusOfferSelectionViewModel, OrchestrationService.GetProductBitmapImage(item));
		}
	}
}
