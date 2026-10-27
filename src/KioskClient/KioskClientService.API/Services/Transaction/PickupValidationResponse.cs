using System.Collections.Generic;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.Transaction;

public class PickupValidationResponse : BaseResponse
{
	public string CustomerProfileNumber { get; set; }

	public string AccountNumber { get; set; }

	public List<long> ReferenceNumbers { get; set; }

	public List<long> CancelledReservations { get; set; }

	public List<long> ValidReservations { get; set; }

	public bool AllowRop { get; set; }

	public List<object> TransactionEntities { get; set; }

	public PickupValidationResponse()
		: base(200)
	{
		base.Success = true;
	}
}
