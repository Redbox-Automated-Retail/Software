using Redbox.KioskEngine.ComponentModel;

namespace Redbox.BrokerServices.Proxy.ComponentModel;

public interface IProductItem
{
	string Barcode { get; set; }

	bool MultiDisc { get; set; }

	LineItemStatus Status { get; set; }
}
