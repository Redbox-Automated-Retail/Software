using System.Collections.Generic;
using System.Threading.Tasks;
using KioskClientService.API.Services.ProfileData;

namespace KioskClientService.API.Services.DataTableService;

public interface IDataTableService
{
	Task<DataTable<T>> GetDataTable<T>();

	Task<GetDataObjectsResult<T>> GetDataObjects<T>(List<long> ids);

	Task<List<long>> GetExistingDataRecordIds<T>();

	Task<AddDataRecordResponse<T>> AddDataRecords<T>(List<long> ids);

	Task<DataUpdateResult> RequestDataTableUpdate<T>(List<RecordRequest> recordRequests);

	bool IsValidUpdateDataTable(UpdateDataTable updateDataTable);

	UpdateDataTable GetUpdateDataTableAttribute<T>();
}
