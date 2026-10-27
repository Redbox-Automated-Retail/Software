using System.Collections.Generic;

namespace Redbox.Rental.Model.Personalization;

public interface IRecommendedTitleId
{
	long Id { get; set; }

	RecommendedTitlesIdType IdType { get; set; }

	List<string> PersonalizationTitleTags { get; set; }

	bool IsWishList { get; }
}
