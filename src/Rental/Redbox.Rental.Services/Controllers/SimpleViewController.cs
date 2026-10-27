using System;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.Model.Ads;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Services.KioskClientService.Ads;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class SimpleViewController : BaseViewController
{
	private static void ConfigureView(SimpleInfo info, bool useIdleTimer, Action<SpeechPart> ttsTextRefreshAction = null)
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl == null)
		{
			return;
		}
		if (useIdleTimer)
		{
			BaseViewController.StopIdleTimer();
		}
		SimpleModel model = info.Model;
		model.Info = info;
		model.CancelAction = info.CancelAction;
		model.ContinueAction = info.ContinueAction;
		model.OtherAction = info.OtherAction;
		if (info.CheckBoxChangedAction != null)
		{
			model.CheckBoxChangedAction = delegate(bool isChecked)
			{
				BaseViewController.ProcessWPFHit();
				info.CheckBoxChangedAction(isChecked);
			};
		}
		if (model.CancelAction != null)
		{
			model.CancelButtonCommand = DynamicRouting.RegisterRoutedCommand(Redbox.Rental.UI.Commands.CancelButtonCommand, CancelButtonCommand_Execute);
		}
		if (model.ContinueAction != null)
		{
			model.ContinueButtonCommand = DynamicRouting.RegisterRoutedCommand(Redbox.Rental.UI.Commands.ContinueButtonCommand, ContinueButtonCommand_Execute, ContinueButtonCommand_CanExecute);
		}
		if (model.OtherAction != null)
		{
			model.OtherButtonCommand = DynamicRouting.RegisterRoutedCommand(Redbox.Rental.UI.Commands.OtherButtonCommand, OtherButtonCommand_Execute);
		}
		viewUserControl.DataContext = model;
		if (ttsTextRefreshAction != null)
		{
			ConfigureTTS(model, ttsTextRefreshAction);
		}
		if (useIdleTimer)
		{
			BaseViewController.StartIdleTimer();
		}
		InvokeAnalytics(model, AnalyticsTypes.context);
	}

	public static void ConfigureTextView(SimpleInfo info)
	{
		BaseViewController.SetCurrentUICulture();
		if (info.Model == null)
		{
			info.Model = new SimpleModel
			{
				ContinueButtonText = Resources.joined_texing_btn_text?.ToUpper(),
				MessageText = Resources.joined_texing_message,
				TitleText = Resources.joined_texing_title
			};
		}
		ConfigureView(info, !info.UseIdleTimer.HasValue || info.UseIdleTimer.Value);
	}

	public static void ConfigureEmailReceiptView(EmailReceiptInfo info)
	{
		BaseViewController.SetCurrentUICulture();
		if (info.Model == null)
		{
			IAd ad = ((!BaseViewController.ApplicationState.IsADAAccessible) ? BaseViewController.GetAd(AdLocation.EmailNotification) : null);
			info.Model = new SimpleModel
			{
				ContinueButtonText = Resources.continue_button_mixed_case.ToUpper(),
				CommentText = Resources.email_receipt_comment,
				TitleText = Resources.email_receipt_title,
				MessageText = info.EmailAddress,
				BannerImage = ad?.Image,
				BannerImageVisibility = ((ad == null) ? Visibility.Collapsed : Visibility.Visible),
				AdImpression = ((ad != null) ? new AdImpression
				{
					Ad = ad,
					AdLocation = AdLocation.EmailNotification
				} : null)
			};
			info?.Model?.AdImpression?.Start();
		}
		ConfigureView(info, info.UseIdleTimer.HasValue && info.UseIdleTimer.Value);
	}

	public static void OnLeaveEmailReceiptView(IViewFrameInstance viewFrameInstance)
	{
		BaseViewController.RecordAdImpression(BaseViewController.GetViewModel<SimpleModel>(viewFrameInstance.Id)?.AdImpression);
	}

	public static void ConfigureSonySweepstakesConfirmationView(SonySweepstakesConfirmationViewInfo info)
	{
		BaseViewController.SetCurrentUICulture();
		if (info.Model == null)
		{
			if (info.ShowRedboxSweepstakes)
			{
				info.Model = new SimpleModel
				{
					TitleText = ((!string.IsNullOrEmpty(info.EmailAddress)) ? Resources.redbox_sweepstakes_confirmation_email_title_text : Resources.redbox_sweepstakes_confirmation_title_text),
					MessageText = ((!string.IsNullOrEmpty(info.EmailAddress)) ? string.Empty : Resources.sony_sweepstakes_confirmation_message_text),
					ContinueButtonText = Resources.got_it_button.ToUpper(),
					CommentText = ((!string.IsNullOrEmpty(info.EmailAddress)) ? Resources.redbox_sweepstakes_confirmation_email_comment_text : Resources.redbox_sweepstakes_confirmation_comment_text)
				};
			}
			else
			{
				info.Model = new SimpleModel
				{
					TitleText = ((!string.IsNullOrEmpty(info.EmailAddress)) ? Resources.sony_sweepstakes_confirmation_email_title_text : Resources.sony_sweepstakes_confirmation_title_text),
					MessageText = ((!string.IsNullOrEmpty(info.EmailAddress)) ? string.Empty : Resources.sony_sweepstakes_confirmation_message_text),
					ContinueButtonText = Resources.got_it_button.ToUpper(),
					CommentText = ((!string.IsNullOrEmpty(info.EmailAddress)) ? Resources.sony_sweepstakes_confirmation_email_comment_text : Resources.sony_sweepstakes_confirmation_comment_text)
				};
			}
		}
		ConfigureView(info, info.UseIdleTimer.HasValue && info.UseIdleTimer.Value);
	}

	public static void OnLeaveSonySweepstakesConfirmationView(IViewFrameInstance viewFrameInstance)
	{
	}

	internal static void ConfigurePerksSignupAlreadymemberView(SimpleInfoWithSignUpFlow info)
	{
		BaseViewController.SetCurrentUICulture();
		if (info.Model == null)
		{
			info.Model = new SimpleModel
			{
				ContinueButtonText = Resources.continue_button_text,
				MessageText = ((info.SignUpFlowContext == SignUpFlowContext.AfterPerksEquivalentOffer) ? Resources.perks_signup_already_member_dont_forget_message : Resources.perks_signup_already_member_message),
				TitleText = Resources.perks_signup_already_member_title
			};
		}
		SimpleModel simpleModel = info.Model;
		Action<SpeechPart> ttsTextRefreshAction = delegate(SpeechPart speechPart)
		{
			speechPart.Texts.Add(new TextPart
			{
				Text = simpleModel?.TitleText
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = simpleModel?.MessageText
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "To continue press ENTER"
			});
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "ENTER",
				Function = "TTSRepeatSpeechPart",
				Action = delegate
				{
					simpleModel?.ContinueButtonCommand?.Execute(null);
				}
			});
			speechPart.Loop = true;
		};
		ConfigureView(info, info.UseIdleTimer.HasValue && info.UseIdleTimer.Value, ttsTextRefreshAction);
	}

	internal static void ConfigurePerksSignupAlreadymemberOptinView(PerksSignupAlreadyMemberOptInViewInfo info)
	{
		if (Redbox.Rental.Services.Session.Session.Instance?.BrowseState?.SignupFlowState != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.SignupFlowState.MarketingOptInShown = true;
		}
		BaseViewController.SetCurrentUICulture();
		if (info.Model == null)
		{
			info.Model = new SimpleModel
			{
				ContinueButtonText = Resources.perks_signup_already_member_optin_accept_button,
				CancelButtonText = Resources.no_thanks_button_text,
				MessageText = ((info.SignUpFlowContext == SignUpFlowContext.AfterPerksEquivalentOffer) ? Resources.perks_signup_already_member_new_member_only_optin_message : ((info.PerksDeclined && info.SignUpFlowContext == SignUpFlowContext.AfterSwipe) ? string.Format(Resources.perks_declined_optin_message, info.EmailAddress) : ((!info.PerksDeclined && info.SignUpFlowContext == SignUpFlowContext.AfterSwipe) ? Resources.perks_signup_already_member_optin_message2 : Resources.perks_signup_already_member_optin_message))),
				TitleText = ((info.PerksDeclined && info.SignUpFlowContext == SignUpFlowContext.AfterSwipe) ? Resources.perks_declined_optin_title : Resources.perks_signup_already_member_optin_title),
				CommentText = ((info.PerksDeclined && info.SignUpFlowContext == SignUpFlowContext.AfterSwipe) ? Resources.perks_declined_optin_comment : ((!info.PerksDeclined && info.SignUpFlowContext == SignUpFlowContext.AfterSwipe) ? null : Resources.perks_signup_already_member_optin_comment)),
				FooterText = ((!info.PerksDeclined && info.SignUpFlowContext == SignUpFlowContext.AfterSwipe) ? string.Format(Resources.perks_signup_already_member_optin_footer, info.EmailAddress) : null),
				OtherButtonText = ((info.PerksDeclined && info.SignUpFlowContext == SignUpFlowContext.AfterSwipe) ? Resources.perks_declined_optin_legal : null)
			};
		}
		SimpleModel simpleModel = info.Model;
		Action<SpeechPart> ttsTextRefreshAction = delegate(SpeechPart speechPart)
		{
			speechPart.Texts.Add(new TextPart
			{
				Text = simpleModel?.CommentText
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = simpleModel?.TitleText
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = simpleModel?.MessageText
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = simpleModel?.FooterText
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "To accept press ENTER"
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "To go back press CANCEL"
			});
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "ENTER",
				Function = "TTSRepeatSpeechPart",
				Action = delegate
				{
					simpleModel?.ContinueButtonCommand?.Execute(null);
				}
			});
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "CANCEL",
				Function = "TTSRepeatSpeechPart",
				Action = delegate
				{
					simpleModel?.CancelButtonCommand?.Execute(null);
				}
			});
			speechPart.Loop = true;
		};
		ConfigureView(info, info.UseIdleTimer.HasValue && info.UseIdleTimer.Value, ttsTextRefreshAction);
	}

	private static void InvokeAnalytics(SimpleModel model, AnalyticsTypes analyticsType)
	{
		model?.Info?.AnalyticsAction?.Invoke(analyticsType);
	}

	public static void OtherButtonCommand_Execute()
	{
		BaseViewController.StopIdleTimer();
		SimpleModel viewModel = BaseViewController.GetViewModel<SimpleModel>();
		InvokeAnalytics(viewModel, AnalyticsTypes.button3Press);
		if (viewModel != null && viewModel.OtherAction != null)
		{
			viewModel.OtherAction();
		}
		else
		{
			OrchestrationService.ViewGoBackSafe("Problem: SimpleModel.OtherAction was not set", popOnly: false);
		}
		BaseViewController.ProcessWPFHit();
	}

	public static void CancelButtonCommand_Execute()
	{
		BaseViewController.StopIdleTimer();
		SimpleModel viewModel = BaseViewController.GetViewModel<SimpleModel>();
		InvokeAnalytics(viewModel, AnalyticsTypes.button2Press);
		if (viewModel != null && viewModel.CancelAction != null)
		{
			viewModel.CancelAction();
		}
		else
		{
			OrchestrationService.ViewGoBackSafe("Problem: SimpleModel.CancelAction was not set", popOnly: false);
		}
		BaseViewController.ProcessWPFHit();
	}

	public static void ContinueButtonCommand_Execute()
	{
		BaseViewController.StopIdleTimer();
		SimpleModel viewModel = BaseViewController.GetViewModel<SimpleModel>();
		InvokeAnalytics(viewModel, AnalyticsTypes.button1Press);
		if (viewModel != null && viewModel.ContinueAction != null)
		{
			viewModel.ContinueAction();
		}
		else
		{
			OrchestrationService.ViewGoBackSafe("Problem: SimpleModel.ContinueAction was not set", popOnly: false);
		}
		BaseViewController.ProcessWPFHit();
	}

	private static bool ContinueButtonCommand_CanExecute()
	{
		SimpleModel viewModel = BaseViewController.GetViewModel<SimpleModel>();
		if (viewModel != null && viewModel.CheckBoxChangedAction != null)
		{
			return viewModel.CheckboxChecked;
		}
		return true;
	}

	private static void ConfigureTTS(SimpleModel viewModel, Action<SpeechPart> speechPartRefreshAction)
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
				Name = "tts_simple_view"
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
				speechPartRefreshAction(part);
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
