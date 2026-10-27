namespace Redbox.Rental.Model.KioskClientService.Transactions;

public class ReturnVisitPromotionInfo
{
	public ReturnVisitPromo Promo { get; set; }

	public bool IsLastDisc { get; set; }

	public bool Success { get; set; }
}
