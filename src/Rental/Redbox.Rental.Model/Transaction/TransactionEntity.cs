namespace Redbox.Rental.Model.Transaction;

public class TransactionEntity : ITransactionEntity
{
	public long DiscId { get; set; }

	public long TitleId { get; set; }

	public string Name { get; set; }

	public string Value { get; set; }
}
