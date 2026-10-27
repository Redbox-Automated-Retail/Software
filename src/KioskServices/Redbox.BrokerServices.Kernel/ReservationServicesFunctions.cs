using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Lua;
using Redbox.Rental.Model.KioskClientService;

namespace Redbox.BrokerServices.Kernel;

internal static class ReservationServicesFunctions
{
	[KernelFunction(Name = "kernel.resvsvcs_cancel")]
	internal static void Cancel(string storeNumber, long referenceNumber, string reasonCode, LuaFunction callback)
	{
		if (ServiceLocator.Instance.GetService<ICallbackService>() != null)
		{
			RemoteServiceCallback callback2 = null;
			ServiceLocator.Instance.GetService<IKioskClientService>().Transaction.CancelReservation(storeNumber, referenceNumber, reasonCode, callback2);
		}
	}
}
