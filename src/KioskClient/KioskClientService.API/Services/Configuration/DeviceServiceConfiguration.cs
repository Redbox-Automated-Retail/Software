namespace KioskClientService.API.Services.Configuration;

[Category(Name = "DeviceService")]
public class DeviceServiceConfiguration : BaseCategorySetting
{
	public bool ChipEnabled { get; set; } = true;

	public bool ContactlessEnabled { get; set; } = true;

	public string DeviceServiceClientUrl { get; set; } = "http://localhost:9000/CardReaderEvents";

	public bool BluefinAutoActivate { get; set; } = true;

	public string BluefinServiceUrl { get; set; }

	[MaskLogValue(VisibleChars = 4)]
	public string BluefinServiceApiKey { get; set; }

	public int BluefinServiceTimeout { get; set; } = 5000;

	public bool CardReadTechnicalFallbackEnabled { get; set; } = true;

	public int CardReadTechnicalFallbackCountLimit { get; set; } = 3;

	public int DeviceServicePleaseWaitDuration { get; set; } = 90000;

	public bool ContactlessVisaEnabled { get; set; } = true;

	public bool ContactlessAmexEnabled { get; set; } = true;

	public bool ContactlessMasterCardEnabled { get; set; } = true;

	public bool ContactlessDiscoverEnabled { get; set; } = true;

	public bool VisaInsertChipEnabled { get; set; } = true;

	public bool AmexInsertChipEnabled { get; set; } = true;

	public bool MasterCardInsertChipEnabled { get; set; } = true;

	public bool DiscoverInsertChipEnabled { get; set; } = true;

	public bool HideMobilePayOption { get; set; }

	public bool HideApplePayImage { get; set; }

	public bool HideGooglePayImage { get; set; }
}
