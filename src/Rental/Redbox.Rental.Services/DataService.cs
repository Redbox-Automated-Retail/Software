using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Cache;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskClientService.Subscriptions;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Local;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.Model.Reservation;
using Redbox.Rental.Model.Reservation.DataFile;
using Redbox.Rental.Services.KioskProduct;

namespace Redbox.Rental.Services;

public class DataService : IDataService
{
	private IKernelService _kernelService;

	private IProfileData _profileData;

	private ICacheData _cacheData;

	private ILocalData _localData;

	private IReservationData _reservationData;

	private IEngineApplication _engine;

	private string _runningPath;

	private string _dataPath;

	private ManualResetEvent _manualResetEvent;

	private bool IsFirstTime = true;

	private IProfileDataInstance _profileStagedInstance;

	private IDataServiceInstance _dataServiceInstance;

	private KioskProductView _kioskProductView;

	private int _inRefresh;

	private Timer _changeTimer;

	private readonly object _lock = new object();

	public string DataPath => _dataPath;

	public string RunningPath => _runningPath;

	public ProfileDataVersion ProfileDataVersion => _dataServiceInstance?.ProfileDataInstance?.ProfileDataVersion;

	private bool HasProfileStagedInstance => _profileStagedInstance != null;

	public DataService(IKernelService kernelService, IDataServiceInstance dataServiceInstance, IProfileData profileData, ICacheData cacheData, ILocalData localData, IReservationData reservationData, IEngineApplication engine)
	{
		_kernelService = kernelService;
		_dataServiceInstance = dataServiceInstance;
		_profileData = profileData;
		_cacheData = cacheData;
		_localData = localData;
		_reservationData = reservationData;
		_engine = engine;
		_runningPath = engine.RunningPath;
		_dataPath = engine.DataPath;
	}

