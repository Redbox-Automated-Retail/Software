using System;

namespace Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects;

[Serializable]
public class CancelReservationResult
{
	public bool CancellationSucceeded { get; set; }

	public string ErrorMessage { get; set; }
}
