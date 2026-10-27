using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Configuration;

public class ConfigLogParameters
{
	public List<string> IncludeCategories { get; set; }

	public List<string> ExcludeCategories { get; set; }

	public List<(string category, string setting)> IncludeSettings { get; set; }

	public List<(string category, string setting)> ExcludeSettings { get; set; }
}
