using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskClientService.Transactions;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services;

public class AfterSwipeOfferService : IAfterSwipeOfferService
{
	private class ThanksDialogStrings
	{
		public string Message { get; set; }

		public string Comment { get; set; }
	}

	private static AfterSwipeOfferService _instance;

	private Action<bool> _continueAction;

	private List<CustomerOffer> _offerList;

	private ITitleProduct _newProduct;

	private IPromotion _previousPromotion;

	private CustomerOffer _selectedOffer;

	private int m_working;

	public static IAfterSwipeOfferService Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new AfterSwipeOfferService();
			}
			return _instance;
		}
	}

	private AfterSwipeOfferService()
	{
	}

	public void StartOfferFlow(Action<bool> callbackAction)
	{
		LogHelper.Instance.Log("AfterSwipeOfferService: StartOfferFlow called.");
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.MarketingGeneral.ShowAfterSwipePersonalizedOffers)
		{
			OrchestrationService.ViewShow("please_wait_view", null);
			_continueAction = callbackAction;
			IPromotionService service = ServiceLocator.Instance.GetService<IPromotionService>();
			_previousPromotion = service.GetPromotion();
			_newProduct = null;
			Redbox.Rental.Services.Session.Session.Instance.PromoOfferState = new PromoOfferState();
			IStoreManager service2 = ServiceLocator.Instance.GetService<IStoreManager>();
			IRentalShoppingCartService service3 = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
			int num = (int)(service2?.KioskId ?? 0);
			string languageCode = OrchestrationService.GetCulture()?.Substring(0, 2);
			GetOffers(num.ToString(), languageCode, StorefrontEventType.OnSwipe.ToString(), service3.CurrentRentalShoppingCart.ShoppingSessionId.ToString(), ProcessCustomerOffers);
		}
		else
		{
			LogHelper.Instance.Log("AfterSwipeOfferService: ShowAfterSwipePersonalizedOffers config is false, not getting offers.");
			callbackAction(obj: true);
		}
	}

	private void GetOffers(string kioskId, string languageCode, string storefrontEventType, string sessionId, RemoteServiceCallback completeCallback)
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
				if (!ServiceLocator.Instance.GetService<IStoreServices>().IsStandAlone)
				{
					IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>().GetRentalShoppingCart(new Guid(sessionId));
					IPromotion promo = rentalShoppingCart?.Promotion;
					ServiceLocator.Instance.GetService<IMacroService>();
					Dictionary<string, object> creditCardData = ShoppingCartMessageHelper.GetCreditCardData(session, includeTrack2: true);
					MessageShoppingCart instance = ShoppingCartMessageHelper.CreateShoppingCart(rentalShoppingCart, promo, titleItemsOnly: true);
					string utcOffset = ShoppingCartMessageHelper.CalcUtcOffset(DateTime.Now);
					LogHelper.Instance.Log("Live call to remote KioskClientService");
					IGetCustomerOffersResult result = ServiceLocator.Instance.GetService<IKioskClientService>().Transaction.GetCustomerOffersAsync(kioskId, creditCardData, instance.ToJson().ToObject<Dictionary<string, object>>(), utcOffset, languageCode, storefrontEventType).Result;
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
					remoteServiceResult.Properties["stand_alone"] = ServiceLocator.Instance.GetService<IStoreServices>().IsStandAlone;
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

	private void ProcessCustomerOffers(IRemoteServiceResult result)
	{
		if (result.Success)
		{
			try
			{
				List<CustomerOffer> property = result.GetProperty<List<CustomerOffer>>("NewOffers");
				List<CustomerOffer> property2 = result.GetProperty<List<CustomerOffer>>("StoredOffers");
				string property3 = result.GetProperty<string>("EmailAddress");
				if (Redbox.Rental.Services.Session.Session.Instance.UserInfo == null)
				{
					Redbox.Rental.Services.Session.Session.Instance.UserInfo = new UserInfo();
				}
				if (!string.IsNullOrEmpty(property3) && string.IsNullOrEmpty(Redbox.Rental.Services.Session.Session.Instance.UserInfo.Email))
				{
					Redbox.Rental.Services.Session.Session.Instance.UserInfo.Email = property3;
				}
				_offerList = new List<CustomerOffer>();
				if ((property?.Count ?? 0) > 0)
				{
					LogHelper.Instance.Log($"AfterSwipeOfferService: Found {property.Count} new offers");
					AddValidOffersToOfferList(property);
				}
				if ((property2?.Count ?? 0) > 0)
				{
					LogHelper.Instance.Log($"AfterSwipeOfferService: Found {property2.Count} stored offers");
					AddValidOffersToOfferList(property2);
				}
				if (_offerList.Count > 0)
				{
					PresentOffersViewInfo parameter = new PresentOffersViewInfo
					{
						CustomerOffers = _offerList,
						CancelAction = ContinueWithOriginalCart,
						ContinueAction = ContinueWithSelectedOffer,
						TimeoutAction = ContinueWithOriginalCart
					};
					Redbox.Rental.Services.Session.Session.Instance.PromoOfferState.WasOfferPresented = true;
					OrchestrationService.ViewShow("PresentOffersView", parameter);
					return;
				}
				LogHelper.Instance.Log("AfterSwipeOfferService: No applicable titles found for returned offers.");
				if ((property == null || property.Count == 0) && (property2 == null || property2.Count == 0))
				{
					LogHelper.Instance.Log("AfterSwipeOfferService: No Customer offers returned.");
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.LogException("AfterSwipeOfferService: Error processing offers", e);
			}
		}
		else
		{
			LogHelper.Instance.Log("AfterSwipeOfferService: Unable to retrieve customer offers.");
			if (result.Errors != null)
			{
				foreach (Redbox.KioskEngine.ComponentModel.Error error in result.Errors)
				{
					LogHelper.Instance.Log(error.Code + ": " + error.Description + " - " + error.Details);
				}
			}
		}
		_continueAction(obj: true);
	}

	private ITitleProduct GetProductOfferWouldApplyTo(CustomerOffer customerOffer)
	{
		ITitleProduct result = null;
		if (customerOffer.RequiresProductInCart)
		{
			IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
			SetPromoForSelectedOffer(null, service, customerOffer);
			IPromotion promotion = service.CurrentRentalShoppingCart.Promotion;
			if (promotion?.PromotionItems?.Count == 1)
			{
				result = promotion.PromotionItems[0].RentalShoppingCartItem.TitleProduct;
			}
			ServiceLocator.Instance.GetService<IPromotionService>().SetPromotion(_previousPromotion);
		}
		return result;
	}

	private void AddValidOffersToOfferList(List<CustomerOffer> newOffers)
	{
		foreach (CustomerOffer newOffer in newOffers)
		{
			if (newOffer.CustomerSegmentType == CustomerSegmentType.NewMemberPerksSignup)
			{
				IPersonalizationSession personalizationSession = Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession;
				if (personalizationSession != null && personalizationSession.IsLoggedIn)
				{
					LogHelper.Instance.Log($"Not showing perks signup offer id {newOffer.OfferId} because user is already signed in.");
				}
			}
			if (newOffer.CustomerSegmentType == CustomerSegmentType.BasketStretch && newOffer.DiscountType != OfferDiscountType.MultiNightPrice)
			{
				SetApplicableProducts(newOffer);
				if (newOffer.ApplicableProducts.Count > 0)
				{
					_offerList.Add(newOffer);
				}
				else
				{
					LogHelper.Instance.Log($"AfterSwipeOfferService: No applicable products found for offer Id {newOffer.OfferId}");
				}
			}
			else if (newOffer.DiscountType == OfferDiscountType.MultiNightPrice)
			{
				if ((newOffer.OfferProducts?.Count ?? 0) > 0)
				{
					_offerList.Add(newOffer);
				}
				else
				{
					LogHelper.Instance.Log($"AfterSwipeOfferService: No offer products returned for offer Id {newOffer.OfferId}");
				}
			}
			else if (newOffer.RequiresProductInCart)
			{
				newOffer.TargetProductInCart = GetProductOfferWouldApplyTo(newOffer);
				if (newOffer.TargetProductInCart == null)
				{
					LogHelper.Instance.Log($"AfterSwipeOfferService: Skipping offer id {newOffer.OfferId} due to no applicable product in cart.");
				}
				else
				{
					_offerList.Add(newOffer);
				}
			}
			else
			{
				_offerList.Add(newOffer);
			}
		}
	}

	private void ContinueWithSelectedOffer(CustomerOffer offer)
	{
		_selectedOffer = offer;
		Redbox.Rental.Services.Session.Session.Instance.PromoOfferState.SelectedOffer = offer;
		Redbox.Rental.Services.Session.Session.Instance.PromoOfferState.WasOfferAccepted = true;
		if (offer.CustomerSegmentType == CustomerSegmentType.BasketStretch && offer.DiscountType != OfferDiscountType.MultiNightPrice)
		{
			SelectNewProductForOffer(offer);
		}
		else if (offer.CustomerSegmentType == CustomerSegmentType.NewMemberPerksSignup)
		{
			BeginPerksSignup();
		}
		else if (offer.OfferScopeType != OfferScopeType.Storeable && offer.DiscountType != OfferDiscountType.Treatment)
		{
			CreateAndShowNewCart(null);
		}
		else
		{
			ContinueWithOriginalCart();
		}
	}

	private void BeginPerksSignup()
	{
		ServiceLocator.Instance.GetService<IPerksSignupService>().StartSignupFlow(delegate(ISignupFlowResult signupFlowResult)
		{
			ContinueAfterSignup(signupFlowResult);
		}, SignUpFlowContext.AfterPerksEquivalentOffer, delegate(ISignupFlowResult signupFlowResult)
		{
			ContinueAfterSignup(signupFlowResult);
		});
	}

	private void ContinueAfterSignup(ISignupFlowResult signupFlowResult)
	{
		Redbox.Rental.Services.Session.Session.Instance.PromoOfferState.WasPerksSignupCompleted = signupFlowResult.AcceptedPerksSignup;
		if (!signupFlowResult.AcceptedPerksSignup || _selectedOffer.DiscountType == OfferDiscountType.Loyalty || _selectedOffer.DiscountType == OfferDiscountType.Treatment)
		{
			ContinueWithOriginalCart();
		}
		else
		{
			CreateAndShowNewCart(_newProduct);
		}
	}

	private void SelectNewProductForOffer(CustomerOffer customerOffer)
	{
		string offerBrowseTitle = GetOfferBrowseTitle(customerOffer);
		PromoOfferBrowseInfo promoOfferBrowseInfo = new PromoOfferBrowseInfo();
		promoOfferBrowseInfo.applicableProducts = customerOffer.ApplicableProducts;
		promoOfferBrowseInfo.CancelAction = ContinueWithOriginalCart;
		promoOfferBrowseInfo.ContinueAction = CreateAndShowNewCart;
		promoOfferBrowseInfo.TimeoutAction = ContinueWithOriginalCart;
		promoOfferBrowseInfo.OfferTitleText = offerBrowseTitle;
		promoOfferBrowseInfo.DiscountedPriceCalc = CalcPromoPrice;
		promoOfferBrowseInfo.AllowMnp = false;
		promoOfferBrowseInfo.IsPurchase = customerOffer.ActionCode == ActionCode.Purchase;
		OrchestrationService.ViewShow("promo_offer_browse_view", promoOfferBrowseInfo);
	}

	private string GetOfferBrowseTitle(CustomerOffer customerOffer)
	{
		string singularTitleFamilyStringFromOffer = CustomerOfferFunctions.GetSingularTitleFamilyStringFromOffer(customerOffer);
		switch (customerOffer.DiscountType)
		{
		case OfferDiscountType.AmountOff:
			if (customerOffer.ActionCode == ActionCode.Purchase)
			{
				return string.Format(Resources.promo_amount_off_purchase_browse_title_text, singularTitleFamilyStringFromOffer, customerOffer.Amount.ToString("#.00"));
			}
			return string.Format(Resources.promo_amount_off_browse_title_text, singularTitleFamilyStringFromOffer, customerOffer.Amount.ToString("#.00"));
		case OfferDiscountType.PercentOff:
			return string.Format(Resources.promo_percent_off_browse_title_text, singularTitleFamilyStringFromOffer, customerOffer.Amount.ToString("##"));
		case OfferDiscountType.Free:
			return string.Format(Resources.promo_free_rental_offer_browse_title_text, singularTitleFamilyStringFromOffer);
		default:
			return string.Empty;
		}
	}

	private void CreateAndShowNewCart(ITitleProduct newProduct)
	{
		_newProduct = newProduct;
		Redbox.Rental.Services.Session.Session.Instance.PromoOfferState.SelectedProduct = newProduct;
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		service.RemoveMultiDiscDiscsFromRentalShoppingCart(service.CurrentRentalShoppingCart.ShoppingSessionId);
		SetPromoForSelectedOffer(newProduct, service, _selectedOffer);
		NewCartConfirmviewParameters parameter = new NewCartConfirmviewParameters
		{
			NewCart = service.CurrentRentalShoppingCart,
			promoProduct = newProduct,
			CancelAction = ContinueWithOriginalCart,
			ContinueAction = ContinueWithCurrentCart,
			TimeoutAction = ContinueWithOriginalCart,
			RemoveOfferAction = ResetCartOffer,
			SelectedOffer = _selectedOffer
		};
		OrchestrationService.ViewShow("new_cart_confirm_view", parameter);
	}

	private void SetMnpPriceForApplicableItems(CustomerOffer offer, IRentalShoppingCart shoppingCart)
	{
		foreach (IRentalShoppingCartTitleItem titleItem in shoppingCart.TitleItems)
		{
			ServiceLocator.Instance.GetService<IRentalShoppingCartService>().AddMultiNightOfferToItem(titleItem, offer);
		}
	}

	private void SetPromoForSelectedOffer(ITitleProduct newProduct, IRentalShoppingCartService shoppingCartService, CustomerOffer selectedOffer)
	{
		IRentalShoppingCart currentRentalShoppingCart = shoppingCartService.CurrentRentalShoppingCart;
		IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = currentRentalShoppingCart.TitleItems.FirstOrDefault((IRentalShoppingCartTitleItem i) => i.ProductId == newProduct?.ProductId);
		IPromotionService service = ServiceLocator.Instance.GetService<IPromotionService>();
		if (selectedOffer.ShouldAdjustPrice(Redbox.Rental.Services.Configuration.Configuration.Instance) && selectedOffer.ActionCode == ActionCode.Purchase)
		{
			LogHelper.Instance.Log("AfterSwipeOfferService.SetPromoForSelectedOffer: Adjusting price for PRM offer.");
			shoppingCartService.SetPurchasePriceForSelectedItem(selectedOffer, rentalShoppingCartTitleItem, service);
		}
		else if (selectedOffer.DiscountType != OfferDiscountType.MultiNightPrice || Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.MarketingGeneral.UseDiscountsForPmnpOffers)
		{
			LogHelper.Instance.Log("AfterSwipeOfferService.SetPromoForSelectedOffer: Creating promo for the offer.");
			service.SetPromoFromCustomerOffer(currentRentalShoppingCart, selectedOffer, rentalShoppingCartTitleItem);
		}
		if (selectedOffer.DiscountType == OfferDiscountType.MultiNightPrice)
		{
			SetMnpPriceForApplicableItems(selectedOffer, currentRentalShoppingCart);
		}
	}

	private decimal CalcPromoAmount(decimal? itemPrice)
	{
		decimal num = 0m;
		if (itemPrice.HasValue)
		{
			if (_selectedOffer.DiscountType == OfferDiscountType.AmountOff)
			{
				num = _selectedOffer.Amount;
			}
			else if (_selectedOffer.DiscountType == OfferDiscountType.PercentOff)
			{
				num = itemPrice.Value * (_selectedOffer.Amount / 100m);
			}
			else if (_selectedOffer.DiscountType == OfferDiscountType.Free)
			{
				num = itemPrice.Value;
			}
			num = Math.Min(num, itemPrice.Value);
		}
		return num;
	}

	private decimal CalcPromoPrice(decimal itemPrice)
	{
		return itemPrice - CalcPromoAmount(itemPrice);
	}

	private void SetApplicableProducts(CustomerOffer offer)
	{
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		List<ITitleProduct> products = null;
		List<long> include = null;
		List<long> exclude = null;
		if (offer.IncludeTitles)
		{
			include = offer.TitleIds;
		}
		else
		{
			exclude = offer.TitleIds;
		}
		ErrorList errorList = service.GetProductsForCustomerOffer(titleTypes: CustomerOfferFunctions.GetTitleTypesFromFormatIds(offer.FormatIds), titleFamily: offer.TitleFamily.GetValueOrDefault(), includeTitleTypes: offer.IncludeFormats, include: include, exclude: exclude, includeOlderThan: offer.IncludeOlderThan, isAdaView: ApplicationState.Instance.IsADAAccessible, isPurchase: offer.ActionCode == ActionCode.Purchase, products: out products);
		if (errorList.Count > 0)
		{
			errorList.ToLogHelper();
		}
		else if (products == null)
		{
			LogHelper.Instance.Log("Error: RentalDataFunctions.RentalGetProducts return null");
		}
		else
		{
			offer.ApplicableProducts = products;
		}
	}

	private void ContinueWithCurrentCart()
	{
		_offerList = null;
		if (_selectedOffer != null && (_selectedOffer.PromotionCode != null || _selectedOffer.OfferDiscountCode != null))
		{
			_selectedOffer.RedeemedDateTime = DateTime.Now;
		}
		_continueAction(obj: true);
	}

	private void ContinueWithOriginalCart()
	{
		_offerList = null;
		ResetCartOffer();
		_continueAction(obj: true);
	}

	private void ResetCartOffer()
	{
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		if (_newProduct != null)
		{
			service.RemoveItemFromRentalShoppingCartByProductId(service.CurrentRentalShoppingCart.ShoppingSessionId, _newProduct.ProductId, userRemoved: true);
			_newProduct = null;
		}
		service.RemovePromoOfferFromCart(service.CurrentRentalShoppingCart.ShoppingSessionId, force: true);
		Redbox.Rental.Services.Session.Session.Instance.PromoOfferState.WasOfferAccepted = false;
		ServiceLocator.Instance.GetService<IPromotionService>().SetPromotion(_previousPromotion);
	}

	private List<ITitleProduct> GetProductsForOffer()
	{
		return null;
	}
}
