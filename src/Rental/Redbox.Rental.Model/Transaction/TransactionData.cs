using System.Collections.Generic;

namespace Redbox.Rental.Model.Transaction;

public class TransactionData : ITransactionData
{
	private List<ITransactionEntity> _transactionEntities = new List<ITransactionEntity>();

	public List<ITransactionEntity> TransactionEntities
	{
		get
		{
			return _transactionEntities;
		}
		set
		{
			_transactionEntities = value;
		}
	}
}
