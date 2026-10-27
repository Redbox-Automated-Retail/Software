using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Installer;

public class OpsMarketResponse
{
	public bool Success { get; set; }

	public int StatusCode { get; set; }

	public List<Error> Errors { get; set; } = new List<Error>();

	public string OpsMarket { get; set; }

	public string KaseyaServer { get; set; }
}
