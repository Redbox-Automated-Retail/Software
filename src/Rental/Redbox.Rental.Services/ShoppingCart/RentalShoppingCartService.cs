using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Promotion;
using Redbox.Rental.Services.Session;

namespace Redbox.Rental.Services.ShoppingCart;

public class RentalShoppingCartService : IRentalShoppingCartService
{
	private readonly object m_syncObject = new object();

	private readonly IDictionary<Guid, IRentalShoppingCart> _rentalShoppingCarts = new Dictionary<Guid, IRentalShoppingCart>();

	public static RentalShoppingCartService Instance => Singleton<RentalShoppingCartService>.Instance;

	public bool CurrentRentalShoppingCartIsFull => CurrentRentalShoppingCart?.IsFull ?? false;

	public IRentalShoppingCart CurrentRentalShoppingCart
	{
		get
		{
			IRentalShoppingCart result = null;
			IShoppingSession currentShoppingSession = GetCurrentShoppingSession();
			if (currentShoppingSession != null)
			{
				result = GetRentalShoppingCart(currentShoppingSession.Id);
			}
			return result;
		}
	}

	public event RentalShoppingCartAdded OnRentalShoppingCartAdded;

	public void Initialize()
	{
		ServiceLocator.Instance.AddService(typeof(IRentalShoppingCartService), Instance);
	}

	public IRentalShoppingCart GetRentalShoppingCart(Guid id)
	{
		IRentalShoppingCart result = null;
		lock (m_syncObject)
		{
			if (_rentalShoppingCarts.ContainsKey(id))
			{
				result = _rentalShoppingCarts[id];
			}
			else
			{
				LogHelper.Instance.Log("Rental Shopping Cart with id: '{0}' was not found.", id);
			}
			return result;
		}
	}

	public IRentalShoppingCart CreateNew(Guid id)
	{
		IRentalShoppingCart value = null;
		Store store = (ServiceLocator.Instance.GetService(typeof(IDataService)) as IDataService)?.GetStore();
		lock (m_syncObject)
		{
			if (!_rentalShoppingCarts.TryGetValue(id, out value))
			{
				value = new RentalShoppingCart(id, (!Redbox.Rental.Services.Configuration.Configuration.Instance.ServiceFeeEnabled) ? 0m : (store?.ServiceFee ?? 0m), (!Redbox.Rental.Services.Configuration.Configuration.Instance.ServiceFeeEnabled) ? 0m : (store?.ServiceFeeTaxRate ?? 0m));
				_rentalShoppingCarts[id] = value;
				LogHelper.Instance.Log("Creating Rental Shopping Cart with id: {0}", id);
			}
		}
		if (value != null && OnRentalShoppingCartAdded != null)
		{
			OnRentalShoppingCartAdded(value);
		}
		return value;
	}

	public bool RemoveRentalShoppingCart(Guid id)
	{
		bool result = false;
		lock (m_syncObject)
		{
			IRentalShoppingCart value = null;
			if (_rentalShoppingCarts.TryGetValue(id, out value))
			{
				result = _rentalShoppingCarts.Remove(id);
				LogHelper.Instance.Log("Removing Rental Shopping Cart with id: {0}", id);
			}
		}
		return result;
	}

	public void AddMultiDiscDiscsToRentalShoppingCart(Guid shoppingCartSessionId)
	{
		IRentalShoppingCart rentalShoppingCart = GetRentalShoppingCart(shoppingCartSessionId);
		if (rentalShoppingCart != null)
		{
			IDataService dataService = ServiceLocator.Instance.GetService(typeof(IDataService)) as IDataService;
			{
				foreach (IRentalShoppingCartTitleItem titleItem in rentalShoppingCart.TitleItems)
				{
					dataService.GetProduct(titleItem.ProductId, excludeTitleRollup: false, out var kioskProduct);
					if (kioskProduct == null || !(kioskProduct is MDVProduct mDVProduct))
					{
						continue;
					}
					foreach (ITitleProduct titleProduct in mDVProduct.TitleProducts)
					{
						if (titleProduct.DiscNumber > 1 && !rentalShoppingCart.HasTitleItem(titleProduct.ProductId))
						{
							IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = CreateRentalShoppingCartItemFromTitleProduct(titleProduct);
							rentalShoppingCartTitleItem.SourceType = ItemSourceType.Browse;
							rentalShoppingCartTitleItem.Action = RentalShoppingCartItemAction.rental;
							rentalShoppingCartTitleItem.Quantity = 1m;
							rentalShoppingCartTitleItem.OfferCode = PersonalizationFunctions.GetLoyaltyOfferCode(titleProduct);
							rentalShoppingCartTitleItem.TivoQueryId = PersonalizationFunctions.GetTivoQueryId(titleProduct);
							rentalShoppingCartTitleItem.PersonalizationTitleTags = PersonalizationFunctions.GetPersonalizationTitleTags(titleProduct);
							rentalShoppingCart.AddItem(rentalShoppingCartTitleItem);
							LogHelper.Instance.Log("MDV - adding product id {0}", titleProduct.ProductId);
						}
					}
				}
				return;
			}
		}
		LogHelper.Instance.Log("Error in RentalShoppingCartService.AddMultiDiscDiscsToCurrentRentalShoppingCart.  Unable to find rental shopping cart with id: {0}", shoppingCartSessionId);
	}

