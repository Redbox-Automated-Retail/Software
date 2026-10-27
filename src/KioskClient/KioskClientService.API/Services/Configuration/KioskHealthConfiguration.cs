namespace KioskClientService.API.Services.Configuration;

[Category(Name = "KioskHealth")]
public class KioskHealthConfiguration : BaseCategorySetting
{
	public bool KioskHealthActive { get; set; } = true;

	public int KioskEventsUnloadInterval { get; set; } = 240;

	public bool TouchScreenActive { get; set; } = true;

	public bool TouchScreenReporting { get; set; } = true;

	public bool TouchScreenHealing { get; set; } = true;

	public string TouchScreenStartCheck { get; set; } = "16:00";

	public string TouchScreenEndCheck { get; set; } = "22:00";

	public int TouchScreenInActivity { get; set; } = 60;

	public int TouchScreenResetsToAlert { get; set; } = 4;

	public bool ArcusActive { get; set; } = true;

	public bool ArcusReporting { get; set; } = true;

	public bool RouterActive { get; set; } = true;

	public bool RouterHealing { get; set; } = true;

	public bool RouterReporting { get; set; } = true;

	public int RouterResetRetryMinutes { get; set; } = 2;

	public int RouterResetAfterXMessageFailures { get; set; } = 2;

	public int RouterResetMaxAttempts { get; set; } = 2;

	public bool HardwareCorrectionStatsActive { get; set; } = true;

	public bool HardwareCorrectionStatsReporting { get; set; } = true;

	public bool ViewActive { get; set; } = true;

	public bool ViewReporting { get; set; } = true;

	public int ViewHealingType { get; set; } = 2;

	public int ViewInActivity { get; set; } = 600;

	public int ViewInActivityABEMode { get; set; } = 900;

	public bool CCReaderActive { get; set; } = true;

	public bool CCReaderSendNoDataEvents { get; set; } = true;

	public int CCReaderNoDataAttemptsToAlert { get; set; } = 3;

	public bool CCReaderSendBadDataEvents { get; set; } = true;

	public int CCReaderBadDataAttemptsToAlert { get; set; } = 3;

	public bool EMVReaderActive { get; set; } = true;

	public bool EMVReaderTrackMultipleFallbacks { get; set; } = true;

	public int EMVReaderMultipleFallbacksThreshold { get; set; } = 3;

	public bool EMVReaderDisconnectAlertActive { get; set; } = true;

	public int EMVReaderDisconnectAlertVelocity { get; set; } = 20;

	public int EMVReaderDisconnectSampleSizeHours { get; set; } = 48;

	public int PingInterval { get; set; } = 600000;

	public bool PingServiceEnabled { get; set; } = true;
}
