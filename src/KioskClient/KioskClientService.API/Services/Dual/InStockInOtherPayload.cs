using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KioskClientService.API.App;
using KioskClientService.API.Validation;

namespace KioskClientService.API.Services.Dual;

public class InStockInOtherPayload : IMessageScrub
{
	[Required]
	[NotEmptyEnumerable]
	public ICollection<long> TitleIds = new List<long>();

	public object Scrub()
	{
		return this.ToJson();
	}
}
