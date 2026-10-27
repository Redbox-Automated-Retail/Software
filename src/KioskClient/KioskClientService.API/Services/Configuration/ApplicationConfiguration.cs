using System.Collections.Generic;
using KioskClientService.API.Ads;

namespace KioskClientService.API.Services.Configuration;

public class ApplicationConfiguration : IApplicationConfiguration
{
	public List<AdLocation> AdsToPreload => new List<AdLocation>
	{
		AdLocation.TitleDetail,
		AdLocation.Welcome
	};

	public Dictionary<string, List<AdLocation>> AdsToPreloadByView => new Dictionary<string, List<AdLocation>>
	{
		{
			"zip_code_view",
			new List<AdLocation> { AdLocation.VSM }
		},
		{
			"vsm_view",
			new List<AdLocation> { AdLocation.Vending }
		}
	};
}
