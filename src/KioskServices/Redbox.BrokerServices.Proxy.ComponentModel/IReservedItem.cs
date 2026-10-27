namespace Redbox.BrokerServices.Proxy.ComponentModel;

public interface IReservedItem
{
	string Barcode { get; set; }

	int ProductId { get; set; }
}
