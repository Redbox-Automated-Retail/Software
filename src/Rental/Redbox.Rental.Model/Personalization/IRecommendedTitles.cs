using System.Collections;
using System.Collections.Generic;

namespace Redbox.Rental.Model.Personalization;

public interface IRecommendedTitles : IList<IRecommendedTitleId>, ICollection<IRecommendedTitleId>, IEnumerable<IRecommendedTitleId>, IEnumerable
{
	string TivoQueryId { get; set; }

	string OfferCode { get; set; }
}
