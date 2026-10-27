using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Cache;

namespace Redbox.Rental.Data;

public class CacheData : ICacheData
{
	public ICacheDataInstance LoadData()
	{
		CacheDataInstance cacheDataInstance = new CacheDataInstance();
		LoadBarcodePosition(cacheDataInstance);
		LoadProductCatalog(cacheDataInstance);
		LoadProductHistory(cacheDataInstance);
		LoadProductToBarcode(cacheDataInstance);
		return cacheDataInstance;
	}

	private void LoadBarcodePosition(ICacheDataInstance instance)
	{
		GetAll("BarcodePosition").ForEach(delegate(KeyValuePair<string, string> item)
		{
			TryCatchData("CacheData.LoadBarcodePosition", item, delegate
			{
				BarcodeProduct barcodePosition = GetBarcodePosition(instance, item, out var _);
				instance.BarcodeProducts[barcodePosition.Barcode] = barcodePosition;
			});
		});
	}

	public BarcodeProduct UpdateBarcodePosition(ICacheDataInstance instance, string key, object data, out bool isNew)
	{
		bool isNew2 = false;
		BarcodeProduct barcodeProduct = null;
		KeyValuePair<string, string> pair = new KeyValuePair<string, string>(key, Convert.ToString(data));
		TryCatchData("CacheData.UpdateBarcodePosition", pair, delegate
		{
			barcodeProduct = GetBarcodePosition(instance, pair, out isNew2);
			instance.BarcodeProducts[barcodeProduct.Barcode] = barcodeProduct;
		});
		isNew = isNew2;
		return barcodeProduct;
	}

	public void RemoveBarcodePosition(ICacheDataInstance instance, string key)
	{
	}

	private BarcodeProduct GetBarcodePosition(ICacheDataInstance instance, KeyValuePair<string, string> pair, out bool isNew)
	{
		isNew = false;
		Dictionary<string, object> dictionary = pair.Value.ParseLuaValue();
		string text = dictionary.ToString("barcode");
		if (!instance.BarcodeProducts.TryGetValue(text, out var value))
		{
			isNew = true;
			string[] array = pair.Key.Split('@');
			value = new BarcodeProduct
			{
				Position = pair.Key,
				Deck = Convert.ToInt32(array[0]),
				Slot = Convert.ToInt32(array[1])
			};
		}
		value.Barcode = text;
		value.Status = dictionary.ToString("status");
		return value;
	}

	private void LoadProductCatalog(ICacheDataInstance instance)
	{
		GetAll("ProductCatalog").ForEach(delegate(KeyValuePair<string, string> item)
		{
			TryCatchData("CacheData.LoadProductCatalog", item, delegate
			{
				TitleCache productCatalog = GetProductCatalog(instance, item, out var _);
				instance.Titles[productCatalog.ProductId] = productCatalog;
			});
		});
	}

	public TitleCache UpdateProductCatalog(ICacheDataInstance instance, string key, object data, out bool isNew)
	{
		bool isNew2 = false;
		TitleCache titleCache = null;
		KeyValuePair<string, string> pair = new KeyValuePair<string, string>(key, Convert.ToString(data));
		TryCatchData("CacheData.UpdateProductCatalog", pair, delegate
		{
			titleCache = GetProductCatalog(instance, pair, out isNew2);
			instance.Titles[titleCache.ProductId] = titleCache;
		});
		isNew = isNew2;
		return titleCache;
	}

	public void RemoveProductCatalog(ICacheDataInstance instance, string key)
	{
		KeyValuePair<string, string> pair = new KeyValuePair<string, string>(key, string.Empty);
		TryCatchData("CacheData.RemoveProductCatalog", pair, delegate
		{
			instance.Titles.Remove(Convert.ToInt64(key));
		});
	}