	public void RemoveMultiDiscDiscsFromRentalShoppingCart(Guid shoppingCartSessionId)
	{
		IRentalShoppingCart rentalShoppingCart = GetRentalShoppingCart(shoppingCartSessionId);
		if (rentalShoppingCart != null)
		{
			IDataService dataService = ServiceLocator.Instance.GetService(typeof(IDataService)) as IDataService;
			List<IRentalShoppingCartTitleItem> list = new List<IRentalShoppingCartTitleItem>();
			foreach (IRentalShoppingCartTitleItem titleItem in rentalShoppingCart.TitleItems)
			{
				dataService.GetProduct(titleItem.ProductId, excludeTitleRollup: true, out var kioskProduct);
				if (kioskProduct != null && kioskProduct is TitleProduct { DiscNumber: not null, DiscNumber: var discNumber } && discNumber > 1)
				{
					list.Add(titleItem);
				}
			}
			{
				foreach (IRentalShoppingCartTitleItem item in list)
				{
					LogHelper.Instance.Log("MDV - removing product id {0}", item.ProductId);
					rentalShoppingCart.RemoveItem(item, userRemoved: false);
				}
				return;
			}
		}
		LogHelper.Instance.Log("Error in RentalShoppingCartService.RemoveMultiDiscDiscsFromRentalShoppingCart.  Unable to find rental shopping cart with id: {0}", shoppingCartSessionId);
	}

	public void RemoveNonReservedProductsFromRentalShoppingCart(Guid shoppingCartSessionId)
	{
		IRentalShoppingCart rentalShoppingCart = GetRentalShoppingCart(shoppingCartSessionId);
		if (rentalShoppingCart != null)
		{
			List<IRentalShoppingCartTitleItem> list = new List<IRentalShoppingCartTitleItem>();
			foreach (IRentalShoppingCartTitleItem titleItem in rentalShoppingCart.TitleItems)
			{
				if (titleItem.SourceType != ItemSourceType.Reservation)
				{
					list.Add(titleItem);
				}
			}
			{
				foreach (IRentalShoppingCartTitleItem item in list)
				{
					LogHelper.Instance.Log("removing non-reserved product id {0}", item.ProductId);
					rentalShoppingCart.RemoveItem(item, userRemoved: false);
				}
				return;
			}
		}
		LogHelper.Instance.Log("Error in RentalShoppingCartService.RemoveNonReservedProductsFromRentalShoppingCart.  Unable to find rental shopping cart with id: {0}", shoppingCartSessionId);
	}

	public void RemovePromoOfferFromCart(Guid shoppingSessionId, bool force = false)
	{
		IPromoOfferState promoOfferState = Redbox.Rental.Services.Session.Session.Instance.PromoOfferState;
		if (promoOfferState != null && promoOfferState.WasOfferAccepted && promoOfferState.SelectedOffer != null && (force || promoOfferState.SelectedOffer.CustomerSegmentType != CustomerSegmentType.NewMemberPerksSignup))
		{
			if (promoOfferState.SelectedProduct != null)
			{
				RemoveItemFromRentalShoppingCartByProductId(shoppingSessionId, promoOfferState.SelectedProduct.ProductId, userRemoved: true);
			}
			IPromotionService service = ServiceLocator.Instance.GetService<IPromotionService>();
			RemoveOfferDiscountsFromCart(shoppingSessionId, promoOfferState);
			service.ClearPromotion();
			promoOfferState.SelectedOffer = null;
			promoOfferState.SelectedProduct = null;
			promoOfferState.WasOfferAccepted = false;
		}
	}

