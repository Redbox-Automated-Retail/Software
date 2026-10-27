using System.Collections.Generic;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Services.KioskProduct;

public class RedboxPlusOfferPeriod : IRedboxPlusOfferPeriod
{
	public int SequenceNum { get; set; }

	public int PeriodLength { get; set; }

	public bool IsPromoPrice { get; set; }

	public decimal Price { get; set; }

	public decimal TaxRate { get; set; }

	public RedboxPlusBillingPeriod BillingPeriod { get; set; }

	public RedboxPlusPeriodUnit PeriodUnit { get; set; }

	public IList<IRedboxPlusBenefit> Benefits { get; set; }

	public RedboxPlusOfferPeriod()
	{
		Benefits = new List<IRedboxPlusBenefit>();
	}
}
