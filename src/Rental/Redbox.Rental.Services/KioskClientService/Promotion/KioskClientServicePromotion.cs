using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService.Promotion;
using Redbox.Rental.Services.App;

namespace Redbox.Rental.Services.KioskClientService.Promotion;

internal class KioskClientServicePromotion : BaseKioskClientService, IKioskClientServicePromotion
{
	private const string _promoCodeValidation = "api/promotion/promocodevalidation";

	public async Task<IDictionary<string, object>> ValidatePromotionCode(Guid guid, string code, long kioskId)
	{
		var anon = new
		{
			MessageId = guid,
			Code = code,
			KioskId = kioskId
		};
		LogHelper.Instance.Log("KioskClientServicePromotion.ValidatePromotionCode - POST'ing following validate promotion code payload to 'api/promotion/promocodevalidation': " + anon.ToJson());
		IHttpService service = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = service.GenerateRequest(base.KioskClientServiceBaseUrl, "api/promotion/promocodevalidation", anon, HttpMethod.Post);
		APIResponse<PromotionCodeValidationResponse> aPIResponse = await service.SendRequestAsync<PromotionCodeValidationResponse>(request, ServiceLocator.Instance.GetService<IConfiguration>().KioskClientServicePromoCodeValidationTimeout, "ValidatePromotionCode", "c:\\build\\Redbox.Rental.Services\\KioskClientService\\Promotion\\KioskClientServicePromotion.cs");
		LogHelper.Instance.Log("KioskClientServicePromotion.ValidatePromotionCode - response: " + aPIResponse.ResponseContent);
		return aPIResponse.ToPromoCodeValidationDictionary();
	}
}
