using System;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class LeadGenerationOfferConfirmationViewController : BaseViewController
{
	private static Action _continueAction;

	public static void ConfigureLeadGenerationOfferConfirmationView(LeadGenerationOfferConfirmationViewParameters leadGenerationOfferConfirmationViewParameters)
	{
		CreateAndStartTimer();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			LeadGenerationOfferConfirmationViewModel leadGenerationOfferConfirmationViewModel = new LeadGenerationOfferConfirmationViewModel();
			_continueAction = leadGenerationOfferConfirmationViewParameters?.ContinueAction;
			leadGenerationOfferConfirmationViewModel.OnGotItButtonClicked = (Action)Delegate.Combine(leadGenerationOfferConfirmationViewModel.OnGotItButtonClicked, new Action(LeadGenerationOfferConfirmationViewModel_OnGotItButtonClicked));
			viewUserControl.DataContext = leadGenerationOfferConfirmationViewModel;
		}
	}

	public static void LeaveLeadGenerationOfferConfirmationView()
	{
		StopAndRemoveTimer();
	}

	private static void CreateAndStartTimer()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		int timeOut = 8000;
		(service?.CreateTimer("LeadGenerationOfferConfirmationViewTimer", timeOut, timeOut, delegate
		{
			BaseViewController.AnalyticsService?.AddTimeoutEvent(timeOut);
			_continueAction?.Invoke();
		}))?.Start();
	}

	private static void StopAndRemoveTimer()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		(service?.GetTimer("LeadGenerationOfferConfirmationViewTimer"))?.Stop();
		service?.RemoveTimer("LeadGenerationOfferConfirmationViewTimer");
	}

	private static void LeadGenerationOfferConfirmationViewModel_OnGotItButtonClicked()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("LeadGenerationOfferConfirmationGotItButton");
		_continueAction?.Invoke();
	}
}
