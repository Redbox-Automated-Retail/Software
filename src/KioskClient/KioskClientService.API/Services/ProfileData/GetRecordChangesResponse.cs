using KioskClientService.API.App;

namespace KioskClientService.API.Services.ProfileData;

public class GetRecordChangesResponse : ApiBaseResponse
{
	public string DataUpdateResponseFileName { get; set; }
}
