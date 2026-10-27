using System.Collections.Generic;

namespace Redbox.Rental.Services.KioskClientService.Transactions;

internal class PickupValidationRequest : MessageBase
{
	public CreditCard CreditCard { get; set; }

	public List<long> ReferenceNumbers { get; set; }

	public PickupValidationRequest()
	{
		ReferenceNumbers = new List<long>();
	}

	public override void ScrubData()
	{
		base.ScrubData();
		CreditCard?.ScrubData();
	}
}
