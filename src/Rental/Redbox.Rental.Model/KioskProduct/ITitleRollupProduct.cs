using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskProduct;

public interface ITitleRollupProduct : ITitleProduct, IKioskProduct
{
	List<ITitleProduct> KioskProducts { get; }

	long ProductGroupId { get; }
}
