using System.Resources;

namespace Redbox.Rental.Model.KioskProduct;

public class TitleTypeProperty
{
	public string Name { get; set; }

	public string NameResource { get; set; }

	public string AlternateNamesResource { get; set; }

	public string GetName(ResourceManager resourceManager)
	{
		return resourceManager?.GetString(NameResource);
	}

	public string GetAlternateNames(ResourceManager resourceManager)
	{
		if (AlternateNamesResource == null)
		{
			return null;
		}
		return resourceManager?.GetString(AlternateNamesResource);
	}
}
