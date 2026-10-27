using System.Threading.Tasks;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.Promotion;

public interface IPromotionService
{
	Task<APIResponse<PromotionCodeValidationResponse>> ValidatePromotionCode(PromotionCodeValidationPayload payload);

	Task<APIResponse<PromotionCodeValidationResponse>> ValidatePromotionCode(string code);
}
