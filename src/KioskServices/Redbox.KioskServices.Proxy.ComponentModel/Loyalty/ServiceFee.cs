namespace Redbox.KioskServices.Proxy.ComponentModel.Loyalty;

public class ServiceFee
{
	public decimal DefaultAmount { get; set; }

	public decimal ActualAmount { get; set; }

	public decimal TaxRate { get; set; }

	public decimal TaxAmount { get; set; }
}
