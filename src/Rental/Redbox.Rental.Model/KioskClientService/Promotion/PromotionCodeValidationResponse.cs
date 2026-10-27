using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Promotion;

public class PromotionCodeValidationResponse
{
	public string ActionType { get; set; }

	public decimal Amount { get; set; }

	public DiscountValidationCampaignTitlesResult CampaignTitles { get; set; }

	public string Error { get; set; }

	public List<int> FormatIds { get; set; }

	public string GetFormat { get; set; }

	public byte? GetQty { get; set; }

	public int? ProductTypeId { get; set; }

	public string PromotionIntentCode { get; set; }

	public string RentFormat { get; set; }

	public byte? RentQty { get; set; }

	public bool? AllowFullDiscount { get; set; }

	public DefaultPromo DefaultPromo { get; set; }
}
