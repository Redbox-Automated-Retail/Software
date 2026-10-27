using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Dual;

public interface IInStockInOtherResponse
{
	List<long> InStock { get; }
}
