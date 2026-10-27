using System.Collections.Generic;

namespace Redbox.BrokerServices.Proxy.ComponentModel;

public interface IReservationResult
{
	bool Success { get; set; }

	string ErrorMessage { get; set; }

	List<IReservedItem> Items { get; set; }
}