	private TitleCache GetProductCatalog(ICacheDataInstance instance, KeyValuePair<string, string> pair, out bool isNew)
	{
		isNew = false;
		Dictionary<string, object> dictionary = pair.Value.ParseLuaValue();
		if (!instance.Titles.TryGetValue(Convert.ToInt64(pair.Key), out var value))
		{
			isNew = true;
			value = new TitleCache
			{
				ProductId = Convert.ToInt64(pair.Key)
			};
		}
		value.IsComingSoon = dictionary.ToBoolean("is_coming_soon");
		value.IsAvailable = dictionary.ToBoolean("is_available");
		value.ProductFamilyName = dictionary.ToString("product_family_name");
		value.InStock = dictionary.ToBoolean("in_stock");
		value.ProductTypeName = dictionary.ToString("product_type_name");
		return value;
	}

	private TitleCache AddTitleCache(ICacheDataInstance instance, long productId)
	{
		if (!instance.Titles.TryGetValue(Convert.ToInt64(productId), out var value))
		{
			value = new TitleCache
			{
				ProductId = Convert.ToInt64(productId)
			};
			instance.Titles[value.ProductId] = value;
		}
		return value;
	}

	private void LoadProductHistory(ICacheDataInstance instance)
	{
		GetAll("ProductHistory").ForEach(delegate(KeyValuePair<string, string> item)
		{
			TryCatchData("CacheData.LoadProductHistory", item, delegate
			{
				TitleCache productHistory = GetProductHistory(instance, item, out var _);
				instance.Titles[productHistory.ProductId] = productHistory;
			});
		});
	}

	public TitleCache UpdateProductHistory(ICacheDataInstance instance, string key, object data, out bool isNew)
	{
		bool isNew2 = false;
		TitleCache titleCache = null;
		KeyValuePair<string, string> pair = new KeyValuePair<string, string>(key, Convert.ToString(data));
		TryCatchData("CacheData.UpdateProductHistory", pair, delegate
		{
			titleCache = GetProductHistory(instance, pair, out isNew2);
			instance.Titles[titleCache.ProductId] = titleCache;
		});
		isNew = isNew2;
		return titleCache;
	}

	public void RemoveProductHistory(ICacheDataInstance instance, string key)
	{
		KeyValuePair<string, string> pair = new KeyValuePair<string, string>(key, string.Empty);
		TryCatchData("CacheData.RemoveProductHistory", pair, delegate
		{
			instance.Titles.Remove(Convert.ToInt64(key));
		});
	}

	private TitleCache GetProductHistory(ICacheDataInstance instance, KeyValuePair<string, string> pair, out bool isNew)
	{
		isNew = false;
		Dictionary<string, object> data = pair.Value.ParseLuaValue();
		if (!instance.Titles.TryGetValue(Convert.ToInt64(pair.Key), out var value))
		{
			isNew = true;
			value = new TitleCache
			{
				ProductId = Convert.ToInt64(pair.Key)
			};
		}
		value.OfrCount = data.ToInt32("ofr_count");
		value.LastMaxOfr = data.ToInt32("last_max_ofr", 1).Value;
		value.OnHandCount = data.ToInt32("on_hand_count");
		value.CreatedDate = data.ToDateTime("created_date", DateTime.Now).Value;
		value.CurrentMaxOfr = data.ToInt32("current_max_ofr");
		value.LastMaxOfrDate = data.ToDateTime("product_type_name", DateTime.Now).Value;
		value.ThinnedOut = data.ToBoolean("thinned_out");
		value.LastDateOutOfStock = data.ToDateTime("last_date_out_of_stock", null);
		value.SoldOut = data.ToDateTime("sold_out", null);
		return value;
	}

	private void LoadProductToBarcode(ICacheDataInstance instance)
	{
		GetAll("ProductToBarcode").ForEach(delegate(KeyValuePair<string, string> item)
		{
			bool isNew;
			TryCatchData("CacheData.LoadProductToBarcode", item, delegate
			{
				UpdateProductToBarcode(instance, item, out isNew);
			});
		});
	}

