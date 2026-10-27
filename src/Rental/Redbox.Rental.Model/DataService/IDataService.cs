using System;
using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskClientService.Subscriptions;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Local;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.Model.Reservation;

namespace Redbox.Rental.Model.DataService;

public interface IDataService
{
	ProfileDataVersion ProfileDataVersion { get; }

	ErrorList Initialize();

	ErrorList Stop();

	ErrorList Start();

	byte[] GetImage(string name);

	ErrorList CacheTablesUpdate(string tableName, string key, object data);

	ErrorList CacheTablesRemove(string tableName, string key);

	bool RefreshIfNeeded();

	ErrorList GetProduct(long productId, bool excludeTitleRollup, out IKioskProduct kioskProduct);

	List<ITitleProduct> GetMovieBrowseProducts();

	ErrorList GetProductByBarcode(string barcode, out IKioskProduct kioskProduct);

	ErrorList GetProducts(BrowseViewFilter browseFilter, TitleFamily titleFamily, TitleType titleType, BrowseSort browseSort, long? genreFilter, long? ratingFilter, List<long> include, List<long> exclude, bool isADAView, decimal? priceRangeLow, decimal? priceRangeHigh, out List<IKioskProduct> products, string studio = null, List<TitleType> excludeTitleTypes = null, bool excludeDiscountRestriction = false);

	ErrorList GetUnfilteredNonTitleRollupProducts(out IEnumerable<IKioskProduct> products);

	ErrorList GetOutOfStockMovieProducts(out IEnumerable<ITitleProduct> products);

	ErrorList GetTitleRollupProduct(long productGroupId, out IKioskProduct kioskProduct);

	ErrorList GetCarouselProducts(int maxCarouselItems, out List<IKioskProduct> products);

	ErrorList GetProductsForCustomerOffer(TitleFamily titleFamily, List<TitleType> titleTypes, bool includeTitleTypes, List<long> include, List<long> exclude, TimeSpan? includeOlderThan, bool isAdaView, bool isPurchase, out List<ITitleProduct> products);

	bool GetIsProductThinned(long productId);

	int GetInStockGameCount();

	Store GetStore();

	Store GetStore(long kioskId);

	List<SubPlatform> GetSubPlatforms();

	List<ProductType> GetProductTypes();

	Title GetProductCatalogTitle(long productId);

	Dictionary<long, Title> GetProductCatalogTitles();

	bool IsAccountOnOfflineBadCardList(string hashId);

	void UpdateAccountOnOfflineBadCardList(string hashId);

	void RemoveAccountFromOfflineBadCardList(string hashId);

	TitleType? GetProductTitleType(long productId);

	List<IReservation> GetReservations(string hashedCardId);

	List<IReservation> GetAllReservations();

	bool SaveReservation(IReservation reservation);

	void DeleteReservation(string referenceNumber);

	IReservation GetReservation(string referenceNumber);

	List<IBarcodeForProductResult> GetBarcodesForProducts(List<ISearchBarcodeForProduct> searchProducts, List<string> excludeBarcodesList);

	bool IsMultiDisc(long productId);

	List<string> GetAllUnmatchedMultiDiscBarcodes();

	long GetProductGroupId(IKioskProduct kioskProduct);

	IRedboxPlusPromoCampaign GetRedboxPlusPromoCampaign(int campaignId);

	ErrorList GetSubscriptionProducts(out List<ISubscriptionProduct> subscriptionProducts);

	List<IRedboxPlusSubscriptionProduct> GetRedboxPlusSubscriptionProducts();

	void AddRemovedQueueItem(RemovedQueueItem item);

	List<RemovedQueueItem> GetRemovedQueueItems();

	void DeleteRemovedQueueItem(RemovedQueueItem item);

	List<long> GetCachedTitleIds();
}
