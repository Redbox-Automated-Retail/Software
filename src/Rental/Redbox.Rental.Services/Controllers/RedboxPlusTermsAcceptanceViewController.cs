using System.Windows;
using Redbox.Rental.Services.RedboxPlus;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class RedboxPlusTermsAcceptanceViewController : BaseViewController
{
	public static void ConfigureRedboxPlusTermsAcceptanceView(RedboxPlusTermsAcceptanceViewParameters redboxPlusTermsAcceptanceViewParameters)
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			BaseViewController.StopIdleTimer();
			RedboxPlusTermsAcceptanceViewModel redboxPlusTermsAcceptanceViewModel = new RedboxPlusTermsAcceptanceViewModel
			{
				RedboxPlusSubscriptionProduct = redboxPlusTermsAcceptanceViewParameters.RedboxPlusSubscriptionProduct
			};
			ConfigureText(redboxPlusTermsAcceptanceViewModel, redboxPlusTermsAcceptanceViewParameters.EmailAddress);
			redboxPlusTermsAcceptanceViewModel.CheckMarkCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), CheckMarkCommand_Execute);
			redboxPlusTermsAcceptanceViewModel.AcceptButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), AcceptButtonCommand_Execute);
			redboxPlusTermsAcceptanceViewModel.BackButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), BackButtonCommand_Execute);
			redboxPlusTermsAcceptanceViewModel.TermsAndPrivacyButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), TermsAndPrivacyButtonCommand_Execute);
			viewUserControl.DataContext = redboxPlusTermsAcceptanceViewModel;
			BaseViewController.StartIdleTimer();
		}
	}

	private static void ConfigureText(RedboxPlusTermsAcceptanceViewModel redboxPlusTermsAcceptanceViewModel, string emailAddress)
	{
		if (redboxPlusTermsAcceptanceViewModel != null)
		{
			OrchestrationService.SetCurrentUICulture();
			redboxPlusTermsAcceptanceViewModel.TitleText = Resources.redbox_plus_terms_acceptance_title_text;
			redboxPlusTermsAcceptanceViewModel.SubtitleText = ((Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.IsLoggedIn == true) ? Resources.redbox_plus_terms_acceptance_subtitle_text_existing_account : Resources.redbox_plus_terms_acceptance_subtitle_text_new_account);
			redboxPlusTermsAcceptanceViewModel.EmailText = emailAddress;
			redboxPlusTermsAcceptanceViewModel.TermsHeader = Resources.redbox_plus_terms_acceptance_terms_header;
			redboxPlusTermsAcceptanceViewModel.TermsText = redboxPlusTermsAcceptanceViewModel.RedboxPlusSubscriptionProduct.AcceptanceTerms;
			ConfigureCheckMarkAndAcceptButton(redboxPlusTermsAcceptanceViewModel);
		}
	}

	private static void ConfigureCheckMarkAndAcceptButton(RedboxPlusTermsAcceptanceViewModel redboxPlusTermsAcceptanceViewModel)
	{
		OrchestrationService.SetCurrentUICulture();
		bool isRedboxPlusTermsAcceptanceCheckMarkChecked = Redbox.Rental.Services.Session.Session.Instance.BrowseState.IsRedboxPlusTermsAcceptanceCheckMarkChecked;
		redboxPlusTermsAcceptanceViewModel.CheckMarkVisibility = ((!isRedboxPlusTermsAcceptanceCheckMarkChecked) ? Visibility.Hidden : Visibility.Visible);
		redboxPlusTermsAcceptanceViewModel.AcceptButtonText = (isRedboxPlusTermsAcceptanceCheckMarkChecked ? Resources.redbox_plus_terms_acceptance_button_enabled_text : Resources.redbox_plus_terms_acceptance_button_disabled_text);
		redboxPlusTermsAcceptanceViewModel.AcceptButtonIsEnabled = isRedboxPlusTermsAcceptanceCheckMarkChecked;
	}

	private static void CheckMarkCommand_Execute()
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("RedboxPlusTermsAcceptanceCheckMark");
		RedboxPlusTermsAcceptanceViewModel viewModel = BaseViewController.GetViewModel<RedboxPlusTermsAcceptanceViewModel>();
		if (viewModel != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.IsRedboxPlusTermsAcceptanceCheckMarkChecked = !Redbox.Rental.Services.Session.Session.Instance.BrowseState.IsRedboxPlusTermsAcceptanceCheckMarkChecked;
			ConfigureCheckMarkAndAcceptButton(viewModel);
		}
	}

	private static void AcceptButtonCommand_Execute()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("RedboxPlusTermsAcceptanceAcceptButton");
		RedboxPlusTermsAcceptanceViewModel viewModel = BaseViewController.GetViewModel<RedboxPlusTermsAcceptanceViewModel>();
		if (viewModel != null)
		{
			RedboxPlusSignupService.AddRedboxPlusSubscriptionPrerequisites(viewModel.RedboxPlusSubscriptionProduct);
		}
	}

	private static void BackButtonCommand_Execute()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("RedboxPlusTermsAcceptanceBackButton");
		OrchestrationService.UnwindToSafe("redbox_plus_offer_detail_view", discard: true, "RedboxPlusTermsAcceptanceBackButton");
	}

	private static void TermsAndPrivacyButtonCommand_Execute()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("RedboxPlusTermsAcceptanceTermsAndPrivacyButton");
		HelpViewController.ShowRedboxPlus();
	}
}
