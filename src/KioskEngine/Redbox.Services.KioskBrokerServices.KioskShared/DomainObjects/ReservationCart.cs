using System;
using System.Collections.Generic;

namespace Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects;

[Serializable]
public class ReservationCart
{
	public int KioskID { get; set; }

	public long ReferenceNumber { get; set; }

	public string CardID { get; set; }

	public string Email { get; set; }

	public DateTime ReserveDate { get; set; }

	public List<ReservationTitle> Titles { get; set; }
}
