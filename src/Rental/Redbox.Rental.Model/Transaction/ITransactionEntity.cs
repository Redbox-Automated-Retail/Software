namespace Redbox.Rental.Model.Transaction;

public interface ITransactionEntity
{
	long DiscId { get; set; }

	long TitleId { get; set; }

	string Name { get; set; }

	string Value { get; set; }
}
