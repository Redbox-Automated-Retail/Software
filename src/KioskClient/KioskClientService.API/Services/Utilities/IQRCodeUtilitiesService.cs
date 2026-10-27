using System.Threading.Tasks;

namespace KioskClientService.API.Services.Utilities;

public interface IQRCodeUtilitiesService
{
	Task<GetQRCodeResponse> GetQRCodeResponseAsync(string valueToEncode);
}
