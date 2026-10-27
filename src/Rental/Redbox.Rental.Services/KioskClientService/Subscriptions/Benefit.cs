using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Redbox.Rental.Services.KioskClientService.Subscriptions;

public class Benefit
{
	public string Id { get; set; }

	public string Name { get; set; }

	public string Description { get; set; }

	public int? Quantity { get; set; }

	public List<Trait> Traits { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	public BenefitPeriod Period { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	public BenefitType Type { get; set; }

	[OnError]
	internal void OnError(StreamingContext context, ErrorContext errorContext)
	{
		errorContext.Handled = false;
		if (errorContext.Member.ToString().Equals("Period", StringComparison.CurrentCultureIgnoreCase))
		{
			(errorContext.OriginalObject as Benefit).Period = BenefitPeriod.Error;
			errorContext.Handled = true;
		}
		if (errorContext.Member.ToString().Equals("Type", StringComparison.CurrentCultureIgnoreCase))
		{
			(errorContext.OriginalObject as Benefit).Type = BenefitType.Error;
			errorContext.Handled = true;
		}
	}
}
