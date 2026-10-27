using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.CustomerProfile;

public class KioskTitleInfo
{
	public string Id { get; set; }

	public string Type { get; set; }

	public List<string> Tags { get; set; }
}
