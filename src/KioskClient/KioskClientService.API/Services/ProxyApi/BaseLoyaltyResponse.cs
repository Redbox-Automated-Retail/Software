using System.Collections.Generic;

namespace KioskClientService.API.Services.ProxyApi;

public abstract class BaseLoyaltyResponse
{
	public List<ItemEstimation> Items = new List<ItemEstimation>();
}
