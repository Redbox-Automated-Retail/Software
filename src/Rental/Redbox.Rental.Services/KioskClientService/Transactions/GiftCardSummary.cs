using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.Rental.Services.KioskClientService.Transactions;

public class GiftCardSummary
{
	public bool Exists { get; set; }

	public GiftCardStatus Status { get; set; }

	public decimal Balance { get; set; }

	public int MaxDiscs { get; set; }

	public int MaxGames { get; set; }

	public GiftCardSummary()
	{
		Exists = true;
	}
}
