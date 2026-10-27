using System.ComponentModel;

namespace KioskClientService.API.Services.ProxyApi;

public enum PriceLevelType
{
	None = 0,
	Title = 1,
	Dvd = 2,
	Bluray = 3,
	Games = 4,
	[Description("4K UHD")]
	FourKUHD = 6
}
