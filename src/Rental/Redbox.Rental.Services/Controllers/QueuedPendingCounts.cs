namespace Redbox.Rental.Services.Controllers;

public class QueuedPendingCounts
{
	public int MovieRentalCount { get; set; }

	public int MoviePurchaseCount { get; set; }

	public int GameRentalCount { get; set; }

	public int GamePurchaseCount { get; set; }

	public int TotalCount => MovieRentalCount + MoviePurchaseCount + GameRentalCount + GamePurchaseCount;
}
