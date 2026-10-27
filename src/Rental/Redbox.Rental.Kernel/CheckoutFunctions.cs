using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Services;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Kernel;

public class CheckoutFunctions
{
	[KernelFunction(Name = "kernel.show_reservation_details_view")]
	internal static void ShowReservationDetailsView(Action continueReservationPickupCallback)
	{
		Redbox.Rental.Services.Controllers.CheckoutFunctions.ShowReservationDetailsView(continueReservationPickupCallback);
	}

	[KernelFunction(Name = "kernel.device_service_report_auth_result")]
	internal static void DeviceServiceReportAuthorizeResult(bool authorizeSuccessful)
	{
		Redbox.Rental.Services.Controllers.CheckoutFunctions.ReportAuthorizeResultToDeviceService(authorizeSuccessful);
	}

	[KernelFunction(Name = "kernel.show_authorizing_message")]
	internal static void ShowAuthorizingMessage()
	{
		OrchestrationService.SetCurrentUICulture();
		OrchestrationService.ViewShow("general_message_view", new GeneralMessageViewModel
		{
			DisableIdleTimerOnEnter = false,
			EnableIdleTimerOnLeave = false,
			MessageText = Resources.authorization_message
		});
	}

	[KernelFunction(Name = "kernel.regular_checkout_preposition_vend_complete")]
	internal static void RegularCheckoutPrepositionVendComplete(LuaTable vendResults)
	{
		if (vendResults != null)
		{
			if ((vendResults.Values?.Count ?? 0) <= 0)
			{
				ServiceLocator.Instance.GetService<IHardwareAlertManagerService>()?.SendAlert("HardwareError", "CheckoutFunctions.RegularCheckoutPrepositionVendComplete", "vendResults empty", null, null, "PrepositionVendResultsEmpty");
				LogHelper.Instance.Log("CheckoutFunctions.RegularCheckoutPrepositionVendComplete vendResults empty, Alert sent");
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
			Redbox.Rental.Services.Controllers.CheckoutFunctions.RegularCheckoutPrepositionVendComplete(list);
		}
		else
		{
			LogHelper.Instance.Log("RegularCheckoutPrepositionVendComplete: Vend result must not be null.");
		}
	}
}
