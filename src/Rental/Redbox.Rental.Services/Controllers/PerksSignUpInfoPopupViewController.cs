using System;
using System.Linq;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class PerksSignUpInfoPopupViewController : BaseViewController
{
	public static void ConfigureView()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			OrchestrationService.SetCurrentUICulture();
			PerksSignUpInfoPopupViewModel perksSignUpInfoPopupViewModel = new PerksSignUpInfoPopupViewModel
			{
				OKButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
				{
					OKButtonClickedAction();
				}, (Func<bool>)null)
			};
			PopulateModelText(perksSignUpInfoPopupViewModel);
			ConfigureTTS(perksSignUpInfoPopupViewModel);
			viewUserControl.DataContext = perksSignUpInfoPopupViewModel;
			BaseViewController.StartIdleTimer();
		}
	}

	private static void PopulateModelText(PerksSignUpInfoPopupViewModel viewModel)
	{
		viewModel.TitleText = Resources.perks_signup_info_title;
		viewModel.EarnHeading = Resources.perks_signup_info_earn_header;
		viewModel.UseHeading = Resources.perks_signup_info_use_header;
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.MarketingGeneral.DisplayUpdatedPerksPoints)
		{
			viewModel.EarnOption1Desc = Resources.perks_signup_info_earn_desc_1_updated;
			viewModel.EarnOption2Desc = Resources.perks_signup_info_earn_desc_2_updated;
			viewModel.EarnOption3Desc = string.Empty;
			viewModel.EarnOption4Desc = string.Empty;
			viewModel.EarnOption4DescVisibility = Visibility.Collapsed;
			viewModel.UseOption1Desc = Resources.perks_signup_info_use_desc_1_updated;
			viewModel.UseOption2Desc = string.Empty;
			viewModel.VisitHeading = Resources.perks_signup_info_visit_header_updated;
		}
		else
		{
			viewModel.EarnOption1Desc = string.Format(Resources.perks_signup_info_earn_desc_1 + " = " + Resources.perks_signup_info_earn_points_1);
			viewModel.EarnOption2Desc = string.Format(Resources.perks_signup_info_earn_desc_2 + " = " + Resources.perks_signup_info_earn_points_2);
			viewModel.EarnOption3Desc = string.Format(Resources.perks_signup_info_earn_desc_3 + " = " + Resources.perks_signup_info_earn_points_3);
			viewModel.EarnOption4Desc = string.Format(Resources.perks_signup_info_earn_desc_4 + " = " + Resources.perks_signup_info_earn_points_4);
			viewModel.EarnOption4DescVisibility = Visibility.Visible;
			viewModel.UseOption1Desc = string.Format(Resources.perks_signup_info_use_desc_1 + " = " + Resources.perks_signup_info_use_points_1);
			viewModel.UseOption2Desc = string.Format(Resources.perks_signup_info_use_desc_2 + " = " + Resources.perks_signup_info_use_points_2);
			viewModel.VisitHeading = Resources.perks_signup_info_visit_header;
		}
		int? num = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.LoyaltySession?.PointsBalance;
		viewModel.BalanceHeading = (num.HasValue ? string.Format(Resources.perks_signup_info_balance_header, num) : string.Empty);
		bool valueOrDefault = ServiceLocator.Instance.GetService<IViewService>()?.Stack?.Any((IViewFrameInstance vfi) => vfi?.ViewFrame?.ViewName == "recommended_titles_popup_view") == true;
		viewModel.OKButtonText = (valueOrDefault ? Resources.back_button.ToUpper() : Resources.okay.ToUpper());
		viewModel.OKButtonHorizontalAlignment = ((!valueOrDefault) ? HorizontalAlignment.Center : HorizontalAlignment.Left);
	}

	private static void OKButtonClickedAction()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("PerksSignupInfoPopupOK");
		OrchestrationService.ViewGoBackSafe("PerksSignupInfoView OK", popOnly: false);
	}

	private static void ConfigureTTS(PerksSignUpInfoPopupViewModel viewModel)
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
						viewModel?.OKButtonCommand.Execute(null);
					}
				});
				part.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "ENTER",
					Action = delegate
					{
						viewModel?.OKButtonCommand.Execute(null);
					}
				});
				part.Texts.Add(new TextPart
				{
					Text = "To go back the previous screen press Enter or Cancel."
				});
				part.Texts.Add(new TextPart
				{
					Text = "How points work: You can earn points by performing the following actions"
				});
				part.Texts.Add(new TextPart
				{
					Text = "Renting a DVD will earn 125 points per night"
				});
				part.Texts.Add(new TextPart
				{
					Text = "Renting a Blu-Ray will earn 125 points per night"
				});
				part.Texts.Add(new TextPart
				{
					Text = "Buying a used disc will earn points per $1 spent"
				});
				part.Texts.Add(new TextPart
				{
					Text = "Renting an On Demand title will earn at least 125 points per transaction"
				});
				part.Texts.Add(new TextPart
				{
					Text = "You can use your points to get free rentals"
				});
				part.Texts.Add(new TextPart
				{
					Text = "Use 1500 points per night to get a free 1-night DVD rental"
				});
				part.Texts.Add(new TextPart
				{
					Text = "Use 1750 points per night to get a free 1-night Blu-ray rental"
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
