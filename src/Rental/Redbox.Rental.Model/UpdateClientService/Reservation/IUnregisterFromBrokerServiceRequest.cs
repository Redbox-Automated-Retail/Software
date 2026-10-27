using DeviceService.ComponentModel;

namespace Redbox.Rental.Model.UpdateClientService.Reservation;

public interface IUnregisterFromBrokerServiceRequest : IMessageBase, IMessageScrub
{
	string AppName { get; set; }

	string Reason { get; set; }
}
