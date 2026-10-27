using System.Threading.Tasks;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.Configuration;

public interface IConfigurationService
{
	Task<ApiBaseResponse> GetKioskConfigurationSettingChanges(long? requestedConfigurationVersionId);

	Task<ApiBaseResponse> TriggerGetKioskConfigurationSettingChanges(TriggerGetConfigChangesRequest triggerGetConfigChangesRequest);

	Task<ApiBaseResponse> TriggerUpdateConfigurationStatus(TriggerUpdateConfigStatusRequest triggerUpdateConfigStatusRequest);

	Task<ConfigurationStatusResponse> GetConfigurationStatus();

	Task<ConfigurationStatusResponse> UpdateConfigurationStatus();

	Task<bool> UpdateConfigurationStatusIfNeeded();
}
