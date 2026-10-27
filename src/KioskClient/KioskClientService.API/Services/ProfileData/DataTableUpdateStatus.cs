using System;

namespace KioskClientService.API.Services.ProfileData;

public class DataTableUpdateStatus
{
	public UpdateDataTable UpdateDataTable { get; set; }

	public DateTime? LastUpdateAttempt { get; set; }

	public int? NextRetryIntervalMinutes { get; set; }

	public DateTime? LastSuccessfullUpdate { get; set; }

	public bool IsDataTableUpdateNeeded(ProfileDataSettings profileDataSettings)
	{
		if (LastSuccessfullUpdate.HasValue && DateTime.Now < LastSuccessfullUpdate.Value.AddHours(profileDataSettings.RefreshIntervalHours))
		{
			return false;
		}
		if (LastUpdateAttempt.HasValue && NextRetryIntervalMinutes.HasValue && DateTime.Now < LastUpdateAttempt.Value.AddMinutes(NextRetryIntervalMinutes.Value))
		{
			return false;
		}
		return true;
	}

	public void InitializeNextRetryInterval(ProfileDataSettings profileDataSettings)
	{
		NextRetryIntervalMinutes = profileDataSettings.StartingRetryIntervalMinutes;
	}

	public void BackoffRetryInterval(ProfileDataSettings profileDataSettings)
	{
		if (!NextRetryIntervalMinutes.HasValue)
		{
			InitializeNextRetryInterval(profileDataSettings);
		}
		else
		{
			NextRetryIntervalMinutes *= 2;
		}
		if (NextRetryIntervalMinutes > profileDataSettings.MaxRetryIntervalMinutes)
		{
			NextRetryIntervalMinutes = profileDataSettings.MaxRetryIntervalMinutes;
		}
	}
}
