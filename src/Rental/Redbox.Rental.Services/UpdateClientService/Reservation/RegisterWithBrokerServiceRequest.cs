using DeviceService.ComponentModel;
using Newtonsoft.Json;
using Redbox.Rental.Model;
using Redbox.Rental.Model.UpdateClientService.Reservation;
using Redbox.Rental.Services.KioskClientService.Transactions;

namespace Redbox.Rental.Services.UpdateClientService.Reservation;

public class RegisterWithBrokerServiceRequest : MessageBase, IRegisterWithBrokerServiceRequest, IMessageBase, IMessageScrub
{
	public override object Scrub()
	{
		return JsonConvert.SerializeObject(this, Formatting.None, new JsonSerializerSettings
		{
			NullValueHandling = NullValueHandling.Ignore
		});
	}
}
