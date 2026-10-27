namespace Redbox.Rental.Model;

public interface IStore
{
	int Id { get; }

	string City { get; }

	string State { get; }

	string ZipCode { get; }

	string DueTime { get; }

	string Address { get; }

	string MarketName { get; }

	string KaseyaMarketName { get; }
}