	private void RemoveOfferDiscountsFromCart(Guid shoppingSessionId, IPromoOfferState offerState)
	{
		IRentalShoppingCart rentalShoppingCart = GetRentalShoppingCart(shoppingSessionId);
		if (rentalShoppingCart == null)
		{
			return;
		}
		foreach (IRentalShoppingCartTitleItem titleItem in rentalShoppingCart.TitleItems)
		{
			if (offerState.SelectedOffer.DiscountType == OfferDiscountType.MultiNightPrice)
			{
				RemoveMultiNightOfferFromItem(titleItem);
			}
		}
	}

	public IRentalShoppingCartTitleItem CreateRentalShoppingCartItemFromProductId(long productId)
	{
		IRentalShoppingCartTitleItem result = null;
		(ServiceLocator.Instance.GetService(typeof(IDataService)) as IDataService).GetProduct(productId, excludeTitleRollup: true, out var kioskProduct);
		if (kioskProduct != null)
		{
			if (kioskProduct is ITitleProduct titleProduct)
			{
				result = CreateRentalShoppingCartItemFromTitleProduct(titleProduct);
			}
			else
			{
				LogHelper.Instance.Log("Error in RentalShoppingCartService.CreateRentalShoppingCartItemFromProductId. Product with id {0} is not a title product", productId);
			}
		}
		else
		{
			LogHelper.Instance.Log("Error in RentalShoppingCartService.CreateRentalShoppingCartItemFromProductId. Unable to find product with id {0}", productId);
		}
		return result;
	}

	public IRentalShoppingCartTitleItem CreateRentalShoppingCartItemFromTitleProduct(ITitleProduct titleProduct)
	{
		RentalShoppingCartTitleItem result = null;
		if (titleProduct != null)
		{
			titleProduct.RatingId.GetValueOrDefault();
			_ = titleProduct is MDVProduct;
			result = new RentalShoppingCartTitleItem(titleProduct);
			titleProduct?.Genres.ForEach(delegate(long x)
			{
				try
				{
					result.Genres.Add((Genres)x);
				}
				catch
				{
					LogHelper.Instance.Log("Error in RentalShoppingCartService.CreateRentalShoppingCartItemFromTitleProduct. Unable to cast value {0} as a Genre", x);
				}
			});
		}
		return result;
	}

	public IRentalShoppingCartSubscriptionItem CreateRentalShoppingCartItemFromSubscriptionProduct(ISubscriptionProduct subscriptionProduct)
	{
		IRentalShoppingCartSubscriptionItem result = null;
		if (subscriptionProduct != null)
		{
			result = new RentalShoppingCartSubscriptionItem(subscriptionProduct);
		}
		return result;
	}

	public void RemoveItemFromRentalShoppingCartByProductId(Guid shoppingCartSessionId, long productId, bool userRemoved)
	{
		IRentalShoppingCart rentalShoppingCart = GetRentalShoppingCart(shoppingCartSessionId);
		if (rentalShoppingCart != null)
		{
			IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = rentalShoppingCart.TitleItems.FirstOrDefault((IRentalShoppingCartTitleItem x) => x.ProductId == productId);
			if (rentalShoppingCartTitleItem != null)
			{
				RemoveItemFromRentalShoppingCart(rentalShoppingCart, rentalShoppingCartTitleItem, userRemoved);
				return;
			}
			LogHelper.Instance.Log("Error in RentalShoppingCartService.RemoveItemFromRentalShoppingCartByProductId.  Unable to find rental shopping cart item with product id {0}", productId);
		}
		else
		{
			LogHelper.Instance.Log("Error in RentalShoppingCartService.RemoveItemFromRentalShoppingCartByProductId.  Unable to find rental shopping cart with id: {0}", shoppingCartSessionId);
		}
	}

	private void RemoveItemFromRentalShoppingCart(IRentalShoppingCart rentalShoppingCart, IBaseRentalShoppingCartItem rentalShoppingCartItem, bool userRemoved)
	{
		if (rentalShoppingCart != null && rentalShoppingCartItem != null)
		{
			rentalShoppingCart.RemoveItem(rentalShoppingCartItem, userRemoved);
			rentalShoppingCart.ValidateAndUpdatePromoCode();
		}
	}

