using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.UI.ControllersLogic;

public class PromoListLogic : BaseLogic
{
	private IPromoListModel _model;

	public static object RentalShoppingCartService { get; private set; }

	protected static IApplicationState ApplicationState => ServiceLocator.Instance.GetService<IApplicationState>();

	public PromoListLogic(IPromoListModel model)
	{
		_model = model;
	}

	public void AddPageNumberPressedAnalytics(int pageNumber)
	{
		BaseLogic.ProcessWPFHit();
		BaseLogic.AnalyticsService?.AddButtonPressEvent("PromoListLogicPageChangeNumber");
	}

	public void AddPreviousPagePressedAnalytics()
	{
		BaseLogic.ProcessWPFHit();
		BaseLogic.AnalyticsService?.AddButtonPressEvent("PromoListLogicPageBack");
	}

	public void AddNextPagePressedAnalytics()
	{
		BaseLogic.ProcessWPFHit();
		BaseLogic.AnalyticsService?.AddButtonPressEvent("PromoListLogicPageNext");
	}

	public void AddRemovePromoButtonPressedAnalytics(StoredPromoCodeModel promoCodeModel)
	{
		BaseLogic.ProcessWPFHit();
		string name = (promoCodeModel.IsAdded ? "PromoListLogicCancelRedeem" : "PromoListLogicRedeem");
		BaseLogic.AnalyticsService?.AddButtonPressEvent(name).AddData(new NameValueData("PromoCode", promoCodeModel.PromoCode));
	}

	public static List<IPerksOfferListItem> CreateOfferListItemModels(IPromoListModel model, IPersonalizationSession personalizationSession, string activePromoCode, char maskCharacter, bool includeOffers, bool includeRedboxPlus)
	{
		List<IPerksOfferListItem> list = new List<IPerksOfferListItem>();
		if (personalizationSession != null && (personalizationSession.StoredPromoCodes != null || personalizationSession.AcceptedOffers != null))
		{
			AddStoredPromos(model, personalizationSession, activePromoCode, maskCharacter, list, includeRedboxPlus);
			AddCustomerOffers(model, personalizationSession, includeOffers, list);
		}
		model.StoredPromoCodes = list;
		return list;
	}

	private static void AddCustomerOffers(IPromoListModel model, IPersonalizationSession personalizationSession, bool includeOffers, List<IPerksOfferListItem> promoModels)
	{
		if (!includeOffers || (personalizationSession.AcceptedOffers?.Count ?? 0) <= 0)
		{
			return;
		}
		int insertIndex = 0;
		foreach (IAcceptedOffer acceptedOffer in personalizationSession.AcceptedOffers)
		{
			PerksOfferListItemModel item = CreatePersOfferListItemModelFromAcceptedOffer(model, acceptedOffer);
			if (promoModels.Count >= insertIndex)
			{
				promoModels.Insert(insertIndex, item);
			}
			else
			{
				promoModels.Add(item);
			}
			if (insertIndex == 0)
			{
				insertIndex = 3;
				continue;
			}
			insertIndex++;
			if (insertIndex <= 4)
			{
				continue;
			}
			break;
		}
	}

	private static void AddStoredPromos(IPromoListModel model, IPersonalizationSession personalizationSession, string activePromoCode, char maskCharacter, List<IPerksOfferListItem> promoModels, bool includeRedboxPlus)
	{
		if (personalizationSession.StoredPromoCodes == null)
		{
			return;
		}
		IDataService dataService = ServiceLocator.Instance.GetService<IDataService>();
		bool redboxPlusPromoAdded = false;
		foreach (IStoredPromoCode storedPromoCode in personalizationSession.StoredPromoCodes)
		{
			string promoCodeExpirationText = DetermineExpirationText(storedPromoCode);
			bool isRedboxPlusPromo = dataService?.GetRedboxPlusPromoCampaign(storedPromoCode.CampaignId) != null;
			if (!isRedboxPlusPromo || (includeRedboxPlus && !redboxPlusPromoAdded))
			{
				StoredPromoCodeModel storedPromoCodeModel = new StoredPromoCodeModel
				{
					PromoCode = storedPromoCode.PromoCode,
					CampaignId = storedPromoCode.CampaignId,
					PromoCodeDescription = (isRedboxPlusPromo ? Resources.redbox_plus_free_1_night_rental : storedPromoCode.CampaignName),
					IsRedboxPlusPromo = isRedboxPlusPromo,
					PromoCodeExpirationText = promoCodeExpirationText,
					ExpirationDate = storedPromoCode.ExpirationDate,
					MaskCharacter = maskCharacter,
					IsAdded = (storedPromoCode.PromoCode == (activePromoCode ?? personalizationSession.SelectedPromo)),
					AddRemoveButtonCommand = model.AddOrRemovePromoCommand,
					AddedToBagText = Resources.promo_added_to_bag,
					Logic = model.PromoLogic,
					IsAdaMode = ApplicationState.IsADAAccessible
				};
				if (isRedboxPlusPromo)
				{
					InsertPromoModel(promoModels, 0, storedPromoCodeModel);
					redboxPlusPromoAdded = true;
				}
				else
				{
					promoModels.Add(storedPromoCodeModel);
				}
			}
		}
	}

