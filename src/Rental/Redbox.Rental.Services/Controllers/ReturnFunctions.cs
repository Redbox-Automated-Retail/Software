using System;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskClientService.Transactions;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class ReturnFunctions
{
	public static void ShowMachineFullWarning()
	{
		Action action = delegate
		{
			ServiceLocator.Instance.GetService<IRedboxRental>()?.CloseOpenSessionAndStartNewRentalSession("machine full return");
		};
		MessagePopupViewController.ShowMachineFullWarning(action, action);
	}

	public static bool IsValidBarcode(string barcode)
	{
		bool flag = false;
		try
		{
			int.Parse(barcode);
			flag = true;
		}
		catch
		{
			flag = false;
		}
		if (flag)
		{
			return barcode.Length == 9;
		}
		return false;
	}

	public static void ShowReturnCompleteViewWithPromoCheck(string barcode, Action closeAction)
	{
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.MarketingReturn.ReturnVisitPromoOfferEnabled)
		{
			ServiceLocator.Instance.GetService<IKioskClientService>().Transaction.ReturnPromoCheck(barcode, delegate(ReturnVisitPromotionInfo promo)
			{
				PromoCheckCallback(promo, closeAction);
			});
		}
		else
		{
			MessagePopupViewController.ShowMessagePopupViewReturnSuccess(closeAction);
		}
	}

	private static void PromoCheckCallback(ReturnVisitPromotionInfo promoInfo, Action closeAction)
	{
		(ServiceLocator.Instance.GetService<IEngineApplication>() as ApplicationContext).MainForm.Invoke((Action)delegate
		{
			if (promoInfo != null && promoInfo.Success && promoInfo.IsLastDisc && promoInfo.Promo != null && DateTime.Now <= promoInfo.Promo.ExpirationDate && !string.IsNullOrEmpty(promoInfo.Promo.PromoCode))
			{
				if (promoInfo.Promo.PromoType == ReturnPromoType.AmountOff)
				{
					decimal? amount = promoInfo.Promo.Amount;
					if ((amount.GetValueOrDefault() > 0m) & amount.HasValue)
					{
						goto IL_00c9;
					}
				}
				if (promoInfo.Promo.PromoType == ReturnPromoType.FirstNightFree)
				{
					goto IL_00c9;
				}
			}
			if (promoInfo != null && promoInfo.Success && promoInfo.IsLastDisc)
			{
				MessagePopupViewController.ShowMessagePopupViewReturnSuccessWithBrowse(delegate
				{
					closeAction?.Invoke();
				}, delegate
				{
					ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddButtonPressEvent("ReturnSuccessWithBrowseClose");
					closeAction?.Invoke();
				}, delegate
				{
					ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddButtonPressEvent("ReturnSuccessWithBrowseSeeNewMovies");
					closeAction?.Invoke();
					OrchestrationService.StartUserInteraction(SessionStartTriggerActionList.Instance.FindByName(SessionStartTriggerActionConstants.ShowBrowseView));
					Redbox.Rental.Services.Session.Session.Instance?.BrowseState?.SetDefaultBrowseStateSet();
					OrchestrationService.ViewShow("browse_view", BrowseFunctions.GetBrowseViewparameters());
				});
			}
			else
			{
				MessagePopupViewController.ShowMessagePopupViewReturnSuccess(closeAction);
			}
			return;
			IL_00c9:
			OrchestrationService.ViewShow("return_visit_promo_offer_view", new ReturnVisitPromoOfferViewParameters
			{
				PromoCode = promoInfo.Promo.PromoCode,
				PromoType = promoInfo.Promo.PromoType,
				Amount = promoInfo.Promo.Amount,
				ExpirationDate = promoInfo.Promo.ExpirationDate,
				CloseAction = closeAction
			});
		});
	}
}
