using System;
using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel.KioskServices;

public class MarketingAdImpressionMessage
{
	public Guid MessageId { get; set; }

	public string MessageType { get; set; }

	public long KioskId { get; set; }

	public string EngineVersion { get; set; }

	public string BundleVersion { get; set; }

	public bool UseMessageControl { get; set; }

	public List<AdCampaign> MarketingAdCampaigns { get; set; }
}
