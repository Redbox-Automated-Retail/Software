using DeviceService.ComponentModel;
using Newtonsoft.Json;
using Redbox.Rental.Model;
using Redbox.Rental.Model.UpdateClientService.Reservation;
using Redbox.Rental.Services.KioskClientService.Transactions;

namespace Redbox.Rental.Services.UpdateClientService.Reservation;

public class UnregisterFromBrokerServiceRequest : MessageBase, IUnregisterFromBrokerServiceRequest, IMessageBase, IMessageScrub
{
	public string AppName { get; set; }

	public string Reason { get; set; }

	public override object Scrub()
	{
		return JsonConvert.SerializeObject(this, Formatting.None, new JsonSerializerSettings
		{
			NullValueHandling = NullValueHandling.Ignore
		});
	}
}
