using System.ComponentModel.DataAnnotations;
using KioskClientService.API.App;
using KioskClientService.API.Validation;

namespace KioskClientService.API.Services.Dual;

public class RefreshPayload : IMessageScrub
{
	[Required]
	[NotDefault]
	public long DualKioskId { get; set; }

	public object Scrub()
	{
		return this.ToJson();
	}
}
