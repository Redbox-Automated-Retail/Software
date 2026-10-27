namespace KioskClientService.API;

public class ProfileDataSettings
{
	public int RefreshIntervalHours { get; set; }

	public int StartingRetryIntervalMinutes { get; set; }

	public int MaxRetryIntervalMinutes { get; set; }
}
