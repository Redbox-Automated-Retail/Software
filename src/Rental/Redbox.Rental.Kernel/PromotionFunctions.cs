using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services;

namespace Redbox.Rental.Kernel;

public static class PromotionFunctions
{
	[KernelFunction(Name = "kernel.rental_promotion_clear_promotion")]
	internal static void ClearPromotion()
	{
		PromotionService.Instance.ClearPromotion();
	}
}
