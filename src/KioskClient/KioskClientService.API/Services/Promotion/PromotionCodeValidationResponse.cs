using System;
using System.Collections.Generic;

namespace KioskClientService.API.Services.Promotion;

public class PromotionCodeValidationResponse
{
	public Guid MessageId { get; set; }

	public long KioskId { get; set; }

	public string Error { get; set; }

	public decimal Amount { get; set; }

	public string PromotionIntentCode { get; set; }

	public byte? RentQty { get; set; }

	public string RentFormat { get; set; }

	public byte? GetQty { get; set; }

	public string GetFormat { get; set; }

	public DiscountValidationCampaignTitlesResult CampaignTitles { get; set; }

	public int? ProductTypeId { get; set; }

	public List<int> FormatIds { get; set; }

	public string ActionType { get; set; }

	public bool AllowFullDiscount { get; set; }

	public DefaultPromo DefaultPromo { get; set; }
}
