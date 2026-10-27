using System.Collections.Generic;
using KioskClientService.API.Vistar;

namespace KioskClientService.API.Services.Ads;

public class GetAssetsRequest
{
	public long KioskId { get; set; }

	public List<VistarDisplayArea> DisplayAreas { get; set; }

	public GetAssetsRequest(long kioskId, List<VistarDisplayArea> displayAreas)
	{
		KioskId = kioskId;
		DisplayAreas = displayAreas;
	}
}
