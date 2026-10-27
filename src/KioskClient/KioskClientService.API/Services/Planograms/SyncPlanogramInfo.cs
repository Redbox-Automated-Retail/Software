using System;
using System.Collections.Generic;
using KioskClientService.API.Services.App;
using KioskClientService.API.Services.ProxyApi;

namespace KioskClientService.API.Services.Planograms;

public class SyncPlanogramInfo : IPersistentData
{
	public bool ForceDownload { get; set; }

	public DateTime LatestChange { get; set; }

	public List<PlanogramInfo> Planograms { get; set; } = new List<PlanogramInfo>();

	public bool SyncComplete { get; set; }
}
