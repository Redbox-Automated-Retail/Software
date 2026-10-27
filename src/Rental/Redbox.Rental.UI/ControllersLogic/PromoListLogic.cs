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
		int num = 0;
		foreach (IAcceptedOffer acceptedOffer in personalizationSession.AcceptedOffers)
		{
			PerksOfferListItemModel item = CreatePersOfferListItemModelFromAcceptedOffer(model, acceptedOffer);
			if (promoModels.Count >= num)
			{
				promoModels.Insert(num, item);
			}
			else
			{
				promoModels.Add(item);
			}
			if (num == 0)
			{
				num = 3;
				continue;
			}
			num++;
			if (num <= 4)
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
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		bool flag = false;
		foreach (IStoredPromoCode storedPromoCode in personalizationSession.StoredPromoCodes)
		{
			string promoCodeExpirationText = DetermineExpirationText(storedPromoCode);
			bool flag2 = service?.GetRedboxPlusPromoCampaign(storedPromoCode.CampaignId) != null;
			if (!flag2 || (includeRedboxPlus && !flag))
			{
				StoredPromoCodeModel storedPromoCodeModel = new StoredPromoCodeModel
				{
					PromoCode = storedPromoCode.PromoCode,
					CampaignId = storedPromoCode.CampaignId,
					PromoCodeDescription = (flag2 ? Resources.redbox_plus_free_1_night_rental : storedPromoCode.CampaignName),
					IsRedboxPlusPromo = flag2,
					PromoCodeExpirationText = promoCodeExpirationText,
					ExpirationDate = storedPromoCode.ExpirationDate,
					MaskCharacter = maskCharacter,
					IsAdded = (storedPromoCode.PromoCode == (activePromoCode ?? personalizationSession.SelectedPromo)),
					AddRemoveButtonCommand = model.AddOrRemovePromoCommand,
					AddedToBagText = Resources.promo_added_to_bag,
					Logic = model.PromoLogic,
					IsAdaMode = ApplicationState.IsADAAccessible
				};
				if (flag2)
				{
					InsertPromoModel(promoModels, 0, storedPromoCodeModel);
					flag = true;
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
			string[] array = offer.EndValueText.Split(' ');
			perksOfferListItemModel.OfferValueText = array[0];
			if (array.Length > 1)
			{
				perksOfferListItemModel.OfferUnitsText = array[1];
			}
		}
		return perksOfferListItemModel;
	}

	private static string FormatTimeLeft(string timeToComplete, DateTime? completeDate)
	{
		string result = string.Empty;
		int result2 = -1;
		if (completeDate.HasValue)
		{
			result2 = (completeDate.Value.Date - DateTime.Now.Date).Days;
		}
		else if (!string.IsNullOrEmpty(timeToComplete))
		{
			string[] array = timeToComplete.Split(' ');
			if (array.Length == 2 && array[1] == "D")
			{
				int.TryParse(array[0], out result2);
			}
		}
		if (result2 > 1 && result2 < 15)
		{
			result = string.Format(Resources.loyalty_offer_days_left, result2);
		}
		else
		{
			switch (result2)
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
		double num = Math.Round((double)(viewModel.StoredPromoCodes.IndexOf(viewModel.StoredPromoCodes.FirstOrDefault((IPerksOfferListItem p) => p is StoredPromoCode && ((StoredPromoCodeModel)p).IsAdded)) + 1) / (double)viewModel.PromosPerPage + 0.4);
		if (num < 1.0)
		{
			num = 1.0;
		}
		return (int)num;
	}
}
