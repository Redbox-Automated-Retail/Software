using System.Collections.Generic;
using System.Threading.Tasks;

namespace KioskClientService.API.Services.Planograms;

public interface IPlanogramService
{
	List<Planogram> GetActivePlanograms(string filenameSearch = "*");

	List<Planogram> GetValidPlanograms(string filenameSearch = "*");

	Task<Planogram> GetSelectedPlanogram();

	Task<SavePlanogramInfoResponse> SaveSelectedPlanogram(string planogramName);

	Task<SyncPlanogramsResponse> SyncPlanograms(bool forceDownload = false);

	Task RefreshIfNeeded();

	Task SyncFilesIfNeeded(int retryCount = 0);
}
