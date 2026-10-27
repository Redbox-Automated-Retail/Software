using System.Collections.Generic;
using KioskClientService.API.Ads;

namespace KioskClientService.API.Services.Configuration;

public interface IApplicationConfiguration
{
	List<AdLocation> AdsToPreload { get; }

	Dictionary<string, List<AdLocation>> AdsToPreloadByView { get; }
}
