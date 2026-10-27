using System.Threading.Tasks;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.Campaigns;

public interface ICampaignService
{
	Task<ApiBaseResponse> RefreshIfNeeded();

	Task<ApiBaseResponse> Refresh();

	Task<GetInCartResponse> GetInCart();

	Task<GetStartScreenResponse> GetStartScreen();
}
