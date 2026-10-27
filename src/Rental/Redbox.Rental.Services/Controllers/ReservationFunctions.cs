using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Redbox.BrokerServices.Proxy.ComponentModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.KioskEngine.Environment.CardDataReader;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Reservation;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Loyalty;
using Redbox.Rental.Services.Promotion;
using Redbox.Rental.Services.Session;
using Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects;

namespace Redbox.Rental.Services.Controllers;

public class ReservationFunctions
{
	public static class Constants
	{
		public const string ReservationsDownloadedFlag = "deddie";

		public const string Reservations = "reservations";

		public const string OnlinePickup = "onlinePickup";

		public const string CartItem = "cartItem";

		public const string Barcode = "barcode";

		public const string MultiDisc = "multi_disc";

		public const string Status = "status";

		public const string Known = "Known";

		public const string Empty = "empty";

		public const string Reserved = "Reserved";
	}

	private class ProductItem : IProductItem
	{
		public string Barcode { get; set; }

		public bool MultiDisc { get; set; }

		public LineItemStatus Status { get; set; }
	}

	public enum ReservationCancelResult
	{
		ReservationCancelled,
		ReservationNotFound,
		Error
	}

	private static Func<string> _getStoreReturnTimeCallback;

	private static Action<List<string>, string> _updateBarcodeStatusCallback;

	private static Action<string, string> _vendOnlyReservedTitlesCallback;

	private static Action<string, string> _reservationAuthorizeCallback;

	private static Action<string, bool> _authorizeOnPickupCallback;

	private static Action _zeroTouchContinueCallback;

	private static readonly object _reservationSyncObject = new object();

	public static int GetRecommendedProductCount(List<IReservation> reservations)
	{
		return GetRecommendedProducts(reservations).Count();
	}

	public static List<string> GetReservedBarcodes()
	{
		List<string> list = new List<string>();
		List<IReservation> allReservations = GetAllReservations();
		if (allReservations != null)
		{
			foreach (IReservation item in allReservations)
			{
				foreach (IReservedTitle reservedTitle in item.ReservationDetails.ReservedTitles)
				{
					if (!string.IsNullOrEmpty(reservedTitle.Barcode))
					{
						list.Add(reservedTitle.Barcode);
					}
				}
			}
		}
		return list;
	}

	public static int GetReservedBarcodesCount()
	{
		int num = 0;
		List<IReservation> allReservations = GetAllReservations();
		if (allReservations != null)
		{
			foreach (IReservation item in allReservations)
			{
				foreach (IReservedTitle reservedTitle in item.ReservationDetails.ReservedTitles)
				{
					if (!string.IsNullOrEmpty(reservedTitle.Barcode))
					{
						num++;
					}
				}
			}
		}
		return num;
	}

	public static bool IsBarcodeReserved(string barcode)
	{
		bool result = false;
		List<IReservation> allReservations = GetAllReservations();
		if (allReservations != null)
		{
			foreach (IReservation item in allReservations)
			{
				foreach (IReservedTitle reservedTitle in item.ReservationDetails.ReservedTitles)
				{
					if (reservedTitle.Barcode == barcode)
					{
						result = true;
						break;
					}
				}
			}
		}
		return result;
	}

	public static List<IReservation> GetAllReservations()
	{
		return ServiceLocator.Instance.GetService<IDataService>()?.GetAllReservations() ?? new List<IReservation>();
	}

	private static string ObfuscateHashedCardId(string hashedCardId)
	{
		return hashedCardId.Substring(0, Math.Min(5, hashedCardId.Length));
	}

	public static void ExpireReservations(List<IReservation> reservations = null)
	{
		DateTime dateTime = DateTime.Parse(GetStoreReturnTime(), new CultureInfo("en-US"));
		LogHelper.Instance.Log($"Reservation expire running for time {dateTime}");
		if (DateTime.Now < dateTime)
		{
			dateTime = dateTime.AddDays(-1.0);
			LogHelper.Instance.Log("Setting expire time back a day since current time is before expire time");
		}
		dateTime = dateTime.Date;
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (reservations == null)
		{
			reservations = service?.GetAllReservations();
		}
		List<IReservationDetails> list = new List<IReservationDetails>();
		foreach (IReservation reservation in reservations)
		{
			if (!(reservation.HashedCardId != "deddie"))
			{
				continue;
			}
			if (reservation.ReservationDetails.TransactionDate < dateTime)
			{
				LogHelper.Instance.Log("Expiring Reference Number " + reservation.ReservationDetails?.ReferenceNumber);
				ServiceLocator.Instance.GetService<IReservationServices>();
				if (long.TryParse(reservation.ReservationDetails?.ReferenceNumber, out var result))
				{
					ServiceLocator.Instance.GetService<IKioskClientService>().Transaction.ExpireReservation(service?.GetStore()?.StoreId.ToString(), result, null);
				}
				list.Add(reservation.ReservationDetails);
			}
			else
			{
				LogHelper.Instance.Log("Reservation has not expired. Reference Number: " + reservation.ReservationDetails?.ReferenceNumber);
			}
		}
		DeleteReservationDetails(list, reservations);
		foreach (IReservationDetails resDetail in list)
		{
			reservations.Remove(reservations.Where((IReservation r) => r.ReferenceNumber == resDetail.ReferenceNumber).FirstOrDefault());
		}
	}

	public static bool IsAuthorizeAtPickup(string referenceNumber, List<IReservation> reservations = null)
	{
		return PerformIsAuthorizeAtPickup(referenceNumber, reservations);
	}

	public static bool IsOnlyAuthorizeAtPickup(List<IReservation> reservations = null)
	{
		return PerformIsAuthorizeAtPickup(null, reservations, evaluateAll: true);
	}

	private static bool PerformIsAuthorizeAtPickup(string referenceNumber, List<IReservation> reservations = null, bool evaluateAll = false)
	{
		bool result = false;
		if (reservations == null)
		{
			reservations = GetAccountReservations();
		}
		foreach (IReservation reservation in reservations)
		{
			if ((referenceNumber == null || reservation.ReferenceNumber == referenceNumber) && reservation.ReservationDetails.AuthorizeAtPickup)
			{
				result = true;
			}
			else if (evaluateAll)
			{
				return false;
			}
		}
		return result;
	}

	private static void DeleteReservationDetails(List<IReservationDetails> reservationDetailsToDelete, List<IReservation> reservations = null)
	{
		if ((reservationDetailsToDelete?.Count ?? 0) <= 0)
		{
			return;
		}
		List<string> barcodeList = new List<string>();
		if (reservations == null)
		{
			reservations = ServiceLocator.Instance.GetService<IDataService>()?.GetAllReservations();
		}
		foreach (IReservationDetails eachReservationDetailToDelete in reservationDetailsToDelete)
		{
			IReservation reservation = reservations.FirstOrDefault((IReservation x) => x.ReferenceNumber == eachReservationDetailToDelete.ReferenceNumber);
			IReservationDetails reservationDetails = reservation?.ReservationDetails;
			if (reservation != null && reservationDetails != null)
			{
				reservationDetails.ReservedTitles.ForEach(delegate(IReservedTitle x)
				{
					barcodeList.Add(x.Barcode);
				});
				DeleteReservation(reservation.ReferenceNumber);
			}
		}
		UpdateBarcodeStatus(barcodeList, "Known");
	}

	public static void SyncReservations()
	{
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (service != null && service.GetReservations("deddie") == null)
		{
			LogHelper.Instance.Log("ReservationFunctions.SyncReservations: downloading reservations from server.");
			ServiceLocator.Instance.GetService<IInventoryService>();
			ServiceLocator.Instance.GetService<IReservationServices>();
			UpdateReservation(new Reservation
			{
				HashedCardId = "deddie",
				Id = Guid.NewGuid()
			});
		}
		else
		{
			LogHelper.Instance.Log("ReservationFunctions.SyncReservations: reservations already downloaded from server.");
		}
	}

	public static void RegisterZeroTouchPickupCallbacks(Action continueReservation)
	{
		_zeroTouchContinueCallback = continueReservation;
	}

	public static void InvokeZeroTouchContinueReservation()
	{
		_zeroTouchContinueCallback?.Invoke();
	}

	public static void SetValidatedReservations(List<string> validatedRefNumbers)
	{
		List<IReservation> list = new List<IReservation>();
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		foreach (string validatedRefNumber in validatedRefNumbers)
		{
			IReservation reservation = service.GetReservation(validatedRefNumber);
			if (reservation != null)
			{
				list.Add(reservation);
			}
		}
		Redbox.Rental.Services.Session.Session.Instance.ValidatedReservations = list;
	}

