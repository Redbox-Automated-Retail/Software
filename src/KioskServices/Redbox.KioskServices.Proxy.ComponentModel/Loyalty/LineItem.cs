using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskServices.Proxy.ComponentModel.Loyalty;

public class LineItem
{
	public long ProductId { get; set; }

	public VendStatus VendStatus { get; set; }

	public string ProductFamily { get; set; }

	public string ProductType { get; set; }

	public bool MultiDisc { get; set; }

	public Pricing Pricing { get; set; }

	public string Barcode { get; set; }

	public BlurayUpsell BlurayUpsell { get; set; }

	public ItemSourceType? SourceType { get; set; }

	public string OfferCode { get; set; }

	public int? DiscNumber { get; set; }

	public string TivoQueryId { get; set; }

	public List<string> PersonalizationTitleTags { get; set; }
}
