using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Reservation;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.Services.Session;

namespace Redbox.Rental.Kernel;

public class ReservationFunctions
{
	private static Action<LuaTable, string> _updateBarcodeStatusCallback;

	[KernelFunction(Name = "kernel.register_vend_only_reserved_titles_callback")]
	internal static void RegisterVendOnlyReservedTitlesCallback(Action<string, string> vendOnlyReservedTitlesCallback)
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.RegisterVendOnlyReservedTitlesCallback(vendOnlyReservedTitlesCallback);
	}

	[KernelFunction(Name = "kernel.register_reservation_authorize_callback")]
	internal static void RegisterReservationAuthorizeCallback(Action<string, string> reservationAuthorizeCallback)
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.RegisterReservationAuthorizeCallback(reservationAuthorizeCallback);
	}

	[KernelFunction(Name = "kernel.register_authorize_on_pickup_callback")]
	internal static void RegisterAuthorizeOnPickupCallback(Action<string, bool> callback)
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.RegisterAuthorizeOnPickupCallback(callback);
	}

	[KernelFunction(Name = "kernel.is_qualified_for_recommendation_on_pickup")]
	internal static bool IsQualifiedForRecommendationOnPickup()
	{
		return Redbox.Rental.Services.Controllers.ReservationFunctions.IsQualifiedForRecommendationOnPickup();
	}

	[KernelFunction(Name = "kernel.add_recommended_products_to_analytics")]
	internal static void AddRecommendedProductsToAnalytics()
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.AddRecommendedProductsToAnalytics();
	}

	[KernelFunction(Name = "kernel.get_account_reservations")]
	internal static void GetAccountReservations()
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.GetAccountReservations();
	}

	[KernelFunction(Name = "kernel.populate_shopping_cart_with_reservation_items")]
	internal static void PopulateShoppingCartWithReservationItems(string sessionId, string referenceNumber)
	{
		Guid id = new Guid(sessionId);
		Redbox.Rental.Services.Controllers.ReservationFunctions.PopulateShoppingCartWithReservationItems(ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.GetRentalShoppingCart(id), referenceNumber);
	}

	[KernelFunction(Name = "kernel.sync_reservations")]
	internal static void SyncReservations()
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.SyncReservations();
	}

	[KernelFunction(Name = "kernel.get_reserved_barcodes")]
	internal static LuaTable GetReservedBarcodes()
	{
		LuaTable result = null;
		List<string> reservedBarcodes = Redbox.Rental.Services.Controllers.ReservationFunctions.GetReservedBarcodes();
		if (reservedBarcodes != null && reservedBarcodes.Count > 0)
		{
			result = reservedBarcodes.ToLuaTable();
		}
		return result;
	}

	[KernelFunction(Name = "kernel.get_reserved_barcodes_count")]
	internal static int GetReservedBarcodesCount()
	{
		return Redbox.Rental.Services.Controllers.ReservationFunctions.GetReservedBarcodesCount();
	}

	[KernelFunction(Name = "kernel.get_account_reservations_count")]
	internal static int GetAccountReservationsCount(bool checkExpire = false)
	{
		return Redbox.Rental.Services.Controllers.ReservationFunctions.GetAccountReservations(checkExpire).Count;
	}

	[KernelFunction(Name = "kernel.get_account_reservations_info")]
	internal static LuaTable GetAccountReservationsInfo()
	{
		List<IReservation> accountReservations = Redbox.Rental.Services.Controllers.ReservationFunctions.GetAccountReservations();
		return new Dictionary<string, object>
		{
			["number_of_reservations"] = accountReservations.Count,
			["reference_numbers"] = accountReservations.Select((IReservation y) => y.ReferenceNumber).ToList(),
			["non_rap_reference_numbers"] = (from y in accountReservations
				where !y.ReservationDetails.AuthorizeAtPickup
				select y.ReferenceNumber).ToList(),
			["rap_reference_numbers"] = (from y in accountReservations
				where y.ReservationDetails.AuthorizeAtPickup
				select y.ReferenceNumber).ToList()
		}.ToLuaTable();
	}

	[KernelFunction(Name = "kernel.is_barcode_reserved")]
	internal static bool IsBarcodeReserved(string barcode)
	{
		return Redbox.Rental.Services.Controllers.ReservationFunctions.IsBarcodeReserved(barcode);
	}

	[KernelFunction(Name = "kernel.register_update_barcode_status_callback")]
	internal static void RegisterUpdateBarcodeStatusCallback(Action<LuaTable, string> updateBarcodeStatusCallback)
	{
		_updateBarcodeStatusCallback = updateBarcodeStatusCallback;
		Redbox.Rental.Services.Controllers.ReservationFunctions.RegisterUpdateBarcodeStatusCallback(UpdateBarcodeStatus);
	}

	internal static void UpdateBarcodeStatus(List<string> barcodes, string status)
	{
		LuaTable luaTable = barcodes.ToLuaTable();
		if (luaTable != null)
		{
			_updateBarcodeStatusCallback?.Invoke(luaTable, status);
		}
	}

	[KernelFunction(Name = "kernel.register_get_store_return_time_callback")]
	internal static void RegisterGetStoreReturnTimeCallback(Func<string> getStoreReturnTimeCallback)
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.RegisterGetStoreReturnTimeCallback(getStoreReturnTimeCallback);
	}

	[KernelFunction(Name = "kernel.cancel_reservation")]
	internal static LuaTable CancelReservation(string referenceNumber)
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.CancelLocalReservation(referenceNumber);
		return new Dictionary<string, object> { { "success", true } }.ToLuaTable();
	}

	[KernelFunction(Name = "kernel.initialize_expire_reservation_timer")]
	internal static void CreateExpireReservationTimer()
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.InitializeExpireReservationTimer();
	}

	[KernelFunction(Name = "kernel.get_reserved_barcodes_to_vend")]
	internal static LuaTable GetReservedBarcodesToVend()
	{
		return Redbox.Rental.Services.Controllers.ReservationFunctions.GetReservedBarcodesToVend().ToLuaTable();
	}

	[KernelFunction(Name = "kernel.preposition_vend_complete")]
	internal static void ReservationPrepositionVendComplete(LuaTable vendResults)
	{
		if (vendResults != null)
		{
			if ((vendResults.Values?.Count ?? 0) <= 0)
			{
				ServiceLocator.Instance.GetService<IHardwareAlertManagerService>()?.SendAlert("HardwareError", "ReservationFunctions.ReservationPrepositionVendComplete", "vendResults empty", null, null, "PrepositionVendResultsEmpty");
				LogHelper.Instance.Log("ReservationFunctions.ReservationPrepositionVendComplete vendResults empty, Alert sent");
			}
			List<VendResult> list = new List<VendResult>();
			foreach (object value in vendResults.Values)
			{
				if (value is LuaTable)
				{
					LuaTable luaTable = value as LuaTable;
					VendResult item = new VendResult
					{
						barcode = luaTable["barcode"]?.ToString(),
						status = luaTable["status"]?.ToString()
					};
					list.Add(item);
				}
			}
			Redbox.Rental.Services.Controllers.ReservationFunctions.ReservationPrepositionVendComplete(list);
		}
		else
		{
			LogHelper.Instance.Log("ReservationPrepositionVendComplete: Vend result must not be null.");
		}
	}

	[KernelFunction(Name = "kernel.recommendation_on_pickup_preposition_vend_complete")]
	internal static void RecommendationOnPickupPrepositionVendComplete(LuaTable vendResults, string referenceNumber, string modifiedCart)
	{
		if (vendResults != null)
		{
			List<VendResult> list = new List<VendResult>();
			foreach (object value in vendResults.Values)
			{
				if (value is LuaTable)
				{
					LuaTable luaTable = value as LuaTable;
					VendResult item = new VendResult
					{
						barcode = luaTable["barcode"]?.ToString(),
						status = luaTable["status"]?.ToString()
					};
					list.Add(item);
				}
			}
			bool result = false;
			bool.TryParse(modifiedCart, out result);
			Redbox.Rental.Services.Controllers.ReservationFunctions.RecommendationOnPickupPrepositionVendComplete(list, referenceNumber, result);
		}
		else
		{
			LogHelper.Instance.Log("ReservationPrepositionVend: Vend result must not be null.");
		}
	}

	[KernelFunction(Name = "kernel.on_pickup_validation_failure")]
	internal static void OnPickupValidationFailure()
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.OnPickupValidationFailure();
	}

	[KernelFunction(Name = "kernel.register_with_broker_service")]
	internal static void RegisterWithBrokerService()
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.RegisterWithBrokerService();
	}

	[KernelFunction(Name = "kernel.set_validated_reservations")]
	internal static void SetValidatedReservations(LuaTable validatedReservations)
	{
		List<string> list = new List<string>();
		if (validatedReservations == null)
		{
			return;
		}
		foreach (object value in validatedReservations.Values)
		{
			list.Add(value.ToString());
		}
		Redbox.Rental.Services.Controllers.ReservationFunctions.SetValidatedReservations(list);
	}

	[KernelFunction(Name = "kernel.register_zero_touch_pickup_callbacks")]
	internal static void RegisterZeroTouchPickupCallbacks(Action continueReservation)
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.RegisterZeroTouchPickupCallbacks(continueReservation);
	}

	[KernelFunction(Name = "kernel.show_reservation_not_found_view")]
	internal static void ShowReservationNotFoundView(Action timeOutAction, Action continueAction)
	{
		MessagePopupViewController.ShowReservationNotFoundView(timeOutAction, continueAction);
	}

	[KernelFunction(Name = "kernel.show_authorize_at_pickup_offline_message")]
	internal static void ShowAuthorizeAtPickupOfflineMessage(Action completeAction)
	{
		Action continueAction = delegate
		{
			completeAction?.Invoke();
			OrchestrationService.StartUserInteraction(SessionStartTriggerActionList.Instance.FindByName(SessionStartTriggerActionConstants.ShowBrowseView));
			Session.Instance?.BrowseState?.SetDefaultBrowseStateSet();
			OrchestrationService.ViewShow("browse_view", BrowseFunctions.GetBrowseViewparameters());
		};
		OrchestrationService.SetCurrentUICulture();
		MessagePopupViewController.ShowAuthorizeAtPickupOfflineMessage(completeAction, completeAction, continueAction);
	}
}
