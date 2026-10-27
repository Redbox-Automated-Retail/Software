using System.Collections.Generic;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.Ads.AdServer;

public class AdsResponse : ApiBaseResponse
{
	public List<Ad> Data { get; set; } = new List<Ad>();
}
