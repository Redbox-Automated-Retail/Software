using System;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class PerksOfferDetailsViewController : BaseViewController
{
	public static void ConfigurePerksOfferDetailsView(PerksOfferListItemModel perksOfferModel)
	{
		BaseViewController.StopIdleTimer();
		CreateAndStartTimer();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			PerksOfferDetailsViewModel perksOfferDetailsViewModel = new PerksOfferDetailsViewModel();
			perksOfferDetailsViewModel.OfferValueText = perksOfferModel.OfferValueText;
			perksOfferDetailsViewModel.OfferUnitsText = perksOfferModel.OfferUnitsText;
			if (perksOfferModel.CurrentValue.HasValue && perksOfferModel.RemainderValue.HasValue && perksOfferModel.MaxValue.HasValue && perksOfferModel.MaxValue > 0 && perksOfferModel.RemainderValue > 0 && perksOfferModel.RemainderValue <= perksOfferModel.MaxValue)
			{
				perksOfferDetailsViewModel.ProgressText = $"{perksOfferModel.CurrentValue.Value} down, {perksOfferModel.RemainderValue.Value} more to go";
			}
			perksOfferDetailsViewModel.CurrentValue = perksOfferModel.CurrentValue;
			perksOfferDetailsViewModel.MaxValue = perksOfferModel.MaxValue;
			perksOfferDetailsViewModel.NameText = perksOfferModel.Name;
			perksOfferDetailsViewModel.DescriptionText = perksOfferModel.Description;
			if (perksOfferModel.StartDate.HasValue && perksOfferModel.EndDate.HasValue)
			{
				perksOfferDetailsViewModel.DateRangeText = $"From {perksOfferModel.StartDate.Value:MM/dd/yyyy} to {perksOfferModel.EndDate.Value:MM/dd/yyyy}";
			}
			perksOfferDetailsViewModel.LegalInformationText = perksOfferModel.LegalInformation;
			perksOfferDetailsViewModel.CancelButtonText = Resources.cancel_button;
			perksOfferDetailsViewModel.OnCancelButtonClicked = (Action)Delegate.Combine(perksOfferDetailsViewModel.OnCancelButtonClicked, new Action(PerksOfferDetailsViewModel_OnCancelButtonClicked));
			ConfigureTTS(perksOfferDetailsViewModel);
			viewUserControl.DataContext = perksOfferDetailsViewModel;
		}
	}

	public static void LeavePerksOfferDetailsView()
	{
		StopAndRemoveTimer();
		BaseViewController.StartIdleTimer();
	}

	private static PerksOfferDetailsViewModel GetPerksOfferDetailsViewModel()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			return viewUserControl.DataContext as PerksOfferDetailsViewModel;
		}
		return null;
	}

	private static void ConfigureTTS(PerksOfferDetailsViewModel perksOfferDetailsViewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null && service.TTSEnabled)
		{
			perksOfferDetailsViewModel.OnGetSpeechControl = () => PerksOfferDetailsViewTextToSpeech.GetSpeechControl(GetPerksOfferDetailsViewModel);
		}
	}

	private static void CreateAndStartTimer()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		int timeOut = OrchestrationService.TTSGetTimeout("long", 40000);
		(service?.CreateTimer("PerksOfferDetailsViewTimer", timeOut, timeOut, delegate
		{
			BaseViewController.AnalyticsService?.AddTimeoutEvent(timeOut);
			OrchestrationService.ViewGoBackSafe("PerksOfferDetailsViewController time out", popOnly: false);
		}))?.Start();
	}

	private static void StopAndRemoveTimer()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		(service?.GetTimer("PerksOfferDetailsViewTimer"))?.Stop();
		service?.RemoveTimer("PerksOfferDetailsViewTimer");
	}

	private static void PerksOfferDetailsViewModel_OnCancelButtonClicked()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("PerksOfferDetailsCancel");
		OrchestrationService.ViewGoBackSafe("PerksOfferDetailsViewCancel", popOnly: false);
	}
}
