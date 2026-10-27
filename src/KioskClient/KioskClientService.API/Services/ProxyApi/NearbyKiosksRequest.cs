using System;
using System.ComponentModel.DataAnnotations;
using KioskClientService.API.App;
using KioskClientService.API.Validation;

namespace KioskClientService.API.Services.ProxyApi;

public class NearbyKiosksRequest : BaseRequest, IMessageScrub
{
	[Required]
	[NotDefault]
	public Guid MessageId { get; set; }

	[Required]
	[NotDefault]
	public long KioskId { get; set; }

	public object Scrub()
	{
		return this.ToJson();
	}
}
