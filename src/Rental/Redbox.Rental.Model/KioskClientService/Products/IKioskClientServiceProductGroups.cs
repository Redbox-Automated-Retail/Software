using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Products;

public interface IKioskClientServiceProductGroups
{
	List<long> GetMoreLikeThis(long productGroupId);

	void RefreshAllMLTData(List<long> productGroupIds);

	ProductGroupSortOrder GetSortOrder();
}
