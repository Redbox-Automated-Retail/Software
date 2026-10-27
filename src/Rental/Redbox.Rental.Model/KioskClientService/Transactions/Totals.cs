namespace Redbox.Rental.Model.KioskClientService.Transactions;

public class Totals
{
	public decimal TaxRate { get; set; }

	public decimal Subtotal { get; set; }

	public decimal DiscountedSubtotal { get; set; }

	public decimal TaxAmount { get; set; }

	public decimal GrandTotal { get; set; }

	public override string ToString()
	{
		return $"TaxRate: {TaxRate / 100m:P}, Subtotal: {Subtotal:C2}, DiscountedSubtotal: {DiscountedSubtotal:C2}, TaxAmount: {TaxAmount:C2}, GrandTotal: {GrandTotal:C2}";
	}
}
