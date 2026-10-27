using System.Collections.Generic;

namespace KioskClientService.API.Vistar;

public class VistarGetAssetsResponse
{
	public List<VistarAsset> asset { get; set; } = new List<VistarAsset>();
}
