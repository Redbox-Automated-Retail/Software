using System;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel.Views;

public class SwipeViewFunctions
{
	[KernelFunction(Name = "kernel.set_swipe_view_parameters")]
	public static void SetSwipeViewParameters(Action cancelAction, Action continueAction, string swipeViewType)
	{
		SwipeViewController.SetSwipeViewParameters(cancelAction, continueAction, swipeViewType, null);
	}
}
