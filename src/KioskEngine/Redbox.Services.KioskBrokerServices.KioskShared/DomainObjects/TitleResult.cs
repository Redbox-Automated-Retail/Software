using System;
using System.Collections.Generic;

namespace Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects;

[Serializable]
public class TitleResult
{
	public int ItemID { get; set; }

	public bool Reservable { get; set; }

	public string ReservedBarcode { get; set; }

	public List<DiscStatus> UnreservableDiscs { get; set; }
}
