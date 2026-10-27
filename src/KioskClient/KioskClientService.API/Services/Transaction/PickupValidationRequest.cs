using System.Collections.Generic;

namespace KioskClientService.API.Services.Transaction;

public class PickupValidationRequest : MessageBase
{
	public CreditCard CreditCard { get; set; }

	public List<long> ReferenceNumbers { get; set; } = new List<long>();

	public override void ScrubData()
	{
		base.ScrubData();
		CreditCard?.ScrubData();
	}
}
