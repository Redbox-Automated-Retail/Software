using KioskClientService.API.App;

namespace KioskClientService.API.Services.ProfileData;

public class GetProfileDataInstanceResponse : ApiBaseResponse
{
	public ProfileDataInstance ProfileDataInstance { get; set; }
}
