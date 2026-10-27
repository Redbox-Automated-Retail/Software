using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel.Views;

public class MessagePopupViewFunctions
{
	[KernelFunction(Name = "kernel.show_message_popup_view_hardware_error")]
	internal static void ShowMessagePopupViewHardwareError(Action timeOutAction, Action continueAction)
	{
		MessagePopupViewController.ShowMessagePopupViewHardwareError(timeOutAction, continueAction);
	}

	[KernelFunction(Name = "kernel.show_message_popup_view_return_backward_inserted")]
	internal static void ShowMessagePopupViewReturnBackwardInserted(Action timeOutAction, Action continueAction)
	{
		IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
		if (service != null && service.AnimatedDiscReturnEnabled)
		{
			AnimatedDiscReturnViewController.ShowAnimatedDiscReturnView(timeOutAction, continueAction);
		}
		else
		{
			MessagePopupViewController.ShowMessagePopupViewReturnBackwardInserted(timeOutAction, continueAction);
		}
	}

	[KernelFunction(Name = "kernel.show_message_popup_view_return_unrecognized_barcode")]
	internal static void ShowMessagePopupViewReturnUnrecognizedBarcode(Action timeOutAction, Action continueAction)
	{
		MessagePopupViewController.ShowMessagePopupViewReturnUnrecognizedBarcode(timeOutAction, continueAction);
	}

	[KernelFunction(Name = "kernel.show_message_popup_view_vend_error")]
	internal static void ShowMessagePopupViewVendError(Action timeOutAction, Action continueAction)
	{
		MessagePopupViewController.ShowMessagePopupViewVendError(timeOutAction, continueAction);
	}

	[KernelFunction(Name = "kernel.hide_message_popup_view")]
	internal static void HideMessagePopupView(bool? popOnly)
	{
		MessagePopupViewController.HideMessagePopupView(popOnly.HasValue && popOnly.Value);
	}

	[KernelFunction(Name = "kernel.is_message_popup_view_showing")]
	internal static bool IsMessagePopupViewShowing()
	{
		return MessagePopupViewController.IsMessagePopupViewShowing();
	}

	[KernelFunction(Name = "kernel.message_popup_view_stop_timer")]
	internal static void MessagePopupViewStopTimer()
	{
		MessagePopupViewController.MessagePopupViewStopTimer();
	}
}
