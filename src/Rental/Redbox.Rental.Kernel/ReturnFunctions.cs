using System;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel;

public class ReturnFunctions
{
	[KernelFunction(Name = "kernel.show_machine_full_warning")]
	internal static void ShowMachineFullWarning()
	{
		Redbox.Rental.Services.Controllers.ReturnFunctions.ShowMachineFullWarning();
	}

	[KernelFunction(Name = "kernel.is_valid_barcode")]
	internal static bool IsValidBarcode(string barcode)
	{
		return Redbox.Rental.Services.Controllers.ReturnFunctions.IsValidBarcode(barcode);
	}

	[KernelFunction(Name = "kernel.show_return_complete_view_with_promo_check")]
	internal static void ShowReturnCompleteViewWithPromoCheck(string barcode, Action closeAction)
	{
		Redbox.Rental.Services.Controllers.ReturnFunctions.ShowReturnCompleteViewWithPromoCheck(barcode, closeAction);
	}
}
