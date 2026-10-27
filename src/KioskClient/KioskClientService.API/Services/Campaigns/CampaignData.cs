using System;
using System.Collections.Generic;
using KioskClientService.API.Services.App;
using KioskClientService.API.Services.ProxyApi;

namespace KioskClientService.API.Services.Campaigns;

public class CampaignData : IPersistentData
{
	public DateTime? LastSyncTime { get; set; }

	public List<InCartDetails> InCarts { get; set; } = new List<InCartDetails>();

	public List<StartScreenDetails> StartScreens { get; set; } = new List<StartScreenDetails>();
}
