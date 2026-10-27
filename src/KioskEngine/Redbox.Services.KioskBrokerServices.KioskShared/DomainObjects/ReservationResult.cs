using System;
using System.Collections.Generic;

namespace Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects;

[Serializable]
public class ReservationResult
{
	public bool ReservationSucceeded { get; set; }

	public string ErrorMessage { get; set; }

	public List<TitleResult> TitleResults { get; set; }
}
