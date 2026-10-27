using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Redbox.Rental.Services.KioskClientService.Subscriptions;

public class SubscriptionPlanItem
{
	public decimal Price { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	public BillingPeriod BillingPeriod { get; set; }

	public bool IsPromoPrice { get; set; }

	public int PeriodLength { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	public PeriodUnit PeriodUnit { get; set; }

	public int SequenceNum { get; set; }

	public List<Benefit> Benefits { get; set; }

	[OnError]
	internal void OnError(StreamingContext context, ErrorContext errorContext)
	{
		errorContext.Handled = false;
		if (errorContext.Member.ToString().Equals("billingPeriod", StringComparison.CurrentCultureIgnoreCase))
		{
			(errorContext.OriginalObject as SubscriptionPlanItem).BillingPeriod = BillingPeriod.Error;
			errorContext.Handled = true;
		}
		if (errorContext.Member.ToString().Equals("periodUnit", StringComparison.CurrentCultureIgnoreCase))
		{
			(errorContext.OriginalObject as SubscriptionPlanItem).PeriodUnit = PeriodUnit.Error;
			errorContext.Handled = true;
		}
	}
}
