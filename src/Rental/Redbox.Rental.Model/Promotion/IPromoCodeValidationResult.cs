using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.Rental.Model.Promotion;

public interface IPromoCodeValidationResult
{
	bool IsStandAlone { get; set; }

	bool IsOnline { get; set; }

	string ServerName { get; set; }

	bool Success { get; set; }

	string StoreNumber { get; set; }

	string PromoCode { get; set; }

	string Response { get; set; }

	decimal Amount { get; set; }

	PromotionIntentCode IntentCode { get; set; }

	int RentQuantity { get; set; }

	PromotionRentFormat RentFormat { get; set; }

	int GetQuantity { get; set; }

	PromotionGetFormat GetFormat { get; set; }

	ICampaignTitles CampaignTitles { get; set; }

	int? ProductTypeId { get; set; }

	List<int> FormatIds { get; set; }

	PromotionActionCode ActionCode { get; set; }

	bool? AllowFullDiscount { get; set; }

	IDefaultPromo DefaultPromo { get; set; }
}
