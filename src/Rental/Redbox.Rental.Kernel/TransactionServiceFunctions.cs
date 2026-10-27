using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService;

namespace Redbox.Rental.Kernel;

public class TransactionServiceFunctions
{
	[KernelFunction(Name = "kernel.merchsvcs_resvauthorize")]
	internal static void ReservationAuthorize(string sessionId, string customerProfileNumber, string referenceNumber, LuaFunction completeCallback)
	{
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService != null)
		{
			IKioskClientService service = ServiceLocator.Instance.GetService<IKioskClientService>();
			ServiceLocator.Instance.GetService<IConfiguration>();
			_ = ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId;
			service.Transaction.ReservationAuthorize(sessionId, customerProfileNumber, long.Parse(referenceNumber), delegate(IRemoteServiceResult r)
			{
				ICallbackService callbackService2 = callbackService;
				ResultCallbackEntry resultCallbackEntry = new ResultCallbackEntry
				{
					Function = completeCallback
				};
				object[] parameters = new LuaTable[1] { r.AsTableExtended() };
				resultCallbackEntry.Parameters = parameters;
				callbackService2.EnqueueCallback(resultCallbackEntry);
			});
		}
	}

	[KernelFunction(Name = "kernel.merchsvcs_validatepickup")]
	internal static void ValidatePickup(string sessionId, string customerProfileNumber, LuaTable referenceNumbers, LuaFunction completeCallback)
	{
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService == null)
		{
			return;
		}
		List<long> list = new List<long>();
		foreach (object value in referenceNumbers.Values)
		{
			list.Add(Convert.ToInt64(value));
		}
		_ = ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId;
		ServiceLocator.Instance.GetService<IKioskClientService>().Transaction.ValidatePickup(sessionId, list, delegate(IRemoteServiceResult r)
		{
			ICallbackService callbackService2 = callbackService;
			ResultCallbackEntry resultCallbackEntry = new ResultCallbackEntry
			{
				Function = completeCallback
			};
			object[] parameters = new LuaTable[1] { r.AsTable() };
			resultCallbackEntry.Parameters = parameters;
			callbackService2.EnqueueCallback(resultCallbackEntry);
		});
	}

	[KernelFunction(Name = "kernel.merchsvcs_pickupauthorize")]
	internal static void PickupAuthorize(string sessionId, string customerProfileNumber, long referenceNumber, bool cartWasModified, LuaFunction callback)
	{
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService == null)
		{
			return;
		}
		IKioskClientService service = ServiceLocator.Instance.GetService<IKioskClientService>();
		RemoteServiceCallback completeCallback = null;
		if (callback != null)
		{
			completeCallback = delegate(IRemoteServiceResult r)
			{
				callbackService.EnqueueCallback(new ResultCallbackEntry
				{
					Function = callback,
					Parameters = new object[2]
					{
						referenceNumber,
						r.AsTable()
					}
				});
			};
		}
		service.Transaction.PickupAuthorize(sessionId, customerProfileNumber, referenceNumber, cartWasModified, completeCallback);
	}
}
