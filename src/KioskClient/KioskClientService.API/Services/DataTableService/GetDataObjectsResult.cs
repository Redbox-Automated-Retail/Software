using System.Collections.Generic;

namespace KioskClientService.API.Services.DataTableService;

public class GetDataObjectsResult<T>
{
	public Dictionary<long, T> DataObjectList { get; set; } = new Dictionary<long, T>();

	public List<long> MissingIds { get; set; } = new List<long>();
}
