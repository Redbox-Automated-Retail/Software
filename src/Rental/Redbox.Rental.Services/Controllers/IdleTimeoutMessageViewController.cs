using System;
using System.Windows;
using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class IdleTimeoutMessageViewController : BaseViewController
{
	public const string ViewIdleTimerName = "idle_timeout_view_timer";

	private static IIdleTimerService IdleTimerService => ServiceLocator.Instance.GetService<IIdleTimerService>();

	public static void ConfigureView(IViewFrameInstance viewFrameInstance)
	{
		IdleTimeoutMessageViewParameter idleTimeoutMessageViewParameter = viewFrameInstance.Parameter as IdleTimeoutMessageViewParameter;
		if (idleTimeoutMessageViewParameter == null)
		{
			idleTimeoutMessageViewParameter = (IdleTimeoutMessageViewParameter)(viewFrameInstance.Parameter = new IdleTimeoutMessageViewParameter
			{
				IdleTimerName = "idle_timer",
				NoAction = delegate
				{
					OrchestrationService.StopIdleTimer("idle_timeout_view_timer");
					OrchestrationService.InvokeIdleTimerExpireSessionCallback();
				},
				YesAction = delegate
				{
					OrchestrationService.StopIdleTimer("idle_timeout_view_timer");
					ServiceLocator.Instance.GetService<IViewService>();
					OrchestrationService.ViewGoBackSafe("need_more_time:second_button", popOnly: false);
				}
			});
		}
		OrchestrationService.SuspendIdleTimer(idleTimeoutMessageViewParameter.IdleTimerName);
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			IdleTimeoutMessageViewModel idleTimeoutMessageViewModel = new IdleTimeoutMessageViewModel();
			OrchestrationService.SetCurrentUICulture();
			idleTimeoutMessageViewModel.ButtonNoText = Resources.quit_button_english.ToUpper();
			idleTimeoutMessageViewModel.ButtonYesText = Resources.im_here_button.ToUpper();
			idleTimeoutMessageViewModel.MessageText = Resources.more_time_prompt_message;
			idleTimeoutMessageViewModel.OnButtonNoClicked = idleTimeoutMessageViewParameter.NoAction;
			idleTimeoutMessageViewModel.OnButtonYesClicked = idleTimeoutMessageViewParameter.YesAction;
			idleTimeoutMessageViewModel.OnGetSpeechControl = () => GetSpeechControl(idleTimeoutMessageViewModel);
			ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
			int num = 15000;
			num = service?.GetTimeout("medium", num) ?? num;
			ConfigureTimeoutMessageViewIdleTimer(num, idleTimeoutMessageViewParameter.NoAction);
			viewUserControl.DataContext = idleTimeoutMessageViewModel;
			CommandManager.InvalidateRequerySuggested();
		}
	}

	private static void ConfigureTimeoutMessageViewIdleTimer(int timeout, Action timeoutAction)
	{
		IIdleTimer obj = IdleTimerService?.CreateIdleTimer("idle_timeout_view_timer");
		obj?.Initialize(timeout, delegate
		{
			OrchestrationService.StopIdleTimer("idle_timeout_view_timer");
			LogHelper.Instance.Log("Idle timeout message view: idle timer expired");
			timeoutAction();
		});
		obj?.Start();
	}

	private static ISpeechControl GetSpeechControl(IdleTimeoutMessageViewModel idleTimeoutMessageViewModel)
	{
		SpeechControl obj = new SpeechControl
		{
			Name = "tts_idle_timeout_message_view"
		};
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 0,
			Name = "Main",
			StartPause = 250,
			EndPause = 5000,
			Loop = true
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			speechPart.Texts.Add(new TextPart
			{
				Text = idleTimeoutMessageViewModel.MessageText
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "Press cancel for " + idleTimeoutMessageViewModel.ButtonNoText
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "Press enter for " + idleTimeoutMessageViewModel.ButtonYesText
			});
			speechPart.Events.Add(new MapKeyPress
			{
				KeyCode = "TAB",
				Function = "TTSNextAudioSnippet"
			});
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "CANCEL",
				Action = delegate
				{
					idleTimeoutMessageViewModel.OnButtonNoClicked();
				}
			});
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "ENTER",
				Action = delegate
				{
					idleTimeoutMessageViewModel.OnButtonYesClicked();
				}
			});
		};
		((ISpeechControl)obj).SpeechParts.Add(speechPart);
		return obj;
	}

	public static void CleanupView(IViewFrameInstance viewFrameInstance)
	{
		OrchestrationService.RemoveIdleTimer("idle_timeout_view_timer");
		IdleTimeoutMessageViewParameter obj = viewFrameInstance.Parameter as IdleTimeoutMessageViewParameter;
		IdleTimeoutMessageViewModel idleTimeoutMessageViewModel = obj?.ViewModel;
		if (idleTimeoutMessageViewModel != null)
		{
			idleTimeoutMessageViewModel.OnButtonNoClicked = null;
		}
		if (BaseViewController.GetViewUserControl() is IdleTimeoutMessageViewUserControl idleTimeoutMessageViewUserControl)
		{
			idleTimeoutMessageViewUserControl.DataContext = null;
		}
		OrchestrationService.ResumeIdleTimer(obj?.IdleTimerName);
	}
}
