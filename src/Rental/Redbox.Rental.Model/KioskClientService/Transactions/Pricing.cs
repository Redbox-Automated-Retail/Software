using System.Text;

namespace Redbox.Rental.Model.KioskClientService.Transactions;

public class Pricing
{
	public decimal InitialNight { get; set; }

	public decimal ExtraNight { get; set; }

	public decimal NonReturn { get; set; }

	public decimal Purchase { get; set; }

	public decimal Expiration { get; set; }

	public int NonReturnDays { get; set; }

	public int PriceSetId { get; set; }

	public int? ProductPriceId { get; set; }

	public int? InitialDays { get; set; }

	public decimal? DefaultInitialNight { get; set; }

	public decimal? DefaultExtraNight { get; set; }

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append($"PriceSetId: {PriceSetId}, InitialNight: {InitialNight:C2}, ExtraNight: {ExtraNight:C2}, NonReturn: {NonReturn:C2}, NonReturnDays: {NonReturnDays}, Purchase: {Purchase:C2}, Expiration: {Expiration:C2}");
		if (ProductPriceId.HasValue)
		{
			stringBuilder.Append($", ProductPriceId: {ProductPriceId.Value}");
		}
		if (InitialDays.HasValue)
		{
			stringBuilder.Append($", InitialDays: {InitialDays.Value}");
		}
		if (DefaultInitialNight.HasValue)
		{
			stringBuilder.Append($", DefaultInitialNight: {DefaultInitialNight.Value:C2}");
		}
		if (DefaultExtraNight.HasValue)
		{
			stringBuilder.Append($", DefaultExtraNight: {DefaultExtraNight.Value:C2}");
		}
		return stringBuilder.ToString();
	}
}