	public ErrorList Initialize()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			LogHelper.Instance.Log("DataService initialization");
			_manualResetEvent = new ManualResetEvent(initialState: false);
			ThreadPool.QueueUserWorkItem(delegate
			{
				Refresh();
			});
			if (_changeTimer == null)
			{
				_changeTimer = new Timer(delegate
				{
					Refresh();
				});
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DataService.Initialize", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public ErrorList Start()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			LogHelper.Instance.Log("DataService start");
			_changeTimer.Change(new TimeSpan(0L), new TimeSpan(0, 0, 120));
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DataService.Start", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public ErrorList Stop()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			LogHelper.Instance.Log("Profile service stop");
			_changeTimer.Change(-1, -1);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("ProfileService.Stop", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public bool RefreshIfNeeded()
	{
		WaitForFirstLoad();
		bool result = false;
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				if (HasProfileStagedInstance)
				{
					UpdateAndRefreshView();
					return true;
				}
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("ProfileService.RefreshIfNeeded", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return result;
	}

	public byte[] GetImage(string name)
	{
		try
		{
			lock (_lock)
			{
				string path = Path.Combine(_dataServiceInstance.ProfileDataInstance.ImagesPath, name);
				if (File.Exists(path))
				{
					return File.ReadAllBytes(path);
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("DataService.GetImage - Unhandled exception occured", e);
		}
		return null;
	}

	public bool IsAccountOnOfflineBadCardList(string hashId)
	{
		return _dataServiceInstance.LocalDataInstance.IsAccountOnOfflineBadCardList(hashId);
	}

	public void UpdateAccountOnOfflineBadCardList(string hashId)
	{
		_dataServiceInstance.LocalDataInstance.UpdateAccountOnOfflineBadCardList(hashId);
	}

	public void RemoveAccountFromOfflineBadCardList(string hashId)
	{
		_dataServiceInstance.LocalDataInstance.RemoveAccountFromOfflineBadCardList(hashId);
	}

	public ErrorList CacheTablesUpdate(string tableName, string key, object data)
	{
		WaitForFirstLoad();
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				bool isNew = false;
				if (tableName.Equals("barcodeposition", StringComparison.CurrentCultureIgnoreCase))
				{
					BarcodeProduct barcodeProduct = _cacheData.UpdateBarcodePosition(_dataServiceInstance.CacheDataInstance, key, data, out isNew);
					if (isNew)
					{
						LogHelper.Instance.Log($"A new barcode position was added for barcode {barcodeProduct.Barcode} position {barcodeProduct.Position}");
					}
				}
				else if (tableName.Equals("producthistory", StringComparison.CurrentCultureIgnoreCase))
				{
					TitleCache titleCache = _cacheData.UpdateProductHistory(_dataServiceInstance.CacheDataInstance, key, data, out isNew);
					if (isNew)
					{
						LogHelper.Instance.Log($"A new product history was added for title {titleCache.ProductId} ");
						_kioskProductView.LoadView(titleCache);
					}
				}
				else if (tableName.Equals("productcatalog", StringComparison.CurrentCultureIgnoreCase))
				{
					TitleCache titleCache2 = _cacheData.UpdateProductCatalog(_dataServiceInstance.CacheDataInstance, key, data, out isNew);
					if (isNew)
					{
						LogHelper.Instance.Log($"A new product catalog was added for title {titleCache2.ProductId} ");
						_kioskProductView.LoadView(titleCache2);
					}
				}
				else if (tableName.Equals("producttobarcode", StringComparison.CurrentCultureIgnoreCase))
				{
					TitleCache titleCache3 = _cacheData.UpdateProductToBarcode(_dataServiceInstance.CacheDataInstance, key, data, out isNew);
					if (isNew)
					{
						LogHelper.Instance.Log($"A new product to barcode was added for title {titleCache3.ProductId} ");
						_kioskProductView.LoadView(titleCache3);
					}
				}
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DataService.CacheTablesUpdate", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public ErrorList CacheTablesRemove(string tableName, string key)
	{
		WaitForFirstLoad();
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				if (tableName.Equals("barcodeposition", StringComparison.CurrentCultureIgnoreCase))
				{
					_cacheData.RemoveBarcodePosition(_dataServiceInstance.CacheDataInstance, key);
				}
				else if (tableName.Equals("producthistory", StringComparison.CurrentCultureIgnoreCase))
				{
					_cacheData.RemoveProductHistory(_dataServiceInstance.CacheDataInstance, key);
				}
				else if (tableName.Equals("productcatalog", StringComparison.CurrentCultureIgnoreCase))
				{
					_cacheData.RemoveProductCatalog(_dataServiceInstance.CacheDataInstance, key);
				}
				else if (tableName.Equals("producttobarcode", StringComparison.CurrentCultureIgnoreCase))
				{
					_cacheData.RemoveProductToBarcode(_dataServiceInstance.CacheDataInstance, key);
				}
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DataService.CacheTablesRemove", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public ErrorList GetTitleRollupProduct(long productGroupId, out IKioskProduct kioskProduct)
	{
		WaitForFirstLoad();
		kioskProduct = null;
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				kioskProduct = _kioskProductView.GetTitleRollupProduct(productGroupId);
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DataService.GetTitleRollupForProduct", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public IRedboxPlusPromoCampaign GetRedboxPlusPromoCampaign(int campaignId)
	{
		WaitForFirstLoad();
		IRedboxPlusPromoCampaign redboxPlusPromoCampaign = null;
		lock (_lock)
		{
			return _kioskProductView.GetRedboxPlusPromoCampaign(campaignId);
		}
	}

	public ErrorList GetSubscriptionProducts(out List<ISubscriptionProduct> subscriptionProducts)
	{
		WaitForFirstLoad();
		subscriptionProducts = new List<ISubscriptionProduct>();
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				subscriptionProducts = _kioskProductView.GetSubscriptionProducts();
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DataService.GetSubscriptionProducts", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public List<IRedboxPlusSubscriptionProduct> GetRedboxPlusSubscriptionProducts()
	{
		WaitForFirstLoad();
		lock (_lock)
		{
			List<ISubscriptionProduct> subscriptionProducts = _kioskProductView.GetSubscriptionProducts();
			List<IRedboxPlusSubscriptionProduct> list = new List<IRedboxPlusSubscriptionProduct>();
			foreach (ISubscriptionProduct item2 in subscriptionProducts)
			{
				if (item2 is IRedboxPlusSubscriptionProduct item)
				{
					list.Add(item);
				}
			}
			return list;
		}
	}

	public ErrorList GetProduct(long productId, bool excludeTitleRollup, out IKioskProduct kioskProduct)
	{
		WaitForFirstLoad();
		kioskProduct = null;
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				kioskProduct = _kioskProductView.GetProduct(productId, excludeTitleRollup);
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DataService.GetProduct", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public ErrorList GetProductByBarcode(string barcode, out IKioskProduct kioskProduct)
	{
		WaitForFirstLoad();
		kioskProduct = null;
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				kioskProduct = _kioskProductView.GetProductByBarcode(barcode);
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DataService.GetProductByBarcode", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public bool IsMultiDisc(long productId)
	{
		bool result = false;
		if (_kioskProductView != null)
		{
			return _kioskProductView?.GetMDVProductGroup(productId) != null;
		}
		return result;
	}

	public List<string> GetAllUnmatchedMultiDiscBarcodes()
	{
		List<string> result = new List<string>();
		if (_kioskProductView != null)
		{
			result = _kioskProductView?.GetAllUnmatchedMultiDiscBarcodes();
		}
		return result;
	}

	public ErrorList GetProducts(BrowseViewFilter browseFilter, TitleFamily titleFamily, TitleType titleType, BrowseSort browseSort, long? genreFilter, long? ratingFilter, List<long> include, List<long> exclude, bool isADAView, decimal? priceRangeLow, decimal? priceRangeHigh, out List<IKioskProduct> products, string studio = null, List<TitleType> excludeTitleTypes = null, bool excludeDiscountRestriction = false)
	{
		WaitForFirstLoad();
		products = null;
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				products = _kioskProductView.GetProducts(browseFilter, titleFamily, titleType, browseSort, genreFilter, ratingFilter, include, exclude, isADAView, priceRangeLow, priceRangeHigh, studio, excludeTitleTypes, excludeDiscountRestriction);
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DataService.GetProducts", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public List<ITitleProduct> GetMovieBrowseProducts()
	{
		WaitForFirstLoad();
		List<ITitleProduct> result = new List<ITitleProduct>();
		new ErrorList();
		try
		{
			lock (_lock)
			{
				result = (from x in _kioskProductView.GetProducts(BrowseViewFilter.All, TitleFamily.Movies, TitleType.All, BrowseSort.SortDate, null, null, null, null, isADAView: false, null, null)
					where x is ITitleProduct
					select x as ITitleProduct).ToList();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("DataService.GetMovieBrowseProducts - Unhandled exception occurred", e);
		}
		return result;
	}

	public ErrorList GetProductsForCustomerOffer(TitleFamily titleFamily, List<TitleType> titleTypes, bool includeTitleTypes, List<long> include, List<long> exclude, TimeSpan? includeOlderThan, bool isAdaView, bool isPurchase, out List<ITitleProduct> products)
	{
		WaitForFirstLoad();
		products = null;
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				products = _kioskProductView.GetProductsForCustomerOffer(titleFamily, titleTypes, includeTitleTypes, include, exclude, includeOlderThan, isAdaView, isPurchase);
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DataService.GetProducts", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public List<IBarcodeForProductResult> GetBarcodesForProducts(List<ISearchBarcodeForProduct> searchProducts, List<string> excludeBarcodesList)
	{
		return _kioskProductView?.GetBarcodesForProducts(searchProducts, excludeBarcodesList);
	}

	public ErrorList GetOutOfStockMovieProducts(out IEnumerable<ITitleProduct> products)
	{
		WaitForFirstLoad();
		products = null;
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				products = _kioskProductView.GetOutOfStockMovies();
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DataService.GetOutOfStockMovieProducts", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public ErrorList GetUnfilteredNonTitleRollupProducts(out IEnumerable<IKioskProduct> products)
	{
		WaitForFirstLoad();
		products = null;
		List<IKioskProduct> outputList = new List<IKioskProduct>();
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				_kioskProductView.TitleProducts.ForEach(delegate(KeyValuePair<long, ITitleProduct> x)
				{
					if (!(x.Value is TitleRollupProduct))
					{
						outputList.Add(x.Value);
					}
				});
			}
			products = outputList;
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DataService.GetProducts", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public ErrorList GetCarouselProducts(int maxCarouselItems, out List<IKioskProduct> products)
	{
		WaitForFirstLoad();
		products = null;
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				products = _kioskProductView.GetCarouselProducts(maxCarouselItems);
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DataService.GetProducts", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public bool GetIsProductThinned(long productId)
	{
		bool result = false;
		PerformActionWhileLocked(delegate
		{
			TitleCache value = null;
			if (_dataServiceInstance != null && _dataServiceInstance.CacheDataInstance != null && _dataServiceInstance.CacheDataInstance.Titles.TryGetValue(productId, out value) && value != null)
			{
				foreach (string key in value.BarcodeProducts.Keys)
				{
					if (value.BarcodeProducts[key] != null && value.BarcodeProducts[key].Status == "Thinned")
					{
						result = true;
					}
				}
			}
		});
		return result;
	}

	public List<long> GetCachedTitleIds()
	{
		List<long> result = new List<long>();
		PerformActionWhileLocked(delegate
		{
			if (_dataServiceInstance?.CacheDataInstance?.Titles != null)
			{
				foreach (KeyValuePair<long, TitleCache> title in _dataServiceInstance.CacheDataInstance.Titles)
				{
					result.Add(title.Value.ProductId);
				}
			}
		});
		return result;
	}

	public int GetInStockGameCount()
	{
		return _kioskProductView?.GetInStockGameCount() ?? 0;
	}

	public TitleType? GetProductTitleType(long productId)
	{
		TitleType? result = null;
		if (_dataServiceInstance?.ProfileDataInstance?.Titles != null && _dataServiceInstance.ProfileDataInstance.Titles.TryGetValue(productId, out var value))
		{
			result = TitleProduct.GetTitleTypeFromProductTypeId(value.ProductTypeId);
		}
		return result;
	}

	public Title GetProductCatalogTitle(long productId)
	{
		Title value = null;
		_dataServiceInstance?.ProfileDataInstance?.Titles?.TryGetValue(productId, out value);
		return value;
	}

	public Dictionary<long, Title> GetProductCatalogTitles()
	{
		return _dataServiceInstance?.ProfileDataInstance?.Titles;
	}

	public List<SubPlatform> GetSubPlatforms()
	{
		List<SubPlatform> list = new List<SubPlatform>();
		if (_dataServiceInstance?.ProfileDataInstance?.SubPlatforms != null)
		{
			foreach (KeyValuePair<long, SubPlatform> subPlatform in _dataServiceInstance.ProfileDataInstance.SubPlatforms)
			{
				list.Add(subPlatform.Value);
			}
		}
		return list;
	}

	public List<ProductType> GetProductTypes()
	{
		List<ProductType> list = new List<ProductType>();
		if (_dataServiceInstance?.ProfileDataInstance?.ProductTypes != null)
		{
			foreach (KeyValuePair<long, ProductType> productType in _dataServiceInstance.ProfileDataInstance.ProductTypes)
			{
				list.Add(productType.Value);
			}
		}
		return list;
	}

	public List<IReservation> GetAllReservations()
	{
		List<IReservation> list = new List<IReservation>();
		if (_dataServiceInstance?.ReservationDataInstance?.DataFiles != null)
		{
			foreach (KeyValuePair<string, IReservationDataFile> dataFile in _dataServiceInstance.ReservationDataInstance.DataFiles)
			{
				IReservation reservationFromReservationDataFile = GetReservationFromReservationDataFile(dataFile.Value);
				if (reservationFromReservationDataFile != null)
				{
					list.Add(reservationFromReservationDataFile);
				}
			}
		}
		return list;
	}

	public List<IReservation> GetReservations(string hashedCardId)
	{
		return (from reservationDataFile in _dataServiceInstance.ReservationDataInstance.DataFiles.Values
			where ((ReservationDataFile_V002)reservationDataFile).HashedCardId == hashedCardId
			select GetReservationFromReservationDataFile(reservationDataFile)).ToList();
	}

	public IReservation GetReservation(string referenceNumber)
	{
		IReservation result = null;
		if (_dataServiceInstance.ReservationDataInstance.DataFiles.TryGetValue(referenceNumber, out var value))
		{
			result = GetReservationFromReservationDataFile(value);
		}
		return result;
	}

	private IReservation GetReservationFromReservationDataFile(IReservationDataFile reservationDataFile)
	{
		IReservation result = null;
		if (reservationDataFile is ReservationDataFile_V002 reservationDataFile2)
		{
			result = ReservationDataInstance.Conversions.ConvertToReservation(reservationDataFile2);
		}
		else
		{
			LogHelper.Instance.Log("Unable to convert IReservationDataFile to instance of ReservationDataFile_VXXX.");
		}
		return result;
	}

	public void DeleteReservation(string referenceNumber)
	{
		if (referenceNumber != null)
		{
			GetReservation(referenceNumber);
			if (_dataServiceInstance.ReservationDataInstance.DataFiles.TryGetValue(referenceNumber, out var value) && value != null)
			{
				LogHelper.Instance.Log("Deleting Reservation data file " + value.FileName);
				_dataServiceInstance.ReservationDataInstance.DataFiles.Remove(referenceNumber);
				_dataServiceInstance.ReservationDataInstance.Delete(value);
			}
		}
		else
		{
			LogHelper.Instance.Log("DeleteReservation - reference number must not be null.");
		}
	}

	public bool SaveReservation(IReservation reservation)
	{
		if (reservation != null && reservation.HashedCardId != null && reservation.ReferenceNumber != null)
		{
			IReservationDataFile reservationDataFile = ReservationDataInstance.Conversions.ConvertToReservationDataFile(reservation);
			_dataServiceInstance.ReservationDataInstance.DataFiles[reservation.ReferenceNumber] = reservationDataFile;
			return _dataServiceInstance.ReservationDataInstance.Save(reservationDataFile);
		}
		return false;
	}

	public Store GetStore()
	{
		long kioskId = ServiceLocator.Instance.GetService<IStoreManager>().KioskId;
		return GetStore(kioskId);
	}

	public Store GetStore(long kioskId)
	{
		Store store;
		if (_dataServiceInstance.ProfileDataInstance.Stores.ContainsKey(kioskId))
		{
			store = _dataServiceInstance.ProfileDataInstance.Stores[kioskId];
			if ((store?.MarketId ?? 0) > 0)
			{
				store.Market = (_dataServiceInstance.ProfileDataInstance.Markets.ContainsKey(store.MarketId) ? _dataServiceInstance.ProfileDataInstance.Markets[store.MarketId] : null);
			}
		}
		else
		{
			store = new Store
			{
				StoreId = 1L
			};
			store.Sellthru = true;
			store.PurchasePrice = 5m;
			store.State = "IL";
			store.PurchaseTaxRate = 8m;
			store.RentalTaxRate = 8m;
			store.ServiceFee = 0.1m;
			store.ServiceFeeTaxRate = 8m;
			store.Zip = "60181";
			LogHelper.Instance.LogError("DataService.RefreshView", "Store is not set in the registry.");
		}
		return store;
	}

	public long GetProductGroupId(IKioskProduct kioskProduct)
	{
		long result = 0L;
		ITitleRollupProduct titleRollupProduct = kioskProduct as ITitleRollupProduct;
		ITitleProduct titleProduct = kioskProduct as ITitleProduct;
		if (titleRollupProduct != null)
		{
			result = ((titleRollupProduct.ProductGroupId > 0) ? titleRollupProduct.ProductGroupId : titleRollupProduct.TitleRollupProductGroupId);
		}
		else if (titleProduct != null)
		{
			result = titleProduct.TitleRollupProductGroupId;
		}
		return result;
	}

	public void AddRemovedQueueItem(RemovedQueueItem item)
	{
		WaitForFirstLoad();
		_localData.AddRemovedQueueItem(item);
	}

	public List<RemovedQueueItem> GetRemovedQueueItems()
	{
		WaitForFirstLoad();
		return _localData.GetRemovedQueueItems();
	}

	public void DeleteRemovedQueueItem(RemovedQueueItem item)
	{
		WaitForFirstLoad();
		_localData.DeleteRemovedQueueItem(item);
	}

	private ErrorList PerformActionWhileLocked(Action action)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				action();
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DataService.PerformActionWhileLocked", "Unhandled exception occurred", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	private void WaitForFirstLoad()
	{
		if (_manualResetEvent == null)
		{
			return;
		}
		try
		{
			LogHelper.Instance.Log("DataService wait for initial data refresh");
			_manualResetEvent.WaitOne(new TimeSpan(0, 15, 0));
			_manualResetEvent.Close();
			_manualResetEvent = null;
			LogHelper.Instance.Log("DataService finished waiting for initial data refresh");
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("DataService.WaitForFirstLoad", "Unhandled exception occurred", ex);
		}
		finally
		{
			_manualResetEvent = null;
		}
	}

	private void Refresh()
	{
		if (Interlocked.CompareExchange(ref _inRefresh, 1, 0) == 1)
		{
			LogHelper.Instance.Log("DataService Refresh - exiting already in refresh");
			return;
		}
		try
		{
			RefreshData();
		}
		finally
		{
			_inRefresh = 0;
			if (_manualResetEvent != null)
			{
				_manualResetEvent.Set();
			}
			if (IsFirstTime)
			{
				IsFirstTime = false;
			}
		}
	}

	private void RefreshData()
	{
		if (IsFirstTime)
		{
			_dataServiceInstance.CacheDataInstance = _cacheData.LoadData();
			_dataServiceInstance.LocalDataInstance = _localData.LoadData();
			_dataServiceInstance.ReservationDataInstance = _reservationData.LoadData();
		}
		IProfileDataInstance profileDataInstance = null;
		if (_profileData.HasStaged() || IsFirstTime)
		{
			_profileData.Activate();
			profileDataInstance = _profileData.LoadDataInstance();
		}
		if (profileDataInstance == null)
		{
			return;
		}
		LogHelper.Instance.Log("DataService RefreshData");
		try
		{
			lock (_lock)
			{
				if (profileDataInstance != null)
				{
					_profileStagedInstance = profileDataInstance;
				}
				if (IsFirstTime)
				{
					LogHelper.Instance.Log("DataService first time refresh if needed");
					UpdateAndRefreshView();
				}
			}
			LogHelper.Instance.Log("DataService RefreshData complete");
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("DataService.RefreshData", "Unhandled exception occurred", ex);
		}
	}

	private void UpdateAndRefreshView()
	{
		if (HasProfileStagedInstance)
		{
			LogHelper.Instance.Log("DataService.UpdateAndRefreshView - Has profile staged instance, updating.");
			_dataServiceInstance.ProfileDataInstance = _profileStagedInstance;
			_profileStagedInstance = null;
			RefreshView();
		}
	}

	private void RefreshView()
	{
		Store store = GetStore();
		IStoreManager service = ServiceLocator.Instance.GetService<IStoreManager>();
		service.SellThru = store.Sellthru;
		service.PurchasePrice = store.PurchasePrice;
		_kioskProductView = new KioskProductView(store, _dataServiceInstance.ProfileDataInstance, _dataServiceInstance.CacheDataInstance, service);
	}
}
