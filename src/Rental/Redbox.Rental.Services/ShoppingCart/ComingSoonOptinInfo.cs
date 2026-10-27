using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Services.ShoppingCart;

public class ComingSoonOptinInfo : IComingSoonOptinInfo
{
	public string EmailAddress { get; set; }

	public List<IOptInItem> OptInItems { get; } = new List<IOptInItem>();

	public void AddProduct(long productId)
	{
		IOptInItem optInItem = ServiceLocator.Instance.GetService<IMarketingServices>()?.NewOptInItem();
		if (optInItem != null)
		{
			optInItem.ItemId = productId;
			optInItem.Item_Type = KioskOptIntItemType.KioskRentalTitleID;
			optInItem.OptInMethodType = OptInMethodType.EmailAddress;
			optInItem.OptInMethodData = EmailAddress;
			optInItem.TemplateName = "EmailTemplate";
			optInItem.CampaignName = "ComingSoonRentalCampaign";
			OptInItems.Add(optInItem);
		}
	}

	public void RemoveProduct(long productId)
	{
		IOptInItem optInItem = OptInItems?.FirstOrDefault((IOptInItem x) => x.ItemId == productId);
		if (optInItem != null)
		{
			OptInItems.Remove(optInItem);
		}
	}
}
