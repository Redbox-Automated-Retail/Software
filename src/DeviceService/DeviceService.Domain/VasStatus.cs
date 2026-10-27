namespace DeviceService.Domain;

public enum VasStatus
{
	Successful = 0,
	Failed = 1,
	Unsupported = 2,
	Invalid = 4,
	DataOverflow = 5,
	VasSuccessful = 6,
	NoVasData = 7,
	InvalidMerchant = 8,
	Unknown = 9
}
