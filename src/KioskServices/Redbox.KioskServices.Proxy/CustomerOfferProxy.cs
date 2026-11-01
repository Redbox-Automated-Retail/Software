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
					IMacroService macroService = ServiceLocator.Instance.GetService<IMacroService>();
					Dictionary<string, object> creditCardData = ProxyMessageHelper.GetCreditCardData(session, includeTrack2: true);
					ShoppingCart shoppingCart = ProxyMessageHelper.CreateShoppingCart(rentalShoppingCart, promo);
					string utcOffset = ProxyMessageHelper.CalcUtcOffset(DateTime.Now);
					if (ServiceLocator.Instance.GetService<IConfiguration>().SendCustomerOffersToKioskClientService)
					{
						LogHelper.Instance.Log("Live call to remote KioskClientService");
						IGetCustomerOffersResult offersResult = ServiceLocator.Instance.GetService<IKioskClientService>().Transaction.GetCustomerOffersAsync(kioskId, creditCardData, shoppingCart.ToJson().ToObject<Dictionary<string, object>>(), utcOffset, languageCode, storefrontEventType).Result;
						remoteServiceResult.Properties["CustomerProfileNumber"] = offersResult.CustomerProfileNumber;
						remoteServiceResult.Properties["AccountNumber"] = offersResult.AccountNumber;
						remoteServiceResult.Properties["EmailAddress"] = offersResult.EmailAddress;
						if (offersResult.StoredOffers != null && offersResult.StoredOffers.Count > 0)
						{
							List<CustomerOffer> storedOffers = new List<CustomerOffer>();
							foreach (CustomerOfferResult storedOffer in offersResult.StoredOffers)
							{
								storedOffers.Add(CustomerOfferFromCustomerOfferResult(storedOffer));
							}
							remoteServiceResult.Properties["StoredOffers"] = storedOffers;
						}
						if (offersResult.NewOffers != null && offersResult.NewOffers.Count > 0)
						{
							List<CustomerOffer> newOffers = new List<CustomerOffer>();
							foreach (CustomerOfferResult newOffer in offersResult.NewOffers)
							{
								newOffers.Add(CustomerOfferFromCustomerOfferResult(newOffer));
							}
							remoteServiceResult.Properties["NewOffers"] = newOffers;
						}
						remoteServiceResult.Success = offersResult.Success;
						if (!remoteServiceResult.Success)
						{
							Redbox.KioskEngine.ComponentModel.Error error = offersResult.Errors.FirstOrDefault();
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
						IDictionary<string, object> offersResponse = KioskServicesProxy.Instance.ExecuteMessage(new
						{
							KioskId = kioskId,
							MessageId = Guid.NewGuid(),
							MessageType = "GetCustomerOffers",
							EngineVersion = macroService["EngineVersion"],
							BundleVersion = macroService["ProductVersion"],
							Lang = languageCode,
							StorefrontEventType = storefrontEventType,
							UtcOffset = utcOffset,
							CreditCard = creditCardData,
							ShoppingCart = shoppingCart
						});
						ServiceLocator.Instance.GetService<IHealthServices>().UpdateOnlineStatus(offersResponse);
						remoteServiceResult.Success = offersResponse.ContainsKey("Success") && (bool)offersResponse["Success"];
						remoteServiceResult.Properties["CustomerProfileNumber"] = GetResponseValue(offersResponse, "CustomerProfileNumber", null)?.ToString();
						remoteServiceResult.Properties["AccountNumber"] = GetResponseValue(offersResponse, "AccountNumber", null)?.ToString();
						remoteServiceResult.Properties["EmailAddress"] = GetResponseValue(offersResponse, "EmailAddress", null)?.ToString();
						if (offersResponse.ContainsKey("StoredOffers") && offersResponse["StoredOffers"] != null)
						{
							ArrayList storedOfferData = (ArrayList)offersResponse["StoredOffers"];
							remoteServiceResult.Properties["StoredOffers"] = new List<CustomerOffer>();
							List<CustomerOffer> parsedStoredOffers = new List<CustomerOffer>();
							foreach (IDictionary<string, object> storedOfferFields in storedOfferData)
							{
								CustomerOffer parsedStoredOffer = new CustomerOffer();
								ParseOffer(storedOfferFields, parsedStoredOffer);
								parsedStoredOffers.Add(parsedStoredOffer);
							}
							remoteServiceResult.Properties["StoredOffers"] = parsedStoredOffers;
						}
						if (offersResponse.ContainsKey("NewOffers") && offersResponse["NewOffers"] != null)
						{
							ArrayList newOfferData = (ArrayList)offersResponse["NewOffers"];
							remoteServiceResult.Properties["NewOffers"] = new List<CustomerOffer>();
							List<CustomerOffer> parsedNewOffers = new List<CustomerOffer>();
							foreach (IDictionary<string, object> newOfferFields in newOfferData)
							{
								CustomerOffer parsedNewOffer = new CustomerOffer();
								ParseOffer(newOfferFields, parsedNewOffer);
								parsedNewOffers.Add(parsedNewOffer);
							}
							remoteServiceResult.Properties["NewOffers"] = parsedNewOffers;
						}
						if (!remoteServiceResult.Success)
						{
							ArrayList errorList = (offersResponse.ContainsKey("Errors") ? (offersResponse["Errors"] as ArrayList) : null);
							if (errorList != null && errorList.Count > 0)
							{
								if (errorList[0] is Dictionary<string, object> firstError)
								{
									object errorResponse = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(KioskLoginResponseErrors?), (string)firstError["Code"]);
									remoteServiceResult.Properties["response"] = errorResponse ?? ((object)KioskLoginResponseErrors.Error);
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
		bool includeFormats = offer.IncludeFormatIds != null && offer.IncludeFormatIds.Count > 0;
		bool includeTitles = offer.IncludeTitleIds != null && offer.IncludeTitleIds.Count > 0;
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
			IncludeFormats = includeFormats,
			FormatIds = (includeFormats ? offer.IncludeFormatIds.ToList() : offer.ExcludeFormatIds.ToList()),
			IncludeTitles = includeTitles,
			TitleIds = (includeTitles ? offer.IncludeTitleIds.ToList() : offer.ExcludeTitleIds.ToList()),
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
		object offerId = GetResponseValue(promo, "OfferId", 0);
		resultOffer.OfferId = long.Parse(offerId.ToString());
		object storedOfferId = GetResponseValue(promo, "StoredOfferId", null);
		if (storedOfferId != null)
		{
			resultOffer.StoredOfferId = long.Parse(storedOfferId.ToString());
		}
		resultOffer.OfferDiscountCode = GetResponseValue(promo, "OfferDiscountCode", null)?.ToString();
		resultOffer.PromotionCode = GetResponseValue(promo, "PromotionCode", null)?.ToString();
		string discountType = GetResponseValue(promo, "DiscountType", null)?.ToString();
		resultOffer.DiscountType = (OfferDiscountType)Enum.Parse(typeof(OfferDiscountType), discountType);
		string offerScopeType = GetResponseValue(promo, "OfferScopeType", null)?.ToString();
		if (offerScopeType != null)
		{
			resultOffer.OfferScopeType = (OfferScopeType)Enum.Parse(typeof(OfferScopeType), offerScopeType);
		}
		string customerSegmentType = GetResponseValue(promo, "CustomerSegmentType", null)?.ToString();
		if (customerSegmentType != null)
		{
			resultOffer.CustomerSegmentType = (CustomerSegmentType)Enum.Parse(typeof(CustomerSegmentType), customerSegmentType);
		}
		string campaignId = GetResponseValue(promo, "CampaignId", null)?.ToString();
		if (campaignId != null)
		{
			resultOffer.CampaignId = long.Parse(campaignId.ToString());
		}
		resultOffer.TitleFamily = ParseTitleFamily(GetResponseValue(promo, "ProductType", null)?.ToString());
		resultOffer.ExpirationDate = (DateTime)GetResponseValue(promo, "ExpirationDate", DateTime.Today);
		resultOffer.IncludeOlderThan = GetResponseTimeSpan(promo, "ExcludeIfWithinRedboxReleaseDate", null);
		resultOffer.Amount = GetResponseDecimal(promo, "DiscountAmount", 0m);
		resultOffer.ActionCode = ParseActionCode(GetResponseValue(promo, "ActionCode", null)?.ToString());
		resultOffer.IncludeFormats = promo.ContainsKey("IncludeFormats") && ((ArrayList)promo["IncludeFormats"]).Count > 0;
		string formatIdsKey = (resultOffer.IncludeFormats ? "IncludeFormats" : "ExcludeFormats");
		ArrayList formatIdData = (promo.ContainsKey(formatIdsKey) ? (promo[formatIdsKey] as ArrayList) : null);
		if (formatIdData != null)
		{
			List<int> formatIds = new List<int>();
			foreach (string formatId in formatIdData)
			{
				if (int.TryParse(formatId, out var parsedFormatId))
				{
					formatIds.Add(parsedFormatId);
				}
			}
			resultOffer.FormatIds = formatIds;
		}
		resultOffer.IncludeTitles = promo.ContainsKey("IncludeTitleIds") && promo["IncludeTitleIds"] is ArrayList && ((ArrayList)promo["IncludeTitleIds"]).Count > 0;
		string titleIdsKey = (resultOffer.IncludeTitles ? "IncludeTitleIds" : "ExcludeTitleIds");
		ArrayList titleIdData = (promo.ContainsKey(titleIdsKey) ? (promo[titleIdsKey] as ArrayList) : null);
		if (titleIdData != null)
		{
			List<long> titleIds = new List<long>();
			foreach (object titleId in titleIdData)
			{
				if (titleId is int)
				{
					titleIds.Add((int)titleId);
				}
				else if (titleId is long)
				{
					titleIds.Add((long)titleId);
				}
			}
			resultOffer.TitleIds = titleIds;
		}
		ArrayList offerProductData = (promo.ContainsKey("OfferProducts") ? (promo["OfferProducts"] as ArrayList) : null);
		if (offerProductData == null)
		{
			return;
		}
		List<OfferProduct> offerProducts = new List<OfferProduct>();
		foreach (IDictionary<string, object> offerProductFields in offerProductData)
		{
			OfferProduct offerProduct = new OfferProduct();
			string productId = GetResponseValue(offerProductFields, "ProductId", "-1").ToString();
			offerProduct.ProductId = long.Parse(productId);
			string numberOfNights = GetResponseValue(offerProductFields, "NumberOfNights", null)?.ToString();
			offerProduct.NumberOfNights = (string.IsNullOrWhiteSpace(numberOfNights) ? ((byte?)null) : new byte?(byte.Parse(numberOfNights)));
			string discountedPrice = GetResponseValue(offerProductFields, "DiscountedPrice", null)?.ToString();
			offerProduct.DiscountedPrice = (string.IsNullOrWhiteSpace(discountedPrice) ? ((decimal?)null) : new decimal?(decimal.Parse(discountedPrice)));
			string totalDiscountAmount = GetResponseValue(offerProductFields, "TotalDiscountAmount", null)?.ToString();
			offerProduct.TotalDiscountAmount = (string.IsNullOrWhiteSpace(totalDiscountAmount) ? ((decimal?)null) : new decimal?(decimal.Parse(totalDiscountAmount)));
			offerProducts.Add(offerProduct);
		}
		resultOffer.OfferProducts = offerProducts;
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
				List<CustomerOffer> newOffers = new List<CustomerOffer>();
				List<CustomerOffer> storedOffers = new List<CustomerOffer>();
				remoteServiceResult.Properties["NewOffers"] = newOffers;
				remoteServiceResult.Properties["StoredOffers"] = storedOffers;
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
					newOffers.Add(customerOffer);
				}
				if (IncludeMovieDiscountInCartOffer)
				{
					newOffers.Add(new CustomerOffer
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
					newOffers.Add(new CustomerOffer
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
					newOffers.Add(new CustomerOffer
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
					storedOffers.Add(new CustomerOffer
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
					storedOffers.Add(new CustomerOffer
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
					newOffers.Add(new CustomerOffer
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
					newOffers.Add(new CustomerOffer
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
					newOffers.Add(new CustomerOffer
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
					newOffers.Add(new CustomerOffer
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
					newOffers.Add(new CustomerOffer
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
					newOffers.Add(new CustomerOffer
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
