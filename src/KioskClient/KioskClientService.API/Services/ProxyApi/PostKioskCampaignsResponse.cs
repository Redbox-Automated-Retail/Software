using System;
using System.Collections.Generic;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.ProxyApi;

public class PostKioskCampaignsResponse : ApiBaseResponse
{
	public DateTime? LastSyncTime { get; set; }

	public List<InCartDetails> InCarts { get; set; }

	public List<StartScreenDetails> StartScreens { get; set; }

	public override object Scrub()
	{
		return "Returning Incarts: " + ((InCarts == null) ? "No update" : InCarts.Count.ToString()) + ", StartScreens: " + ((StartScreens == null) ? "No update" : StartScreens.Count.ToString());
	}
}
