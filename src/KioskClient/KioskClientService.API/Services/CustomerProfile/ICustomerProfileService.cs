using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.ProxyApi;

namespace KioskClientService.API.Services.CustomerProfile;

public interface ICustomerProfileService
{
	Task<APIResponse<CustomerProfileKioskLoginResponse>> Login(CustomerProfileLoginRequest request);
}
