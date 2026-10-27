using System;
using DeviceService.ComponentModel;
using Newtonsoft.Json;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;

namespace Redbox.Rental.Services.KioskClientService.Transactions;

public class ReconcileResult : BaseResponse, IReconcileResult, IMessageScrub
{
	public string CustomerProfileNumber { get; set; }

	public object Scrub()
	{
		try
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}
		catch (Exception arg)
		{
			return $"Exception in ReconcileResult.Scrub: {arg}";
		}
	}
}
