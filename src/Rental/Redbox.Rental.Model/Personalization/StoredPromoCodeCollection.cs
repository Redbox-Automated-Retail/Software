using System.Collections.Generic;
using Redbox.Core;
using Redbox.Rental.Model.DataService;

namespace Redbox.Rental.Model.Personalization;

public class StoredPromoCodeCollection : List<IStoredPromoCode>
{
	public List<IStoredPromoCode> GetRedboxPlusPromos()
	{
		List<IStoredPromoCode> list = new List<IStoredPromoCode>();
		IDataService dataService = ServiceLocator.Instance.GetService<IDataService>();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			IStoredPromoCode current = enumerator.Current;
			if (dataService?.GetRedboxPlusPromoCampaign(current.CampaignId) != null)
			{
				list.Add(current);
			}
		}
		return list;
	}
}
