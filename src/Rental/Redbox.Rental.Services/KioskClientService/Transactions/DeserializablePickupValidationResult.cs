using System.Collections.Generic;
using Redbox.Rental.Model;

namespace Redbox.Rental.Services.KioskClientService.Transactions;

public class DeserializablePickupValidationResult : BaseResponse
{
	public string CustomerProfileNumber { get; set; }

	public string AccountNumber { get; set; }

	public List<long> ReferenceNumbers { get; set; }

	public List<long> CancelledReservations { get; set; }

	public List<long> ValidReservations { get; set; }

	public bool AllowRop { get; set; }

	public List<TransactionEntity> TransactionEntities { get; set; }

	public string Response { get; set; }

	public bool IsOnline { get; set; }

	public bool IsStandalone { get; set; }

	public string CancelReason { get; set; }
}
