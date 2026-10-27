using System;
using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Promotion;

namespace Redbox.Rental.Model.ShoppingCart;

public interface IRentalShoppingCart
{
	Guid ShoppingSessionId { get; }

	int Count { get; }

	bool IsFull { get; }

	IList<IBaseRentalShoppingCartItem> Items { get; }

	IList<IRentalShoppingCartTitleItem> TitleItems { get; }

	IList<IRentalShoppingCartSubscriptionItem> SubscriptionItems { get; }

	int MaximumCapacity { get; }

	IUpsellInfo UpsellInfo { get; }

	IComingSoonOptinInfo ComingSoonOptInInfo { get; }

	bool InCartPromoOfferMade { get; set; }

	bool InCartPromoOfferCancel { get; set; }

	bool InCartDoNotShow { get; set; }

	bool InCartTitleMarketing { get; set; }

	ITotals Totals { get; }

	IPromotion Promotion { get; }

	bool PickupRecommendationsShown { get; set; }

	bool PickupRecommendationsSelected { get; }

	OptInConfirmationStatus MarketingOptInConfirmationStatus { get; set; }

	IList<IRentalShoppingCartTitleItem> RedeemableItems { get; }

	IList<IRentalShoppingCartTitleItem> ToBeRedeemedItems { get; }

	int TotalPointsToBeEarned { get; }

	bool HasItemsToBeRedeemed { get; }

	bool HasRedboxPlusFreeOneNightRentalSelected { get; }

	IServiceFee ServiceFee { get; set; }

	bool ServiceFeeEnabled { get; }

	decimal DefaultServiceFee { get; }

	event RentalShoppingCartChanged OnChange;

	void Abandon(string reason);

	bool AddItem(IBaseRentalShoppingCartItem rentalShoppingCartItem);

	bool AddTitleItem(IRentalShoppingCartTitleItem rentalShoppingCartItem, IPricingRecord pricingRecord);

	bool AddSubscriptionItem(IRentalShoppingCartSubscriptionItem rentalShoppingCartItem);

	void RemoveItem(IBaseRentalShoppingCartItem rentalShoppingCartItem, bool userRemoved);

	void Clear();

	IBaseRentalShoppingCartItem GetItem(int index);

	bool HasTitleItem(long productId);

	bool HasAnyTitleRollupTitle(ITitleRollupProduct titleRollupProduct);

	bool HasSubscriptionItem(ISubscriptionProduct subscriptionProduct);

	bool HasSubscriptionItem(SubscriptionType subscriptionType);

	bool HasItemAction(RentalShoppingCartItemAction action);

	bool HasItemWithAction(long productId, RentalShoppingCartItemAction action);

	bool HasMultiDiscItem();

	void TriggerChangeNotification();

	void ClearPromoCode();

	IPromoCodeValidationInfo ValidateAndUpdatePromoCode();

	void ReValidatePromoCodeAfterVend();

	void ClearLoyaltyPointsToBeEarned();

	void ClearAllLoyaltyPointsRecords();

	List<IRentalShoppingCartTitleItem> GetTitleItemsByCriteria(IRentalShoppingCartFilterCriteria filterCriteria);

	List<IRentalShoppingCartTitleItem> GetTitleItemsByCriteria(RentalShoppingCartItemAction? action, RentalShoppingCartItemSequence? sequence, TitleType titleType, TitleFamily titleFamily, List<long> includeProducts, List<long> excludeProducts, List<TitleType> productTypes, int? productTypeId, List<int> formatIds, bool includeMultiNightPricedItems, bool includeFlashDeals, bool includeLoyaltyRedeemedItems, bool? includeRedboxPlusFreeOneNightRentalItems, bool includeDiscountRestrictionItems);

	void UpdateServiceFee();

	void SetDefaultServiceFee(decimal defaultServiceFee);

	IRentalShoppingCart ShallowClone();
}
