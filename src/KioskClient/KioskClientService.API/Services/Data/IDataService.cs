using System.Threading.Tasks;
using KioskClientService.API.Models;
using Microsoft.AspNetCore.Http;

namespace KioskClientService.API.Services.Data;

public interface IDataService
{
	Task<DataModels.KioskDataResponse> SendVersions(HttpRequest httpRequest, DataModels.KioskVersionRequest request);

	Task<DataModels.KioskDataResponse> SendStatistics(HttpRequest httpRequest, DataModels.KioskStatisticsRequest request);

	Task<DataModels.KioskDataResponse> SendProperties(HttpRequest httpRequest, DataModels.KioskPropertiesRequest request);

	Task<DataModels.KioskDataResponse> SendKioskAlert(HttpRequest httpRequest, DataModels.KioskAlertRequest request);

	Task<DataModels.KioskDataResponse> SendAuditEvent(HttpRequest httpRequest, DataModels.AuditEventRequest request);

	Task<DataModels.KioskDataResponse> SendKioskEvents(HttpRequest httpRequest, DataModels.KioskEventsRequest request);

	Task<DataModels.KioskDataResponse> SendKioskFunctionCheck(HttpRequest httpRequest, DataModels.KioskFunctionCheckRequest request);
}
