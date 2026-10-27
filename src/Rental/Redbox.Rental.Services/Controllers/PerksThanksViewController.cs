using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

internal class PerksThanksViewController : BaseViewController
{
	public static void ConfigureView(PerksThanksInfo info)
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			OrchestrationService.SetCurrentUICulture();
			PerksThanksViewModel perksThanksViewModel = new PerksThanksViewModel();
			PopulateModel(perksThanksViewModel, info);
			ConfigureTTS(perksThanksViewModel);
			viewUserControl.DataContext = perksThanksViewModel;
		}
	}

	private static void PopulateModel(PerksThanksViewModel viewModel, PerksThanksInfo info)
	{
		viewModel.ContinueAction = info.ContinueAction;
		viewModel.AnalyticsAction = info.AnalyticsAction;
		viewModel.ContinueButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
		{
			OKButtonClickedAction();
		}, (Func<bool>)null);
		viewModel.ContinueButtonText = Resources.continue_button_mixed_case;
		viewModel.TitleText = info.TitleText;
		viewModel.CommentText = info.CommentText;
		viewModel.MessageText = info.MessageText;
		PerksThanksInfo.ImageType topImage = info.TopImage;
		if (topImage != PerksThanksInfo.ImageType.PerksStar && topImage == PerksThanksInfo.ImageType.CheckMark)
		{
			viewModel.DisplayImage = new BitmapImage(new Uri("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/done-icon.png"));
		}
		else
		{
			viewModel.DisplayImage = new BitmapImage(new Uri("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/member-icon.png"));
		}
		if (info.ShowPerksIcons)
		{
			viewModel.PerksIconsVisibility = Visibility.Visible;
			viewModel.PerksFreeRentalText = Resources.perks_thanks_free_rental_icon_text;
			viewModel.PerksDealsText = Resources.offer_info_special_deals;
			viewModel.PerksBdayGiftText = Resources.offer_info_bday_gift;
			viewModel.TitleMargin = new Thickness(0.0, 36.0, 0.0, 0.0);
		}
	}

	private static void OKButtonClickedAction()
	{
		PerksThanksViewModel viewModel = BaseViewController.GetViewModel<PerksThanksViewModel>();
		InvokeAnalytics(viewModel, AnalyticsTypes.button1Press);
		if (viewModel != null && viewModel.ContinueAction != null)
		{
			viewModel.ContinueAction();
		}
		else
		{
			OrchestrationService.ViewGoBackSafe("Problem: PerksThanksViewModel.ContinueAction was not set", popOnly: false);
		}
		BaseViewController.ProcessWPFHit();
	}

	private static void InvokeAnalytics(PerksThanksViewModel model, AnalyticsTypes analyticsType)
	{
		model?.AnalyticsAction?.Invoke(analyticsType);
	}

	private static void ConfigureTTS(PerksThanksViewModel viewModel)
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
				part.Texts.Add(new TextPart
				{
					Text = viewModel?.TitleText
				});
				part.Texts.Add(new TextPart
				{
					Text = viewModel?.MessageText
				});
				if (viewModel.PerksIconsVisibility == Visibility.Visible)
				{
					part.Texts.Add(new TextPart
					{
						Text = "Your benefits include: " + viewModel.PerksFreeRentalText + ", " + viewModel.PerksDealsText + ", and " + viewModel.PerksBdayGiftText
					});
				}
				part.Texts.Add(new TextPart
				{
					Text = viewModel?.CommentText
				});
				part.Texts.Add(new TextPart
				{
					Text = "To continue press ENTER"
				});
				part.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "ENTER",
					Function = "TTSRepeatSpeechPart",
					Action = delegate
					{
						viewModel?.ContinueButtonCommand?.Execute(null);
					}
				});
				part.Loop = true;
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
