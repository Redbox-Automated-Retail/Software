namespace DeviceService.Domain;

public enum VasCmd
{
	SetEnable = 1,
	SetMode = 3,
	GetData = 13,
	ClearAppleMerchantList = 500,
	AddAppleMerchantId = 501,
	GetAppleMerchantCount = 502,
	ClearGoogleMerchantList = 510,
	AddGoogleMerchantId = 511,
	GetGoogleMerchantCount = 512,
	Unknown = 513
}
