using System.Collections.Generic;
using Redbox.Rental.Model.KioskClientService.Dual;
using Redbox.Rental.Services.App;

namespace Redbox.Rental.Services.KioskClientService.Dual;

public class InStockInOtherResponse : ApiBaseResponse, IInStockInOtherResponse
{
	public List<long> InStock { get; set; } = new List<long>();
}
