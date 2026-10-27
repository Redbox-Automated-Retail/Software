namespace KioskClientService.API.Models;

public class OptInItem
{
	public long ItemId { get; set; }

	public KioskOptIntItemType Item_Type { get; set; }

	public OptInMethodType OptInMethodType { get; set; }

	public string OptInMethodData { get; set; }

	public string TemplateName { get; set; }

	public string CampaignName { get; set; }
}
