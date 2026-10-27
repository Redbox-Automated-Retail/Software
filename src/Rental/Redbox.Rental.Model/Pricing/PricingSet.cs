using System.Collections.Generic;
using System.Linq;

namespace Redbox.Rental.Model.Pricing;

public class PricingSet : IPricingSet
{
	private List<IPricingRecord> _priceRecords = new List<IPricingRecord>();

	public string Name { get; set; }

	public string ProgramName { get; set; }

	public string PriceSetId { get; set; }

	public List<IPricingRecord> PriceRecords
	{
		get
		{
			return _priceRecords;
		}
		set
		{
			_priceRecords = value;
		}
	}

	public bool IsEqual(IPricingSet comparePricingSet)
	{
		if (comparePricingSet != null && PricingRecordsAreEqual(PriceRecords, comparePricingSet.PriceRecords))
		{
			return PricingRecordsAreEqual(comparePricingSet.PriceRecords, PriceRecords);
		}
		return false;
	}

	private bool PricingRecordsAreEqual(List<IPricingRecord> list1, List<IPricingRecord> list2)
	{
		bool result = true;
		foreach (IPricingRecord eachSourcePriceRecord in list1)
		{
			IPricingRecord pricingRecord = list2.FirstOrDefault((IPricingRecord x) => x.TitleFamily == eachSourcePriceRecord.TitleFamily && x.TitleType == eachSourcePriceRecord.TitleType);
			if (pricingRecord != null)
			{
				if (!eachSourcePriceRecord.IsEqual(pricingRecord))
				{
					result = false;
					break;
				}
				continue;
			}
			result = false;
			break;
		}
		return result;
	}
}
