using System.Collections.ObjectModel;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class OptionSelectionDialogController : BaseViewController
{
	public static void ConfigureDialog(OptionSelectionDialogInfo parameter)
	{
		BaseViewController.StopIdleTimer();
		CreateAndStartTimer();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			OptionSelectionDialogViewModel optionSelectionDialogViewModel = GetViewModel();
			if (optionSelectionDialogViewModel == null)
			{
				optionSelectionDialogViewModel = new OptionSelectionDialogViewModel();
			}
			optionSelectionDialogViewModel.HeaderText = parameter.HeaderText;
			optionSelectionDialogViewModel.DescriptionText = parameter.DescriptionText;
			optionSelectionDialogViewModel.CancelButtonText = parameter.CancelButtonText;
			optionSelectionDialogViewModel.CancelCommand = parameter.CancelCommand;
			optionSelectionDialogViewModel.ButtonData = parameter.ButtonData;
			ObservableCollection<ButtonData> buttonData = optionSelectionDialogViewModel.ButtonData;
			if (buttonData != null && buttonData.Count == 3)
			{
				optionSelectionDialogViewModel.DialogWidth = 920;
			}
			else
			{
				optionSelectionDialogViewModel.DialogWidth = 720;
			}
			ConfigureTTS(optionSelectionDialogViewModel);
			viewUserControl.DataContext = optionSelectionDialogViewModel;
		}
	}

	private static void ConfigureTTS(OptionSelectionDialogViewModel viewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null && service.TTSEnabled)
		{
			viewModel.OnGetSpeechControl = () => OptionSelectionDialogViewTextToSpeech.GetSpeechControl(GetViewModel);
		}
	}

	private static OptionSelectionDialogViewModel GetViewModel()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			return viewUserControl.DataContext as OptionSelectionDialogViewModel;
		}
		return null;
	}

	private static void CreateAndStartTimer()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		int timeOut = OrchestrationService.TTSGetTimeout("medium", 15000);
		(service?.CreateTimer("OptionSelectionDialogViewTimer", timeOut, timeOut, delegate
		{
			BaseViewController.AnalyticsService?.AddTimeoutEvent(timeOut);
			OrchestrationService.ViewGoBackSafe("OptionSelectionDialogViewController time out", popOnly: false);
		}))?.Start();
	}

	private static void StopAndRemoveTimer()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		(service?.GetTimer("OptionSelectionDialogViewTimer"))?.Stop();
		service?.RemoveTimer("OptionSelectionDialogViewTimer");
	}

	public static void LeaveDialog()
	{
		StopAndRemoveTimer();
		BaseViewController.StartIdleTimer();
	}
}
