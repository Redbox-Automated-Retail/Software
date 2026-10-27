using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel.KioskServices;

public interface IPickupValidationResult
{
	bool Success { get; set; }

	string CustomerProfileNumber { get; set; }

	string AccountNumber { get; set; }

	List<long> ReferenceNumbers { get; set; }

	List<long> CancelledReservations { get; set; }

	List<long> ValidReservations { get; set; }

	bool AllowRop { get; set; }

	List<ITransactionEntity> TransactionEntities { get; set; }

	string Response { get; set; }

	bool IsOnline { get; set; }

	bool IsStandalone { get; set; }

	string CancelReason { get; set; }
}
