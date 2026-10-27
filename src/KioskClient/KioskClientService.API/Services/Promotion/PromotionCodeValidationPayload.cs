using System;
using System.ComponentModel.DataAnnotations;
using KioskClientService.API.Validation;

namespace KioskClientService.API.Services.Promotion;

public class PromotionCodeValidationPayload
{
	[NotDefault]
	public Guid MessageId { get; set; }

	[Range(1, int.MaxValue, ErrorMessage = "{0} is not within the valid range of {1} and {2}")]
	public long KioskId { get; set; }

	[Required]
	[StringLength(24, MinimumLength = 2)]
	public string Code { get; set; }
}
