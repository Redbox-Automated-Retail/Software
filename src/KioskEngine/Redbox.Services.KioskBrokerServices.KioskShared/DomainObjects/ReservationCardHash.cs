using System;

namespace Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects;

[Serializable]
public class ReservationCardHash
{
	public string CardBinHash { get; set; }

	public string Last4 { get; set; }
}
