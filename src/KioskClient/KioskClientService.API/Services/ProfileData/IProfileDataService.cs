using System.Collections.Generic;
using System.Threading.Tasks;

namespace KioskClientService.API.Services.ProfileData;

public interface IProfileDataService
{
	Task<TriggerProductDataUpdateResponse> TriggerProductDataUpdate();

	Task ExecuteScheduledDataUpdateIfNeeded();

	Task<AddDataRecordResponse<Product>> AddProducts(List<long> productIds);

	Task<GetProfileDataInstanceResponse> GetProfileDataInstance(bool refreshFromFiles = false);

	GetProfileDataInstancePendingUpdateResponse HasPendingUpdates();
}
