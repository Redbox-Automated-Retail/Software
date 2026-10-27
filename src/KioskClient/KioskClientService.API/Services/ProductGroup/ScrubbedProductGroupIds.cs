using System.Collections.Generic;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.ProductGroup;

public class ScrubbedProductGroupIds : List<long>, IMessageScrub
{
	public void VerifyNoInvalidProductGroupIds()
	{
		Remove(0L);
		Remove(1139L);
	}

	public object Scrub()
	{
		return $"count: {base.Count}";
	}
}
