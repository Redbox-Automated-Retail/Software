using System.Collections.Generic;

namespace Redbox.Rental.Model.Transaction;

public interface ITransactionData
{
	List<ITransactionEntity> TransactionEntities { get; set; }
}
