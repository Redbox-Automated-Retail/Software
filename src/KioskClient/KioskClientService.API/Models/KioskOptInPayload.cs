using System;
using System.Collections.Generic;
using KioskClientService.API.App;

namespace KioskClientService.API.Models;

public class KioskOptInPayload : IMessageScrub
{
	public Guid MessageId { get; set; }

	public long KioskId { get; set; }

	public DateTime RequestDate { get; set; }

	public List<OptInItem> OptInItems { get; set; } = new List<OptInItem>();

	public object Scrub()
	{
		KioskOptInPayload kioskOptInPayload = new KioskOptInPayload
		{
			MessageId = MessageId,
			KioskId = KioskId,
			RequestDate = RequestDate
		};
		foreach (OptInItem optInItem in OptInItems)
		{
			kioskOptInPayload.OptInItems.Add(new OptInItem
			{
				ItemId = optInItem.ItemId,
				Item_Type = optInItem.Item_Type,
				OptInMethodType = optInItem.OptInMethodType,
				OptInMethodData = ObfuscationHelper.ObfuscateStringData(optInItem.OptInMethodData, 1, 1),
				CampaignName = optInItem.CampaignName,
				TemplateName = optInItem.TemplateName
			});
		}
		return kioskOptInPayload;
	}
}
