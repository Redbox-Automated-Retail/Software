using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Models;

namespace KioskClientService.API.Services.Data;

public interface IDataClient
{
	Task<APIResponse<DataModels.KioskDataResponse>> SendVersions(DataModels.KioskBaseRequest request);

	Task<APIResponse<DataModels.KioskDataResponse>> SendStatistics(DataModels.KioskBaseRequest request);

	Task<APIResponse<DataModels.KioskDataResponse>> SendProperties(DataModels.KioskBaseRequest request);

	Task<APIResponse<DataModels.KioskDataResponse>> SendKioskAlert(DataModels.KioskBaseRequest request);

	Task<APIResponse<DataModels.KioskDataResponse>> SendAuditEvent(DataModels.KioskBaseRequest request);

	Task<APIResponse<DataModels.KioskDataResponse>> SendKioskEvents(DataModels.KioskBaseRequest request);

	Task<APIResponse<DataModels.KioskDataResponse>> SendKioskFunctionCheck(DataModels.KioskBaseRequest request);
}
