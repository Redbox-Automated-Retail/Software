using System;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class SignInThanksViewController : BaseViewController
{
	public static void ConfigureView(SignInThanksViewParameters parameters)
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			BaseViewController.StopIdleTimer();
			BaseViewController.SetCurrentUICulture();
			SignInThanksViewModel dataContext = new SignInThanksViewModel
			{
				MessageText = string.Format(Resources.sign_in_thanks_message, ObfuscateEmailAddress(parameters.EmailAddress)),
				ThanksText = Resources.sign_in_thanks_text
			};
			viewUserControl.DataContext = dataContext;
			CreateAndStartTimer(parameters.ContinueAction, parameters.DelaySeconds, parameters.PopOnlyOnViewGoBack);
		}
	}

	private static string ObfuscateEmailAddress(string emailAddress)
	{
		if (string.IsNullOrEmpty(emailAddress))
		{
			return string.Empty;
		}
		string result = emailAddress;
		int num = emailAddress.IndexOf('@');
		if (num > 2)
		{
			result = emailAddress.Substring(0, 2) + new string('*', num - 2) + emailAddress.Substring(num);
		}
		return result;
	}

	public static void LeaveView()
	{
		StopAndRemoveTimer();
	}

	private static void CreateAndStartTimer(Action continueAction, int delaySeconds, bool popOnly)
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		int timeOut = delaySeconds * 1000;
		(service?.CreateTimer("SignInThanksViewTimer", timeOut, timeOut, delegate
		{
			BaseViewController.AnalyticsService?.AddTimeoutEvent(timeOut);
			OrchestrationService.ViewGoBackSafe("SignInThanksViewTimeout", popOnly);
			continueAction?.Invoke();
		}))?.Start();
	}

	private static void StopAndRemoveTimer()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		(service?.GetTimer("SignInThanksViewTimer"))?.Stop();
		service?.RemoveTimer("SignInThanksViewTimer");
	}
}
