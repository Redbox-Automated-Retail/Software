using System.Collections.Generic;
using Redbox.BrokerServices.Proxy.ComponentModel;

namespace Redbox.BrokerServices.Proxy;

public class LocalReservationResult : IReservationResult
{
	public bool Success { get; set; }

	public string ErrorMessage { get; set; }

	public List<IReservedItem> Items { get; set; }

	public LocalReservationResult()
	{
		Items = new List<IReservedItem>();
	}
}