	public void SendRentalShoppingCartToAnalytics(IRentalShoppingCart rentalShoppingCart)
	{
		LogHelper.Instance.Log("Gather Up Cart Data for Analytics");
		if (!rentalShoppingCart.PickupRecommendationsShown)
		{
			_ = rentalShoppingCart.Totals.SubTotal;
			_ = rentalShoppingCart.Totals.TaxAmount;
			_ = rentalShoppingCart.Totals.GrandTotal;
		}
		ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddShoppingCart(rentalShoppingCart);
	}

	public List<string> AssignBarcodesToVend(IRentalShoppingCart rentalShoppingCart)
	{
		LogHelper.Instance.Log("RentalShoppingCartService.AssignBarcodesToVend");
		List<string> list = new List<string>();
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		List<ISearchBarcodeForProduct> list2 = new List<ISearchBarcodeForProduct>();
		if (rentalShoppingCart != null)
		{
			foreach (IRentalShoppingCartTitleItem titleItem in rentalShoppingCart.TitleItems)
			{
				list2.Add(new SearchBarcodeForProduct
				{
					ProductId = titleItem.ProductId,
					Barcode = titleItem.Barcode,
					Action = titleItem.Action
				});
			}
			List<IBarcodeForProductResult> list3 = service?.GetBarcodesForProducts(list2, null);
			if (list3 != null)
			{
				foreach (IBarcodeForProductResult eachBarcodeForProductResult in list3)
				{
					if (eachBarcodeForProductResult.BarcodeAssigned)
					{
						IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = rentalShoppingCart.TitleItems.FirstOrDefault((IRentalShoppingCartTitleItem x) => x.ProductId == eachBarcodeForProductResult.ProductId && x.Action == eachBarcodeForProductResult.Action);
						if (rentalShoppingCartTitleItem != null)
						{
							rentalShoppingCartTitleItem.Barcode = eachBarcodeForProductResult.Barcode;
							LogHelper.Instance.Log($"Assigning barcode {eachBarcodeForProductResult.Barcode} to rental shopping cart item with product {eachBarcodeForProductResult.ProductId} and action {eachBarcodeForProductResult.Action}");
							if (eachBarcodeForProductResult.Status.HasValue)
							{
								rentalShoppingCartTitleItem.Status = eachBarcodeForProductResult.Status.Value;
							}
						}
					}
					if (!string.IsNullOrEmpty(eachBarcodeForProductResult.Barcode))
					{
						list.Add(eachBarcodeForProductResult.Barcode);
					}
				}
			}
		}
		return list;
	}

	private IShoppingSession GetCurrentShoppingSession()
	{
		return ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetCurrentSession();
	}

