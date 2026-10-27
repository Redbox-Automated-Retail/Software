using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Loyalty;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Promotion;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.ShoppingCart;

public class RentalShoppingCart : IRentalShoppingCart
{
	private delegate IPromoCodeValidationInfo PromotionValidationFunction(IPromotion promotion, PromotionFunctionFilters promotionFunctionFilters);

	private class PromotionFunctionFilters
	{
		public bool IncludeMultiNightPricingItems { get; set; }

		public bool IncludeFlashDeals { get; set; }

		public bool IncludeDiscountRestrictionItems { get; set; }

		public RentalShoppingCartItemAction Action { get; set; }

		public PromotionFunctionFilters(IPromotion promotion)
		{
			int action;
			if (promotion != null)
			{
				_ = promotion.ActionCode;
				action = (int)promotion.CartItemAction;
			}
			else
			{
				action = 0;
			}
			Action = (RentalShoppingCartItemAction)action;
			IncludeFlashDeals = true;
			IncludeDiscountRestrictionItems = promotion?.AllowFullDiscount ?? true;
		}
	}

	private bool? _serviceFeeEnabled;

	private bool _inCartTitleMarketing;

	private IUpsellInfo _upsellInfo;

	private IComingSoonOptinInfo _comingSoonOptinInfo;

	private readonly object _syncObject = new object();

	private IList<IBaseRentalShoppingCartItem> _items = new List<IBaseRentalShoppingCartItem>();

	private ITotals _totals;

	private IServiceFee _serviceFee;

	private decimal _defaultServiceFee;

	private decimal _defaultServiceFeeTaxRate;

	private bool _isAfterVend;

	public int MaximumCapacity => 5;

	public Guid ShoppingSessionId { get; private set; }

	public IList<IBaseRentalShoppingCartItem> Items
	{
		get
		{
			lock (_syncObject)
			{
				List<IBaseRentalShoppingCartItem> list = new List<IBaseRentalShoppingCartItem>();
				list.AddRange(_items);
				return list;
			}
		}
	}

	public IList<IRentalShoppingCartTitleItem> TitleItems
	{
		get
		{
			lock (_syncObject)
			{
				List<IRentalShoppingCartTitleItem> list = new List<IRentalShoppingCartTitleItem>();
				list.AddRange(from x in _items
					where x is IRentalShoppingCartTitleItem
					select x as IRentalShoppingCartTitleItem);
				return list;
			}
		}
	}

	public IList<IRentalShoppingCartSubscriptionItem> SubscriptionItems
	{
		get
		{
			lock (_syncObject)
			{
				List<IRentalShoppingCartSubscriptionItem> list = new List<IRentalShoppingCartSubscriptionItem>();
				list.AddRange(from x in _items
					where x is IRentalShoppingCartSubscriptionItem
					select x as IRentalShoppingCartSubscriptionItem);
				return list;
			}
		}
	}

	public int Count
	{
		get
		{
			lock (_syncObject)
			{
				return _items.Count;
			}
		}
	}

	public bool IsFull
	{
		get
		{
			lock (_syncObject)
			{
				return _items.Count >= MaximumCapacity;
			}
		}
	}

	public IUpsellInfo UpsellInfo
	{
		get
		{
			if (_upsellInfo == null)
			{
				_upsellInfo = new UpsellInfo(this);
			}
			return _upsellInfo;
		}
	}

	public IComingSoonOptinInfo ComingSoonOptInInfo
	{
		get
		{
			if (_comingSoonOptinInfo == null)
			{
				_comingSoonOptinInfo = new ComingSoonOptinInfo();
			}
			return _comingSoonOptinInfo;
		}
	}

	public IPromotion Promotion => PromotionService.Instance.GetPromotion();

	public decimal DefaultServiceFee => _defaultServiceFee;

	public ITotals Totals
	{
		get
		{
			if (_totals == null)
			{
				_totals = new Totals(this);
			}
			return _totals;
		}
	}

	public OptInConfirmationStatus MarketingOptInConfirmationStatus { get; set; }

	public IList<IRentalShoppingCartTitleItem> RedeemableItems => TitleItems.Where(delegate(IRentalShoppingCartTitleItem x)
	{
		ILoyaltyPointsRecord loyaltyPointsRecord = x.LoyaltyPointsRecord;
		return loyaltyPointsRecord != null && loyaltyPointsRecord.CanRedeem && !x.LoyaltyPointsRecord.ToBeRedeemed;
	}).ToList();

	public IList<IRentalShoppingCartTitleItem> ToBeRedeemedItems => TitleItems.Where(delegate(IRentalShoppingCartTitleItem x)
	{
		ILoyaltyPointsRecord loyaltyPointsRecord = x.LoyaltyPointsRecord;
		return loyaltyPointsRecord != null && loyaltyPointsRecord.CanRedeem && x.LoyaltyPointsRecord.ToBeRedeemed;
	}).ToList();

	public int TotalPointsToBeEarned => TitleItems.Where(delegate(IRentalShoppingCartTitleItem x)
	{
		ILoyaltyPointsRecord loyaltyPointsRecord = x.LoyaltyPointsRecord;
		return loyaltyPointsRecord != null && loyaltyPointsRecord.PointsToBeEarned.HasValue && !x.LoyaltyPointsRecord.ToBeRedeemed;
	}).Sum((IRentalShoppingCartTitleItem x) => x.LoyaltyPointsRecord.PointsToBeEarned.Value);

	public bool HasItemsToBeRedeemed => TitleItems.Any((IRentalShoppingCartTitleItem x) => x.LoyaltyPointsRecord?.ToBeRedeemed ?? false);

	public bool HasRedboxPlusFreeOneNightRentalSelected => TitleItems.Any((IRentalShoppingCartTitleItem x) => x?.IsRedboxPlusFreeOneNightRental ?? false);

	public IServiceFee ServiceFee
	{
		get
		{
			if (_serviceFee == null)
			{
				_serviceFee = new ServiceFee();
			}
			return _serviceFee;
		}
		set
		{
			_serviceFee = value;
		}
	}

	public bool ServiceFeeEnabled
	{
		get
		{
			if (!_serviceFeeEnabled.HasValue)
			{
				Store store = ServiceLocator.Instance.GetService<IDataService>()?.GetStore();
				_serviceFeeEnabled = Redbox.Rental.Services.Configuration.Configuration.Instance.ServiceFeeEnabled && store != null && store.ServiceFee > 0m;
			}
			return _serviceFeeEnabled.Value;
		}
	}

	private IShoppingSession ShoppingSession => ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(ShoppingSessionId.ToString());

	public bool InCartPromoOfferMade { get; set; }

	public bool InCartPromoOfferCancel { get; set; }

	public bool InCartPromoOfferAccepted
	{
		get
		{
			if (InCartPromoOfferMade)
			{
				return !InCartPromoOfferCancel;
			}
			return false;
		}
	}

	public bool InCartDoNotShow { get; set; }

	public bool InCartTitleMarketing
	{
		get
		{
			return _inCartTitleMarketing;
		}
		set
		{
			_inCartTitleMarketing = value;
		}
	}

	public bool PickupRecommendationsShown { get; set; }

	public bool PickupRecommendationsSelected
	{
		get
		{
			if (PickupRecommendationsShown)
			{
				return TitleItems.Any((IRentalShoppingCartTitleItem i) => i.SourceType == ItemSourceType.Recommendation);
			}
			return false;
		}
	}

	public event RentalShoppingCartChanged OnChange;

	public RentalShoppingCart(Guid shoppingSessionId, decimal defaultServiceFee, decimal defaultServiceFeeTaxRate)
	{
		ShoppingSessionId = shoppingSessionId;
		_defaultServiceFee = defaultServiceFee;
		_defaultServiceFeeTaxRate = defaultServiceFeeTaxRate;
	}

	public void SetDefaultServiceFee(decimal defaultServiceFee)
	{
		_defaultServiceFee = defaultServiceFee;
		UpdateServiceFee();
	}

	public void TriggerChangeNotification()
	{
		OnChange?.Invoke(this);
	}

	public bool AddItem(IBaseRentalShoppingCartItem rentalShoppingCartItem)
	{
		bool result = false;
		if (rentalShoppingCartItem is IRentalShoppingCartTitleItem)
		{
			result = AddTitleItem(rentalShoppingCartItem as IRentalShoppingCartTitleItem, null);
		}
		else if (rentalShoppingCartItem is IRentalShoppingCartSubscriptionItem)
		{
			result = AddSubscriptionItem(rentalShoppingCartItem as IRentalShoppingCartSubscriptionItem);
		}
		else
		{
			LogHelper.Instance.Log($"Unable to add item of type {rentalShoppingCartItem?.GetType()} to shopping cart");
		}
		return result;
	}

