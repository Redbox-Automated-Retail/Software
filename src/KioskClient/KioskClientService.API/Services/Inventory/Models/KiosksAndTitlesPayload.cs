using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KioskClientService.API.App;
using KioskClientService.API.Validation;

namespace KioskClientService.API.Services.Inventory.Models;

public class KiosksAndTitlesPayload : IMessageScrub
{
	[Required]
	[NotDefault]
	public Guid MessageId { get; set; }

	[Required]
	[NotDefault]
	public long SourceKioskId { get; set; }

	[Required]
	[NotEmptyEnumerable]
	public List<long> KioskIds { get; set; } = new List<long>();

	public List<long> TitleIds { get; set; } = new List<long>();

	public object Scrub()
	{
		return this.ToJson();
	}
}