	private static void InsertPromoModel(List<IPerksOfferListItem> promoModels, int insertPos, IPerksOfferListItem promoModelToInsert)
	{
		if (promoModels.Count > insertPos)
		{
			promoModels.Insert(insertPos, promoModelToInsert);
		}
		else
		{
			promoModels.Add(promoModelToInsert);
		}
	}

	private static PerksOfferListItemModel CreatePersOfferListItemModelFromAcceptedOffer(IPromoListModel promoListModel, IAcceptedOffer offer)
	{
		string status = offer.Status;
		string statusText = ((status == "Active") ? Resources.loyalty_offer_in_progress : string.Empty);
		string timeToComplete = FormatTimeLeft(offer.TimeToComplete, offer.CompleteDate);
		PerksOfferListItemModel perksOfferListItemModel = new PerksOfferListItemModel
		{
			OfferStatus = status,
			StatusText = statusText,
			Name = offer.Name,
			TimeToComplete = timeToComplete,
			Description = offer.Description,
			LegalInformation = offer.LegalInformation,
			CurrentValue = offer.CurrentValue,
			MaxValue = offer.MaxValue,
			RemainderValue = offer.RemainderValue,
			StartDate = offer.StartDate,
			EndDate = offer.EndDate,
			DetailsButtonText = Resources.loyalty_offer_details_button_text,
			CongratsText = Resources.loyalty_offer_congrats,
			YouEarnedText = Resources.loyalty_offer_you_earned,
			ForCompletingText = Resources.loyalty_offer_for_completing,
			IsAdaMode = ApplicationState.IsADAAccessible,
			DetailsButtonCommand = promoListModel.DetailsCommand
		};
		if (!string.IsNullOrEmpty(offer.EndValueText))
		{
			string[] parts = offer.EndValueText.Split(' ');
			perksOfferListItemModel.OfferValueText = parts[0];
			if (parts.Length > 1)
			{
				perksOfferListItemModel.OfferUnitsText = parts[1];
			}
		}
		return perksOfferListItemModel;
	}

	private static string FormatTimeLeft(string timeToComplete, DateTime? completeDate)
	{
		string result = string.Empty;
		int daysLeft = -1;
		if (completeDate.HasValue)
		{
			daysLeft = (completeDate.Value.Date - DateTime.Now.Date).Days;
		}
		else if (!string.IsNullOrEmpty(timeToComplete))
		{
			string[] parts = timeToComplete.Split(' ');
			if (parts.Length == 2 && parts[1] == "D")
			{
				int.TryParse(parts[0], out daysLeft);
			}
		}
		if (daysLeft > 1 && daysLeft < 15)
		{
			result = string.Format(Resources.loyalty_offer_days_left, daysLeft);
		}
		else
		{
			switch (daysLeft)
			{
			case 1:
				result = Resources.loyalty_offer_one_day_left;
				break;
			case 0:
				result = Resources.loyalty_offer_last_day;
				break;
			}
		}
		return result;
	}

	private static string DetermineExpirationText(IStoredPromoCode promo)
	{
		string result = string.Empty;
		if (!string.IsNullOrEmpty(promo.ExpirationText))
		{
			result = promo.ExpirationText;
		}
		else if (promo.DaysUntilExpiration.HasValue)
		{
			if (promo.DaysUntilExpiration.Value == 0)
			{
				result = Resources.promo_expires_today;
			}
			else if (promo.DaysUntilExpiration.Value == 1)
			{
				result = Resources.promo_expires_tomorrow;
			}
			else if (promo.DaysUntilExpiration > 1 && promo.DaysUntilExpiration <= 3)
			{
				result = string.Format(Resources.promo_expires_in_days, promo.DaysUntilExpiration.Value);
			}
		}
		return result;
	}

	private static object GetWordForNumber(int value)
	{
		return value switch
		{
			1 => Resources.number_1, 
			2 => Resources.number_2, 
			3 => Resources.number_3, 
			_ => value.ToString(), 
		};
	}

	public static int FindPageOfSelectedPromo(IPromoListModel viewModel)
	{
		double page = Math.Round((double)(viewModel.StoredPromoCodes.IndexOf(viewModel.StoredPromoCodes.FirstOrDefault((IPerksOfferListItem p) => p is StoredPromoCode && ((StoredPromoCodeModel)p).IsAdded)) + 1) / (double)viewModel.PromosPerPage + 0.4);
		if (page < 1.0)
		{
			page = 1.0;
		}
		return (int)page;
	}
}
