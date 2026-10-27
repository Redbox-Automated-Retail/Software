using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KioskClientService.API.App;
using KioskClientService.API.Validation;

namespace KioskClientService.API.Services.Dual;

public class UpdateTitlesPayload : IMessageScrub
{
	[Required]
	[NotDefault]
	public long DualKioskId { get; set; }

	[Required]
	[NotEmptyEnumerable]
	public List<long> TitleIds { get; set; } = new List<long>();

	public object Scrub()
	{
		return this.ToJson();
	}
}
