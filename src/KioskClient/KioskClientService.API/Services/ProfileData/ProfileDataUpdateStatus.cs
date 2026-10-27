using System.Collections.Generic;
using KioskClientService.API.Services.App;

namespace KioskClientService.API.Services.ProfileData;

public class ProfileDataUpdateStatus : IPersistentData
{
	public List<DataTableUpdateStatus> UpdateStatusList { get; set; } = new List<DataTableUpdateStatus>();
}
