using System.Collections.Generic;
using System.Linq;

namespace KioskClientService.API.Services.ProfileData;

public class DataTable<T>
{
	public Dictionary<long, DataRecord<T>> Data { get; set; } = new Dictionary<long, DataRecord<T>>();

	public List<T> GetData()
	{
		return Data?.Values?.Select((DataRecord<T> x) => x.Data)?.ToList();
	}
}
