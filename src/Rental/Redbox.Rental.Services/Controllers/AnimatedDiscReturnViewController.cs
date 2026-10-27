using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class AnimatedDiscReturnViewController : BaseViewController
{
	private static Action _timeoutAction;

	private static Action _continueAction;

	public static void ShowAnimatedDiscReturnView(Action timeoutAction, Action continueAction)
	{
		_continueAction = continueAction;
		_timeoutAction = timeoutAction;
		OrchestrationService.ViewShow("animated_disc_return", new AnimatedDiscReturnViewParameter
		{
			TimeoutAction = timeoutAction,
			GotItButtonAction = continueAction,
			ViewAnalyticsName = "animated_disc_return"
		});
	}

	public static void ConfigureAnimatedDiscReturnView()
	{
		if (BaseViewController.GetViewUserControl() is AnimatedDiscReturnViewUserControl animatedDiscReturnViewUserControl)
		{
			OrchestrationService.StopIdleTimer("idle_timer");
			OrchestrationService.SetCurrentUICulture();
			AnimatedDiscReturnViewModel viewModel = new AnimatedDiscReturnViewModel
			{
				Title = Resources.disc_return_error_title,
				Message = Resources.disc_return_error_message,
				TutorialPart1 = Resources.disc_return_tutorial_part1,
				TutorialPart2 = Resources.disc_return_tutorial_part2,
				TutorialPart3 = Resources.disc_return_tutorial_part3,
				GotItButtonText = Resources.got_it_button,
				OnTimeout = _timeoutAction,
				OnGotItButtonClicked = delegate
				{
					BaseViewController.AnalyticsService?.AddButtonPressEvent("AnimatedDiscReturnGotIt");
					_continueAction?.Invoke();
				}
			};
			viewModel.OnGetSpeechControl = delegate
			{
				ISpeechControl result = new SpeechControl
				{
					Name = "tts_animated_disc_return_view"
				};
				AddSpeechPartTitle(result, viewModel);
				AddSpeechPartMessage(result, viewModel);
				result.SpeechParts.ForEach(delegate(ISpeechPart x)
				{
					x.SpeechControl = result;
				});
				return result;
			};
			animatedDiscReturnViewUserControl.DataContext = viewModel;
			animatedDiscReturnViewUserControl.Timeout = GetDefaultTimeout();
		}
		else
		{
			LogHelper.Instance.Log("Could not load AnimatedDiscReturnViewUserControl");
		}
	}

	private static void AddBaseEvents(ISpeechPart speechPart)
	{
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = "CANCEL",
			Action = _continueAction
		});
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = "MINUS",
			Action = _continueAction
		});
	}

	private static void AddSpeechPartTitle(ISpeechControl result, AnimatedDiscReturnViewModel viewModel)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 0,
			Name = "Title",
			Loop = false,
			StartPause = 500,
			EndPause = 1000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			AddBaseEvents(speechPart);
			BaseViewController.GetViewUserControl();
			speechPart.Texts.Add(new TextPart
			{
				Text = viewModel?.Title
			});
		};
		result.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartMessage(ISpeechControl result, AnimatedDiscReturnViewModel viewModel)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 1,
			Name = "Message",
			Loop = true,
			StartPause = 0,
			EndPause = 5000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			AddBaseEvents(speechPart);
			BaseViewController.GetViewUserControl();
			speechPart.Texts.Add(new TextPart
			{
				Text = viewModel?.Message
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = viewModel?.TutorialPart1
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = viewModel?.TutorialPart2
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = viewModel?.TutorialPart3
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "If you wish to go back, press the CANCEL key."
			});
		};
		result.SpeechParts.Add(speechPart);
	}

	private static int GetDefaultTimeout()
	{
		return OrchestrationService.TTSGetTimeout("long", 40000);
	}
}
