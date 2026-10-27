using System.Threading.Tasks;
using KioskClientService.API.Models;

namespace KioskClientService.API.Services.Authentication;

public interface IAuthenticationService
{
	Task<KioskModels.KioskAuthenticateResponse> Authenticate(KioskModels.KioskAuthenticateRequest request);
}
