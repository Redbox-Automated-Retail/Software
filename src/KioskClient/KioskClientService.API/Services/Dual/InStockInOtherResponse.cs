using System.Collections.Generic;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.Dual;

public class InStockInOtherResponse : ApiBaseResponse
{
	public List<long> InStock = new List<long>();

	public override object Scrub()
	{
		return this.ToJson();
	}
}
