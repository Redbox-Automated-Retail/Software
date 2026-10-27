using System.Threading.Tasks;
using KioskClientService.API.Models;

namespace KioskClientService.API.Services.Installer;

public interface IInstallerService
{
	Task<InstallerModels.OpsMarketResponse> GetOpsMarket(int kioskid);

	Task<InstallerModels.PendingBannersResponse> GetPendingBanners(int stateid);

	Task<InstallerModels.PendingKiosksResponse> GetPendingKiosks(short stateid, int bannerId);

	Task<InstallerModels.PendingStatesResponse> GetPendingStates();
}
