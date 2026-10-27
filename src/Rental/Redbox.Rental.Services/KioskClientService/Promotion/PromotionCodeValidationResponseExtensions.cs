using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Redbox.Rental.Model.KioskClientService.Promotion;
using Redbox.Rental.Services.App;

namespace Redbox.Rental.Services.KioskClientService.Promotion;

internal static class PromotionCodeValidationResponseExtensions
{
	private static void AddDataFields(Dictionary<string, object> result, PromotionCodeValidationResponse response)
	{
		result.Add("Amount", response.Amount);
		if (response.PromotionIntentCode != null)
		{
			result.Add("PromotionIntentCode", response.PromotionIntentCode);
		}
		if (!string.IsNullOrEmpty(response.RentFormat))
		{
			result.Add("RentFormat", response.RentFormat);
		}
		result.Add("RentQty", response.RentQty);
		result.Add("GetQty", response.GetQty);
		if (!string.IsNullOrEmpty(response.GetFormat))
		{
			result.Add("GetFormat", response.GetFormat);
		}
		if (response.CampaignTitles != null)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["Include"] = response.CampaignTitles.Include;
			ArrayList titles = new ArrayList();
			dictionary["Titles"] = titles;
			if (response.CampaignTitles.Titles != null)
			{
				response.CampaignTitles.Titles.ForEach(delegate(long t)
				{
					titles.Add(t);
				});
			}
			result.Add("CampaignTitles", dictionary);
		}
		if (response.ProductTypeId.HasValue)
		{
			result.Add("ProductTypeId", response.ProductTypeId);
		}
		if (response.FormatIds != null)
		{
			ArrayList formatIds = new ArrayList();
			response.FormatIds.ForEach(delegate(int f)
			{
				formatIds.Add(f);
			});
			result.Add("FormatIds", formatIds);
		}
		if (response.ActionType != null)
		{
			result.Add("ActionType", response.ActionType);
		}
		if (response.AllowFullDiscount.HasValue)
		{
			result.Add("AllowFullDiscount", response.AllowFullDiscount);
		}
		if (response.DefaultPromo != null)
		{
			result.Add("DefaultPromo", response.DefaultPromo);
		}
	}

	public static Dictionary<string, object> ToPromoCodeValidationDictionary(this APIResponse<PromotionCodeValidationResponse> response)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("ServerBusy", false);
		dictionary.Add("IsOnline", true);
		PromotionCodeValidationResponse promotionCodeValidationResponse = JsonConvert.DeserializeObject<PromotionCodeValidationResponse>(response.ResponseContent);
		AddDataFields(dictionary, promotionCodeValidationResponse);
		if (response.IsSuccessStatusCode)
		{
			dictionary.Add("Success", true);
			dictionary.Add("HasErrors", false);
		}
		else
		{
			dictionary.Add("Success", false);
			dictionary.Add("HasErrors", true);
			Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
			dictionary2.Add("Code", promotionCodeValidationResponse.Error);
			dictionary.Add("Errors", new ArrayList { dictionary2 });
		}
		return dictionary;
	}
}
