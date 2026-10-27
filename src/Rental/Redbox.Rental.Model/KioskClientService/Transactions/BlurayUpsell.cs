namespace Redbox.Rental.Model.KioskClientService.Transactions;

public class BlurayUpsell
{
	public int ProductGroupId { get; set; }

	public string Offer { get; set; }

	public long TitleId { get; set; }

	public int? TypeId { get; set; }

	public override string ToString()
	{
		return $"ProductGroupId {ProductGroupId}, Offer {Offer}, TitleId {TitleId}, TypeId {TypeId}";
	}
}
