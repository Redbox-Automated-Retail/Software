using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Model.KioskClientService.Transactions;

public class LineItem
{
	public long ProductId { get; set; }

	public string SubscriptionId { get; set; }

	public string Barcode { get; set; }

	public VendStatus VendStatus { get; set; }

	public string ProductFamily { get; set; }

	public string ProductType { get; set; }

	public string ProductName { get; set; }

	public Pricing Pricing { get; set; }

	public BlurayUpsell BlurayUpsell { get; set; }

	public ItemSourceType? SourceType { get; set; }

	public string OfferCode { get; set; }

	public string TivoQueryId { get; set; }

	public List<string> PersonalizationTitleTags { get; set; }

	public string TempPassword { get; set; }

	public bool MultiDisc { get; set; }

	public int? DiscNumber { get; set; }

	public override string ToString()
	{
		return $"ProductId: {ProductId}, SubscriptionId: {SubscriptionId}, Barcode: {Barcode} VendStatus: {VendStatus}, ProductFamily: {ProductFamily}, ProductType: {ProductType}, Pricing: ({Pricing}), BlurayUpsell: ({BlurayUpsell}), SourceType: {SourceType}, OfferCode: {OfferCode}";
	}
}
