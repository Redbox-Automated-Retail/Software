using System.Collections.Generic;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.ProfileData;

public class AddDataRecordResponse<T> : ApiBaseResponse
{
	public List<T> DataObjects { get; set; } = new List<T>();

	public bool TableFileUpdated { get; set; }
}
