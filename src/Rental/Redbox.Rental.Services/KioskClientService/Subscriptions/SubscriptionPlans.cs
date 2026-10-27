using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskClientService.Subscriptions;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Services.KioskProduct;

namespace Redbox.Rental.Services.KioskClientService.Subscriptions;

public class SubscriptionPlans : ISubscriptionPlans
{
	public bool Active { get; set; }

	public List<ISubscriptionProduct> SubscriptionProducts { get; set; } = new List<ISubscriptionProduct>();

	public List<IRedboxPlusPromoCampaign> RedboxPlusPromoCampaigns { get; set; } = new List<IRedboxPlusPromoCampaign>();

	public SubscriptionPlans(GetSubscriptionPlanResponse getSubscriptionPlanResponse)
	{
		if (getSubscriptionPlanResponse == null)
		{
			return;
		}
		Active = getSubscriptionPlanResponse.Active;
		if (getSubscriptionPlanResponse.SubscriptionPlans != null)
		{
			IDataService service = ServiceLocator.Instance.GetService<IDataService>();
			foreach (SubscriptionPlan subscriptionPlan in getSubscriptionPlanResponse.SubscriptionPlans)
			{
				if (subscriptionPlan.SubscriptionId == "STARZ")
				{
					LogHelper.Instance.Log("SubscriptionPlans - ignoring Starz subscriptions.  SubscriptionId: " + subscriptionPlan.SubscriptionId);
				}
				else if (subscriptionPlan.SubscriptionId == "RBPLUS")
				{
					CreateRedboxPlusSubscriptionProduct(service, subscriptionPlan);
				}
				else
				{
					LogHelper.Instance.Log("SubscriptionPlans - unexpected SubscriptionId value " + subscriptionPlan.SubscriptionId);
				}
			}
		}
		if (getSubscriptionPlanResponse.RedboxPlusPromoCampaigns == null)
		{
			return;
		}
		foreach (RedboxPlusPromoCampaign redboxPlusPromoCampaign in getSubscriptionPlanResponse.RedboxPlusPromoCampaigns)
		{
			RedboxPlusPromoCampaigns.Add(redboxPlusPromoCampaign);
		}
	}

	private void CreateRedboxPlusSubscriptionProduct(IDataService dataService, SubscriptionPlan subscriptionPlan)
	{
		if (subscriptionPlan == null || dataService == null)
		{
			return;
		}
		RedboxPlusSubscriptionProduct redboxPlusSubscriptionProduct = new RedboxPlusSubscriptionProduct(dataService?.GetStore())
		{
			SubscriptionId = subscriptionPlan.Id,
			ProductName = subscriptionPlan.Name,
			ProductDescription = subscriptionPlan.Description,
			IsPromotional = subscriptionPlan.IsPromotional,
			TaxRate = (dataService?.GetStore().RentalTaxRate / (decimal?)100).GetValueOrDefault()
		};
		RedboxPlusSubscriptionConstructorHelper.PopulateHardCodedFields(redboxPlusSubscriptionProduct);
		foreach (SubscriptionPlanItem planItem in subscriptionPlan.PlanItems)
		{
			IRedboxPlusOfferPeriod redboxPlusOfferPeriod = new RedboxPlusOfferPeriod
			{
				SequenceNum = planItem.SequenceNum,
				PeriodLength = planItem.PeriodLength,
				IsPromoPrice = planItem.IsPromoPrice,
				Price = planItem.Price
			};
			Enum.TryParse<RedboxPlusBillingPeriod>(planItem.BillingPeriod.ToString(), out var result);
			redboxPlusOfferPeriod.BillingPeriod = result;
			if (planItem.Benefits != null)
			{
				foreach (Benefit benefit in planItem.Benefits)
				{
					RedboxPlusBenefit redboxPlusBenefit = new RedboxPlusBenefit
					{
						Id = benefit.Id,
						Description = benefit.Description,
						Quantity = benefit.Quantity.GetValueOrDefault()
					};
					Enum.TryParse<RedboxPlusBenefitType>(benefit.Type.ToString(), out var result2);
					redboxPlusBenefit.RedboxPlusBenefitType = result2;
					Enum.TryParse<RedboxPlusBenefitPeriod>(benefit.Period.ToString(), out var result3);
					redboxPlusBenefit.Period = result3;
					redboxPlusOfferPeriod.Benefits.Add(redboxPlusBenefit);
				}
			}
			redboxPlusSubscriptionProduct.OfferPeriods.Add(redboxPlusOfferPeriod);
		}
		SubscriptionProducts.Add(redboxPlusSubscriptionProduct);
	}
}
