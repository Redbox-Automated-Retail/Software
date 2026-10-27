namespace Redbox.KioskEngine.ComponentModel.KioskServices;

public interface IOptInItem
{
	long ItemId { get; set; }

	KioskOptIntItemType Item_Type { get; set; }

	OptInMethodType OptInMethodType { get; set; }

	string OptInMethodData { get; set; }

	string TemplateName { get; set; }

	string CampaignName { get; set; }
}