	public static void InitializeExpireReservationTimer()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		ITimer timer = service.GetTimer("reservation-expire-timer");
		if (timer == null)
		{
			timer = service.CreateTimer("reservation-expire-timer", 30000, 7200000, delegate
			{
				ExpireReservations();
			});
		}
		timer.Start();
	}

	public static string GetStoreReturnTime()
	{
		return _getStoreReturnTimeCallback?.Invoke();
	}

	public static void RegisterGetStoreReturnTimeCallback(Func<string> getStoreReturnTimeCallback)
	{
		_getStoreReturnTimeCallback = getStoreReturnTimeCallback;
	}

	public static void UpdateBarcodeStatus(List<string> barcodes, string status)
	{
		if (barcodes != null && barcodes.Count > 0)
		{
			_updateBarcodeStatusCallback?.Invoke(barcodes, status);
		}
	}

	public static void AddRecommendedProductsToAnalytics()
	{
		List<IKioskProduct> recommendedProducts = GetRecommendedProducts();
		recommendedProducts.Select((IKioskProduct x) => (x as ITitleProduct)?.ProductId.ToString()).Join(",");
		IRentalShoppingCart obj = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
		IAnalyticsService service = ServiceLocator.Instance.GetService<IAnalyticsService>();
		if (obj == null || service == null)
		{
			return;
		}
		List<ProductData> products = new List<ProductData>();
		recommendedProducts.ForEach(delegate(IKioskProduct p)
		{
			ITitleProduct titleProduct = p as ITitleProduct;
			if (p != null)
			{
				products.Add(ProductData.ToProductData(titleProduct));
			}
		});
		service?.CurrentSession?.AddData(new ROPRecommendedProducts
		{
			RecommendedProducts = products,
			RecommendationsFlow = true
		});
	}

	public static List<IKioskProduct> GetRecommendedProducts(List<IReservation> reservations = null)
	{
		LogHelper.Instance.Log("ReservationFunctions.GetRecommendedProducts", LogEntryType.Info);
		if (reservations == null)
		{
			reservations = GetAccountReservations();
		}
		new List<IKioskProduct>();
		List<long> excludeProducts = new List<long>();
		reservations.ForEach(delegate(IReservation x)
		{
			x.ReservationDetails.HistoryTitleIds.ForEach(delegate(int t)
			{
				excludeProducts.Add(t);
			});
		});
		bool flag = false;
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		foreach (string item in GetReservedBarcodesToVend(reservations))
		{
			service.GetProductByBarcode(item, out var kioskProduct);
			if (kioskProduct is ITitleProduct titleProduct)
			{
				excludeProducts.Add(titleProduct.ProductId);
				if (titleProduct.TitleType == TitleType.Bluray)
				{
					flag = true;
				}
			}
			else
			{
				LogHelper.Instance.Log("ReservationFunctions.GetRecommendedProducts unable to find product using barcode: " + item, LogEntryType.Info);
			}
		}
		LogHelper.Instance.Log(string.Format("ReservationFunctions.GetRecommendedProducts GenerateRecommendations({0}, {1})", flag, string.Join(",", excludeProducts)), LogEntryType.Info);
		return RecommendationOnPickupViewController.GenerateRecommendations(flag, excludeProducts);
	}

	private static IReservationDetails FindReservationDetailsWithDuplicateTitles(IReservationDetails searchForReservationDetails, List<IReservation> reservations)
	{
		IReservationDetails result = null;
		foreach (IReservation reservation in reservations)
		{
			if (searchForReservationDetails.ReferenceNumber != reservation.ReservationDetails.ReferenceNumber && reservation.ReservationDetails.ReservedTitles.Count == searchForReservationDetails.ReservedTitles.Count && reservation.ReservationDetails.ReservedTitles.All((IReservedTitle x) => searchForReservationDetails.ReservedTitles.Any((IReservedTitle y) => x.TitleId == y.TitleId)))
			{
				result = reservation.ReservationDetails;
			}
		}
		return result;
	}

	public static List<string> CheckForDuplicateReservations()
	{
		List<string> list = new List<string>();
		LogHelper.Instance.Log("ReservationFunctions.CheckForDuplicateReservations called.", LogEntryType.Info);
		List<IReservation> accountReservations = GetAccountReservations();
		foreach (IReservation item in accountReservations)
		{
			if (list.Contains(item.ReferenceNumber))
			{
				continue;
			}
			IReservationDetails reservationDetails = FindReservationDetailsWithDuplicateTitles(item.ReservationDetails, accountReservations);
			if (reservationDetails == null)
			{
				continue;
			}
			IReservationDetails reservationDetails2 = ((reservationDetails.TransactionDate < item.ReservationDetails.TransactionDate) ? reservationDetails : item.ReservationDetails);
			if (!list.Contains(reservationDetails2.ReferenceNumber))
			{
				string text = reservationDetails?.ReservedTitles.Select((IReservedTitle x) => x.TitleId).ToList().Join(", ");
				LogHelper.Instance.Log("ReservationFunctions.CheckForDuplicateReservations found duplicate reservations " + item?.ReferenceNumber + " and " + reservationDetails?.ReferenceNumber + " with titles " + text + ".  Removing duplicate reservation " + reservationDetails2?.ReferenceNumber + ".", LogEntryType.Info);
				list.Add(reservationDetails2.ReferenceNumber);
			}
		}
		return list;
	}

	public static void DeleteReservation(string referenceNumber)
	{
		ServiceLocator.Instance.GetService<IDataService>()?.DeleteReservation(referenceNumber);
		List<IReservation> validatedReservations = Redbox.Rental.Services.Session.Session.Instance.ValidatedReservations;
		if (validatedReservations != null)
		{
			IReservation reservation = validatedReservations.FirstOrDefault((IReservation r) => r.ReferenceNumber == referenceNumber);
			if (reservation != null)
			{
				Redbox.Rental.Services.Session.Session.Instance.ValidatedReservations.Remove(reservation);
			}
		}
	}

	public static bool UpdateReservation(IReservation reservation)
	{
		LogHelper.Instance.Log("ReservationFunctions.UpdateReservation " + ObfuscateHashedCardId(reservation?.HashedCardId));
		return ServiceLocator.Instance.GetService<IDataService>()?.SaveReservation(reservation) ?? false;
	}

	public static void RegisterUpdateBarcodeStatusCallback(Action<List<string>, string> updateBarcodeStatusCallback)
	{
		_updateBarcodeStatusCallback = updateBarcodeStatusCallback;
	}

	public static IReservationDetails PopulateShoppingCartWithReservationItems(IRentalShoppingCart rentalShoppingCart, string referenceNumber = null, bool clearCart = true)
	{
		IReservationDetails reservationDetails = null;
		try
		{
			IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
			GetAccountReservations().ForEach(delegate(IReservation y)
			{
				if (referenceNumber == null || y.ReferenceNumber == referenceNumber)
				{
					reservationDetails = y.ReservationDetails;
				}
			});
			if (reservationDetails != null)
			{
				if (clearCart)
				{
					rentalShoppingCart.Clear();
					PromotionService.Instance.ClearPromotion();
					rentalShoppingCart.PickupRecommendationsShown = false;
				}
				rentalShoppingCart?.SetDefaultServiceFee(reservationDetails.DefaultServiceFee);
				LogHelper.Instance.Log($"Setting cart default service fee to {reservationDetails.DefaultServiceFee} from reservation.");
				decimal num = 0m;
				decimal value = 0m;
				List<IPromotionItem> list = new List<IPromotionItem>();
				RentalShoppingCartItemAction rentalShoppingCartItemAction = RentalShoppingCartItemAction.rental;
				IDataService service2 = ServiceLocator.Instance.GetService<IDataService>();
				foreach (IReservedTitle reservedTitle2 in reservationDetails.ReservedTitles)
				{
					string barcode = reservedTitle2.Barcode;
					ErrorList product = service2.GetProduct(reservedTitle2.TitleId, excludeTitleRollup: false, out var kioskProduct);
					if (product == null || product.Count == 0)
					{
						if (kioskProduct == null)
						{
							LogHelper.Instance.Log("Unable to find kiosk product using dataService.GetProductByBarcode( " + barcode + " )");
							Inventory inventory = ServiceLocator.Instance.GetService<IInventoryService>()?.GetInventory(barcode);
							if (inventory != null)
							{
								LogHelper.Instance.Log($"Found product id {inventory.TitleId} for barcode {barcode}");
								service2.GetProduct(inventory.TitleId, excludeTitleRollup: true, out kioskProduct);
								LogHelper.Instance.Log(string.Format("kiosk product for product id {0} {1}found", inventory.TitleId, (kioskProduct == null) ? "not " : ""));
							}
							else
							{
								LogHelper.Instance.Log("Unable to find product id from Inventory Service.");
							}
						}
						ITitleProduct titleProduct = kioskProduct as ITitleProduct;
						if (titleProduct != null)
						{
							LogHelper.Instance.Log("Found titleProduct for barcode " + barcode);
							IReservedTitle reservedTitle = reservationDetails.ReservedTitles.FirstOrDefault((IReservedTitle x) => x.TitleId == titleProduct.ProductId);
							if (reservedTitle != null)
							{
								LogHelper.Instance.Log("Found reserved title for barcode " + barcode);
								IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = service?.CreateRentalShoppingCartItemFromTitleProduct(titleProduct);
								rentalShoppingCartTitleItem.SourceType = ItemSourceType.Reservation;
								rentalShoppingCartTitleItem.Barcode = barcode;
								rentalShoppingCartTitleItem.Action = reservedTitle.Action;
								rentalShoppingCartTitleItem.Quantity = 1m;
								rentalShoppingCartTitleItem.Status = LineItemStatus.Pending;
								rentalShoppingCartTitleItem.OfferCode = PersonalizationFunctions.GetLoyaltyOfferCode(rentalShoppingCartTitleItem.TitleProduct);
								rentalShoppingCartTitleItem.TivoQueryId = PersonalizationFunctions.GetTivoQueryId(rentalShoppingCartTitleItem.TitleProduct) ?? PersonalizationFunctions.GetTivoQueryIdFromTransactionEntities(rentalShoppingCartTitleItem.TitleProduct);
								rentalShoppingCartTitleItem.PersonalizationTitleTags = PersonalizationFunctions.GetPersonalizationTitleTags(rentalShoppingCartTitleItem.TitleProduct);
								LogHelper.Instance.Log($"Checking discounts for reserved title Id {titleProduct.ProductId}");
								if (reservedTitle.DiscountType == ReservationDiscountType.Loyalty)
								{
									rentalShoppingCartTitleItem.LoyaltyPointsRecord = new LoyaltyPointsRecord(rentalShoppingCartTitleItem)
									{
										ToBeRedeemed = true,
										PointsToRedeem = (int)reservedTitle.LoyaltyPoints
									};
								}
								else if (reservedTitle.DiscountType == ReservationDiscountType.Promotion && reservedTitle.Discount != 0m)
								{
									list.Add(new PromotionItem
									{
										PromotionAmount = Math.Abs(reservedTitle.Discount),
										RentalShoppingCartItem = rentalShoppingCartTitleItem
									});
									value += reservedTitle.Discount;
									rentalShoppingCartItemAction = reservedTitle.Action;
								}
								LogHelper.Instance.Log($"Getting pricing record for reserved title Id {titleProduct.ProductId}");
								IPricingRecord pricingRecord = reservationDetails.PricingSet?.PriceRecords.FirstOrDefault((IPricingRecord x) => (titleProduct.TitleFamily == TitleFamily.Movies && titleProduct.TitleType == TitleType.DVD && x.TitleType == TitleType.DVD && x.TitleFamily == TitleFamily.Movies) || (titleProduct.TitleFamily == TitleFamily.Movies && titleProduct.TitleType == TitleType.Bluray && x.TitleType == TitleType.Bluray && x.TitleFamily == TitleFamily.Movies) || (titleProduct.TitleFamily == TitleFamily.Movies && titleProduct.TitleType == TitleType._4KUHD && x.TitleType == TitleType._4KUHD && x.TitleFamily == TitleFamily.Movies) || (titleProduct.TitleFamily == TitleFamily.Games && x.TitleType == TitleType.All && x.TitleFamily == TitleFamily.Games));
								if (pricingRecord != null)
								{
									LogHelper.Instance.Log($"Found pricing record for reserved title Id {titleProduct.ProductId}");
									pricingRecord = pricingRecord.Clone();
									if (reservedTitle.Action == RentalShoppingCartItemAction.rental)
									{
										pricingRecord.InitialNight = reservedTitle.Price;
									}
									else
									{
										pricingRecord.Purchase = reservedTitle.Price;
									}
								}
								else
								{
									LogHelper.Instance.Log("ReservationFunctions.PopulateShoppingCartWithReservationItems - barcode " + barcode + " couldn't find a pricing record.", LogEntryType.Info);
								}
								if (reservationDetails.AppliedTitleMarketing && reservedTitle.DiscountType == ReservationDiscountType.Promotion)
								{
									num += reservedTitle.Discount;
								}
								if (!rentalShoppingCart.HasTitleItem(titleProduct.ProductId))
								{
									LogHelper.Instance.Log($"Adding reserved product {rentalShoppingCartTitleItem.ProductId} to recommendation on pickup cart");
									rentalShoppingCart.AddTitleItem(rentalShoppingCartTitleItem, pricingRecord);
								}
							}
							else
							{
								LogHelper.Instance.Log("ReservationFunctions.PopulateShoppingCartWithReservationItems - reservation title barcode " + barcode + " couldn't find a product.", LogEntryType.Info);
							}
						}
						else
						{
							LogHelper.Instance.Log("ReservationFunctions.PopulateShoppingCartWithReservationItems - barcode " + barcode + " couldn't find a product.", LogEntryType.Info);
						}
					}
					else
					{
						LogHelper.Instance.Log($"ReservationFunctions.PopulateShoppingCartWithReservationItems - dataService.GetProductByBarcode returned errors: {product}", LogEntryType.Info);
					}
				}
				decimal num2 = 0m;
				if (reservationDetails.AppliedHiveOnlinePromo)
				{
					num2 = reservationDetails.HiveOnlinePromoDiscount;
					LogHelper.Instance.Log($"ReservationFunctions:- applied hive online promo, reserved discount amount: {num2}");
				}
				else if (reservationDetails.AppliedTitleMarketing)
				{
					num2 = num;
					LogHelper.Instance.Log($"ReservationFunctions:- applied title marketing promo, reserved discount amount: {num2}");
				}
				num2 = Math.Abs(num2);
				value = Math.Abs(value);
				if (num2 > 0m)
				{
					IPromotion promotion = new Redbox.Rental.Services.Promotion.Promotion
					{
						Name = reservationDetails.PromoCode,
						OriginalPrice = num2,
						Price = num2,
						Quantity = 1,
						Intent = PromotionIntentCode.FixedValue,
						OriginalPercent = 0m,
						Percent = 0m,
						RentQuantity = 0,
						RentFormat = null,
						GetQuantity = 0,
						GetFormat = null,
						CampaignTitles = null,
						ProductTypeId = null,
						FormatIds = null,
						PromotionItems = list,
						ActionCode = ((rentalShoppingCartItemAction != RentalShoppingCartItemAction.purchase) ? PromotionActionCode.Rental : PromotionActionCode.Purchase)
					};
					PromotionService.Instance.SetPromotion(promotion);
					if (value != num2)
					{
						service.CurrentRentalShoppingCart.ValidateAndUpdatePromoCode();
					}
					service.CurrentRentalShoppingCart.UpdateServiceFee();
				}
			}
			else
			{
				LogHelper.Instance.Log("ReservationFunctions.PopulateShoppingCartWithReservationItems reservationDetails is null.", LogEntryType.Info);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Exception building cart from reservation", e);
		}
		return reservationDetails;
	}

	public static bool IsQualifiedForRecommendationOnPickup()
	{
		bool flag = false;
		List<IReservation> accountReservations = GetAccountReservations();
		int count = accountReservations.Count;
		bool flag2 = accountReservations.Any((IReservation x) => x.ReservationDetails.IsGiftCard);
		bool flag3 = accountReservations.Any((IReservation x) => x.ReservationDetails.IsMultiNightPrice);
		bool flag4 = accountReservations.Any((IReservation x) => x.ReservationDetails.IsMultiDiscVend);
		bool flag5 = accountReservations.Any((IReservation x) => x.ReservationDetails.IsLoyaltyRedemption);
		bool flag6 = accountReservations.Any((IReservation x) => x.ReservationDetails.ReservedTitles.Any((IReservedTitle w) => w.Action == RentalShoppingCartItemAction.purchase));
		bool flag7 = OrchestrationService.IsGiftCard();
		bool isInZeroTouchMode = Redbox.Rental.Services.Session.Session.Instance.IsInZeroTouchMode;
		bool flag8 = accountReservations.All((IReservation x) => x.ReservationDetails.AuthorizeAtPickup);
		List<string> reservedBarcodesToVend = GetReservedBarcodesToVend(accountReservations);
		LogHelper.Instance.Log($"IsQualifiedForRecommendationOnPickup() - num_reservations= {reservedBarcodesToVend.Count} num_reserved_titles= {count} is_RAP={flag8}");
		LogHelper.Instance.Log($"IsQualifiedForRecommendationOnPickup() part 2 - is_gift_card={flag7}   is_gc: {flag2}   is_mnp: {flag3}   is_mdv= {flag4}");
		LogHelper.Instance.Log($"IsQualifiedForRecommendationOnPickup() part 3 - is_loyalty_redemption= {flag5}  has_purchase= {flag6}  is_zero_touch= {isInZeroTouchMode}");
		if (((count == 1 && reservedBarcodesToVend.Count < 5) & flag8) && !flag7 && !isInZeroTouchMode && !flag2 && !flag3 && !flag4 && !flag5 && !flag6 && GetRecommendedProductCount(accountReservations) > 2)
		{
			flag = true;
		}
		LogHelper.Instance.Log($"IsQualifiedForRecommendationOnPickup() qualifies: {flag}");
		return flag;
	}

	public static List<string> GetReservedBarcodesToVend(List<IReservation> reservations = null)
	{
		List<string> result = new List<string>();
		if (reservations == null)
		{
			reservations = GetAccountReservations();
		}
		LogHelper.Instance.Log($"ReservationFunctions.GetBarcodesToVend - reservation count: {reservations?.Count ?? 0}");
		if (reservations != null)
		{
			foreach (IReservation reservation in reservations)
			{
				reservation.ReservationDetails.ReservedTitles.ForEach(delegate(IReservedTitle x)
				{
					result.Add(x.Barcode);
					LogHelper.Instance.Log("ReservationFunctions.GetBarcodesToVend - Barcode: " + x.Barcode);
				});
			}
		}
		LogHelper.Instance.Log($"ReservationFunctions.GetBarcodesToVend - barcode count: {result.Count}");
		return result;
	}

	public static List<IReservation> GetAccountReservations(bool runExpire = false)
	{
		List<IReservation> list = new List<IReservation>();
		if (Redbox.Rental.Services.Session.Session.Instance.ValidatedReservations == null)
		{
			ITrackData trackData = Redbox.Rental.Services.Session.Session.Instance?.TrackData;
			if (trackData != null && trackData.HasValidData())
			{
				List<IReservation> allReservations = ServiceLocator.Instance.GetService<IDataService>()?.GetAllReservations();
				string cardHashId = trackData.CardHashId;
				string reservationHashId = trackData.ReservationHashId;
				list = GetReservationsForHashedCardId(cardHashId, reservationHashId, allReservations);
				if (runExpire)
				{
					ExpireReservations(list);
				}
				LogHelper.Instance.Log($"ReservationFunctions:GetAccountReservations - found: {list?.Count}");
			}
			else
			{
				LogHelper.Instance.Log("ReservationFunctions.GetAccountReservations - track data is not valid");
			}
		}
		else
		{
			list.AddRange(Redbox.Rental.Services.Session.Session.Instance.ValidatedReservations);
			LogHelper.Instance.Log($"ReservationFunctions.GetAccountReservation - returned {list.Count} validated reservations. ");
		}
		return list;
	}

	public static ReservationCancelResult CancelLocalReservation(string referenceNumber)
	{
		ReservationCancelResult result = ReservationCancelResult.ReservationCancelled;
		LogHelper.Instance.Log("ReservationFunctions:CancelReservation called for Reference Number: " + referenceNumber);
		lock (_reservationSyncObject)
		{
			IDataService service = ServiceLocator.Instance.GetService<IDataService>();
			List<IReservationDetails> list = new List<IReservationDetails>();
			IReservation reservation = service?.GetReservation(referenceNumber);
			if (reservation != null)
			{
				LogHelper.Instance.Log("ReservationFunctions.CancelReservation removed reservation for Reference Number " + referenceNumber + ":");
				list.Add(reservation.ReservationDetails);
			}
			if (list.Count == 0)
			{
				result = ReservationCancelResult.ReservationNotFound;
				LogHelper.Instance.Log("ReservationFunctions.CancelReservation - reservation not found for Reference Number: " + referenceNumber);
			}
			else
			{
				DeleteReservationDetails(list, service?.GetAllReservations());
			}
		}
		return result;
	}

	public static void OnPickupValidationFailure()
	{
		ProcessReservation("ReservationFunctions.OnPickupValidationFailure", LineItemStatus.CustomerDisabled, null, isFromReservationPrepositionVendComplete: false, commitShoppingCart: true, "Pickup Validation Failure.");
	}

	public static void ReservationPrepositionVendComplete(List<VendResult> vendResultList)
	{
		ProcessReservation("ReservationFunctions.ReservationPrepositionVendComplete", LineItemStatus.InventoryError, vendResultList, isFromReservationPrepositionVendComplete: true, commitShoppingCart: false, null);
	}

	private static void ProcessReservation(string logHeaderText, LineItemStatus defaultLineItemStatus, List<VendResult> vendResultList, bool isFromReservationPrepositionVendComplete, bool commitShoppingCart, string commitReason)
	{
		LogHelper.Instance.Log(logHeaderText + " Begin");
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		IInventoryService service2 = ServiceLocator.Instance.GetService<IInventoryService>();
		Dictionary<string, List<IProductItem>> dictionary = new Dictionary<string, List<IProductItem>>();
		List<string> list = new List<string>();
		if (commitShoppingCart)
		{
			OrchestrationService.ShoppingCartCommit(rentalShoppingCart?.ShoppingSessionId.ToString(), commitReason, shouldEndAndSave: false);
		}
		foreach (IReservation accountReservation in GetAccountReservations())
		{
			dictionary.Clear();
			foreach (IReservedTitle eachReservedTitle in accountReservation.ReservationDetails.ReservedTitles)
			{
				Inventory inventory = service2.GetInventory(eachReservedTitle.Barcode);
				IKioskProduct kioskProduct = null;
				service?.GetProduct(inventory?.TitleId ?? 0, excludeTitleRollup: false, out kioskProduct);
				MDVProduct mDVProduct = kioskProduct as MDVProduct;
				LineItemStatus result = defaultLineItemStatus;
				if (vendResultList != null)
				{
					Enum.TryParse<LineItemStatus>(vendResultList?.FirstOrDefault((VendResult x) => x.barcode == eachReservedTitle.Barcode)?.status, ignoreCase: true, out result);
				}
				IProductItem productItem = new ProductItem
				{
					Barcode = eachReservedTitle.Barcode,
					MultiDisc = (mDVProduct != null),
					Status = result
				};
				if (productItem.Status != LineItemStatus.Fullfilled)
				{
					LogHelper.Instance.Log("Adding " + productItem.Barcode + " to reset barcode list");
					list.Add(productItem.Barcode);
					if (productItem.Status == LineItemStatus.Pending || productItem.Status == LineItemStatus.ProductSelected)
					{
						productItem.Status = LineItemStatus.NotFullfilled;
					}
				}
				string key = eachReservedTitle.Action.ToString();
				if (!dictionary.TryGetValue(key, out var value))
				{
					value = (dictionary[key] = new List<IProductItem>());
				}
				value.Add(productItem);
			}
			LogHelper.Instance.Log(logHeaderText + " results for reservation " + accountReservation?.ReferenceNumber);
			foreach (string key2 in dictionary.Keys)
			{
				LogHelper.Instance.Log(logHeaderText + " results for group " + key2);
				foreach (IProductItem item in dictionary[key2])
				{
					LogHelper.Instance.Log($"{logHeaderText} group_result  barcode: {item?.Barcode}  multi_disc: {item?.MultiDisc}  status: {item?.Status}");
				}
			}
			if (dictionary.Count == 0)
			{
				LogHelper.Instance.Log("Reservation " + accountReservation?.ReferenceNumber + " had 0 barcodes, skipping Pickup()/PickupReconcile() call");
				continue;
			}
			long.TryParse(accountReservation?.ReferenceNumber, out var result2);
			IDictionary<string, ReadOnlyCollection<IProductItem>> dictionary2 = new Dictionary<string, ReadOnlyCollection<IProductItem>>();
			foreach (string key3 in dictionary.Keys)
			{
				dictionary2[key3] = new ReadOnlyCollection<IProductItem>(dictionary[key3]);
			}
			if (rentalShoppingCart != null && !rentalShoppingCart.PickupRecommendationsShown)
			{
				LogHelper.Instance.Log(logHeaderText + ": Calling PopulateShoppingCartWithReservationItems() for ref# " + accountReservation?.ReferenceNumber);
				PopulateShoppingCartWithReservationItems(rentalShoppingCart, accountReservation?.ReferenceNumber);
			}
			IKioskClientService service3 = ServiceLocator.Instance.GetService<IKioskClientService>();
			LogHelper.Instance.Log(logHeaderText + ": Calling KioskClientService.Transaction.ReservationPickup()");
			service3.Transaction.ReservationPickup(rentalShoppingCart.ShoppingSessionId.ToString(), Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.CustomerProfileNumber, accountReservation.ReservationDetails?.EmailAddress, result2, dictionary2, null);
			LogHelper.Instance.Log(logHeaderText + ": Removing reservation for reference nuber... " + accountReservation.ReferenceNumber);
			DeleteReservation(accountReservation.ReferenceNumber);
		}
		if (list.Count > 0)
		{
			LogHelper.Instance.Log($"{logHeaderText}: resetting status to Known for {list.Count} discs");
			UpdateBarcodeStatus(list, "Known");
		}
		if (rentalShoppingCart != null)
		{
			OrchestrationService.CloseAnalytics(rentalShoppingCart.ShoppingSessionId, isSessionAbandoned: false);
		}
		LogHelper.Instance.Log(logHeaderText + " COMPLETE");
	}

	public static void RecommendationOnPickupPrepositionVendComplete(List<VendResult> vendResultList, string referenceNumber, bool modifiedCart)
	{
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
		if (rentalShoppingCart != null)
		{
			IShoppingSession shoppingSession = ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(rentalShoppingCart.ShoppingSessionId.ToString());
			if (shoppingSession != null)
			{
				UpdateRentalShoppingCartItemStatuses(vendResultList, rentalShoppingCart);
				List<string> list = new List<string>();
				ProcessRentalShoppingCartItems(rentalShoppingCart, list);
				List<IReservation> accountReservations = GetAccountReservations();
				CallMerchantService(referenceNumber, modifiedCart, rentalShoppingCart, shoppingSession, accountReservations);
				LogHelper.Instance.Log("ReservationFunctions.ReservationPrepositionVend: Removing reservations for reference number..." + referenceNumber);
				DeleteReservation(referenceNumber);
				if (list.Count > 0)
				{
					LogHelper.Instance.Log($"ReservationFunctions.ReservationPrepositionVend: resetting status to Known for {list.Count} discs");
					UpdateBarcodeStatus(list, "Known");
				}
				OrchestrationService.CloseShoppingCartAnalytics(rentalShoppingCart.ShoppingSessionId, isSessionAbandoned: false);
				shoppingSession.End();
			}
			else
			{
				LogHelper.Instance.Log("Unable find shopping cart session for " + rentalShoppingCart.ShoppingSessionId.ToString());
			}
		}
		else
		{
			LogHelper.Instance.Log("Unable to update shopping cart item statuses.  Unable to get current rental shopping cart.");
		}
	}

	public static void RegisterWithBrokerService()
	{
		RegisterWithLegacyBrokerService();
		ServiceLocator.Instance.GetService<IReservationService>()?.RegisterWithAWSBrokerService();
	}

	private static void RegisterWithLegacyBrokerService()
	{
		LogHelper.Instance.Log("ReservationFunctions.RegisterWithBrokerService");
		IReservationServices service = ServiceLocator.Instance.GetService<IReservationServices>();
		string storeNumber = ServiceLocator.Instance.GetService<IDataService>()?.GetStore()?.StoreId.ToString();
		service?.Register(storeNumber, GetpricesForBrokerService(isForRecommendationOnPickup: false), CreateReservationCallback, CancelReservationCallback);
	}

	public static IReservationResult CreateReservationCallback(string cardId, string email, long referenceNumber, DateTime reserveDate, IDictionary<string, List<int>> productsToReserve, bool appliedCredit, bool appliedHiveOnlinePromotion, bool appliedTitleMarketing, string customerNumber, decimal discountedSubTotal, decimal grandTotal, List<int> historyTitleIDs, decimal hiveOnlinePromotionDiscount, bool isMultiNightPricing, int numberOfCreditsRemaining, string promoCode, decimal subTotal, decimal tax, List<ReservationTitle3> reservationTitles, bool isGc, List<string> gcIds, List<string> ccIds, decimal taxRate, string zip, bool isMdv, bool isLoyaltyRedemption, decimal defaultServiceFee, decimal actualServiceFee, bool authorizeAtPickup, ReservationCardHash cardHashData, List<ReservationCardHash> alternateCardHashes)
	{
		LogHelper.Instance.Log("ReservationFunctions.CreateReservationCallback: Begin");
		List<ReservationRequestTitle> list = new List<ReservationRequestTitle>();
		foreach (ReservationTitle3 reservationTitle in reservationTitles)
		{
			ReservationRequestTitle item = new ReservationRequestTitle
			{
				ItemID = reservationTitle.ItemID,
				TitleID = reservationTitle.TitleID,
				ReservationType = (ReservationTitleType)reservationTitle.ReservationType,
				Price = reservationTitle.Price,
				Discount = reservationTitle.Discount,
				DiscountType = (ReservationDiscountType)reservationTitle.DiscountType,
				DiscountedPrice = reservationTitle.DiscountedPrice,
				NumberOfCredits = reservationTitle.NumberOfCredits,
				LoyaltyPoints = reservationTitle.LoyaltyPoints
			};
			list.Add(item);
		}
		ReservationRequestCardHash reservationCardHash = null;
		if (cardHashData != null)
		{
			reservationCardHash = new ReservationRequestCardHash
			{
				CardBinHash = cardHashData.CardBinHash,
				Last4 = cardHashData.Last4
			};
		}
		List<ReservationRequestCardHash> list2 = null;
		if (alternateCardHashes != null)
		{
			list2 = new List<ReservationRequestCardHash>();
			foreach (ReservationCardHash alternateCardHash in alternateCardHashes)
			{
				list2.Add(new ReservationRequestCardHash
				{
					CardBinHash = alternateCardHash.CardBinHash,
					Last4 = alternateCardHash.Last4
				});
			}
		}
		IReservationRequestResult reservationRequestResult = CreateReservation(new ReservationRequest
		{
			CardID = cardId,
			Email = email,
			ReferenceNumber = referenceNumber,
			ReserveDate = reserveDate,
			AppliedHiveOnlinePromotion = appliedHiveOnlinePromotion,
			AppliedTitleMarketing = appliedTitleMarketing,
			CustomerNumber = customerNumber,
			DiscountedSubTotal = discountedSubTotal,
			GrandTotal = grandTotal,
			HistoryTitleIDs = historyTitleIDs,
			HiveOnlinePromotionDiscount = hiveOnlinePromotionDiscount,
			IsMnp = isMultiNightPricing,
			PromoCode = promoCode,
			SubTotal = subTotal,
			Tax = tax,
			Titles = list,
			IsGc = isGc,
			GcIds = gcIds,
			CcIds = ccIds,
			TaxRate = taxRate,
			Zip = zip,
			IsMdv = isMdv,
			LoyaltyRedemption = isLoyaltyRedemption,
			DefaultServiceFeeAmount = defaultServiceFee,
			ActualServiceFeeAmount = actualServiceFee,
			AuthorizeAtPickup = authorizeAtPickup,
			ReservationCardHash = reservationCardHash,
			AlternateCardHashes = list2
		});
		IReservationServices service = ServiceLocator.Instance.GetService<IReservationServices>();
		IReservationResult reservationResult = service.NewReservationResult();
		reservationResult.Success = reservationRequestResult.Status == ReservationRequestStatus.ReservationCreated;
		reservationResult.ErrorMessage = ((reservationRequestResult.Status == ReservationRequestStatus.TitleNotAvailable) ? null : reservationRequestResult.ErrorMessage);
		reservationResult.Items = new List<IReservedItem>();
		foreach (IReservationRequestResultItem item2 in reservationRequestResult.Items)
		{
			IReservedItem reservedItem = service.NewReservedItem();
			reservedItem.Barcode = item2.Barcode;
			reservedItem.ProductId = item2.ProductId;
			reservationResult.Items.Add(reservedItem);
		}
		return reservationResult;
	}

	public static IReservationRequestResult CreateReservation(IReservationRequest reservationRequest)
	{
		LogHelper.Instance.Log("ReservationFunctions.CreateReservation: Begin");
		ReservationRequestResult reservationRequestResult = new ReservationRequestResult();
		if (IsValidReservationRequest(reservationRequest, reservationRequestResult))
		{
			lock (_reservationSyncObject)
			{
				List<string> barcodesAlreadyInPlay = GetBarcodesAlreadyInPlay();
				List<IBarcodeForProductResult> barcodesForNewReservation = GetBarcodesForNewReservation(reservationRequest?.Titles, barcodesAlreadyInPlay);
				foreach (IBarcodeForProductResult eachBarcodeForProductResult in barcodesForNewReservation)
				{
					ReservationRequestTitle reservationRequestTitle = reservationRequest?.Titles.FirstOrDefault((ReservationRequestTitle x) => x.TitleID == eachBarcodeForProductResult.ProductId);
					IReservationRequestResultItem reservationRequestResultItem = new ReservationRequestResultItem
					{
						ItemId = (reservationRequestTitle?.ItemID ?? 0),
						ProductId = (int)eachBarcodeForProductResult.ProductId,
						Barcode = (eachBarcodeForProductResult.BarcodeAssigned ? eachBarcodeForProductResult.Barcode : "empty")
					};
					reservationRequestResult.Items.Add(reservationRequestResultItem);
					if (eachBarcodeForProductResult.BarcodeAssigned)
					{
						LogHelper.Instance.Log($"ReservationFunctions.CreateReservation: found barcode {reservationRequestResultItem?.Barcode} for product id: {reservationRequestResultItem?.ProductId}");
					}
				}
				if (barcodesForNewReservation.Count > 0 && barcodesForNewReservation.All((IBarcodeForProductResult x) => x.BarcodeAssigned))
				{
					LogHelper.Instance.Log("ReservationFunctions.CreateReservation: Creating reservation...");
					UpdateBarcodeStatus(barcodesForNewReservation.Select((IBarcodeForProductResult x) => x.Barcode).ToList(), "Reserved");
					reservationRequestResult.Status = ((!CreateAndSaveNewReservation(reservationRequest, barcodesForNewReservation)) ? ReservationRequestStatus.SystemError : ReservationRequestStatus.ReservationCreated);
					if (OrchestrationService.IsRentalSessionActive() && ApplicationState.Instance.NeedToReloadCarousel)
					{
						StartViewController.ReloadCarousel(force: true);
					}
				}
				else
				{
					LogHelper.Instance.Log("ReservationFunctions.CreateReservation: Could not satisfy reservation request; unable to reserve requested inventory.");
					string text = string.Join(", ", (from y in barcodesForNewReservation
						where !y.BarcodeAssigned
						select y.ProductId)?.ToList());
					reservationRequestResult.ErrorMessage = "Title(s) unavailable: " + text;
					reservationRequestResult.Status = ReservationRequestStatus.TitleNotAvailable;
				}
			}
		}
		return reservationRequestResult;
	}

	private static bool IsValidReservationRequest(IReservationRequest reservationRequest, ReservationRequestResult reservationRequestResult)
	{
		bool flag = false;
		if (reservationRequest == null)
		{
			reservationRequestResult.ErrorMessage = "Reservation Request required.";
		}
		else if (reservationRequest.ReferenceNumber == 0L)
		{
			reservationRequestResult.ErrorMessage = "ReferenceNumber must not be 0.";
		}
		else if (reservationRequest.Titles == null || reservationRequest.Titles.Count == 0)
		{
			reservationRequestResult.ErrorMessage = "Titles are required.";
		}
		else if (reservationRequest.Titles != null && reservationRequest.Titles.Any((ReservationRequestTitle x) => x.ReservationType == ReservationTitleType._Unspecified))
		{
			reservationRequestResult.ErrorMessage = $"Title type must not be {ReservationTitleType._Unspecified}.";
		}
		else if (string.IsNullOrEmpty(reservationRequest.CardID) && (string.IsNullOrEmpty(reservationRequest?.ReservationCardHash?.CardBinHash) || string.IsNullOrEmpty(reservationRequest?.ReservationCardHash?.Last4)))
		{
			reservationRequestResult.ErrorMessage = "CardID or ReservationCardHash (CardBinHash & Last4) must have a value";
		}
		else
		{
			flag = true;
		}
		if (!flag)
		{
			reservationRequestResult.Status = ReservationRequestStatus.InvalidRequest;
		}
		return flag;
	}

	private static List<IBarcodeForProductResult> GetBarcodesForNewReservation(List<ReservationRequestTitle> reservationTitles, List<string> barcodesAlreadyInPlay)
	{
		List<ISearchBarcodeForProduct> list = new List<ISearchBarcodeForProduct>();
		if (reservationTitles != null)
		{
			foreach (ReservationRequestTitle reservationTitle in reservationTitles)
			{
				RentalShoppingCartItemAction result = RentalShoppingCartItemAction.rental;
				Enum.TryParse<RentalShoppingCartItemAction>(reservationTitle.ReservationType.ToString(), ignoreCase: true, out result);
				list.Add(new SearchBarcodeForProduct
				{
					ProductId = reservationTitle.TitleID,
					Action = result
				});
			}
		}
		return ServiceLocator.Instance.GetService<IDataService>()?.GetBarcodesForProducts(list, barcodesAlreadyInPlay);
	}

	private static List<string> GetBarcodesAlreadyInPlay()
	{
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		IRentalShoppingCart rentalShoppingCart = service?.CurrentRentalShoppingCart;
		List<string> list = service?.AssignBarcodesToVend(rentalShoppingCart);
		AddBarcodesForMDVProducts(rentalShoppingCart, list);
		List<long> userVisibleProducts = new List<long>();
		switch (ServiceLocator.Instance.GetService<IViewService>()?.Peek())
		{
		case "start_view":
			if (OrchestrationService.IsRentalSessionActive())
			{
				AddTitleProductToUserVisibleProducts(userVisibleProducts, Redbox.Rental.Services.Session.Session.Instance?.BrowseState?.CenterCarouselProduct);
			}
			break;
		case "browse_view":
		case "recommendation_on_pickup_view":
		case "upsell_view":
			if (Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseProductsShown == null)
			{
				break;
			}
			foreach (IKioskProduct item in Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseProductsShown)
			{
				AddTitleProductToUserVisibleProducts(userVisibleProducts, item);
			}
			break;
		case "product_detail_view":
			AddTitleProductToUserVisibleProducts(userVisibleProducts, Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectionSelectedProduct);
			break;
		}
		List<string> userVisibleBarcodes = GetUserVisibleBarcodes(userVisibleProducts, list);
		List<string> list2 = new List<string>();
		list2.AddRange(list);
		list2.AddRange(userVisibleBarcodes);
		return list2;
	}

	private static bool CreateAndSaveNewReservation(IReservationRequest reservationRequest, List<IBarcodeForProductResult> barcodeForProductResults)
	{
		if (reservationRequest == null || barcodeForProductResults == null)
		{
			return false;
		}
		List<IReservation> allReservations = GetAllReservations();
		IReservation reservation = null;
		string hashedCardId = reservationRequest.CardID;
		bool flag = reservationRequest?.ReservationCardHash?.CardBinHash != null && reservationRequest?.ReservationCardHash?.Last4 != null;
		if (flag)
		{
			hashedCardId = CreateCardHash(reservationRequest.ReservationCardHash);
		}
		reservation = allReservations.FirstOrDefault((IReservation x) => x.ReferenceNumber == reservationRequest.ReferenceNumber.ToString());
		bool flag2 = reservation == null;
		if (flag2)
		{
			reservation = new Reservation
			{
				HashedCardId = hashedCardId,
				Id = Guid.NewGuid(),
				ReferenceNumber = reservationRequest.ReferenceNumber.ToString()
			};
		}
		IReservationDetails reservationDetails = new ReservationDetails
		{
			ReferenceNumber = reservationRequest.ReferenceNumber.ToString(),
			ActualServiceFee = reservationRequest.ActualServiceFeeAmount,
			AppliedHiveOnlinePromo = reservationRequest.AppliedHiveOnlinePromotion,
			AppliedTitleMarketing = reservationRequest.AppliedTitleMarketing,
			AuthorizeAtPickup = reservationRequest.AuthorizeAtPickup,
			CustomerNumber = reservationRequest.CustomerNumber,
			DefaultServiceFee = reservationRequest.DefaultServiceFeeAmount,
			DiscountedSubTotal = reservationRequest.DiscountedSubTotal,
			EmailAddress = reservationRequest.Email,
			GrandTotal = reservationRequest.GrandTotal,
			HiveOnlinePromoDiscount = reservationRequest.HiveOnlinePromotionDiscount,
			IsGiftCard = reservationRequest.IsGc,
			IsLoyaltyRedemption = reservationRequest.LoyaltyRedemption,
			IsMultiDiscVend = reservationRequest.IsMdv,
			IsMultiNightPrice = reservationRequest.IsMnp,
			PromoCode = reservationRequest.PromoCode,
			ReservationVersion = 3,
			SubTotal = reservationRequest.SubTotal,
			Tax = reservationRequest.Tax,
			TaxRate = reservationRequest.TaxRate,
			TransactionDate = reservationRequest.ReserveDate,
			ZipCode = reservationRequest.Zip,
			PricingSet = GetPriceSetForBrokerService(isForRecommendationOnPickup: true)
		};
		if (flag && reservationRequest.AlternateCardHashes != null)
		{
			foreach (ReservationRequestCardHash alternateCardHash in reservationRequest.AlternateCardHashes)
			{
				reservationRequest.CcIds.Add(CreateCardHash(alternateCardHash));
			}
		}
		if (reservationRequest.CcIds != null)
		{
			reservationDetails.CreditCardIds.AddRange(reservationRequest.CcIds);
		}
		if (reservationRequest.GcIds != null)
		{
			reservationDetails.GiftCardIds.AddRange(reservationRequest.GcIds);
		}
		if (reservationRequest.HistoryTitleIDs != null)
		{
			reservationDetails.HistoryTitleIds.AddRange(reservationRequest.HistoryTitleIDs);
		}
		if (reservationRequest.Titles != null)
		{
			foreach (ReservationRequestTitle eachReservationTitle in reservationRequest.Titles)
			{
				RentalShoppingCartItemAction action = RentalShoppingCartItemAction.rental;
				Enum.TryParse<RentalShoppingCartItemAction>(eachReservationTitle.ReservationType.ToString(), ignoreCase: true, out action);
				IBarcodeForProductResult barcodeForProductResult = barcodeForProductResults.FirstOrDefault((IBarcodeForProductResult x) => x.ProductId == eachReservationTitle.TitleID && x.Action == action);
				if (barcodeForProductResult != null)
				{
					IReservedTitle item = new ReservedTitle
					{
						TitleId = eachReservationTitle.TitleID,
						Action = action,
						Barcode = barcodeForProductResult.Barcode,
						Discount = eachReservationTitle.Discount,
						DiscountedPrice = eachReservationTitle.DiscountedPrice,
						DiscountType = eachReservationTitle.DiscountType,
						LoyaltyPoints = eachReservationTitle.LoyaltyPoints,
						Price = eachReservationTitle.Price
					};
					reservationDetails.ReservedTitles.Add(item);
				}
			}
		}
		reservation.ReservationDetails = reservationDetails;
		LogTheNewlyCreatedReservation(reservation, flag2, reservationDetails);
		return UpdateReservation(reservation);
	}

	private static string CreateCardHash(ReservationRequestCardHash cardHashData)
	{
		string text = CardHelper.GenerateHash(cardHashData.Last4);
		return cardHashData.CardBinHash + text;
	}

	private static void LogTheNewlyCreatedReservation(IReservation reservation, bool isNewReservation, IReservationDetails newReservationDetails)
	{
		string text = (isNewReservation ? "Adding" : "Updating");
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append($"{text} reservation: id: {reservation.Id},  card id: ...{ObfuscateHashedCardId(reservation.HashedCardId)},  Reference Number: {newReservationDetails.ReferenceNumber}, ");
		stringBuilder.Append($"Transation date: {newReservationDetails.TransactionDate},  Actual Service Fee: {newReservationDetails.ActualServiceFee},  ");
		stringBuilder.Append($"Applied Hive Online Promo: {newReservationDetails.AppliedHiveOnlinePromo},  Applied Title Marketing: {newReservationDetails.AppliedTitleMarketing},  ");
		stringBuilder.Append($"Authorize At Pickup: {newReservationDetails.AuthorizeAtPickup},  Customer Number: {newReservationDetails.CustomerNumber},  ");
		stringBuilder.Append($"Default Service Fee: {newReservationDetails.DefaultServiceFee},  Hive Online Promo Discount: {newReservationDetails.HiveOnlinePromoDiscount},  ");
		stringBuilder.Append($"Is Gift Card: {newReservationDetails.IsGiftCard},  Is Loyalty Redemption: {newReservationDetails.IsLoyaltyRedemption},  ");
		stringBuilder.Append($"Is Multi Disc Vend: {newReservationDetails.IsMultiDiscVend},  Is Multi Night Price: {newReservationDetails.IsMultiNightPrice},  ");
		stringBuilder.Append($"Reservation Version: {newReservationDetails.ReservationVersion},  Promo Code: {newReservationDetails.PromoCode},  ");
		stringBuilder.Append($"Tax Rate: {newReservationDetails.TaxRate},  Tax: {newReservationDetails.Tax},  ");
		stringBuilder.Append($"Sub Total: {newReservationDetails.SubTotal},  Discounted Sub Total: {newReservationDetails.DiscountedSubTotal},  ");
		stringBuilder.Append($"Grand Total: {newReservationDetails.GrandTotal}");
		LogHelper.Instance.Log(stringBuilder.ToString());
		foreach (IReservedTitle reservedTitle in newReservationDetails.ReservedTitles)
		{
			LogHelper.Instance.Log($"Reserved title {newReservationDetails.ReservedTitles.IndexOf(reservedTitle) + 1}: Barcode: {reservedTitle.Barcode}," + $" Product Id: {reservedTitle.TitleId}, Action: {reservedTitle.Action}, Price: {reservedTitle.Price}, Discount: {reservedTitle.Discount}," + $" Discount Type: {reservedTitle.DiscountType}, Discounted Price: {reservedTitle.DiscountedPrice}, Loyalty Points: {reservedTitle.LoyaltyPoints}");
		}
	}

	private static List<string> GetUserVisibleBarcodes(List<long> userVisibleProducts, List<string> excludeBarcodes)
	{
		List<string> list = new List<string>();
		if (userVisibleProducts != null)
		{
			IDataService service = ServiceLocator.Instance.GetService<IDataService>();
			List<ISearchBarcodeForProduct> list2 = new List<ISearchBarcodeForProduct>();
			foreach (long userVisibleProduct in userVisibleProducts)
			{
				list2.Add(new SearchBarcodeForProduct
				{
					ProductId = userVisibleProduct,
					Action = RentalShoppingCartItemAction.rental
				});
			}
			foreach (IBarcodeForProductResult item in service?.GetBarcodesForProducts(list2, excludeBarcodes))
			{
				if (item.BarcodeAssigned)
				{
					list.Add(item.Barcode);
				}
			}
		}
		return list;
	}

	private static void AddTitleProductToUserVisibleProducts(List<long> userVisibleProducts, IKioskProduct kioskProduct)
	{
		if (kioskProduct is ITitleRollupProduct { KioskProducts: not null } titleRollupProduct)
		{
			{
				foreach (ITitleProduct kioskProduct2 in titleRollupProduct.KioskProducts)
				{
					userVisibleProducts.Add(kioskProduct2.ProductId);
				}
				return;
			}
		}
		if (kioskProduct is MDVProduct mDVProduct)
		{
			{
				foreach (ITitleProduct titleProduct2 in mDVProduct.TitleProducts)
				{
					userVisibleProducts.Add(titleProduct2.ProductId);
				}
				return;
			}
		}
		if (kioskProduct is ITitleProduct titleProduct)
		{
			userVisibleProducts.Add(titleProduct.ProductId);
		}
	}

	private static void AddBarcodesForMDVProducts(IRentalShoppingCart rentalShoppingCart, List<string> rentalShoppingCartBarcodes)
	{
		if (rentalShoppingCart == null)
		{
			return;
		}
		List<long> list = new List<long>();
		foreach (IRentalShoppingCartTitleItem titleItem in rentalShoppingCart.TitleItems)
		{
			if (!titleItem.MultiDisc || !(titleItem.TitleProduct is MDVProduct { TitleProducts: not null } mDVProduct))
			{
				continue;
			}
			foreach (ITitleProduct titleProduct in mDVProduct.TitleProducts)
			{
				if (!rentalShoppingCart.HasTitleItem(titleProduct.ProductId))
				{
					list.Add(titleProduct.ProductId);
				}
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		List<ISearchBarcodeForProduct> list2 = new List<ISearchBarcodeForProduct>();
		foreach (long item in list)
		{
			list2.Add(new SearchBarcodeForProduct
			{
				ProductId = item,
				Action = RentalShoppingCartItemAction.rental
			});
		}
		List<IBarcodeForProductResult> list3 = ServiceLocator.Instance.GetService<IDataService>()?.GetBarcodesForProducts(list2, rentalShoppingCartBarcodes);
		if (list3 == null)
		{
			return;
		}
		foreach (IBarcodeForProductResult item2 in list3.Where((IBarcodeForProductResult x) => x.BarcodeAssigned))
		{
			rentalShoppingCartBarcodes.Add(item2.Barcode);
		}
	}

	private static ILocalCancelReservationResult CancelReservationCallback(long referenceNumber, bool cancelOnServer = true)
	{
		ReservationCancelResult reservationCancelResult = ReservationCancelResult.Error;
		reservationCancelResult = ((!cancelOnServer) ? CancelLocalReservation(referenceNumber.ToString()) : CancelLocalAndServerReservation(referenceNumber.ToString()));
		ILocalCancelReservationResult localCancelReservationResult = ServiceLocator.Instance.GetService<IReservationServices>().NewCancelReservationResult();
		localCancelReservationResult.Success = reservationCancelResult == ReservationCancelResult.ReservationCancelled;
		return localCancelReservationResult;
	}

	public static ReservationCancelResult CancelLocalAndServerReservation(string referenceNumber)
	{
		string storeNumber = ServiceLocator.Instance.GetService<IDataService>()?.GetStore()?.StoreId.ToString();
		ServiceLocator.Instance.GetService<IReservationServices>();
		if (long.TryParse(referenceNumber, out var result))
		{
			ServiceLocator.Instance.GetService<IKioskClientService>().Transaction.CancelReservation(storeNumber, result, null, null);
		}
		else
		{
			LogHelper.Instance.Log("Error in ReservationFunctions.CancelLocalAndServerReservation. Unable to parse reference number to long: " + referenceNumber);
		}
		return CancelLocalReservation(referenceNumber);
	}

	public static IDictionary<string, IDictionary<string, IDictionary<string, decimal>>> GetpricesForBrokerService(bool isForRecommendationOnPickup)
	{
		Dictionary<string, IDictionary<string, IDictionary<string, decimal>>> obj = new Dictionary<string, IDictionary<string, IDictionary<string, decimal>>>
		{
			{
				"Movie",
				new Dictionary<string, IDictionary<string, decimal>>()
			},
			{
				"Game",
				new Dictionary<string, IDictionary<string, decimal>>()
			}
		};
		IPricingSet priceSetForBrokerService = GetPriceSetForBrokerService(isForRecommendationOnPickup);
		IDictionary<string, decimal> value = CreatePricingRecordDictionary(priceSetForBrokerService.PriceRecords.FirstOrDefault((IPricingRecord x) => x.TitleFamily == TitleFamily.Movies && x.TitleType == TitleType.DVD), includePurchasePrice: true);
		IDictionary<string, decimal> value2 = CreatePricingRecordDictionary(priceSetForBrokerService.PriceRecords.FirstOrDefault((IPricingRecord x) => x.TitleFamily == TitleFamily.Movies && x.TitleType == TitleType.Bluray));
		IDictionary<string, decimal> value3 = CreatePricingRecordDictionary(priceSetForBrokerService.PriceRecords.FirstOrDefault((IPricingRecord x) => x.TitleFamily == TitleFamily.Movies && x.TitleType == TitleType._4KUHD));
		IDictionary<string, decimal> value4 = CreatePricingRecordDictionary(priceSetForBrokerService.PriceRecords.FirstOrDefault((IPricingRecord x) => x.TitleFamily == TitleFamily.Games && x.TitleType == TitleType.All));
		((IDictionary<string, IDictionary<string, IDictionary<string, decimal>>>)obj)["Movie"]["default"] = value;
		((IDictionary<string, IDictionary<string, IDictionary<string, decimal>>>)obj)["Movie"]["BluRay"] = value2;
		((IDictionary<string, IDictionary<string, IDictionary<string, decimal>>>)obj)["Movie"]["_4kuhd"] = value3;
		((IDictionary<string, IDictionary<string, IDictionary<string, decimal>>>)obj)["Game"]["default"] = value4;
		return obj;
	}

	private static IPricingSet GetPriceSetForBrokerService(bool isForRecommendationOnPickup)
	{
		IPricingService service = ServiceLocator.Instance.GetService<IPricingService>();
		return new PricingSet
		{
			PriceSetId = "1",
			PriceRecords = 
			{
				service.GetProductTypePricingRecord(TitleFamily.Movies, TitleType.DVD, isForRecommendationOnPickup),
				service.GetProductTypePricingRecord(TitleFamily.Movies, TitleType.Bluray, isForRecommendationOnPickup),
				service.GetProductTypePricingRecord(TitleFamily.Movies, TitleType._4KUHD, isForRecommendationOnPickup),
				service.GetProductTypePricingRecord(TitleFamily.Games, TitleType.All, isForRecommendationOnPickup)
			}
		};
	}

	private static IDictionary<string, decimal> CreatePricingRecordDictionary(IPricingRecord pricingRecord, bool includePurchasePrice = false)
	{
		IDictionary<string, decimal> dictionary = new Dictionary<string, decimal>
		{
			{ "initial_night", pricingRecord.InitialNight },
			{ "extra_night", pricingRecord.ExtraNight },
			{ "non_return", pricingRecord.NonReturn },
			{ "non_return_days", pricingRecord.NonReturnDays },
			{ "expiration_price", pricingRecord.ExpirationPrice },
			{
				"pricesetid",
				pricingRecord.PriceSetId ?? 1
			}
		};
		if (includePurchasePrice)
		{
			dictionary["purchase_price"] = pricingRecord.Purchase;
		}
		return dictionary;
	}

	private static void CallMerchantService(string referenceNumber, bool modifiedCart, IRentalShoppingCart rentalShoppingCart, IShoppingSession session, List<IReservation> reservations)
	{
		if (session.Status != ShoppingSessionStatus.Committed)
		{
			return;
		}
		long.TryParse(referenceNumber, out var result);
		IReservation reservation = reservations.FirstOrDefault((IReservation r) => r.ReferenceNumber == referenceNumber);
		if (IsAuthorizeAtPickup(referenceNumber, reservations))
		{
			IKioskClientService service = ServiceLocator.Instance.GetService<IKioskClientService>();
			LogHelper.Instance.Log("ReservationFunctions.ReservationPrepositionVend: Calling KioskClientService.Transaction.ReservationPickup()");
			service.Transaction.ReservationPickup(rentalShoppingCart.ShoppingSessionId.ToString(), Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.CustomerProfileNumber, reservation?.ReservationDetails?.EmailAddress, result, new Dictionary<string, ReadOnlyCollection<IProductItem>>(), null);
			return;
		}
		LogHelper.Instance.Log("ReservationFunctions.ReservationPrepositionVend: Calling MerchantServices.ModResvPickup()");
		if (Redbox.Rental.Services.Session.Session.Instance?.UserInfo?.Email != null)
		{
			session.ShoppingCart.SetEmail(Redbox.Rental.Services.Session.Session.Instance?.UserInfo?.Email);
		}
		LogHelper.Instance.Log("Error: ModResvPickup not supported by TransactionService");
		OrchestrationService.ShoppingCartEndSession(rentalShoppingCart.ShoppingSessionId.ToString());
		LogHelper.Instance.Log("ReservationFunctions.ReservationPrepositionVend: Calling MerchantServices.ModResvPickup() complete");
	}

	private static void ProcessRentalShoppingCartItems(IRentalShoppingCart rentalShoppingCart, List<string> barcodesToReset)
	{
		foreach (IRentalShoppingCartTitleItem titleItem in rentalShoppingCart.TitleItems)
		{
			LogHelper.Instance.Log("> local cart item: " + titleItem.Barcode);
			if (titleItem.Status == LineItemStatus.Pending || titleItem.Status == LineItemStatus.ProductSelected)
			{
				LogHelper.Instance.Log("Item not fullfilled: " + titleItem.Barcode);
				titleItem.Status = LineItemStatus.NotFullfilled;
			}
			if (titleItem.Status != LineItemStatus.Fullfilled)
			{
				LogHelper.Instance.Log("> Adding " + titleItem.Barcode + " to reset barcode list");
				barcodesToReset.Add(titleItem.Barcode);
			}
			LogHelper.Instance.Log($"Session update Line Item: {titleItem.Barcode} for {titleItem.Action} Price:" + $" {titleItem.Price} {titleItem.TitleFamily} - {titleItem.TitleType} source type: {titleItem.SourceType}");
		}
	}

	private static void UpdateRentalShoppingCartItemStatuses(List<VendResult> vendResultList, IRentalShoppingCart rentalShoppingCart)
	{
		if (vendResultList != null)
		{
			foreach (VendResult eachVendResult in vendResultList)
			{
				LogHelper.Instance.Log("> vend_results barcode: " + eachVendResult?.barcode + ", status: " + eachVendResult?.status);
				IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = rentalShoppingCart.TitleItems.FirstOrDefault((IRentalShoppingCartTitleItem x) => x.Barcode == eachVendResult.barcode);
				if (rentalShoppingCartTitleItem != null)
				{
					if (Enum.TryParse<LineItemStatus>(eachVendResult.status, out var result))
					{
						rentalShoppingCartTitleItem.Status = result;
					}
					else
					{
						LogHelper.Instance.Log("Error in RentalShoppingCartFunctions.UpdateItemStatus.  Unable to parse status value " + eachVendResult.status + " for barcode " + rentalShoppingCartTitleItem.Barcode);
					}
				}
				else
				{
					LogHelper.Instance.Log("Unable to find rental shopping cart item with barcode: " + eachVendResult?.barcode);
				}
			}
			return;
		}
		LogHelper.Instance.Log("ReservationFunctions.UpdateRentalShoppingCartItemStatuses - error vendResultList is null");
	}

	private static List<IReservation> GetReservationsForHashedCardId(string swipedhashedCardId, string newHashedCardId, List<IReservation> allReservations)
	{
		List<IReservation> list = new List<IReservation>();
		if (!string.IsNullOrEmpty(swipedhashedCardId))
		{
			list.AddRange(allReservations.Where((IReservation r) => r.HashedCardId == swipedhashedCardId));
		}
		if (!string.IsNullOrEmpty(newHashedCardId) && newHashedCardId != swipedhashedCardId)
		{
			list.AddRange(allReservations.Where((IReservation r) => r.HashedCardId == newHashedCardId));
		}
		if (list.Count == 0)
		{
			foreach (IReservation allReservation in allReservations)
			{
				if (!string.IsNullOrEmpty(swipedhashedCardId))
				{
					if (allReservation.ReservationDetails.CreditCardIds.Contains(swipedhashedCardId))
					{
						LogHelper.Instance.Log("swiped card ..." + ObfuscateHashedCardId(swipedhashedCardId) + " matches associated credit card");
						list.Add(allReservation);
						break;
					}
					if (allReservation.ReservationDetails.GiftCardIds.Contains(swipedhashedCardId))
					{
						LogHelper.Instance.Log("swiped card ..." + ObfuscateHashedCardId(swipedhashedCardId) + " matches associated gift card");
						list.Add(allReservation);
						break;
					}
				}
				if (!string.IsNullOrEmpty(newHashedCardId) && newHashedCardId != swipedhashedCardId && allReservation.ReservationDetails.CreditCardIds.Contains(newHashedCardId))
				{
					LogHelper.Instance.Log("swiped card ..." + ObfuscateHashedCardId(newHashedCardId) + " matches associated credit card");
					list.Add(allReservation);
					break;
				}
			}
		}
		return list;
	}

	public static void RegisterVendOnlyReservedTitlesCallback(Action<string, string> vendOnlyReservedTitlesCallback)
	{
		_vendOnlyReservedTitlesCallback = vendOnlyReservedTitlesCallback;
	}

	public static void InvokeVendOnlyReservedTitlesCallback(string emailAddress, string referenceNumber)
	{
		_vendOnlyReservedTitlesCallback?.Invoke(emailAddress, referenceNumber);
	}

	public static void RegisterReservationAuthorizeCallback(Action<string, string> reservationAuthorizeCallback)
	{
		_reservationAuthorizeCallback = reservationAuthorizeCallback;
	}

	public static void InvokeReservationAuthorizeCallback(string emailAddress, string referenceNumber)
	{
		_reservationAuthorizeCallback?.Invoke(emailAddress, referenceNumber);
	}

	public static void RegisterAuthorizeOnPickupCallback(Action<string, bool> callback)
	{
		_authorizeOnPickupCallback = callback;
	}

	public static void InvokeAuthorizeOnPickupCallback(string referenceNumber, bool cartWasModified)
	{
		_authorizeOnPickupCallback?.Invoke(referenceNumber, cartWasModified);
	}
}
