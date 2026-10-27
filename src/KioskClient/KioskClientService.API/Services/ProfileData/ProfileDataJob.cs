using System.Threading.Tasks;
using Coravel.Invocable;

namespace KioskClientService.API.Services.ProfileData;

public class ProfileDataJob : IProfileDataJob, IInvocable
{
	private IProfileDataService _profileDataService;

	public ProfileDataJob(IProfileDataService profileDataService)
	{
		_profileDataService = profileDataService;
	}

	public async Task Invoke()
	{
		await (_profileDataService?.ExecuteScheduledDataUpdateIfNeeded());
	}
}