	public void AddMultiNightOfferToItem(IRentalShoppingCartTitleItem rentalShoppingCartItem, CustomerOffer selectedOffer)
	{
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.MarketingGeneral.UseDiscountsForPmnpOffers)
		{
			LogHelper.Instance.Log("The UseDiscountsForPmnpOffers config setting is true, creating a discount for the item");
			AddMultiNightDiscountPromo(rentalShoppingCartItem, selectedOffer);
		}
		else
		{
			LogHelper.Instance.Log("The UseDiscountsForPmnpOffers config setting is false, setting the mnp price for the item");
			SetMultiNightPriceForOffer(rentalShoppingCartItem, selectedOffer);
		}
	}

	private void AddMultiNightDiscountPromo(IRentalShoppingCartTitleItem rentalShoppingCartItem, CustomerOffer selectedOffer)
	{
		IPromotion promotion = ServiceLocator.Instance.GetService<IPromotionService>().GetPromotion();
		if (promotion == null)
		{
			LogHelper.Instance.LogError("", "Unable to add Multi Night Offer to item when no Promotion has been assigned.");
			return;
		}
		OfferProduct offerProd = selectedOffer.OfferProducts.FirstOrDefault((OfferProduct o) => o.ProductId == rentalShoppingCartItem.ProductId);
		if (offerProd != null && offerProd.NumberOfNights.HasValue && offerProd.DiscountedPrice.HasValue && !promotion.PromotionItems.Any((IPromotionItem d) => d.RentalShoppingCartItem.ProductId == offerProd.ProductId))
		{
			rentalShoppingCartItem.PricingRecord.InitialDays = offerProd.NumberOfNights;
			decimal num = rentalShoppingCartItem.PricingRecord.DefaultInitialNightPrice.Value + rentalShoppingCartItem.PricingRecord.ExtraNight * (decimal)(offerProd.NumberOfNights.Value - 1);
			rentalShoppingCartItem.PricingRecord.InitialNight = num;
			rentalShoppingCartItem.Price = num;
			promotion.PromotionItems.Add(new PromotionItem
			{
				RentalShoppingCartItem = rentalShoppingCartItem,
				PromotionAmount = rentalShoppingCartItem.PricingRecord.ExtraNight * (decimal)(offerProd.NumberOfNights.Value - 1) - offerProd.DiscountedPrice.Value
			});
			promotion.Price = promotion.PromotionItems.Sum((IPromotionItem x) => x.PromotionAmount);
		}
	}

	private void SetMultiNightPriceForOffer(IRentalShoppingCartTitleItem rentalShoppingCartItem, CustomerOffer selectedOffer)
	{
		OfferProduct offerProduct = selectedOffer?.OfferProducts?.FirstOrDefault((OfferProduct o) => o.ProductId == rentalShoppingCartItem.ProductId);
		if (offerProduct != null && offerProduct.NumberOfNights.HasValue && offerProduct.DiscountedPrice.HasValue)
		{
			IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = rentalShoppingCartItem;
			if (rentalShoppingCartTitleItem != null && rentalShoppingCartTitleItem.PricingRecord?.InitialDays == 1)
			{
				rentalShoppingCartItem.PricingRecord.InitialDays = offerProduct.NumberOfNights;
				decimal num = rentalShoppingCartItem.PricingRecord.DefaultInitialNightPrice.Value + offerProduct.DiscountedPrice.Value * (decimal)(offerProduct.NumberOfNights.Value - 1);
				rentalShoppingCartItem.PricingRecord.InitialNight = num;
				rentalShoppingCartItem.Price = num;
			}
		}
	}

	public void RemoveMultiNightOfferFromItem(IRentalShoppingCartTitleItem rentalShoppingCartItem)
	{
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.MarketingGeneral.UseDiscountsForPmnpOffers)
		{
			RemoveMultiNightDiscountPromo(rentalShoppingCartItem);
		}
		else
		{
			RemoveMultiNightPriceForOffer(rentalShoppingCartItem);
		}
	}

	private void RemoveMultiNightDiscountPromo(IRentalShoppingCartTitleItem rentalShoppingCartItem)
	{
		IPromotion promotion = ServiceLocator.Instance.GetService<IPromotionService>().GetPromotion();
		if (promotion == null)
		{
			return;
		}
		rentalShoppingCartItem.PricingRecord.InitialDays = 1;
		rentalShoppingCartItem.PricingRecord.InitialNight = rentalShoppingCartItem.PricingRecord.DefaultInitialNightPrice.Value;
		rentalShoppingCartItem.Price = rentalShoppingCartItem.PricingRecord.InitialNight;
		IPromotionItem promotionItem = promotion.PromotionItems.FirstOrDefault((IPromotionItem d) => d.RentalShoppingCartItem.ProductId == rentalShoppingCartItem.ProductId);
		if (promotionItem != null)
		{
			promotion.PromotionItems.Remove(promotionItem);
			promotion.Price = promotion.PromotionItems.Sum((IPromotionItem x) => x.PromotionAmount);
		}
	}

	private void RemoveMultiNightPriceForOffer(IRentalShoppingCartTitleItem rentalShoppingCartItem)
	{
		rentalShoppingCartItem.PricingRecord.InitialDays = 1;
		rentalShoppingCartItem.PricingRecord.InitialNight = rentalShoppingCartItem.PricingRecord.DefaultInitialNightPrice.Value;
		rentalShoppingCartItem.Price = rentalShoppingCartItem.PricingRecord.InitialNight;
	}

	public void SetPurchasePriceForSelectedItem(CustomerOffer selectedOffer, IRentalShoppingCartTitleItem newLineItem, IPromotionService promotionService)
	{
		decimal num = promotionService.CalcInCartOfferPromoAmount(newLineItem, selectedOffer);
		newLineItem.PricingRecord.Purchase -= num;
		newLineItem.Price = newLineItem.PricingRecord.Purchase;
	}

	private RentalShoppingCartService()
	{
	}
}
