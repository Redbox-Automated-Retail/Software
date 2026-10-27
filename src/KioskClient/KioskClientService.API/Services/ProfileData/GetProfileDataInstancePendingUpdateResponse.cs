using KioskClientService.API.App;

namespace KioskClientService.API.Services.ProfileData;

public class GetProfileDataInstancePendingUpdateResponse : ApiBaseResponse
{
	public bool HasPendingUpdate { get; set; }
}
