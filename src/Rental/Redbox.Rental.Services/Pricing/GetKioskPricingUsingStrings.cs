using System;
using System.Collections.Generic;
using ProductPricing.Model;
using Redbox.Core;

namespace Redbox.Rental.Services.Pricing;

internal class GetKioskPricingUsingStrings
{
	public IDictionary<string, KioskPricing> KioskPricing { get; set; }

	public IDictionary<string, DefaultRentalPricing> DefaultRentalPricing { get; set; }

	public IDictionary<string, RentalPricing> RentalPricing { get; set; }

	public IDictionary<string, PurchasePricing> PurchasePricing { get; set; }

	public static GetKioskPricing ToGetKioskPricing(GetKioskPricingUsingStrings kp)
	{
		GetKioskPricing result = new GetKioskPricing
		{
			KioskPricing = new Dictionary<long, KioskPricing>(),
			DefaultRentalPricing = new Dictionary<long, DefaultRentalPricing>(),
			RentalPricing = new Dictionary<long, RentalPricing>(),
			PurchasePricing = new Dictionary<long, PurchasePricing>()
		};
		kp.KioskPricing.ForEach(delegate(KeyValuePair<string, KioskPricing> x)
		{
			result.KioskPricing.Add(Convert.ToInt32(x.Key), x.Value);
		});
		kp.DefaultRentalPricing.ForEach(delegate(KeyValuePair<string, DefaultRentalPricing> x)
		{
			result.DefaultRentalPricing.Add(Convert.ToInt32(x.Key), x.Value);
		});
		kp.RentalPricing.ForEach(delegate(KeyValuePair<string, RentalPricing> x)
		{
			result.RentalPricing.Add(Convert.ToInt32(x.Key), x.Value);
		});
		kp.PurchasePricing.ForEach(delegate(KeyValuePair<string, PurchasePricing> x)
		{
			result.PurchasePricing.Add(Convert.ToInt32(x.Key), x.Value);
		});
		return result;
	}

	public static GetKioskPricingUsingStrings ToGetKioskPricingUsingStrings(GetKioskPricing kp)
	{
		GetKioskPricingUsingStrings result = new GetKioskPricingUsingStrings
		{
			KioskPricing = new Dictionary<string, KioskPricing>(),
			DefaultRentalPricing = new Dictionary<string, DefaultRentalPricing>(),
			RentalPricing = new Dictionary<string, RentalPricing>(),
			PurchasePricing = new Dictionary<string, PurchasePricing>()
		};
		kp.KioskPricing.ForEach(delegate(KeyValuePair<long, KioskPricing> x)
		{
			result.KioskPricing.Add(x.Key.ToString(), x.Value);
		});
		kp.DefaultRentalPricing.ForEach(delegate(KeyValuePair<long, DefaultRentalPricing> x)
		{
			result.DefaultRentalPricing.Add(x.Key.ToString(), x.Value);
		});
		kp.RentalPricing.ForEach(delegate(KeyValuePair<long, RentalPricing> x)
		{
			result.RentalPricing.Add(x.Key.ToString(), x.Value);
		});
		kp.PurchasePricing.ForEach(delegate(KeyValuePair<long, PurchasePricing> x)
		{
			result.PurchasePricing.Add(x.Key.ToString(), x.Value);
		});
		return result;
	}
}
