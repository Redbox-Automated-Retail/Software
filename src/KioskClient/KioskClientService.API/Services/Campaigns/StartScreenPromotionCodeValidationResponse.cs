using KioskClientService.API.Services.Promotion;

namespace KioskClientService.API.Services.Campaigns;

public class StartScreenPromotionCodeValidationResponse : PromotionCodeValidationResponse
{
	public string Code { get; set; }

	public StartScreenPromotionCodeValidationResponse()
	{
	}

	public StartScreenPromotionCodeValidationResponse(string code, PromotionCodeValidationResponse outer)
	{
		Code = code;
		base.MessageId = outer.MessageId;
		base.KioskId = outer.KioskId;
		base.Error = outer.Error;
		base.Amount = outer.Amount;
		base.PromotionIntentCode = outer.PromotionIntentCode;
		base.RentQty = outer.RentQty;
		base.RentFormat = outer.RentFormat;
		base.GetQty = outer.GetQty;
		base.GetFormat = outer.GetFormat;
		base.CampaignTitles = outer.CampaignTitles;
		base.ProductTypeId = outer.ProductTypeId;
		base.FormatIds = outer.FormatIds;
		base.ActionType = outer.ActionType;
		base.AllowFullDiscount = outer.AllowFullDiscount;
		base.DefaultPromo = outer.DefaultPromo;
	}
}
