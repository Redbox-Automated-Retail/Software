using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskProduct;

public interface IRedboxPlusOfferPeriod
{
	int SequenceNum { get; set; }

	int PeriodLength { get; set; }

	bool IsPromoPrice { get; set; }

	decimal Price { get; set; }

	decimal TaxRate { get; set; }

	RedboxPlusBillingPeriod BillingPeriod { get; set; }

	RedboxPlusPeriodUnit PeriodUnit { get; set; }

	IList<IRedboxPlusBenefit> Benefits { get; }
}
