using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Redbox.BrokerServices.Proxy.ComponentModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects;
using Redbox.Services.KioskBrokerServices.KioskShared.Enums;

namespace Redbox.BrokerServices.Proxy;

public class ReservationCallbackEntry3 : ICallbackEntry
{
	internal readonly AutoResetEvent ResetEvent = new AutoResetEvent(initialState: false);

	public string Name => "ReservationCallbackEntry3";

	public ReservationResult BrokerResult { get; private set; }

	public ReservationCart3 Cart { get; internal set; }

	public ReservationCallbackEntry3()
	{
		BrokerResult = new ReservationResult();
	}

	public void Invoke()
	{
		Dictionary<string, object> cartData = Cart.ToJson().ToObject<Dictionary<string, object>>();
		if (cartData.ContainsKey("Email") && cartData["Email"] != null)
		{
			cartData["Email"] = ObfuscateMessageData(cartData["Email"].ToString(), 'x', 1, 0);
		}
		if (cartData.ContainsKey("CardID") && cartData["CardID"] != null)
		{
			cartData["CardID"] = ObfuscateMessageData(cartData["CardID"].ToString(), 'x', 3, 3);
		}
		if (cartData.ContainsKey("ReservationCardHash") && cartData["ReservationCardHash"] != null)
		{
			Dictionary<string, object> cardHash = (Dictionary<string, object>)cartData["ReservationCardHash"];
			if (cardHash.ContainsKey("CardBinHash") && cardHash["CardBinHash"] != null)
			{
				cardHash["CardBinHash"] = ObfuscateMessageData(cardHash["CardBinHash"].ToString(), 'x', 3, 3);
			}
			if (cardHash.ContainsKey("Last4") && cardHash["Last4"] != null)
			{
				cardHash["Last4"] = ObfuscateMessageData(cardHash["Last4"].ToString(), 'x', 1, 0);
			}
		}
		if (cartData.ContainsKey("AlternateCardHashes") && cartData["AlternateCardHashes"] != null)
		{
			foreach (Dictionary<string, object> alternateCardHash in (ArrayList)cartData["AlternateCardHashes"])
			{
				if (alternateCardHash.ContainsKey("CardBinHash") && alternateCardHash["CardBinHash"] != null)
				{
					alternateCardHash["CardBinHash"] = ObfuscateMessageData(alternateCardHash["CardBinHash"].ToString(), 'x', 3, 3);
				}
				if (alternateCardHash.ContainsKey("Last4") && alternateCardHash["Last4"] != null)
				{
					alternateCardHash["Last4"] = ObfuscateMessageData(alternateCardHash["Last4"].ToString(), 'x', 1, 0);
				}
			}
		}
		LogHelper.Instance.Log("BrokerServicesProxy received a remote reservation request: {0}", cartData.ToJson());
		if (ReservationServicesProxy.Instance.RequestCallback3 == null)
		{
			BrokerResult.ErrorMessage = "No reservation request callback registered.";
			BrokerResult.ReservationSucceeded = false;
			ResetEvent.Set();
			LogHelper.Instance.Log("ReservationCallbackEntry3:Invoke() No reservation request callback registered.");
			return;
		}
		Dictionary<string, List<int>> titleIdsByType = new Dictionary<string, List<int>>();
		foreach (ReservationTitle3 title in Cart.Titles)
		{
			string key = ((title.ReservationType == ReservationType.Purchase) ? "purchase" : "rental");
			List<int> titleIds = (titleIdsByType.ContainsKey(key) ? titleIdsByType[key] : (titleIdsByType[key] = new List<int>()));
			titleIds.Add(title.TitleID);
		}
		IReservationResult reservationResult = ReservationServicesProxy.Instance.RequestCallback3(Cart.CardID, Cart.Email, Cart.ReferenceNumber, Cart.ReserveDate, titleIdsByType, Cart.AppliedCredit, Cart.AppliedHiveOnlinePromotion, Cart.AppliedTitleMarketing, Cart.CustomerNumber, Cart.DiscountedSubTotal, Cart.GrandTotal, Cart.HistoryTitleIDs, Cart.HiveOnlinePromotionDiscount, Cart.IsMnp, Cart.NumberOfCreditsRemaining, Cart.PromoCode, Cart.SubTotal, Cart.Tax, Cart.Titles, Cart.IsGc, Cart.GcIds, Cart.CcIds, Cart.TaxRate, Cart.Zip, Cart.IsMdv, Cart.LoyaltyRedemption, Cart.DefaultServiceFeeAmount, Cart.ActualServiceFeeAmount, Cart.AuthorizeAtPickup, Cart.ReservationCardHash, Cart.AlternateCardHashes);
		BrokerResult.ReservationSucceeded = reservationResult.Success;
		BrokerResult.ErrorMessage = reservationResult.ErrorMessage;
		List<TitleResult> titleResults = new List<TitleResult>();
		foreach (IReservedItem reservedItem in reservationResult.Items)
		{
			foreach (ReservationTitle3 cartTitle in Cart.Titles)
			{
				if (cartTitle.TitleID == reservedItem.ProductId)
				{
					titleResults.Add(new TitleResult
					{
						ItemID = cartTitle.ItemID,
						Reservable = (reservedItem.Barcode != "empty"),
						ReservedBarcode = reservedItem.Barcode
					});
					break;
				}
			}
		}
		BrokerResult.TitleResults = titleResults;
		LogHelper.Instance.Log("BrokerServicesProxy reservation response: {0}", reservationResult.ToJson());
		ResetEvent.Set();
	}

	private string ObfuscateMessageData(string msgText, char newChar, int leading, int trailing)
	{
		if (leading + trailing > msgText.Length)
		{
			leading = 0;
			trailing = 0;
		}
		string leadingText = ((leading > 0) ? msgText.Substring(0, leading) : string.Empty);
		string trailingText = ((trailing > 0) ? msgText.Substring(msgText.Length - trailing, trailing) : string.Empty);
		string mask = new string(newChar, msgText.Length - (leading + trailing));
		return leadingText + mask + trailingText;
	}
}
