using System;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class PerksConfirmViewController : BaseViewController
{
	public static void ConfigureView(PerksConfirmViewParameters parameters)
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			if (Redbox.Rental.Services.Session.Session.Instance?.BrowseState?.SignupFlowState != null)
			{
				Redbox.Rental.Services.Session.Session.Instance.BrowseState.SignupFlowState.WasPerksSignupOfferPresented = true;
			}
			OrchestrationService.SetCurrentUICulture();
			PerksConfirmViewModel perksConfirmViewModel = new PerksConfirmViewModel();
			perksConfirmViewModel.AcceptButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				AcceptButtonClickedAction(parameters.AcceptAction);
			}, (Func<bool>)null);
			perksConfirmViewModel.BackButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				BackButtonClickedAction(parameters.CancelAction);
			}, (Func<bool>)null);
			perksConfirmViewModel.TermsButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				TermsButtonClickedAction(parameters.IdleTimerName);
			}, (Func<bool>)null);
			PopulateModelText(perksConfirmViewModel, parameters.EmailAddress, parameters.SignUpFlowContext, viewUserControl);
			ConfigureTTS(perksConfirmViewModel, parameters.EmailAddress);
			viewUserControl.DataContext = perksConfirmViewModel;
		}
	}

	private static void TermsButtonClickedAction(string idleTimerName)
	{
		ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddButtonPressEvent("PerksConfirmTermsButton");
		BaseViewController.StopIdleTimer();
		HelpViewController.ShowJoinPerks(delegate
		{
			ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddButtonPressEvent("HelpBack");
			OrchestrationService.ViewGoBackSafe("Help back", popOnly: false);
		}, idleTimerName);
	}

	private static void AcceptButtonClickedAction(Action acceptAction)
	{
		ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddButtonPressEvent("PerksConfirmAcceptAndSignUpButton");
		acceptAction?.Invoke();
	}

	private static void PopulateModelText(PerksConfirmViewModel viewModel, string emailAddress, SignUpFlowContext signUpFlowContext, FrameworkElement viewUserControl)
	{
		viewModel.TitleText = ((signUpFlowContext == SignUpFlowContext.AfterSwipe || signUpFlowContext == SignUpFlowContext.AfterSignIn) ? Resources.perks_confirm_score_a_free_1_title_text : Resources.perks_confirm_title_text);
		viewModel.TitleStyle = WPFFunctions.GetStyle(viewUserControl, (OrchestrationService.IsCultureSpanish() && (signUpFlowContext == SignUpFlowContext.AfterSwipe || signUpFlowContext == SignUpFlowContext.AfterSignIn)) ? "font_museo_slab_900_40" : "font_museo_slab_900_50");
		viewModel.MessageText = string.Format((signUpFlowContext == SignUpFlowContext.AfterSwipe || signUpFlowContext == SignUpFlowContext.AfterSignIn) ? Resources.perks_confirm_sign_up_message_text : Resources.perks_confirm_message_text, emailAddress);
		viewModel.TermsText = Resources.perks_confirm_terms_text;
		viewModel.AcceptButtonText = Resources.perks_confirm_accept_button_text;
		viewModel.BackButtonText = ((signUpFlowContext == SignUpFlowContext.AfterSwipe || signUpFlowContext == SignUpFlowContext.AfterSignIn) ? Resources.no_thanks_button : Resources.back_button);
		viewModel.TermsButtonText = Resources.terms_link_text;
		viewModel.LegalText = ((signUpFlowContext != SignUpFlowContext.AfterSwipe && signUpFlowContext != SignUpFlowContext.AfterSignIn) ? null : (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.MarketingGeneral.DisplayUpdatedPerksPoints ? Resources.sign_in_legal_text_updated : Resources.sign_in_legal_text));
	}

	private static void BackButtonClickedAction(Action cancelAction)
	{
		ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddButtonPressEvent("PerksConfirmBackButton");
		cancelAction?.Invoke();
	}

	private static void ConfigureTTS(PerksConfirmViewModel viewModel, string email)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service == null || !service.TTSEnabled || viewModel == null)
		{
			return;
		}
		viewModel.ProcessGetSpeechControl = delegate
		{
			SpeechControl obj = new SpeechControl
			{
				Name = "tts_perks_sign_up_info_view"
			};
			SpeechPart part = new SpeechPart
			{
				Name = "Intro",
				Loop = true,
				StartPause = 250,
				EndPause = 10000
			};
			part.Refresh = delegate
			{
				part.Clear();
				part.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "CANCEL",
					Action = delegate
					{
						viewModel?.BackButtonCommand.Execute(null);
					}
				});
				part.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "ENTER",
					Action = delegate
					{
						viewModel?.AcceptButtonCommand.Execute(null);
					}
				});
				part.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "TAB",
					Action = delegate
					{
						viewModel?.TermsButtonCommand.Execute(null);
					}
				});
				part.Texts.Add(new TextPart
				{
					Text = "Just to confirm."
				});
				part.Texts.Add(new TextPart
				{
					Text = "Sign up using " + email
				});
				part.Texts.Add(new TextPart
				{
					Text = "By pressing enter to Accept & Sign Up you are agreeing to the Rewards Terms, Redbox Terms of Use, Privacy Policy and also to get Redbox emails and special offers. Payment card required to use Redbox Perks Points."
				});
				part.Texts.Add(new TextPart
				{
					Text = "To Accept and Sign Up press Enter."
				});
				part.Texts.Add(new TextPart
				{
					Text = "To go back the previous screen press Cancel."
				});
				part.Texts.Add(new TextPart
				{
					Text = "To hear the rewards terms, press tab."
				});
			};
			obj.SpeechParts.Add(part);
			int i = 0;
			obj.SpeechParts.ForEach(delegate(ISpeechPart x)
			{
				x.Sequence = i++;
			});
			return obj;
		};
	}
}
