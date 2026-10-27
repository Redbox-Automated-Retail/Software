using System;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services;

namespace Redbox.Rental.Kernel;

public static class OrchestrationFunctions
{
	[KernelFunction(Name = "kernel.view_analytics_start")]
	internal static void ViewAnalyticsStart(string userTriggerAction, string sessionType)
	{
		OrchestrationService.CreateStartViewAnalytics(userTriggerAction, sessionType);
	}

	[KernelFunction(Name = "kernel.register_pricing_service")]
	internal static void RegisterPricingService(Action pricingSetChangeCallback)
	{
		OrchestrationService.RegisterPricingService(pricingSetChangeCallback);
	}

	[KernelFunction(Name = "kernel.register_idle_timer_expire_session_callback")]
	internal static void RegisterIdleTimerExpireSessionCallback(Action idleTimerExpireSessionCallback)
	{
		OrchestrationService.RegisterIdleTimerExpireSessionCallback(idleTimerExpireSessionCallback);
	}

	[KernelFunction(Name = "kernel.register_start_new_rental_session_callback")]
	internal static void RegisterStartNewRentalSessionCallback(Action startNewRentalSessionCallback)
	{
		OrchestrationService.RegisterStartNewRentalSessionCallback(startNewRentalSessionCallback);
	}

	[KernelFunction(Name = "kernel.register_start_idle_timer_callback")]
	internal static void RegisterStartIdleTimerCallback(Action startIdleTimerCallback)
	{
		OrchestrationService.RegisterStartIdleTimerCallback(startIdleTimerCallback);
	}

	[KernelFunction(Name = "kernel.register_localization_reload_callback")]
	internal static void RegisterLocalizationReloadCallback(Action callback)
	{
		OrchestrationService.RegisterLocalizationReloadCallback(callback);
	}

	[KernelFunction(Name = "kernel.register_finish_vend_callback")]
	internal static void RegisterFinishVendCallback(Action finishVendCallback)
	{
		OrchestrationService.RegisterFinishVendCallback(finishVendCallback);
	}

	[KernelFunction(Name = "kernel.register_preposition_vend_callback")]
	internal static void RegisterPrepositionVendCallback(Action prepositionVendCallback)
	{
		OrchestrationService.RegisterPrepositionVendCallback(prepositionVendCallback);
	}

	[KernelFunction(Name = "kernel.register_preposition_vend_abort_callback")]
	internal static void RegisterPrepositionVendAbortCallback(Action prepositionVendAbortCallback)
	{
		OrchestrationService.RegisterPrepositionVendAbortCallback(prepositionVendAbortCallback);
	}

	[KernelFunction(Name = "kernel.register_is_vend_job_completed_callback")]
	internal static void RegisterIsVendJobCompletedCallback(Func<bool> isVendJobCompleteCallback)
	{
		OrchestrationService.RegisterIsVendJobCompletedCallback(isVendJobCompleteCallback);
	}

	[KernelFunction(Name = "kernel.reload_pricing_service")]
	internal static void ReloadPricingService()
	{
		OrchestrationService.ReloadPricingService();
	}

	[KernelFunction(Name = "kernel.does_shopping_cart_contain_all_reserved_barcodes")]
	internal static bool DoesShoppingCartContainAllReservedBarcodes()
	{
		return OrchestrationService.DoesShoppingCartContainAllReservedBarcodes();
	}

	[KernelFunction(Name = "kernel.shopping_cart_commit")]
	internal static void ShoppingCartCommit(string sessionId, string reason, string shouldEndAndSave)
	{
		bool result = false;
		bool.TryParse(shouldEndAndSave, out result);
		OrchestrationService.ShoppingCartCommit(sessionId, reason, result);
	}
}
