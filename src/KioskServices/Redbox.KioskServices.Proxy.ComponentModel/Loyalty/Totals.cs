namespace Redbox.KioskServices.Proxy.ComponentModel.Loyalty;

public class Totals
{
	public decimal TaxRate { get; set; }

	public decimal Subtotal { get; set; }

	public decimal DiscountedSubtotal { get; set; }

	public decimal TaxAmount { get; set; }

	public decimal GrandTotal { get; set; }
}
