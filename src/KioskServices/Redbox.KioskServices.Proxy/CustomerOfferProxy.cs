using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.KioskServices.Proxy.ComponentModel.Loyalty;
using Redbox.KioskServices.Proxy.Request;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.KioskServices.Proxy;

public class CustomerOfferProxy : ICustomerOfferService
{
	private int m_working;

	public static CustomerOfferProxy Instance => Singleton<CustomerOfferProxy>.Instance;

	public bool LiveCall
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Customer Offer Services", "LiveCall", defaultValue: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Customer Offer Services", "LiveCall", value);
		}
	}

	public bool SimulateSuccess
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Customer Offer Services", "SimulateSuccess", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Customer Offer Services", "SimulateSuccess", value);
		}
	}

	public bool IncludeFreeGameInCartOffer
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Customer Offer Services", "IncludeFreeGameInCartOffer", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Customer Offer Services", "IncludeFreeGameInCartOffer", value);
		}
	}

	public bool IncludeMovieDiscountInCartOffer
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Customer Offer Services", "IncludeMovieDiscountInCartOffer", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Customer Offer Services", "IncludeMovieDiscountInCartOffer", value);
		}
	}

	public bool IncludeFracRedeemableFreeMovieOffer
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Customer Offer Services", "IncludeFracRedeemableFreeMovieOffer", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Customer Offer Services", "IncludeFracRedeemableFreeMovieOffer", value);
		}
	}

	public bool IncludeFracRedeemableDiscountedDiscOffer
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Customer Offer Services", "IncludeFracRedeemableDiscountedDiscOffer", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Customer Offer Services", "IncludeFracRedeemableDiscountedDiscOffer", value);
		}
	}

	public bool IncludeFracStorableFreeMovieOffer
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Customer Offer Services", "IncludeFracStorableFreeMovieOffer", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Customer Offer Services", "IncludeFracStorableFreeMovieOffer", value);
		}
	}

	public bool IncludeFracStorableDiscountedDiscOffer
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Customer Offer Services", "IncludeFracStorableDiscountedDiscOffer", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Customer Offer Services", "IncludeFracStorableDiscountedDiscOffer", value);
		}
	}

	public bool IncludeMNPInCartOffer
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Customer Offer Services", "IncludeInCartMNPOffer", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Customer Offer Services", "IncludeInCartMNPOffer", value);
		}
	}

	public bool IncludePerksFreeRentalOffer
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Customer Offer Services", "IncludePerksFreeRentalOffer", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Customer Offer Services", "IncludePerksFreeRentalOffer", value);
		}
	}

	public bool IncludePerksAmountOffOffer
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Customer Offer Services", "IncludePerksAmountOffOffer", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Customer Offer Services", "IncludePerksAmountOffOffer", value);
		}
	}

	public bool IncludePerksExtraPointsOffer
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Customer Offer Services", "IncludePerksExtraPointsOffer", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Customer Offer Services", "IncludePerksExtraPointsOffer", value);
		}
	}

	public bool IncludePerksReminderMessage
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Customer Offer Services", "IncludePerksReminderMessage", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Customer Offer Services", "IncludePerksReminderMessage", value);
		}
	}

	public bool IncludePurchaseOffer
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Customer Offer Services", "IncludePurchaseOffer", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Customer Offer Services", "IncludePurchaseOffer", value);
		}
	}

	public List<long> MnpProductIds
	{
		get
		{
			List<long> list = new List<long>();
			string value = ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Customer Offer Services", "MnpProductIds", "");
			if (!string.IsNullOrEmpty(value))
			{
				string[] array = value.Split(',');
				for (int i = 0; i < array.Length; i++)
				{
					if (long.TryParse(array[i], out var result))
					{
						list.Add(result);
					}
				}
			}
			return list;
		}
		set
		{
			string value2 = value.Join(",");
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Customer Offer Services", "MnpProductIds", value2);
		}
	}

	public bool IncludeTitles
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Customer Offer Services", "IncludeTitles", defaultValue: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Customer Offer Services", "IncludeTitles", value);
		}
	}

	public List<int> FormatIds
	{
		get
		{
			List<int> list = new List<int>();
			string value = ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Customer Offer Services", "ForamtIds", "");
			if (!string.IsNullOrEmpty(value))
			{
				string[] array = value.Split(',');
				for (int i = 0; i < array.Length; i++)
				{
					if (int.TryParse(array[i], out var result))
					{
						list.Add(result);
					}
				}
			}
			return list;
		}
		set
		{
			value.Join(",");
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Customer Offer Services", "FormatIds", service);
		}
	}

	public KioskLoginResponseErrors ResponseCode
	{
		get
		{
			string value = ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Customer Offer Services", "KioskLoginResponseError", "Error");
			return (KioskLoginResponseErrors)Enum.Parse(typeof(KioskLoginResponseErrors), value, ignoreCase: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Customer Offer Services", "KioskLoginResponseError", value);
		}
	}

	private CustomerOfferProxy()
	{
	}

	public void GetOffers(string kioskId, string languageCode, string storefrontEventType, string sessionId, RemoteServiceCallback completeCallback)
	{
		if (LiveCall)
		{
			PerformGetOffersLiveCall(kioskId, languageCode, storefrontEventType, sessionId, completeCallback);
		}
		else
		{
			PerformGetOffersSimulatedCall(completeCallback);
		}
	}

	private void PerformGetOffersLiveCall(string kioskId, string languageCode, string storefrontEventType, string sessionId, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			IShoppingSession session = ServiceLocator.Instance.GetService<IShoppingSessionService>().GetSession(sessionId);
			if (session == null)
			{
				LogHelper.Instance.Log("The specified shopping session does not exist: {0}", sessionId);
				return;
			}
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				if (!ServiceLocator.Instance.GetService<Redbox.KioskServices.Proxy.ComponentModel.IStoreServices>().IsStandAlone)
				{
					IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>().GetRentalShoppingCart(new Guid(sessionId));
					IPromotion promo = rentalShoppingCart?.Promotion;
					IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
					Dictionary<string, object> creditCardData = ProxyMessageHelper.GetCreditCardData(session, includeTrack2: true);
					ShoppingCart shoppingCart = ProxyMessageHelper.CreateShoppingCart(rentalShoppingCart, promo);
					string utcOffset = ProxyMessageHelper.CalcUtcOffset(DateTime.Now);
					if (ServiceLocator.Instance.GetService<IConfiguration>().SendCustomerOffersToKioskClientService)
					{
						LogHelper.Instance.Log("Live call to remote KioskClientService");
						IGetCustomerOffersResult result = ServiceLocator.Instance.GetService<IKioskClientService>().Transaction.GetCustomerOffersAsync(kioskId, creditCardData, shoppingCart.ToJson().ToObject<Dictionary<string, object>>(), utcOffset, languageCode, storefrontEventType).Result;
						remoteServiceResult.Properties["CustomerProfileNumber"] = result.CustomerProfileNumber;
						remoteServiceResult.Properties["AccountNumber"] = result.AccountNumber;
						remoteServiceResult.Properties["EmailAddress"] = result.EmailAddress;
						if (result.StoredOffers != null && result.StoredOffers.Count > 0)
						{
							List<CustomerOffer> list = new List<CustomerOffer>();
							foreach (CustomerOfferResult storedOffer in result.StoredOffers)
							{
								list.Add(CustomerOfferFromCustomerOfferResult(storedOffer));
							}
							remoteServiceResult.Properties["StoredOffers"] = list;
						}
						if (result.NewOffers != null && result.NewOffers.Count > 0)
						{
							List<CustomerOffer> list2 = new List<CustomerOffer>();
							foreach (CustomerOfferResult newOffer in result.NewOffers)
							{
								list2.Add(CustomerOfferFromCustomerOfferResult(newOffer));
							}
							remoteServiceResult.Properties["NewOffers"] = list2;
						}
						remoteServiceResult.Success = result.Success;
						if (!remoteServiceResult.Success)
						{
							Redbox.KioskEngine.ComponentModel.Error error = result.Errors.FirstOrDefault();
							if (error != null)
							{
								object fieldsForResponseCode = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(KioskLoginResponseErrors?), error.Code);
								remoteServiceResult.Properties["response"] = fieldsForResponseCode ?? ((object)KioskLoginResponseErrors.Error);
							}
							else
							{
								remoteServiceResult.Properties["response"] = KioskLoginResponseErrors.Error;
							}
						}
					}
					else
					{
						LogHelper.Instance.Log($"Live call to remote ICustomerOfferProxy, URL: {KioskServicesProxy.Instance.Url}");
						IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(new
						{
							KioskId = kioskId,
							MessageId = Guid.NewGuid(),
							MessageType = "GetCustomerOffers",
							EngineVersion = service["EngineVersion"],
							BundleVersion = service["ProductVersion"],
							Lang = languageCode,
							StorefrontEventType = storefrontEventType,
							UtcOffset = utcOffset,
							CreditCard = creditCardData,
							ShoppingCart = shoppingCart
						});
						ServiceLocator.Instance.GetService<IHealthServices>().UpdateOnlineStatus(dictionary);
						remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
						remoteServiceResult.Properties["CustomerProfileNumber"] = GetResponseValue(dictionary, "CustomerProfileNumber", null)?.ToString();
						remoteServiceResult.Properties["AccountNumber"] = GetResponseValue(dictionary, "AccountNumber", null)?.ToString();
						remoteServiceResult.Properties["EmailAddress"] = GetResponseValue(dictionary, "EmailAddress", null)?.ToString();
						if (dictionary.ContainsKey("StoredOffers") && dictionary["StoredOffers"] != null)
						{
							ArrayList obj = (ArrayList)dictionary["StoredOffers"];
							remoteServiceResult.Properties["StoredOffers"] = new List<CustomerOffer>();
							List<CustomerOffer> list3 = new List<CustomerOffer>();
							foreach (IDictionary<string, object> item in obj)
							{
								CustomerOffer customerOffer = new CustomerOffer();
								ParseOffer(item, customerOffer);
								list3.Add(customerOffer);
							}
							remoteServiceResult.Properties["StoredOffers"] = list3;
						}
						if (dictionary.ContainsKey("NewOffers") && dictionary["NewOffers"] != null)
						{
							ArrayList obj2 = (ArrayList)dictionary["NewOffers"];
							remoteServiceResult.Properties["NewOffers"] = new List<CustomerOffer>();
							List<CustomerOffer> list4 = new List<CustomerOffer>();
							foreach (IDictionary<string, object> item2 in obj2)
							{
								CustomerOffer customerOffer2 = new CustomerOffer();
								ParseOffer(item2, customerOffer2);
								list4.Add(customerOffer2);
							}
							remoteServiceResult.Properties["NewOffers"] = list4;
						}
						if (!remoteServiceResult.Success)
						{
							ArrayList arrayList = (dictionary.ContainsKey("Errors") ? (dictionary["Errors"] as ArrayList) : null);
							if (arrayList != null && arrayList.Count > 0)
							{
								if (arrayList[0] is Dictionary<string, object> dictionary2)
								{
									object fieldsForResponseCode2 = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(KioskLoginResponseErrors?), (string)dictionary2["Code"]);
									remoteServiceResult.Properties["response"] = fieldsForResponseCode2 ?? ((object)KioskLoginResponseErrors.Error);
								}
							}
							else
							{
								remoteServiceResult.Properties["response"] = KioskLoginResponseErrors.Error;
							}
						}
					}
				}
				else
				{
					LogHelper.Instance.Log("In stand alone mode, skipping call to get offers.", LogEntryType.Info);
					remoteServiceResult.Properties["response"] = KioskLoginResponseErrors.LoyaltyIsDown;
					remoteServiceResult.Properties["is_online"] = false;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in CustomerOfferProxy.PerformGetOffersLiveCall.", e);
				remoteServiceResult.Properties["response"] = KioskLoginResponseErrors.Error;
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					remoteServiceResult.Properties["stand_alone"] = ServiceLocator.Instance.GetService<Redbox.KioskServices.Proxy.ComponentModel.IStoreServices>().IsStandAlone;
					LogHelper.Instance.Log("IRemoteServiceResult: {0}", remoteServiceResult?.ToObfuscatedString());
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from CustomerOfferProxy.PerformGetOffersLiveCall."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private CustomerOffer CustomerOfferFromCustomerOfferResult(CustomerOfferResult offer)
	{
		bool flag = offer.IncludeFormatIds != null && offer.IncludeFormatIds.Count > 0;
		bool flag2 = offer.IncludeTitleIds != null && offer.IncludeTitleIds.Count > 0;
		CustomerOffer customerOffer = new CustomerOffer
		{
			OfferId = offer.OfferId,
			StoredOfferId = offer.StoredOfferId,
			OfferDiscountCode = offer.OfferDiscountCode,
			PromotionCode = offer.PromotionCode,
			DiscountType = (OfferDiscountType)Enum.Parse(typeof(OfferDiscountType), offer.DiscountType),
			CampaignId = offer.CampaignId,
			TitleFamily = ParseTitleFamily(offer.ProductType),
			ExpirationDate = offer.ExpirationDate,
			IncludeOlderThan = offer.ExcludeIfWithinRedboxReleaseDate,
			Amount = offer.DiscountAmount,
			ActionCode = ParseActionCode(offer.ActionCode),
			IncludeFormats = flag,
			FormatIds = (flag ? offer.IncludeFormatIds.ToList() : offer.ExcludeFormatIds.ToList()),
			IncludeTitles = flag2,
			TitleIds = (flag2 ? offer.IncludeTitleIds.ToList() : offer.ExcludeTitleIds.ToList()),
			RedeemedDateTime = offer.RedeemedDateTime
		};
		if (!string.IsNullOrEmpty(offer.OfferScopeType))
		{
			customerOffer.OfferScopeType = (OfferScopeType)Enum.Parse(typeof(OfferScopeType), offer.OfferScopeType);
		}
		if (!string.IsNullOrEmpty(offer.CustomerSegmentType))
		{
			customerOffer.CustomerSegmentType = (CustomerSegmentType)Enum.Parse(typeof(CustomerSegmentType), offer.CustomerSegmentType);
		}
		customerOffer.OfferProducts = offer.OfferProducts?.Select((OfferProductResult op) => new OfferProduct
		{
			ProductId = op.ProductId,
			NumberOfNights = op.NumberOfNights,
			DiscountedPrice = op.DiscountedPrice,
			TotalDiscountAmount = op.TotalDiscountAmount
		}).ToList();
		return customerOffer;
	}

	private void ParseOffer(IDictionary<string, object> promo, CustomerOffer resultOffer)
	{
		object responseValue = GetResponseValue(promo, "OfferId", 0);
		resultOffer.OfferId = long.Parse(responseValue.ToString());
		object responseValue2 = GetResponseValue(promo, "StoredOfferId", null);
		if (responseValue2 != null)
		{
			resultOffer.StoredOfferId = long.Parse(responseValue2.ToString());
		}
		resultOffer.OfferDiscountCode = GetResponseValue(promo, "OfferDiscountCode", null)?.ToString();
		resultOffer.PromotionCode = GetResponseValue(promo, "PromotionCode", null)?.ToString();
		string value = GetResponseValue(promo, "DiscountType", null)?.ToString();
		resultOffer.DiscountType = (OfferDiscountType)Enum.Parse(typeof(OfferDiscountType), value);
		string text = GetResponseValue(promo, "OfferScopeType", null)?.ToString();
		if (text != null)
		{
			resultOffer.OfferScopeType = (OfferScopeType)Enum.Parse(typeof(OfferScopeType), text);
		}
		string text2 = GetResponseValue(promo, "CustomerSegmentType", null)?.ToString();
		if (text2 != null)
		{
			resultOffer.CustomerSegmentType = (CustomerSegmentType)Enum.Parse(typeof(CustomerSegmentType), text2);
		}
		string text3 = GetResponseValue(promo, "CampaignId", null)?.ToString();
		if (text3 != null)
		{
			resultOffer.CampaignId = long.Parse(text3.ToString());
		}
		resultOffer.TitleFamily = ParseTitleFamily(GetResponseValue(promo, "ProductType", null)?.ToString());
		resultOffer.ExpirationDate = (DateTime)GetResponseValue(promo, "ExpirationDate", DateTime.Today);
		resultOffer.IncludeOlderThan = GetResponseTimeSpan(promo, "ExcludeIfWithinRedboxReleaseDate", null);
		resultOffer.Amount = GetResponseDecimal(promo, "DiscountAmount", 0m);
		resultOffer.ActionCode = ParseActionCode(GetResponseValue(promo, "ActionCode", null)?.ToString());
		resultOffer.IncludeFormats = promo.ContainsKey("IncludeFormats") && ((ArrayList)promo["IncludeFormats"]).Count > 0;
		string key = (resultOffer.IncludeFormats ? "IncludeFormats" : "ExcludeFormats");
		ArrayList arrayList = (promo.ContainsKey(key) ? (promo[key] as ArrayList) : null);
		if (arrayList != null)
		{
			List<int> list = new List<int>();
			foreach (string item in arrayList)
			{
				if (int.TryParse(item, out var result))
				{
					list.Add(result);
				}
			}
			resultOffer.FormatIds = list;
		}
		resultOffer.IncludeTitles = promo.ContainsKey("IncludeTitleIds") && promo["IncludeTitleIds"] is ArrayList && ((ArrayList)promo["IncludeTitleIds"]).Count > 0;
		string key2 = (resultOffer.IncludeTitles ? "IncludeTitleIds" : "ExcludeTitleIds");
		ArrayList arrayList2 = (promo.ContainsKey(key2) ? (promo[key2] as ArrayList) : null);
		if (arrayList2 != null)
		{
			List<long> list2 = new List<long>();
			foreach (object item2 in arrayList2)
			{
				if (item2 is int)
				{
					list2.Add((int)item2);
				}
				else if (item2 is long)
				{
					list2.Add((long)item2);
				}
			}
			resultOffer.TitleIds = list2;
		}
		ArrayList arrayList3 = (promo.ContainsKey("OfferProducts") ? (promo["OfferProducts"] as ArrayList) : null);
		if (arrayList3 == null)
		{
			return;
		}
		List<OfferProduct> list3 = new List<OfferProduct>();
		foreach (IDictionary<string, object> item3 in arrayList3)
		{
			OfferProduct offerProduct = new OfferProduct();
			string s = GetResponseValue(item3, "ProductId", "-1").ToString();
			offerProduct.ProductId = long.Parse(s);
			string text4 = GetResponseValue(item3, "NumberOfNights", null)?.ToString();
			offerProduct.NumberOfNights = (string.IsNullOrWhiteSpace(text4) ? ((byte?)null) : new byte?(byte.Parse(text4)));
			string text5 = GetResponseValue(item3, "DiscountedPrice", null)?.ToString();
			offerProduct.DiscountedPrice = (string.IsNullOrWhiteSpace(text5) ? ((decimal?)null) : new decimal?(decimal.Parse(text5)));
			string text6 = GetResponseValue(item3, "TotalDiscountAmount", null)?.ToString();
			offerProduct.TotalDiscountAmount = (string.IsNullOrWhiteSpace(text6) ? ((decimal?)null) : new decimal?(decimal.Parse(text6)));
			list3.Add(offerProduct);
		}
		resultOffer.OfferProducts = list3;
	}

	private ActionCode ParseActionCode(string code)
	{
		ActionCode result = ActionCode.Rental;
		if (code == ActionCode.Purchase.ToString())
		{
			result = ActionCode.Purchase;
		}
		return result;
	}

	private TitleFamily? ParseTitleFamily(string productTypeString)
	{
		if (string.IsNullOrEmpty(productTypeString) || productTypeString.ToUpper() == "ANY")
		{
			return TitleFamily.All;
		}
		return (TitleFamily)Enum.Parse(typeof(TitleFamily), productTypeString);
	}

	private void PerformGetOffersSimulatedCall(RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				LogHelper.Instance.Log("Simulate call to remote ICustomerOfferService.GetOffers");
				remoteServiceResult.Success = SimulateSuccess;
				remoteServiceResult.Properties["server_name"] = "SimulatedCall";
				remoteServiceResult.Properties["EmailAddress"] = PersonalizationServicesProxySimulator.Instance.LoginEmail;
				List<CustomerOffer> list = new List<CustomerOffer>();
				List<CustomerOffer> list2 = new List<CustomerOffer>();
				remoteServiceResult.Properties["NewOffers"] = list;
				remoteServiceResult.Properties["StoredOffers"] = list2;
				if (IncludeMNPInCartOffer)
				{
					CustomerOffer customerOffer = new CustomerOffer
					{
						OfferId = 1234L,
						DiscountType = OfferDiscountType.MultiNightPrice,
						Amount = 0.5m,
						TitleFamily = TitleFamily.Movies,
						OfferScopeType = OfferScopeType.Session,
						IncludeOlderThan = TimeSpan.FromDays(35.0),
						ExpirationDate = new DateTime(2019, 7, 25),
						CustomerSegmentType = CustomerSegmentType.PersonalizedMultiNightPrice,
						OfferProducts = new List<OfferProduct>(),
						OfferDiscountCode = "ExtraNight50c",
						CampaignId = 123L
					};
					foreach (long mnpProductId in MnpProductIds)
					{
						customerOffer.OfferProducts.Add(new OfferProduct
						{
							ProductId = mnpProductId,
							DiscountedPrice = 0.5m,
							NumberOfNights = (byte)2
						});
					}
					list.Add(customerOffer);
				}
				if (IncludeMovieDiscountInCartOffer)
				{
					list.Add(new CustomerOffer
					{
						OfferId = 2L,
						DiscountType = OfferDiscountType.AmountOff,
						Amount = 1m,
						TitleFamily = TitleFamily.Movies,
						OfferScopeType = OfferScopeType.Session,
						IncludeOlderThan = TimeSpan.FromDays(35.0),
						CustomerSegmentType = CustomerSegmentType.BasketStretch,
						OfferDiscountCode = "MovieDollarOff",
						CampaignId = 123L
					});
				}
				if (IncludePurchaseOffer)
				{
					list.Add(new CustomerOffer
					{
						OfferId = 9L,
						DiscountType = OfferDiscountType.AmountOff,
						Amount = 1m,
						TitleFamily = TitleFamily.Movies,
						OfferScopeType = OfferScopeType.Session,
						IncludeOlderThan = TimeSpan.FromDays(35.0),
						CustomerSegmentType = CustomerSegmentType.BasketStretch,
						OfferDiscountCode = "PrmDollarOff",
						CampaignId = 1234L,
						ActionCode = ActionCode.Purchase
					});
				}
				if (IncludeFreeGameInCartOffer)
				{
					list.Add(new CustomerOffer
					{
						DiscountType = OfferDiscountType.Free,
						Amount = 0m,
						TitleFamily = TitleFamily.Games,
						OfferScopeType = OfferScopeType.Session,
						IncludeOlderThan = TimeSpan.FromDays(35.0),
						ExpirationDate = new DateTime(2019, 7, 25),
						PromotionCode = "DVDONME",
						CustomerSegmentType = CustomerSegmentType.BasketStretch,
						CampaignId = 123L
					});
				}
				if (IncludeFracRedeemableFreeMovieOffer)
				{
					list2.Add(new CustomerOffer
					{
						OfferId = 1L,
						StoredOfferId = 3L,
						DiscountType = OfferDiscountType.Free,
						Amount = 0m,
						TitleFamily = TitleFamily.Movies,
						OfferScopeType = OfferScopeType.StoredRedeemable,
						FormatIds = new List<int> { 1, 2, 19 },
						IncludeFormats = true,
						IncludeOlderThan = TimeSpan.FromDays(35.0),
						ExpirationDate = new DateTime(2019, 7, 25),
						OfferDiscountCode = "FracRedeemFree",
						CustomerSegmentType = CustomerSegmentType.Frac,
						CampaignId = 123L
					});
				}
				if (IncludeFracRedeemableDiscountedDiscOffer)
				{
					list2.Add(new CustomerOffer
					{
						OfferId = 2L,
						StoredOfferId = 4L,
						DiscountType = OfferDiscountType.AmountOff,
						Amount = 1.25m,
						TitleFamily = TitleFamily.All,
						OfferScopeType = OfferScopeType.StoredRedeemable,
						IncludeOlderThan = TimeSpan.FromDays(35.0),
						ExpirationDate = new DateTime(2019, 7, 5),
						CustomerSegmentType = CustomerSegmentType.Frac,
						OfferDiscountCode = "FracRedeemDisc",
						CampaignId = 123L
					});
				}
				if (IncludeFracStorableFreeMovieOffer)
				{
					list.Add(new CustomerOffer
					{
						OfferId = 5L,
						DiscountType = OfferDiscountType.Free,
						Amount = 0m,
						TitleFamily = TitleFamily.Movies,
						OfferScopeType = OfferScopeType.Storeable,
						IncludeOlderThan = TimeSpan.FromDays(35.0),
						ExpirationDate = new DateTime(2019, 7, 25),
						OfferDiscountCode = "FracStorableFree",
						CustomerSegmentType = CustomerSegmentType.Frac,
						CampaignId = 123L
					});
				}
				if (IncludeFracStorableDiscountedDiscOffer)
				{
					list.Add(new CustomerOffer
					{
						OfferId = 6L,
						DiscountType = OfferDiscountType.AmountOff,
						Amount = 1.25m,
						TitleFamily = TitleFamily.All,
						OfferScopeType = OfferScopeType.Storeable,
						IncludeOlderThan = TimeSpan.FromDays(35.0),
						ExpirationDate = new DateTime(2019, 7, 5),
						PromotionCode = null,
						OfferDiscountCode = "FracStorableDicount",
						CustomerSegmentType = CustomerSegmentType.Frac,
						CampaignId = 123L
					});
				}
				if (IncludePerksFreeRentalOffer)
				{
					list.Add(new CustomerOffer
					{
						OfferId = 7L,
						DiscountType = OfferDiscountType.Free,
						Amount = 0m,
						TitleFamily = TitleFamily.All,
						OfferScopeType = OfferScopeType.Session,
						IncludeOlderThan = TimeSpan.FromDays(35.0),
						ExpirationDate = new DateTime(2019, 7, 5),
						PromotionCode = null,
						CustomerSegmentType = CustomerSegmentType.NewMemberPerksSignup,
						OfferDiscountCode = "PerksFreeRental",
						CampaignId = 123L
					});
				}
				if (IncludePerksAmountOffOffer)
				{
					list.Add(new CustomerOffer
					{
						OfferId = 7L,
						DiscountType = OfferDiscountType.AmountOff,
						Amount = 1.00m,
						TitleFamily = TitleFamily.All,
						OfferScopeType = OfferScopeType.Session,
						IncludeOlderThan = TimeSpan.FromDays(35.0),
						ExpirationDate = new DateTime(2019, 11, 5),
						PromotionCode = null,
						CustomerSegmentType = CustomerSegmentType.NewMemberPerksSignup,
						OfferDiscountCode = "PerksAmountOff",
						CampaignId = 123L
					});
				}
				if (IncludePerksExtraPointsOffer)
				{
					list.Add(new CustomerOffer
					{
						OfferId = 7L,
						DiscountType = OfferDiscountType.Loyalty,
						Amount = 1500m,
						TitleFamily = TitleFamily.All,
						OfferScopeType = OfferScopeType.Session,
						IncludeOlderThan = TimeSpan.FromDays(35.0),
						ExpirationDate = new DateTime(2019, 11, 5),
						PromotionCode = null,
						CustomerSegmentType = CustomerSegmentType.NewMemberPerksSignup,
						OfferDiscountCode = "PerksExtraDVDPoints",
						CampaignId = 123L
					});
				}
				if (IncludePerksReminderMessage)
				{
					list.Add(new CustomerOffer
					{
						OfferId = 0L,
						DiscountType = OfferDiscountType.Treatment,
						Amount = 0m,
						TitleFamily = TitleFamily.All,
						OfferScopeType = OfferScopeType.Session,
						IncludeOlderThan = TimeSpan.FromDays(35.0),
						ExpirationDate = new DateTime(2019, 11, 5),
						PromotionCode = null,
						CustomerSegmentType = CustomerSegmentType.NewMemberPerksSignup,
						OfferDiscountCode = "NoOffer",
						CampaignId = 123L
					});
				}
				if (!SimulateSuccess)
				{
					remoteServiceResult.Properties["response"] = ResponseCode;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in PersonalizationServicesProxy.PerformSimulatedLogin.", e);
				remoteServiceResult.Properties["response"] = KioskLoginResponseErrors.Error;
			}
			finally
			{
				remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
				remoteServiceResult.Properties["stand_alone"] = ServiceLocator.Instance.GetService<Redbox.KioskServices.Proxy.ComponentModel.IStoreServices>().IsStandAlone;
				LogHelper.Instance.Log("IRemoteServiceResult: {0}", remoteServiceResult?.ToObfuscatedString());
				ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
				{
					Callback = completeCallback,
					Result = remoteServiceResult,
					Message = "Invoking completeCallback from PersonalizationServicesProxy.PerformSimulatedLogin."
				});
			}
		});
	}

	private static object GetResponseValue(IDictionary<string, object> response, string key, object DefaultValue)
	{
		if (!response.ContainsKey(key) || response[key] == null)
		{
			return DefaultValue;
		}
		return response[key];
	}

	private DateTime GetDateTime(IDictionary<string, object> dateInfo)
	{
		object obj = dateInfo["theDate"];
		DateTime result;
		if (obj.GetType() == typeof(int) && (int)obj != 0)
		{
			result = new DateTime((int)dateInfo["theDate"]);
		}
		else if (obj.GetType() == typeof(long) && (long)obj != 0L)
		{
			result = new DateTime((long)dateInfo["theDate"]);
		}
		else
		{
			if (!((string)dateInfo["dateTimeKind"] != "Unspecified"))
			{
				return Convert.ToDateTime((string)dateInfo["dateString"]);
			}
			result = new DateTime((long)dateInfo["theDate"]);
		}
		return result;
	}

	private TimeSpan? GetTimeSpan(IDictionary<string, object> tsInfo)
	{
		TimeSpan? result = null;
		object obj = tsInfo["theTimeSpan"];
		if (obj.GetType() == typeof(int) && (int)obj != 0)
		{
			result = new TimeSpan((int)tsInfo["theTimeSpan"]);
		}
		else if (obj.GetType() == typeof(long) && (long)obj != 0L)
		{
			result = new TimeSpan((long)tsInfo["theTimeSpan"]);
		}
		else if ((string)tsInfo["dateTimeKind"] != "Unspecified")
		{
			result = new TimeSpan((long)tsInfo["theTimeSpan"]);
		}
		return result;
	}

	private DateTime? GetResponseDateTime(IDictionary<string, object> response, string key)
	{
		if (response.ContainsKey(key) && response[key] != null)
		{
			IDictionary<string, object> dateInfo = response[key] as IDictionary<string, object>;
			return GetDateTime(dateInfo);
		}
		return null;
	}

	private TimeSpan? GetResponseTimeSpan(IDictionary<string, object> response, string key, TimeSpan? dflt)
	{
		if (response.ContainsKey(key) && response[key] != null)
		{
			IDictionary<string, object> tsInfo = response[key] as IDictionary<string, object>;
			TimeSpan? timeSpan = GetTimeSpan(tsInfo);
			if (timeSpan.HasValue)
			{
				return timeSpan;
			}
		}
		return dflt;
	}

	private decimal GetResponseDecimal(IDictionary<string, object> response, string key, decimal dflt)
	{
		if (response.ContainsKey(key) && response[key] != null)
		{
			return Convert.ToDecimal(response[key]);
		}
		return dflt;
	}
}