	public bool AddTitleItem(IRentalShoppingCartTitleItem rentalShoppingCartTitleItem, IPricingRecord pricingRecord)
	{
		bool result = false;
		lock (_syncObject)
		{
			if (rentalShoppingCartTitleItem != null)
			{
				if (!HasTitleItem(rentalShoppingCartTitleItem.ProductId))
				{
					SetRentalShoppingCartItemPricing(rentalShoppingCartTitleItem, pricingRecord);
					result = BaseAddItem(rentalShoppingCartTitleItem);
					LogHelper.Instance.Log("Adding to Rental Shopping Cart: {0} Item: {1}", ShoppingSessionId, rentalShoppingCartTitleItem);
					Totals.UpdateSubTotalGroups();
					ValidateAndUpdatePromoCode();
					UpdateServiceFee();
				}
				else
				{
					LogHelper.Instance.Log($"Unable to add item with product title id {rentalShoppingCartTitleItem.ProductId}.  Product title id is already in the shopping cart.");
				}
			}
		}
		return result;
	}

	public bool AddSubscriptionItem(IRentalShoppingCartSubscriptionItem rentalShoppingCartSubscriptionItem)
	{
		bool result = false;
		lock (_syncObject)
		{
			if (rentalShoppingCartSubscriptionItem?.SubscriptionProduct != null)
			{
				if (!SubscriptionItems.Any((IRentalShoppingCartSubscriptionItem x) => x.SubscriptionProduct.SubscriptionType == rentalShoppingCartSubscriptionItem.SubscriptionProduct.SubscriptionType))
				{
					result = BaseAddItem(rentalShoppingCartSubscriptionItem);
				}
				else
				{
					LogHelper.Instance.Log($"Unable to add item with subscription type {rentalShoppingCartSubscriptionItem?.SubscriptionProduct?.SubscriptionType}.  Subscription type is already in the shopping cart.");
				}
			}
		}
		return result;
	}

	private bool BaseAddItem(IBaseRentalShoppingCartItem baseRentalShoppingCartItem)
	{
		bool result = false;
		lock (_syncObject)
		{
			if (baseRentalShoppingCartItem != null)
			{
				baseRentalShoppingCartItem.Sequence = (InCartPromoOfferMade ? RentalShoppingCartItemSequence.post_offer : RentalShoppingCartItemSequence.pre_offer);
				_items.Add(baseRentalShoppingCartItem);
				Totals.UpdateSubTotalGroups();
				ValidateAndUpdatePromoCode();
				UpdateServiceFee();
				result = true;
			}
		}
		if (OnChange != null)
		{
			OnChange(this);
		}
		return result;
	}

	public void RemoveItem(IBaseRentalShoppingCartItem rentalShoppingCartItem, bool userRemoved)
	{
		bool flag = false;
		lock (_syncObject)
		{
			if (rentalShoppingCartItem != null && _items.Contains(rentalShoppingCartItem))
			{
				_items.Remove(rentalShoppingCartItem);
				flag = true;
				Totals.UpdateSubTotalGroups();
				if (rentalShoppingCartItem is IRentalShoppingCartTitleItem)
				{
					IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = rentalShoppingCartItem as IRentalShoppingCartTitleItem;
					LogHelper.Instance.Log("User removed shopping cart title item {0} {1}", rentalShoppingCartTitleItem?.SortName, rentalShoppingCartTitleItem?.ProductId);
				}
				else if (rentalShoppingCartItem is IRentalShoppingCartSubscriptionItem)
				{
					IRentalShoppingCartSubscriptionItem rentalShoppingCartSubscriptionItem = rentalShoppingCartItem as IRentalShoppingCartSubscriptionItem;
					LogHelper.Instance.Log("User removed shopping cart subscription item. Type= {0} {1}", rentalShoppingCartSubscriptionItem?.SubscriptionProduct?.SubscriptionType, rentalShoppingCartSubscriptionItem?.SubscriptionProduct?.SubscriptionId);
				}
				else
				{
					LogHelper.Instance.Log($"Removed shopping cart item of unknown type. {rentalShoppingCartItem}");
				}
				Redbox.Rental.Services.Session.Session.Instance.BrowseState.ShoppingCartFullMessagesShown = false;
				ValidateAndUpdatePromoCode();
				UpdateServiceFee();
				ClearLoyaltyPointsToBeEarned();
			}
		}
		if (flag && OnChange != null)
		{
			OnChange(this);
		}
	}

	public void Clear()
	{
		bool flag = false;
		lock (_syncObject)
		{
			if (_items.Count > 0)
			{
				flag = true;
			}
			_items.Clear();
			Totals.UpdateSubTotalGroups();
		}
		if (flag && OnChange != null)
		{
			OnChange(this);
		}
	}

	public IBaseRentalShoppingCartItem GetItem(int index)
	{
		IBaseRentalShoppingCartItem result = null;
		lock (_syncObject)
		{
			if (index < _items.Count)
			{
				result = _items[index];
			}
		}
		return result;
	}

	public bool HasTitleItem(long productId)
	{
		lock (_syncObject)
		{
			return TitleItems.Any((IRentalShoppingCartTitleItem x) => x.ProductId == productId);
		}
	}

	public bool HasAnyTitleRollupTitle(ITitleRollupProduct titleRollupProduct)
	{
		lock (_syncObject)
		{
			bool result = false;
			if (titleRollupProduct?.KioskProducts != null)
			{
				foreach (ITitleProduct kioskProduct in titleRollupProduct.KioskProducts)
				{
					if (HasTitleItem(kioskProduct.ProductId))
					{
						result = true;
					}
				}
			}
			return result;
		}
	}

	public bool HasSubscriptionItem(ISubscriptionProduct subscriptionProduct)
	{
		lock (_syncObject)
		{
			return subscriptionProduct != null && SubscriptionItems.Any((IRentalShoppingCartSubscriptionItem x) => x.SubscriptionProduct.SubscriptionType == subscriptionProduct.SubscriptionType && x.SubscriptionProduct.SubscriptionId == subscriptionProduct.SubscriptionId);
		}
	}

	public bool HasSubscriptionItem(SubscriptionType subscriptionType)
	{
		lock (_syncObject)
		{
			return SubscriptionItems.Any((IRentalShoppingCartSubscriptionItem x) => x?.SubscriptionProduct != null && x.SubscriptionProduct.SubscriptionType == subscriptionType);
		}
	}

	public bool HasItemAction(RentalShoppingCartItemAction action)
	{
		lock (_syncObject)
		{
			return TitleItems.Any((IRentalShoppingCartTitleItem x) => x.Action == action);
		}
	}

	public bool HasItemWithAction(long productId, RentalShoppingCartItemAction action)
	{
		lock (_syncObject)
		{
			return TitleItems.Any((IRentalShoppingCartTitleItem x) => x.ProductId == productId && x.Action == action);
		}
	}

	public bool HasMultiDiscItem()
	{
		lock (_syncObject)
		{
			return TitleItems.Any((IRentalShoppingCartTitleItem x) => x.MultiDisc);
		}
	}

	public void ClearPromoCode()
	{
		LogHelper.Instance.Log("> Clear Promo Code was called");
		PromotionService.Instance?.ClearPromotion();
	}

	public void UpdateServiceFee()
	{
		decimal obj = ServiceFee?.ActualAmount ?? 0m;
		decimal num = TitleItems.Where((IRentalShoppingCartTitleItem x) => x?.TitleProduct?.IsEmptyCase == true).Sum((IRentalShoppingCartTitleItem x) => x.PurchasePrice);
		TitleItems.Count(delegate(IRentalShoppingCartTitleItem x)
		{
			bool? obj2;
			if (x == null)
			{
				obj2 = null;
			}
			else
			{
				ITitleProduct titleProduct = x.TitleProduct;
				obj2 = ((titleProduct != null) ? new bool?(!titleProduct.IsEmptyCase) : ((bool?)null));
			}
			bool? flag = obj2;
			return flag == true;
		});
		ServiceFee.DefaultAmount = _defaultServiceFee;
		ServiceFee.TaxRate = _defaultServiceFeeTaxRate;
		if (Totals.SubTotal - num > 0m)
		{
			ServiceFee.ActualAmount = _defaultServiceFee;
		}
		else
		{
			ServiceFee.ActualAmount = 0m;
		}
		if (obj != (ServiceFee?.ActualAmount ?? 0m))
		{
			LogHelper.Instance.Log($"Setting service fee: {ServiceFee?.ActualAmount ?? 0m}");
		}
	}