	public void RemoveProductToBarcode(ICacheDataInstance instance, string key)
	{
		KeyValuePair<string, string> pair = new KeyValuePair<string, string>(key, string.Empty);
		TryCatchData("CacheData.RemoveProductToBarcode", pair, delegate
		{
			RemoveProductToBarcode(instance, pair);
		});
	}

	private void RemoveProductToBarcode(ICacheDataInstance instance, KeyValuePair<string, string> pair)
	{
		if (!instance.Titles.TryGetValue(Convert.ToInt64(pair.Key), out var value))
		{
			LogHelper.Instance.LogError("CacheData.RemoveProductToBarcode", $"TitleCache does not exist for title_id: {pair.Key}");
		}
		else
		{
			value.BarcodeProducts.Clear();
		}
	}

	public TitleCache UpdateProductToBarcode(ICacheDataInstance instance, string key, object data, out bool isNew)
	{
		bool isNew2 = false;
		TitleCache titleCache = null;
		KeyValuePair<string, string> pair = new KeyValuePair<string, string>(key, Convert.ToString(data));
		TryCatchData("CacheData.UpdateProductToBarcode", pair, delegate
		{
			titleCache = UpdateProductToBarcode(instance, pair, out isNew2);
		});
		isNew = isNew2;
		return titleCache;
	}

	private TitleCache UpdateProductToBarcode(ICacheDataInstance instance, KeyValuePair<string, string> pair, out bool isNew)
	{
		isNew = false;
		Dictionary<string, object> valueData = pair.Value.ParseLuaValue();
		if (!instance.Titles.TryGetValue(Convert.ToInt64(pair.Key), out var titleCache))
		{
			titleCache = AddTitleCache(instance, Convert.ToInt64(pair.Key));
			isNew = true;
		}
		List<BarcodeProduct> list = new List<BarcodeProduct>();
		titleCache.BarcodeProducts.ForEach(delegate(KeyValuePair<string, BarcodeProduct> item)
		{
			list.Add(item.Value);
		});
		list.ForEach(delegate(BarcodeProduct item)
		{
			if (!valueData.ContainsKey(item.Barcode) && titleCache.BarcodeProducts.ContainsKey(item.Barcode))
			{
				_ = titleCache.BarcodeProducts[item.Barcode];
				titleCache.BarcodeProducts.Remove(item.Barcode);
			}
		});
		valueData.ForEach(delegate(KeyValuePair<string, object> b)
		{
			string key = b.Key;
			if (!instance.BarcodeProducts.TryGetValue(key, out var value))
			{
				LogHelper.Instance.LogError("CacheData.LoadProductToBarcode", $"BarcodeProduct does not exist for barcode: {key}");
			}
			else
			{
				titleCache.BarcodeProducts[key] = value;
			}
		});
		return titleCache;
	}

	private void TryCatchData(string name, KeyValuePair<string, string> pair, Action action)
	{
		try
		{
			action();
		}
		catch (Exception e)
		{
			try
			{
				LogHelper.Instance.Log($"{name} - Unhandled exception occurred, skipping {$"Key: {pair.Key} Value: {pair.Value}"}", e);
			}
			catch (Exception e2)
			{
				LogHelper.Instance.Log("CacheData.TryCatchData - An unhandled exception ocurred", e2);
			}
		}
	}

	private Dictionary<string, string> GetAll(string tableName)
	{
		Dictionary<string, string> dict = new Dictionary<string, string>();
		try
		{
			IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
			if (service == null)
			{
				return dict;
			}
			Dictionary<string, object> all = service.GetAll("cache", tableName);
			if (all == null)
			{
				return dict;
			}
			all.ForEach(delegate(KeyValuePair<string, object> item)
			{
				if (item.Value is string)
				{
					dict[item.Key] = Convert.ToString(item.Value);
				}
			});
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"CacheData.GetAll{tableName} - An unhandled exception ocurred", e);
		}
		return dict;
	}
}
