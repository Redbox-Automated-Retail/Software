namespace UpdateClientService.API.Services.Transfer;

public enum EoAuthnCap
{
	None = 0,
	MutualAuth = 1,
	StaticCloaking = 32,
	DynamicCloaking = 64,
	AnyAuthority = 128,
	MakeFullSIC = 256,
	Default = 2048,
	SecureRefs = 2,
	AccessControl = 4,
	AppID = 8,
	Dynamic = 16,
	RequireFullSIC = 512,
	AutoImpersonate = 1024,
	NoCustomMarshal = 8192,
	DisableAAA = 4096
}