	public IPromoCodeValidationInfo ValidateAndUpdatePromoCode()
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = new PromoCodeValidationInfo();
		try
		{
			if (PromotionService.Instance.IsFreeInCartOfferRunning)
			{
				promoCodeValidationInfo = PromotionService.Instance.ValidateFreeInCartOffer();
				if (!promoCodeValidationInfo.IsCartValid && InCartPromoOfferMade)
				{
					ClearPromoCode();
					InCartPromoOfferMade = false;
				}
			}
			else if (PromotionService.Instance.IsRentTwoInCartOfferRunning)
			{
				if ((PromotionService.Instance?.ValidateRentTwoInCartPromo()?.QualifyingItems?.Count()).GetValueOrDefault() < 2 && InCartPromoOfferMade)
				{
					ClearPromoCode();
					InCartPromoOfferMade = false;
					InCartPromoOfferCancel = false;
				}
			}
			else if (InCartPromoOfferAccepted)
			{
				promoCodeValidationInfo = PromotionService.Instance.ValidateRegularInCartPromo(this);
				if (promoCodeValidationInfo.IsCartValid && Promotion == null)
				{
					if (PromotionService.Instance.IsFirstNightFreeItemInCartOfferRunning)
					{
						PromotionService.Instance.SetPromoToFirstNightFreeItemInCart(this);
					}
					else
					{
						PromotionService.Instance.SetPromoToFixedPriceInCart(this, promoCodeValidationInfo.Price);
					}
				}
			}
			IPromotion promotion = Promotion;
			if (promotion != null && promotion.Intent == PromotionIntentCode.FirstNightGameGameRequired && !PromotionService.Instance.IsFreeInCartOfferRunning)
			{
				promoCodeValidationInfo = ValidatePromoConsideringConditions(ValidateFirstNightGame, Promotion);
				if (!promoCodeValidationInfo.IsCartValid)
				{
					ClearPromoCode();
				}
			}
			IPromotion promotion2 = Promotion;
			if (promotion2 != null && promotion2.IsRedboxPlusFreeOneNightRental && !HasRedboxPlusFreeOneNightRentalSelected)
			{
				ClearPromoCode();
			}
			if (Promotion != null)
			{
				if (!InCartPromoOfferAccepted)
				{
					if (Promotion.IsRedboxPlusFreeOneNightRental)
					{
						promoCodeValidationInfo = ValidatePromoConsideringConditions(ValidateRedboxPlusFreeOneNightRental, Promotion);
					}
					else if (Promotion.Intent == PromotionIntentCode.RxGx)
					{
						promoCodeValidationInfo = ValidatePromoConsideringConditions(ValidateRxGxPromo, Promotion);
					}
					else if (Promotion.Intent == PromotionIntentCode.PercentOffCartTotal)
					{
						promoCodeValidationInfo = ValidatePromoConsideringConditions(ValidatePercentOffPromo, Promotion);
					}
					else if (Promotion.Intent == PromotionIntentCode.FirstNightDVD)
					{
						promoCodeValidationInfo = ValidatePromoConsideringConditions(ValidateFirstNightDvd, Promotion);
					}
					else if (Promotion.Intent == PromotionIntentCode.FirstNightBluray)
					{
						promoCodeValidationInfo = ValidatePromoConsideringConditions(ValidateFirstNightBluRay, Promotion);
					}
					else if (Promotion.Intent == PromotionIntentCode.FirstNightGame || Promotion.Intent == PromotionIntentCode.FirstNightGameGameRequired)
					{
						promoCodeValidationInfo = ValidatePromoConsideringConditions(ValidateFirstNightGame, Promotion);
					}
					else if (Promotion.Intent == PromotionIntentCode.FirstNightFreeItem)
					{
						promoCodeValidationInfo = ValidatePromoConsideringConditions(ValidateFirstNightFree, Promotion);
					}
					else
					{
						if (Promotion.Intent == PromotionIntentCode.FixedValue)
						{
							ICampaignTitles campaignTitles = Promotion.CampaignTitles;
							if (campaignTitles != null && campaignTitles.Titles?.Count > 0)
							{
								promoCodeValidationInfo = ValidatePromoConsideringConditions(ValidateTitleSpecificPromo, Promotion);
								goto IL_0390;
							}
						}
						if (Promotion.Intent == PromotionIntentCode.FixedValue)
						{
							promoCodeValidationInfo = ValidatePromoConsideringConditions(ValidateFixedValuePromo, Promotion);
						}
						else if (Promotion.IsExternallyValidated)
						{
							promoCodeValidationInfo = ValidatePromoConsideringConditions(SetResultForExternallyValidatedPromo, Promotion);
						}
					}
				}
				goto IL_0390;
			}
			goto end_IL_0006;
			IL_0390:
			if (!Promotion.IsExternallyValidated)
			{
				Promotion.QualifyingItems.Clear();
				Promotion.PromotionItems.Clear();
				Promotion.Price = promoCodeValidationInfo.Price;
				Promotion.Percent = promoCodeValidationInfo.Percent;
				if (promoCodeValidationInfo.IsCartValid)
				{
					promoCodeValidationInfo.QualifyingItems.ForEach(delegate(IRentalShoppingCartTitleItem x)
					{
						Promotion.QualifyingItems.Add(x);
					});
					promoCodeValidationInfo.PromotionItems.ForEach(delegate(IPromotionItem x)
					{
						Promotion.PromotionItems.Add(x);
					});
				}
			}
			LogHelper.Instance.Log(string.Format("Validate Promo {0}.  IsValid:{1}{2}{3}{4}  Total Promo discounts: {5}", Promotion?.Intent, promoCodeValidationInfo.IsCartValid, promoCodeValidationInfo.IsCartInvalidBecauseOfFlashDeal ? " InvalidBecauseOfFlashDeal" : null, promoCodeValidationInfo.IsCartInvalidBecauseOfMultiNightPricing ? " InvalidBecauseOfMultiNightPricing" : null, promoCodeValidationInfo.IsCartInvalidBecauseOfDiscountRestriction ? " InvalidBecauseOfDiscountRestriction" : null, promoCodeValidationInfo.Price));
			LogHelper.Instance.Log("Promo Qualifying items: {0}", promoCodeValidationInfo.QualifyingItems.Select((IRentalShoppingCartTitleItem x) => x.ProductId).ToList().Join(", "));
			LogHelper.Instance.Log("Promo Redemption items: {0}", promoCodeValidationInfo.PromotionItems.Select((IPromotionItem x) => $"{x.RentalShoppingCartItem?.ProductId} {x.PromotionAmount:C2}").ToList().Join(", "));
			LogHelper.Instance.Log("Loyalty Redemption items: {0}", (from x in TitleItems
				where x?.LoyaltyPointsRecord?.ToBeRedeemed == true
				select $"{x.ProductId} {(x?.PricingRecord?.InitialNight).GetValueOrDefault():C2}").ToList().Join(", "));
			LogHelper.Instance.Log("Total Loyalty discounts {0}", TitleItems.Where((IRentalShoppingCartTitleItem x) => x?.LoyaltyPointsRecord?.ToBeRedeemed == true).Sum((IRentalShoppingCartTitleItem x) => (x?.PricingRecord?.InitialNight).GetValueOrDefault()));
			LogHelper.Instance.Log("Total discounts {0}", Totals.DiscountAmountUsed);
			end_IL_0006:;
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException($"Exception validating {Promotion?.Intent} promo", e);
			promoCodeValidationInfo.IsCartValid = false;
		}
		return promoCodeValidationInfo;
	}

	public void ReValidatePromoCodeAfterVend()
	{
		_isAfterVend = true;
		CustomerOffer customerOffer = Redbox.Rental.Services.Session.Session.Instance?.PromoOfferState?.SelectedOffer;
		bool valueOrDefault = Redbox.Rental.Services.Session.Session.Instance?.PromoOfferState?.WasOfferAccepted == true;
		if (Promotion != null)
		{
			Promotion.IsValidatedPostVend = true;
			if (Promotion.IsProductSpecific)
			{
				ValidateAndUpdatePromoCode();
			}
			if ((customerOffer != null) & valueOrDefault)
			{
				if ((Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.MarketingGeneral.UseDiscountsForPmnpOffers && customerOffer.DiscountType == OfferDiscountType.MultiNightPrice) || (customerOffer.OfferScopeType == OfferScopeType.StoredRedeemable && Promotion.IsProductSpecific))
				{
					RemoveNonVendedPromoItems();
					if (Promotion.Price == 0m)
					{
						Redbox.Rental.Services.Session.Session.Instance.PromoOfferState.SelectedOffer = null;
						Redbox.Rental.Services.Session.Session.Instance.PromoOfferState.WasOfferAccepted = false;
					}
				}
				else if (TitleItems.All((IRentalShoppingCartTitleItem i) => i.Status != LineItemStatus.Fullfilled) && customerOffer.DiscountType != OfferDiscountType.Loyalty)
				{
					Redbox.Rental.Services.Session.Session.Instance.PromoOfferState.SelectedOffer = null;
					customerOffer = null;
					Redbox.Rental.Services.Session.Session.Instance.PromoOfferState.WasOfferAccepted = false;
				}
			}
		}
		if (valueOrDefault && !string.IsNullOrEmpty(customerOffer?.OfferDiscountCode))
		{
			RecordOfferCodesForDiscsUsingOffer(customerOffer);
		}
	}

	private void RecordOfferCodesForDiscsUsingOffer(CustomerOffer acceptedOffer)
	{
		List<IDiscOfferCode> list = new List<IDiscOfferCode>();
		if (Promotion != null)
		{
			if ((Promotion.PromotionItems?.Count ?? 0) > 0)
			{
				foreach (IPromotionItem promotionItem in Promotion.PromotionItems)
				{
					list.Add(new DiscOfferCode
					{
						Barcode = promotionItem.RentalShoppingCartItem.Barcode,
						OfferCode = acceptedOffer.OfferDiscountCode
					});
				}
			}
		}
		else
		{
			foreach (IRentalShoppingCartTitleItem item in TitleItems)
			{
				if (item.Status != LineItemStatus.Fullfilled)
				{
					continue;
				}
				if (acceptedOffer.DiscountType == OfferDiscountType.MultiNightPrice)
				{
					OfferProduct offerProduct = acceptedOffer.OfferProducts.FirstOrDefault((OfferProduct o) => o.ProductId == item.ProductId);
					if (offerProduct != null && item.PricingRecord.InitialDays == offerProduct.NumberOfNights)
					{
						list.Add(new DiscOfferCode
						{
							Barcode = item.Barcode,
							OfferCode = acceptedOffer.OfferDiscountCode
						});
					}
				}
				else if (item.Action == RentalShoppingCartItemAction.purchase && item.Price != item.PurchasePrice)
				{
					list.Add(new DiscOfferCode
					{
						Barcode = item.Barcode,
						OfferCode = acceptedOffer.OfferDiscountCode
					});
				}
			}
		}
		Redbox.Rental.Services.Session.Session.Instance.PromoOfferState.DiscOfferCodes = list;
	}

	private void RemoveNonVendedPromoItems()
	{
		List<IPromotionItem> list = new List<IPromotionItem>();
		foreach (IPromotionItem promoItem in Promotion.PromotionItems)
		{
			IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = TitleItems.FirstOrDefault((IRentalShoppingCartTitleItem i) => i.ProductId == promoItem.RentalShoppingCartItem.ProductId);
			if (rentalShoppingCartTitleItem == null || rentalShoppingCartTitleItem.Status != LineItemStatus.Fullfilled)
			{
				list.Add(promoItem);
			}
		}
		foreach (IPromotionItem item in list)
		{
			Promotion.PromotionItems.Remove(item);
		}
		Promotion.Price = Promotion.PromotionItems.Sum((IPromotionItem x) => x.PromotionAmount);
	}

	public void ClearLoyaltyPointsToBeEarned()
	{
		TitleItems.ForEach(delegate(IRentalShoppingCartTitleItem x)
		{
			if (x.LoyaltyPointsRecord != null)
			{
				x.LoyaltyPointsRecord.PointsToBeEarned = null;
			}
		});
	}

	public void ClearAllLoyaltyPointsRecords()
	{
		TitleItems.ForEach(delegate(IRentalShoppingCartTitleItem x)
		{
			x.LoyaltyPointsRecord = null;
		});
	}

	public void Abandon(string reason)
	{
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.ComingSoonOptInEnabled)
		{
			List<IOptInItem> optInItems = ComingSoonOptInInfo.OptInItems;
			if (optInItems != null && optInItems.Count > 0)
			{
				string s = ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.GetValue<string>("Store", "ID");
				ServiceLocator.Instance.GetService<IMarketingServices>()?.KioskOptIn(long.Parse(s), optInItems, null);
			}
		}
		ShoppingSession?.Abandon(reason);
		ShoppingSession?.End();
	}

	private void SetRentalShoppingCartItemPricing(IRentalShoppingCartTitleItem rentalShoppingCartTitleItem, IPricingRecord pricingRecord)
	{
		IPricingService service = ServiceLocator.Instance.GetService<IPricingService>();
		if (service == null)
		{
			LogHelper.Instance.Log("RentalShoppingCart.SetRentalShoppingCartItemPricing - pricing service cannot be null", LogEntryType.Error);
		}
		else if (rentalShoppingCartTitleItem.Action == RentalShoppingCartItemAction.purchase)
		{
			if (pricingRecord == null)
			{
				pricingRecord = rentalShoppingCartTitleItem.TitleProduct.PurchasePriceRecord;
			}
			rentalShoppingCartTitleItem.PricingRecord = pricingRecord.Clone();
			rentalShoppingCartTitleItem.Price = rentalShoppingCartTitleItem.PricingRecord.Purchase;
			if (rentalShoppingCartTitleItem.TitleType == TitleType.DigitalCode)
			{
				rentalShoppingCartTitleItem.TaxRate = service.GetDigitalPurchaseTaxRate() / 100m;
			}
			else
			{
				rentalShoppingCartTitleItem.TaxRate = service.GetPurchaseTaxRate() / 100m;
			}
		}
		else
		{
			if (rentalShoppingCartTitleItem.Action != RentalShoppingCartItemAction.rental)
			{
				return;
			}
			if (pricingRecord == null)
			{
				pricingRecord = service.GetOneDayProductPricingRecord(rentalShoppingCartTitleItem.TitleProduct.ProductId);
			}
			if (pricingRecord == null)
			{
				LogHelper.Instance.Log($"RentalShoppingCart.SetRentalShoppingCartItemPricing - pricing record was not found for product ID {rentalShoppingCartTitleItem.TitleProduct.ProductId}", LogEntryType.Error);
				return;
			}
			rentalShoppingCartTitleItem.PricingRecord = pricingRecord.Clone();
			rentalShoppingCartTitleItem.TaxRate = service.GetRentalTaxRate() / 100m;
			if (rentalShoppingCartTitleItem.MultiDisc && rentalShoppingCartTitleItem.DiscNumber > 1)
			{
				rentalShoppingCartTitleItem.PricingRecord.InitialNight = 0m;
				rentalShoppingCartTitleItem.PricingRecord.ExtraNight = 0m;
				rentalShoppingCartTitleItem.PricingRecord.NonReturn = 0m;
				rentalShoppingCartTitleItem.PricingRecord.ExpirationPrice = 0m;
				rentalShoppingCartTitleItem.PricingRecord.DefaultInitialNightPrice = 0m;
				rentalShoppingCartTitleItem.PricingRecord.DefaultExtraNightPrice = 0m;
				rentalShoppingCartTitleItem.PricingRecord.PromoValue = 0m;
				rentalShoppingCartTitleItem.Price = 0m;
			}
			else
			{
				rentalShoppingCartTitleItem.Price = rentalShoppingCartTitleItem.PricingRecord.InitialNight;
			}
		}
	}

	private IRentalShoppingCartFilterCriteria GetPromoEligibleItemsFilterCriteria(PromotionFunctionFilters promotionFunctionFilters)
	{
		return new RentalShoppingCartFilterCriteria
		{
			Action = (promotionFunctionFilters?.Action ?? RentalShoppingCartItemAction.rental),
			Sequence = null,
			TitleType = TitleType.All,
			TitleFamily = TitleFamily.All,
			IncludeProducts = ((Promotion?.CampaignTitles?.HasTitles == true && Promotion.CampaignTitles.Include) ? Promotion.CampaignTitles.Titles : null),
			ExcludeProducts = ((Promotion?.CampaignTitles?.HasTitles == true && !Promotion.CampaignTitles.Include) ? Promotion.CampaignTitles.Titles : null),
			ProductTypeId = Promotion?.ProductTypeId,
			FormatIds = Promotion?.FormatIds,
			IncludeLoyaltyRedeemedItems = false,
			IncludeMultiNightPricedItems = (promotionFunctionFilters?.IncludeMultiNightPricingItems ?? false),
			IncludeFlashDeals = (promotionFunctionFilters?.IncludeFlashDeals ?? true),
			IncludeDiscountRestrictionItems = (promotionFunctionFilters?.IncludeDiscountRestrictionItems ?? false)
		};
	}

	private IPromoCodeValidationInfo ValidateTitleSpecificPromo(IPromotion promotion, PromotionFunctionFilters promotionFunctionFilters)
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = new PromoCodeValidationInfo();
		List<IRentalShoppingCartTitleItem> list = (from x in GetTitleItemsByCriteria(GetPromoEligibleItemsFilterCriteria(promotionFunctionFilters))
			orderby (promotion.ActionCode != PromotionActionCode.Rental) ? x.PricingRecord.Purchase : x.PricingRecord.InitialNight descending, x.ProductId
			select x).ToList();
		promoCodeValidationInfo.IsCartValid = list.Any();
		foreach (IRentalShoppingCartTitleItem item in list)
		{
			decimal promotionAmount = Math.Min(promotion.OriginalPrice, ((promotion.ActionCode == PromotionActionCode.Rental) ? item?.PricingRecord?.InitialNight : item?.PricingRecord?.Purchase).GetValueOrDefault());
			promoCodeValidationInfo.PromotionItems.Add(new PromotionItem
			{
				RentalShoppingCartItem = item,
				PromotionAmount = promotionAmount
			});
			promoCodeValidationInfo.QualifyingItems.Add(item);
		}
		promoCodeValidationInfo.Price = promoCodeValidationInfo.PromotionItems.Sum((IPromotionItem x) => x.PromotionAmount);
		return promoCodeValidationInfo;
	}

	private IPromoCodeValidationInfo SetResultForExternallyValidatedPromo(IPromotion promotion, PromotionFunctionFilters promotionFunctionFilters)
	{
		PromoCodeValidationInfo obj = new PromoCodeValidationInfo
		{
			IsCartValid = true
		};
		((IPromoCodeValidationInfo)obj).Price = ((IPromoCodeValidationInfo)obj).PromotionItems.Sum((IPromotionItem x) => x.PromotionAmount);
		return obj;
	}

	private IPromoCodeValidationInfo ValidateFixedValuePromo(IPromotion promotion, PromotionFunctionFilters promotionFunctionFilters)
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = new PromoCodeValidationInfo();
		List<IRentalShoppingCartTitleItem> list = (from x in GetTitleItemsByCriteria(GetPromoEligibleItemsFilterCriteria(promotionFunctionFilters))
			orderby (promotion.ActionCode != PromotionActionCode.Rental) ? x.PricingRecord.Purchase : x.PricingRecord.InitialNight descending, x.ProductId
			select x).ToList();
		promoCodeValidationInfo.IsCartValid = list.Any();
		decimal originalPrice = promotion.OriginalPrice;
		foreach (IRentalShoppingCartTitleItem item in list)
		{
			if (originalPrice > 0m)
			{
				decimal num = Math.Min(originalPrice, ((promotion.ActionCode != PromotionActionCode.Rental) ? item?.PricingRecord?.Purchase : item?.PricingRecord?.InitialNight).GetValueOrDefault());
				promoCodeValidationInfo.PromotionItems.Add(new PromotionItem
				{
					RentalShoppingCartItem = item,
					PromotionAmount = num
				});
				promoCodeValidationInfo.QualifyingItems.Add(item);
				originalPrice -= num;
			}
		}
		promoCodeValidationInfo.Price = promoCodeValidationInfo.PromotionItems.Sum((IPromotionItem x) => x.PromotionAmount);
		return promoCodeValidationInfo;
	}

	private IPromoCodeValidationInfo ValidateFirstNightDvd(IPromotion promotion, PromotionFunctionFilters promotionFunctionFilters)
	{
		return ValidateFirstNightByTitleType(promotion, TitleType.DVD, promotionFunctionFilters);
	}

	private IPromoCodeValidationInfo ValidateFirstNightBluRay(IPromotion promotion, PromotionFunctionFilters promotionFunctionFilters)
	{
		return ValidateFirstNightByTitleType(promotion, TitleType.Bluray, promotionFunctionFilters);
	}

	private IPromoCodeValidationInfo ValidateFirstNightByTitleType(IPromotion promotion, TitleType titleType, PromotionFunctionFilters promotionFunctionFilters)
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = new PromoCodeValidationInfo();
		IRentalShoppingCartFilterCriteria promoEligibleItemsFilterCriteria = GetPromoEligibleItemsFilterCriteria(promotionFunctionFilters);
		promoEligibleItemsFilterCriteria.TitleType = titleType;
		IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = (from x in GetTitleItemsByCriteria(promoEligibleItemsFilterCriteria)
			orderby x.PricingRecord.InitialNight descending, x.ProductId
			select x).FirstOrDefault();
		promoCodeValidationInfo.IsCartValid = rentalShoppingCartTitleItem != null;
		promoCodeValidationInfo.Price = (rentalShoppingCartTitleItem?.PricingRecord?.InitialNight).GetValueOrDefault();
		if (rentalShoppingCartTitleItem != null)
		{
			promoCodeValidationInfo.PromotionItems.Add(new PromotionItem
			{
				RentalShoppingCartItem = rentalShoppingCartTitleItem,
				PromotionAmount = promoCodeValidationInfo.Price
			});
			promoCodeValidationInfo.QualifyingItems.Add(rentalShoppingCartTitleItem);
		}
		if ((promotion.Intent == PromotionIntentCode.FirstNightDVD || promotion.Intent == PromotionIntentCode.FirstNightBluray) && rentalShoppingCartTitleItem == null)
		{
			promoEligibleItemsFilterCriteria.TitleType = TitleType.All;
			List<IRentalShoppingCartTitleItem> list = (from x in GetTitleItemsByCriteria(promoEligibleItemsFilterCriteria)
				orderby x.PricingRecord.InitialNight descending, x.ProductId
				select x).ToList();
			IPricingService service = ServiceLocator.Instance.GetService<IPricingService>();
			IPricingRecord pricingRecord = service?.GetActiveProductTypePricingRecord(TitleFamily.Movies, titleType);
			if (pricingRecord == null)
			{
				pricingRecord = service?.GetProductTypePricingRecord(TitleFamily.Movies, titleType, useAdjustedPrices: true);
			}
			promoCodeValidationInfo.IsCartValid = list.Any();
			decimal num = pricingRecord?.PromoValue ?? 0m;
			foreach (IRentalShoppingCartTitleItem item in list)
			{
				if (num > 0m)
				{
					decimal num2 = Math.Min(num, (item?.PricingRecord?.InitialNight).GetValueOrDefault());
					promoCodeValidationInfo.PromotionItems.Add(new PromotionItem
					{
						RentalShoppingCartItem = item,
						PromotionAmount = num2
					});
					promoCodeValidationInfo.QualifyingItems.Add(item);
					num -= num2;
				}
			}
			promoCodeValidationInfo.Price = promoCodeValidationInfo.PromotionItems.Sum((IPromotionItem x) => x.PromotionAmount);
		}
		return promoCodeValidationInfo;
	}

	private IPromoCodeValidationInfo ValidateFirstNightGame(IPromotion promotion, PromotionFunctionFilters promotionFunctionFilters)
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = new PromoCodeValidationInfo();
		IRentalShoppingCartFilterCriteria promoEligibleItemsFilterCriteria = GetPromoEligibleItemsFilterCriteria(promotionFunctionFilters);
		promoEligibleItemsFilterCriteria.TitleFamily = TitleFamily.Games;
		IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = (from x in GetTitleItemsByCriteria(promoEligibleItemsFilterCriteria)
			orderby x.PricingRecord.InitialNight descending, x.ProductId
			select x).FirstOrDefault();
		promoCodeValidationInfo.IsCartValid = rentalShoppingCartTitleItem != null;
		promoCodeValidationInfo.Price = (rentalShoppingCartTitleItem?.PricingRecord?.InitialNight).GetValueOrDefault();
		if (rentalShoppingCartTitleItem != null)
		{
			promoCodeValidationInfo.PromotionItems.Add(new PromotionItem
			{
				RentalShoppingCartItem = rentalShoppingCartTitleItem,
				PromotionAmount = promoCodeValidationInfo.Price
			});
			promoCodeValidationInfo.QualifyingItems.Add(rentalShoppingCartTitleItem);
		}
		if (promotion.Intent == PromotionIntentCode.FirstNightGame && rentalShoppingCartTitleItem == null)
		{
			promoEligibleItemsFilterCriteria.TitleFamily = TitleFamily.All;
			List<IRentalShoppingCartTitleItem> list = (from x in GetTitleItemsByCriteria(promoEligibleItemsFilterCriteria)
				orderby x.PricingRecord.InitialNight descending, x.ProductId
				select x).ToList();
			IPricingService service = ServiceLocator.Instance.GetService<IPricingService>();
			IPricingRecord pricingRecord = service?.GetActiveProductTypePricingRecord(TitleFamily.Games, TitleType.All);
			if (pricingRecord == null)
			{
				pricingRecord = service?.GetProductTypePricingRecord(TitleFamily.Games, TitleType.All, useAdjustedPrices: true);
			}
			promoCodeValidationInfo.IsCartValid = list.Any();
			decimal num = pricingRecord?.PromoValue ?? 0m;
			foreach (IRentalShoppingCartTitleItem item in list)
			{
				if (num > 0m)
				{
					decimal num2 = Math.Min(num, (item?.PricingRecord?.InitialNight).GetValueOrDefault());
					promoCodeValidationInfo.PromotionItems.Add(new PromotionItem
					{
						RentalShoppingCartItem = item,
						PromotionAmount = num2
					});
					promoCodeValidationInfo.QualifyingItems.Add(item);
					num -= num2;
				}
			}
			promoCodeValidationInfo.Price = promoCodeValidationInfo.PromotionItems.Sum((IPromotionItem x) => x.PromotionAmount);
		}
		return promoCodeValidationInfo;
	}

	private IPromoCodeValidationInfo ValidateFirstNightFree(IPromotion promotion, PromotionFunctionFilters promotionFunctionFilters)
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = new PromoCodeValidationInfo();
		IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = (from x in GetTitleItemsByCriteria(GetPromoEligibleItemsFilterCriteria(promotionFunctionFilters))
			orderby x.PricingRecord.InitialNight descending, x.ProductId
			select x).FirstOrDefault();
		promoCodeValidationInfo.IsCartValid = rentalShoppingCartTitleItem != null;
		promoCodeValidationInfo.Price = (rentalShoppingCartTitleItem?.PricingRecord?.InitialNight).GetValueOrDefault();
		if (rentalShoppingCartTitleItem != null)
		{
			promoCodeValidationInfo.PromotionItems.Add(new PromotionItem
			{
				RentalShoppingCartItem = rentalShoppingCartTitleItem,
				PromotionAmount = promoCodeValidationInfo.Price
			});
			promoCodeValidationInfo.QualifyingItems.Add(rentalShoppingCartTitleItem);
		}
		return promoCodeValidationInfo;
	}

	private IPromoCodeValidationInfo ValidateRedboxPlusFreeOneNightRental(IPromotion promotion, PromotionFunctionFilters promotionFunctionFilters)
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = new PromoCodeValidationInfo();
		IRentalShoppingCartFilterCriteria promoEligibleItemsFilterCriteria = GetPromoEligibleItemsFilterCriteria(promotionFunctionFilters);
		promoEligibleItemsFilterCriteria.IncludeRedboxPlusFreeOneNightRentalItems = true;
		promoEligibleItemsFilterCriteria.IncludeFlashDeals = true;
		IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = (from x in GetTitleItemsByCriteria(promoEligibleItemsFilterCriteria)
			orderby x.PricingRecord.InitialNight descending, x.ProductId
			select x).FirstOrDefault();
		promoCodeValidationInfo.IsCartValid = rentalShoppingCartTitleItem != null;
		promoCodeValidationInfo.Price = (rentalShoppingCartTitleItem?.PricingRecord?.InitialNight).GetValueOrDefault();
		if (rentalShoppingCartTitleItem != null)
		{
			promoCodeValidationInfo.PromotionItems.Add(new PromotionItem
			{
				RentalShoppingCartItem = rentalShoppingCartTitleItem,
				PromotionAmount = promoCodeValidationInfo.Price
			});
			promoCodeValidationInfo.QualifyingItems.Add(rentalShoppingCartTitleItem);
		}
		return promoCodeValidationInfo;
	}

	private IPromoCodeValidationInfo ValidatePromoConsideringConditions(PromotionValidationFunction validationFunction, IPromotion promotion)
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = validationFunction(promotion, new PromotionFunctionFilters(promotion));
		if (!promoCodeValidationInfo.IsCartValid)
		{
			IPromoCodeValidationInfo promoCodeValidationInfo2 = validationFunction(promotion, new PromotionFunctionFilters(promotion)
			{
				IncludeFlashDeals = true
			});
			IPromoCodeValidationInfo promoCodeValidationInfo3 = validationFunction(promotion, new PromotionFunctionFilters(promotion)
			{
				IncludeMultiNightPricingItems = true
			});
			IPromoCodeValidationInfo promoCodeValidationInfo4 = validationFunction(promotion, new PromotionFunctionFilters(promotion)
			{
				IncludeDiscountRestrictionItems = true
			});
			IPromoCodeValidationInfo promoCodeValidationInfo5 = validationFunction(promotion, new PromotionFunctionFilters(promotion)
			{
				IncludeFlashDeals = true,
				IncludeMultiNightPricingItems = true
			});
			IPromoCodeValidationInfo promoCodeValidationInfo6 = validationFunction(promotion, new PromotionFunctionFilters(promotion)
			{
				IncludeFlashDeals = true,
				IncludeDiscountRestrictionItems = true
			});
			IPromoCodeValidationInfo promoCodeValidationInfo7 = validationFunction(promotion, new PromotionFunctionFilters(promotion)
			{
				IncludeMultiNightPricingItems = true,
				IncludeDiscountRestrictionItems = true
			});
			IPromoCodeValidationInfo promoCodeValidationInfo8 = validationFunction(promotion, new PromotionFunctionFilters(promotion)
			{
				IncludeFlashDeals = true,
				IncludeMultiNightPricingItems = true,
				IncludeDiscountRestrictionItems = true
			});
			if (promoCodeValidationInfo2.IsCartValid)
			{
				OrchestrationService.SetCurrentUICulture();
				promoCodeValidationInfo.Title = Resources.promo_code_error_invalid_cart_items_flash_deal_title;
				promoCodeValidationInfo.Message = Resources.promo_code_error_invalid_cart_items_flash_deal_message;
				promoCodeValidationInfo.IsCartInvalidBecauseOfFlashDeal = true;
			}
			else if (promoCodeValidationInfo3.IsCartValid)
			{
				OrchestrationService.SetCurrentUICulture();
				promoCodeValidationInfo.Title = Resources.promo_code_error_invalid_cart_items;
				promoCodeValidationInfo.Message = null;
				promoCodeValidationInfo.IsCartInvalidBecauseOfMultiNightPricing = true;
			}
			else if (promoCodeValidationInfo4.IsCartValid)
			{
				OrchestrationService.SetCurrentUICulture();
				promoCodeValidationInfo.Title = Resources.promo_code_error_invalid_cart_items;
				promoCodeValidationInfo.Message = null;
				promoCodeValidationInfo.IsCartInvalidBecauseOfDiscountRestriction = true;
			}
			else if (promoCodeValidationInfo5.IsCartValid)
			{
				OrchestrationService.SetCurrentUICulture();
				promoCodeValidationInfo.Title = Resources.promo_code_error_invalid_cart_items_flash_deal_title;
				promoCodeValidationInfo.Message = Resources.promo_code_error_invalid_cart_items_flash_deal_message;
				promoCodeValidationInfo.IsCartInvalidBecauseOfFlashDeal = true;
				promoCodeValidationInfo.IsCartInvalidBecauseOfMultiNightPricing = true;
			}
			else if (promoCodeValidationInfo6.IsCartValid)
			{
				OrchestrationService.SetCurrentUICulture();
				promoCodeValidationInfo.Title = Resources.promo_code_error_invalid_cart_items_flash_deal_title;
				promoCodeValidationInfo.Message = Resources.promo_code_error_invalid_cart_items_flash_deal_message;
				promoCodeValidationInfo.IsCartInvalidBecauseOfFlashDeal = true;
				promoCodeValidationInfo.IsCartInvalidBecauseOfDiscountRestriction = true;
			}
			else if (promoCodeValidationInfo7.IsCartValid)
			{
				OrchestrationService.SetCurrentUICulture();
				promoCodeValidationInfo.Title = Resources.promo_code_error_invalid_cart_items;
				promoCodeValidationInfo.Message = null;
				promoCodeValidationInfo.IsCartInvalidBecauseOfMultiNightPricing = true;
				promoCodeValidationInfo.IsCartInvalidBecauseOfDiscountRestriction = true;
			}
			else if (promoCodeValidationInfo8.IsCartValid)
			{
				OrchestrationService.SetCurrentUICulture();
				promoCodeValidationInfo.Title = Resources.promo_code_error_invalid_cart_items_flash_deal_title;
				promoCodeValidationInfo.Message = Resources.promo_code_error_invalid_cart_items_flash_deal_message;
				promoCodeValidationInfo.IsCartInvalidBecauseOfFlashDeal = true;
				promoCodeValidationInfo.IsCartInvalidBecauseOfMultiNightPricing = true;
				promoCodeValidationInfo.IsCartInvalidBecauseOfDiscountRestriction = true;
			}
		}
		return promoCodeValidationInfo;
	}

	private IPromoCodeValidationInfo ValidatePercentOffPromo(IPromotion promotion, PromotionFunctionFilters promotionFunctionFilters)
	{
		IPromoCodeValidationInfo promoCodeValidationInfo = new PromoCodeValidationInfo();
		List<IRentalShoppingCartTitleItem> list = GetTitleItemsByCriteria(GetPromoEligibleItemsFilterCriteria(promotionFunctionFilters)).ToList();
		decimal arg = list.Sum((IRentalShoppingCartTitleItem x) => ((promotion.ActionCode != PromotionActionCode.Rental) ? x.PricingRecord?.Purchase : x.PricingRecord?.InitialNight).GetValueOrDefault());
		Func<decimal, decimal> func = (decimal originalValue) => decimal.Round(originalValue * (promotion?.Percent ?? 0m), 2, MidpointRounding.AwayFromZero);
		decimal num = func(arg);
		decimal num2 = 0m;
		promoCodeValidationInfo.IsCartValid = list.Any();
		promoCodeValidationInfo.Price = num;
		promoCodeValidationInfo.Percent = Promotion.Percent;
		foreach (IRentalShoppingCartTitleItem item in list)
		{
			PromotionItem promotionItem = new PromotionItem
			{
				RentalShoppingCartItem = item,
				PromotionAmount = func(((promotion.ActionCode == PromotionActionCode.Rental) ? item?.PricingRecord?.InitialNight : item?.PricingRecord?.Purchase).GetValueOrDefault())
			};
			promoCodeValidationInfo.PromotionItems.Add(promotionItem);
			promoCodeValidationInfo.QualifyingItems.Add(item);
			num2 += promotionItem.PromotionAmount;
		}
		if (promoCodeValidationInfo.PromotionItems != null && promoCodeValidationInfo.PromotionItems.Count >= 1)
		{
			decimal num3 = num - num2;
			promoCodeValidationInfo.PromotionItems[0].PromotionAmount += num3;
		}
		return promoCodeValidationInfo;
	}

	private IPromoCodeValidationInfo ValidateRxGxPromo(IPromotion promotion, PromotionFunctionFilters promotionFunctionFilters)
	{
		IPromoCodeValidationInfo result = new PromoCodeValidationInfo();
		if (promotion.RentQuantity != 0 && promotion.RentFormat.HasValue && promotion.GetQuantity != 0 && promotion.GetFormat.HasValue)
		{
			List<IRentalShoppingCartTitleItem> rentItems = (from x in GetTitleItemsByCriteria(RentalShoppingCartItemAction.rental, null, GetTitleTypeFromPromotionFormat(promotion.RentFormat.Value), GetTitleFamilyFromPromotionFormat(promotion.RentFormat.Value), null, null, null, null, null, includeMultiNightPricedItems: false, includeFlashDeals: true, includeLoyaltyRedeemedItems: false, false, includeDiscountRestrictionItems: false)
				orderby promotion.RentFormat == PromotionRentFormat.Any && promotion.GetFormat != PromotionGetFormat.Any && GetTitleTypeFromPromotionFormat(promotion.GetFormat.Value) == x.TitleType, x.PricingRecord.InitialNight descending, x.ProductId descending
				select x).Take(promotion.RentQuantity).ToList();
			List<IRentalShoppingCartTitleItem> list = (from x in GetTitleItemsByCriteria(RentalShoppingCartItemAction.rental, null, GetTitleTypeFromPromotionFormat(promotion.GetFormat.Value), GetTitleFamilyFromPromotionFormat(promotion.GetFormat.Value), null, null, null, null, null, includeMultiNightPricedItems: false, includeFlashDeals: true, includeLoyaltyRedeemedItems: false, false, includeDiscountRestrictionItems: false)
				where !rentItems.Contains(x)
				orderby x.PricingRecord.InitialNight descending, x.ProductId
				select x).Take(promotion.GetQuantity).ToList();
			List<IRentalShoppingCartTitleItem> list2 = new List<IRentalShoppingCartTitleItem>();
			list2.AddRange(list);
			list2.AddRange(rentItems);
			int num = RxGxGetRequiredCount("Dvd", promotion.RentQuantity, promotion.RentFormat.Value, promotion.GetQuantity, promotion.GetFormat.Value);
			int num2 = Math.Max(num - list2.Count((IRentalShoppingCartTitleItem x) => x.TitleType == TitleType.DVD), 0);
			int num3 = RxGxGetRequiredCount("BluRay", promotion.RentQuantity, promotion.RentFormat.Value, promotion.GetQuantity, promotion.GetFormat.Value);
			int num4 = Math.Max(num3 - list2.Count((IRentalShoppingCartTitleItem x) => x.TitleType == TitleType.Bluray), 0);
			int num5 = RxGxGetRequiredCount("FourKUhd", promotion.RentQuantity, promotion.RentFormat.Value, promotion.GetQuantity, promotion.GetFormat.Value);
			int num6 = Math.Max(num5 - list2.Count((IRentalShoppingCartTitleItem x) => x.TitleType == TitleType._4KUHD), 0);
			int num7 = RxGxGetRequiredCount("Game", promotion.RentQuantity, promotion.RentFormat.Value, promotion.GetQuantity, promotion.GetFormat.Value);
			int num8 = Math.Max(num7 - list2.Count((IRentalShoppingCartTitleItem x) => x.TitleFamily == TitleFamily.Games), 0);
			int num9 = Math.Max(RxGxGetRequiredCount("Any", promotion.RentQuantity, promotion.RentFormat.Value, promotion.GetQuantity, promotion.GetFormat.Value) + num + num3 + num5 + num7 - num2 - num4 - num6 - num8 - list2.Count(), 0);
			bool flag = num2 == 0 && num4 == 0 && num6 == 0 && num8 == 0 && num9 == 0;
			result.IsCartValid = flag;
			result.Price = (flag ? list.Sum((IRentalShoppingCartTitleItem x) => x.PricingRecord.InitialNight) : 0m);
			result.Title = RxGxGetTitle(promotion.RentQuantity, promotion.RentFormat.Value, promotion.GetQuantity, promotion.GetFormat.Value);
			result.Message = RxGxGetMessage(num2, num4, num6, num8, num9);
			rentItems.ForEach(delegate(IRentalShoppingCartTitleItem x)
			{
				result.QualifyingItems.Add(x);
			});
			list.ForEach(delegate(IRentalShoppingCartTitleItem x)
			{
				result.PromotionItems.Add(new PromotionItem
				{
					RentalShoppingCartItem = x,
					PromotionAmount = x.PricingRecord.InitialNight
				});
			});
		}
		return result;
	}

	private TitleFamily GetTitleFamilyFromPromotionFormat(PromotionRentFormat rentFormat)
	{
		if (rentFormat == PromotionRentFormat.Game)
		{
			return TitleFamily.Games;
		}
		return TitleFamily.All;
	}

	private TitleFamily GetTitleFamilyFromPromotionFormat(PromotionGetFormat getFormat)
	{
		if (getFormat == PromotionGetFormat.Game)
		{
			return TitleFamily.Games;
		}
		return TitleFamily.All;
	}

	private TitleType GetTitleTypeFromPromotionFormat(PromotionRentFormat rentFormat)
	{
		return rentFormat switch
		{
			PromotionRentFormat.Dvd => TitleType.DVD, 
			PromotionRentFormat.BluRay => TitleType.Bluray, 
			PromotionRentFormat.FourKUhd => TitleType._4KUHD, 
			_ => TitleType.All, 
		};
	}

	private TitleType GetTitleTypeFromPromotionFormat(PromotionGetFormat getFormat)
	{
		return getFormat switch
		{
			PromotionGetFormat.Dvd => TitleType.DVD, 
			PromotionGetFormat.BluRay => TitleType.Bluray, 
			PromotionGetFormat.FourKUhd => TitleType._4KUHD, 
			_ => TitleType.All, 
		};
	}

	private string RxGxGetMessage(int dvdsRemaining, int bluRaysRemaining, int fourKUhdsRemaining, int gamesRemaining, int anyRemaining)
	{
		string result = string.Empty;
		if (dvdsRemaining > 0 || bluRaysRemaining > 0 || fourKUhdsRemaining > 0 || gamesRemaining > 0 || anyRemaining > 0)
		{
			string promo_code_rxgx_message = Resources.promo_code_rxgx_message;
			int count = 0;
			List<string> conditions = new List<string>();
			Action<int, string> action = delegate(int amountRemaining, string format)
			{
				if (amountRemaining > 0)
				{
					conditions.Add(string.Format("{0} {1}{2}", amountRemaining, format, (amountRemaining > 1) ? "s" : ""));
					count++;
				}
			};
			action(dvdsRemaining, Resources.dvd);
			action(bluRaysRemaining, Resources.blu_ray);
			action(fourKUhdsRemaining, Resources._4k_uhd);
			action(gamesRemaining, Resources.game);
			action(anyRemaining, Resources.other_item);
			result = string.Format(promo_code_rxgx_message, string.Join($" {Resources.and} ", conditions));
		}
		return result;
	}

	private string RxGxGetTitle(int rentQuantity, PromotionRentFormat rentFormat, int getQuantity, PromotionGetFormat getFormat)
	{
		string promo_code_rxgx_title = Resources.promo_code_rxgx_title;
		string empty = string.Empty;
		empty = rentFormat switch
		{
			PromotionRentFormat.Dvd => Resources.dvd, 
			PromotionRentFormat.BluRay => Resources.blu_ray, 
			PromotionRentFormat.FourKUhd => Resources._4k_uhd, 
			PromotionRentFormat.Game => Resources.game, 
			PromotionRentFormat.Any => Resources.any, 
			_ => string.Empty, 
		};
		if (rentQuantity > 1 && rentFormat != PromotionRentFormat.Any)
		{
			empty = $"{empty}s";
		}
		string empty2 = string.Empty;
		empty2 = getFormat switch
		{
			PromotionGetFormat.Dvd => Resources.dvd, 
			PromotionGetFormat.BluRay => Resources.blu_ray, 
			PromotionGetFormat.FourKUhd => Resources._4k_uhd, 
			PromotionGetFormat.Game => Resources.game, 
			PromotionGetFormat.Any => Resources.any, 
			_ => string.Empty, 
		};
		if (getQuantity > 1 && getFormat != PromotionGetFormat.Any)
		{
			empty2 = $"{empty2}s";
		}
		return string.Format(promo_code_rxgx_title, rentQuantity, empty, getQuantity, empty2);
	}

	private int RxGxGetRequiredCount(string format, byte rentQuantity, PromotionRentFormat rentFormat, byte getQuantity, PromotionGetFormat getFormat)
	{
		int num = 0;
		if (format == rentFormat.ToString())
		{
			num += rentQuantity;
		}
		if (format == getFormat.ToString())
		{
			num += getQuantity;
		}
		return num;
	}

	private bool IsDiscountRestrictionAndFullValuePromotion(IRentalShoppingCartTitleItem rentalShoppingCartTitleItem)
	{
		bool num = (rentalShoppingCartTitleItem?.TitleProduct?.DiscountRestriction).GetValueOrDefault() != DiscountRestriction.None;
		int num4;
		if (!((Promotion?.OriginalPrice ?? 0m) >= (rentalShoppingCartTitleItem?.Price ?? 0m)))
		{
			decimal? num2 = Promotion?.OriginalPercent;
			decimal num3 = 1;
			if (!((num2.GetValueOrDefault() == num3) & num2.HasValue))
			{
				IPromotion promotion = Promotion;
				if (promotion == null || promotion.Intent != PromotionIntentCode.FirstNightDVD)
				{
					IPromotion promotion2 = Promotion;
					if (promotion2 == null || promotion2.Intent != PromotionIntentCode.FirstNightBluray)
					{
						IPromotion promotion3 = Promotion;
						num4 = ((promotion3 != null && promotion3.Intent == PromotionIntentCode.FirstNightFreeItem) ? 1 : 0);
						goto IL_00f0;
					}
				}
				num4 = 1;
				goto IL_00f0;
			}
		}
		num4 = 1;
		goto IL_00f0;
		IL_00f0:
		bool flag = (byte)num4 != 0;
		return num & flag;
	}

	public List<IRentalShoppingCartTitleItem> GetTitleItemsByCriteria(IRentalShoppingCartFilterCriteria filterCriteria)
	{
		return GetTitleItemsByCriteria(filterCriteria.Action, filterCriteria.Sequence, filterCriteria.TitleType, filterCriteria.TitleFamily, filterCriteria.IncludeProducts, filterCriteria.ExcludeProducts, filterCriteria.ProductTypes, filterCriteria.ProductTypeId, filterCriteria.FormatIds, filterCriteria.IncludeMultiNightPricedItems, filterCriteria.IncludeFlashDeals, filterCriteria.IncludeLoyaltyRedeemedItems, filterCriteria.IncludeRedboxPlusFreeOneNightRentalItems, filterCriteria.IncludeDiscountRestrictionItems);
	}

	public List<IRentalShoppingCartTitleItem> GetTitleItemsByCriteria(RentalShoppingCartItemAction? action, RentalShoppingCartItemSequence? sequence, TitleType titleType, TitleFamily titleFamily, List<long> includeProducts, List<long> excludeProducts, List<TitleType> productTypes, int? productTypeId, List<int> formatIds, bool includeMultiNightPricedItems, bool includeFlashDeals, bool includeLoyaltyRedeemedItems, bool? includeRedboxPlusFreeOnNightRentalItems, bool includeDiscountRestrictionItems)
	{
		List<IRentalShoppingCartTitleItem> list = new List<IRentalShoppingCartTitleItem>();
		foreach (IRentalShoppingCartTitleItem titleItem in TitleItems)
		{
			if ((!action.HasValue || titleItem.Action == action) && (!sequence.HasValue || titleItem.Sequence == sequence) && (titleType == TitleType.All || titleItem.TitleType == titleType) && (titleFamily == TitleFamily.All || titleItem.TitleFamily == titleFamily) && (includeProducts == null || includeProducts.Count == 0 || includeProducts.Contains(titleItem.ProductId)) && (excludeProducts == null || excludeProducts.Count == 0 || !excludeProducts.Contains(titleItem.ProductId)) && (productTypes == null || productTypes.Count == 0 || productTypes.Contains(titleItem.TitleType)) && (!productTypeId.HasValue || productTypeId == 0 || (TitleFamily?)titleItem.TitleFamily == (TitleFamily?)productTypeId) && (formatIds == null || formatIds.Count == 0 || formatIds.Contains((int)titleItem.TitleType)) && (!titleItem.DiscNumber.HasValue || titleItem.DiscNumber == 1) && (action == RentalShoppingCartItemAction.purchase || ((includeMultiNightPricedItems || titleItem?.PricingRecord?.IsMultiNightPrice != true) && (includeFlashDeals || !titleItem.HasDeal))) && (includeLoyaltyRedeemedItems || titleItem?.LoyaltyPointsRecord?.ToBeRedeemed != true) && (!includeRedboxPlusFreeOnNightRentalItems.HasValue || (includeRedboxPlusFreeOnNightRentalItems == true && titleItem != null && titleItem.IsRedboxPlusFreeOneNightRental) || (includeRedboxPlusFreeOnNightRentalItems == false && (titleItem == null || !titleItem.IsRedboxPlusFreeOneNightRental))) && (includeDiscountRestrictionItems || !IsDiscountRestrictionAndFullValuePromotion(titleItem)) && (!_isAfterVend || titleItem.Status == LineItemStatus.Fullfilled))
			{
				list.Add(titleItem);
			}
		}
		return list;
	}

	public IRentalShoppingCart ShallowClone()
	{
		RentalShoppingCart rentalShoppingCart = new RentalShoppingCart(ShoppingSessionId, _defaultServiceFee, _defaultServiceFeeTaxRate);
		foreach (IRentalShoppingCartTitleItem titleItem in TitleItems)
		{
			rentalShoppingCart.AddTitleItem(titleItem, titleItem.PricingRecord);
		}
		return rentalShoppingCart;
	}
}
