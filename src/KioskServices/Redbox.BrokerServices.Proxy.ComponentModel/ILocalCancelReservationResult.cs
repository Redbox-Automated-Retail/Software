namespace Redbox.BrokerServices.Proxy.ComponentModel;

public interface ILocalCancelReservationResult
{
	bool Success { get; set; }

	string ErrorMessage { get; set; }
}
