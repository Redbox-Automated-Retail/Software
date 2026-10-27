using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Profile;

namespace Redbox.Rental.Data;

public class ProfileData : IProfileData
{
	private string _dataPath;

	private string _profileStagedPath;

	private string _profileActivePath;

	private const string ProfileStagedName = "profile.active";

	private const string ProfileActiveName = "profile.data";

	private string _imagesPath;

	private string _imagesDirectoryName;

	private string _keyValueSql = "SELECT [Key], [Value] FROM {0}";

	private string _keyValueSqlKey = "SELECT [Key], [Value] FROM {0} WHERE [Key] = '{1}'";

	public ProfileData(string dataPath, string imagesDirectoryName)
	{
		_dataPath = dataPath;
		_imagesDirectoryName = imagesDirectoryName;
		_profileStagedPath = Path.Combine(_dataPath, "profile.active");
		_profileActivePath = Path.Combine(_dataPath, "profile.data");
		_imagesPath = Path.Combine(_dataPath, _imagesDirectoryName);
	}

	public bool HasStaged()
	{
		return File.Exists(_profileStagedPath);
	}

	public bool HasActive()
	{
		return File.Exists(_profileActivePath);
	}

	public bool Activate()
	{
		if (!HasStaged())
		{
			return false;
		}
		try
		{
			if (File.Exists(_profileActivePath))
			{
				File.Delete(_profileActivePath);
			}
			File.Move(_profileStagedPath, _profileActivePath);
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("ProfileData.Activate - Unhandled exception occurred", e);
			return false;
		}
	}

	public IProfileDataInstance LoadDataInstance()
	{
		ProfileDataInstance profileDataInstance = new ProfileDataInstance();
		profileDataInstance.ImagesPath = _imagesPath;
		using (VistaDBDatabaseProvider vistaDBDatabaseProvider = new VistaDBDatabaseProvider("profile.data", _profileActivePath))
		{
			if (vistaDBDatabaseProvider.Open())
			{
				LoadMarkets(vistaDBDatabaseProvider, profileDataInstance);
				LoadTitles(vistaDBDatabaseProvider, profileDataInstance);
				LoadProductFamily(vistaDBDatabaseProvider, profileDataInstance);
				LoadProductGroup(vistaDBDatabaseProvider, profileDataInstance);
				LoadProductType(vistaDBDatabaseProvider, profileDataInstance);
				LoadStore(vistaDBDatabaseProvider, profileDataInstance);
				LoadSubPlatform(vistaDBDatabaseProvider, profileDataInstance);
				LoadVersion(vistaDBDatabaseProvider, profileDataInstance);
				ExtractImages(vistaDBDatabaseProvider, profileDataInstance);
				vistaDBDatabaseProvider.Close();
			}
			else
			{
				ServiceLocator.Instance.GetService<IEnvironmentNotificationService>()?.RaiseCorruptDb();
			}
		}
		return profileDataInstance;
	}

	private void LoadMarkets(VistaDBDatabaseProvider db, ProfileDataInstance instance)
	{
		DataTable sqlData = db.GetSqlData(string.Format(_keyValueSql, "Market"));
		ConvertToList(sqlData, "LoadMarkets", delegate(string key, string value)
		{
			Dictionary<string, object> dictionary = value.ParseLuaValue();
			Market market = new Market
			{
				MarketId = dictionary.ToInt64("market_id"),
				MarketName = dictionary.ToString("market_name")
			};
			instance.Markets[market.MarketId] = market;
		});
	}

	private void LoadTitles(VistaDBDatabaseProvider db, ProfileDataInstance instance)
	{
		DataTable sqlData = db.GetSqlData(string.Format(_keyValueSql, "ProductCatalog"));
		ConvertToList(sqlData, "LoadTitles", delegate(string key, string value)
		{
			Dictionary<string, object> dictionary = value.ParseLuaValue();
			Title item = new Title
			{
				ProductId = dictionary.ToInt64("product_id"),
				RunningTime = dictionary.ToString("running_time"),
				Starring = dictionary.ToListString("starring"),
				ProductTypeId = dictionary.ToInt32("product_type_id"),
				RatingId = dictionary.ToInt64("rating_id"),
				LongName = dictionary.ToString("long_name"),
				SortName = dictionary.ToString("sort_name"),
				ImageFile = dictionary.ToString("image_file"),
				Description = dictionary.ToString("description"),
				ReleaseDate = dictionary.ToDateTime("release_date", null),
				SellThru = dictionary.ToBoolean("sell_thru"),
				SellThruNew = dictionary.ToBoolean("sell_thru_new"),
				BoxOfficeGross = dictionary.ToDecimal("box_office_gross"),
				Directors = dictionary.ToListString("directors"),
				ComingSoonDays = dictionary.ToInt32("coming_soon_days"),
				SortDate = dictionary.ToDateTime("sort_date", null),
				NationalStreetDate = dictionary.ToDateTime("national_street_date", null),
				ClosedCaptioned = dictionary.ToBoolean("closed_captioned"),
				MerchandiseDate = dictionary.ToDateTime("merchandise_date", null),
				SubPlatformId = dictionary.ToInt64("sub_platform_id", null),
				NumberOfPlayersText = dictionary.ToString("number_of_players_text"),
				DoNotRentDate = dictionary.ToDateTime("do_not_rent_date", null),
				SellThruDate = dictionary.ToDateTime("sellthru_date", null),
				SellThruPrice = dictionary.ToDecimal("sellthru_price", null),
				DiscNumber = dictionary.ToInt32("disc_number", null),
				DigitalHDRentPrice = dictionary.ToDecimal("digital_hd_rent_price", null),
				DigitalSDRentPrice = dictionary.ToDecimal("digital_sd_rent_price", null),
				Studio = dictionary.ToString("studio"),
				RedboxPlusEligibleDate = dictionary.ToDateTime("redbox_plus_eligible_date", null)
			};
			if (!Enum.TryParse<DiscountRestriction>(dictionary.ToString("discount_restriction"), out var result))
			{
				result = DiscountRestriction.None;
			}
			item.DiscountRestriction = result;
			item.Genres = new List<long>();
			dictionary.ToDictionaryObject("genres").ForEach(delegate(object g)
			{
				if (g is KeyValuePair<string, object> keyValuePair && keyValuePair.Value is bool && (bool)keyValuePair.Value)
				{
					item.Genres.Add(Convert.ToInt64(keyValuePair.Key));
				}
			});
			instance.Titles[item.ProductId] = item;
		});
	}

	private void LoadProductFamily(VistaDBDatabaseProvider db, ProfileDataInstance instance)
	{
		DataTable sqlData = db.GetSqlData(string.Format(_keyValueSql, "ProductFamily"));
		ConvertToList(sqlData, "LoadProductFamily", delegate(string key, string value)
		{
			Dictionary<string, object> dictionary = value.ParseLuaValue();
			ProductFamily item = new ProductFamily
			{
				ProductFamilyId = dictionary.ToInt64("product_family_id"),
				ProductFamilyName = dictionary.ToString("product_family_name")
			};
			item.Genres = new List<long>();
			dictionary.ToDictionaryObject("genres").ForEach(delegate(object g)
			{
				if (g is KeyValuePair<string, object> keyValuePair && keyValuePair.Value is bool && (bool)keyValuePair.Value)
				{
					item.Genres.Add(Convert.ToInt64(keyValuePair.Key));
				}
			});
			instance.ProductFamilies[item.ProductFamilyId] = item;
		});
	}

	private void LoadProductGroup(VistaDBDatabaseProvider db, ProfileDataInstance instance)
	{
		DataTable sqlData = db.GetSqlData(string.Format(_keyValueSql, "ProductGroup"));
		ConvertToList(sqlData, "LoadProductGroup", delegate(string key, string value)
		{
			Dictionary<string, object> dictionary = value.ParseLuaValue();
			ProductGroup productGroup = new ProductGroup
			{
				ProductGroupId = dictionary.ToInt64("product_group_id"),
				ProductGroupName = dictionary.ToString("product_group_name"),
				ProductGroupTypeId = dictionary.ToInt32("product_group_type_id"),
				ProductIds = dictionary.ToListLong("product_id_list")
			};
			instance.ProductGroups[productGroup.ProductGroupId] = productGroup;
		});
		instance.ProductGroups.Where((KeyValuePair<long, ProductGroup> pair) => pair.Value.ProductGroupTypeId == 1).ForEach(delegate(KeyValuePair<long, ProductGroup> pair)
		{
			pair.Value.ProductIds.ForEach(delegate(long pn)
			{
				instance.TitleRollupXREF[pn] = new ProductGroupXREF
				{
					ProductId = pn,
					ProductGroupId = pair.Value.ProductGroupId
				};
			});
		});
		instance.ProductGroups.Where((KeyValuePair<long, ProductGroup> pair) => pair.Value.ProductGroupTypeId == 3).ForEach(delegate(KeyValuePair<long, ProductGroup> pair)
		{
			pair.Value.ProductIds.ForEach(delegate(long pn)
			{
				instance.MultiDiscXREF[pn] = new ProductGroupXREF
				{
					ProductId = pn,
					ProductGroupId = pair.Value.ProductGroupId
				};
			});
		});
		instance.ProductGroups.Where((KeyValuePair<long, ProductGroup> pair) => pair.Value.ProductGroupTypeId == 6).ForEach(delegate(KeyValuePair<long, ProductGroup> pair)
		{
			pair.Value.ProductIds.ForEach(delegate(long pn)
			{
				instance.MultiDiscWorkaroundXREF[pn] = new ProductGroupXREF
				{
					ProductId = pn,
					ProductGroupId = pair.Value.ProductGroupId
				};
			});
		});
	}

	private void LoadProductType(VistaDBDatabaseProvider db, ProfileDataInstance instance)
	{
		DataTable sqlData = db.GetSqlData(string.Format(_keyValueSql, "ProductType"));
		ConvertToList(sqlData, "LoadProductType", delegate(string key, string value)
		{
			Dictionary<string, object> dictionary = value.ParseLuaValue();
			ProductType productType = new ProductType
			{
				ProductTypeId = dictionary.ToInt64("product_type_id"),
				ProductTypeName = dictionary.ToString("product_type_name"),
				ProductFamilyId = dictionary.ToInt64("product_family_id")
			};
			instance.ProductTypes[productType.ProductTypeId] = productType;
		});
	}

	private void LoadStore(VistaDBDatabaseProvider db, ProfileDataInstance instance)
	{
		long num = ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId ?? 0;
		DataTable data = ((num <= 0) ? db.GetSqlData(string.Format(_keyValueSql, "Store")) : db.GetSqlData(string.Format(_keyValueSqlKey, "Store", num)));
		ConvertToList(data, "LoadStore", delegate(string key, string value)
		{
			Dictionary<string, object> dictionary = value.ParseLuaValue();
			Store store = new Store
			{
				StoreId = dictionary.ToInt64("store_id"),
				Address = dictionary.ToString("address"),
				ReturnTime = dictionary.ToString("return_time", "09:00PM"),
				RentalTaxRate = dictionary.ToDecimal("rental_tax_rate"),
				PurchaseTaxRate = dictionary.ToDecimal("purchase_tax_rate"),
				PurchasePrice = dictionary.ToDecimal("purchase_price"),
				VendorId = dictionary.ToInt64("vendor_id"),
				MarketId = dictionary.ToInt64("market_id"),
				BannerId = dictionary.ToInt64("banner_id"),
				Sellthru = dictionary.ToBoolean("sell_thru"),
				OpenDate = dictionary.ToDateTime("open_date", null),
				InvSync = dictionary.ToBoolean("inv_sync"),
				Address1 = dictionary.ToString("address1"),
				Address2 = dictionary.ToString("address2"),
				City = dictionary.ToString("city"),
				County = dictionary.ToString("county"),
				State = dictionary.ToString("state"),
				Zip = dictionary.ToString("zip"),
				CollectionMethod = dictionary.ToInt32("collection_method", 6),
				ServiceFee = dictionary.ToDecimal("service_fee"),
				ServiceFeeTaxRate = dictionary.ToDecimal("service_fee_tax_rate"),
				ReturnOnlyModeDate = dictionary.ToDateTime("return_only_mode_date", null)
			};
			decimal? num2 = dictionary.ToDecimal("digital_purchase_tax_rate", null);
			if (num2.HasValue)
			{
				store.DigitalPurchaseTaxRate = num2.Value;
			}
			else
			{
				store.DigitalPurchaseTaxRate = store.PurchaseTaxRate;
			}
			instance.Stores[store.StoreId] = store;
		});
	}

	private void LoadSubPlatform(VistaDBDatabaseProvider db, ProfileDataInstance instance)
	{
		DataTable sqlData = db.GetSqlData(string.Format(_keyValueSql, "SubPlatform"));
		ConvertToList(sqlData, "LoadSubPlatform", delegate(string key, string value)
		{
			Dictionary<string, object> dictionary = value.ParseLuaValue();
			SubPlatform subPlatform = new SubPlatform
			{
				SubPlatformId = dictionary.ToInt64("subplatform_id"),
				SubPlatformName = dictionary.ToString("subplatform_name")
			};
			instance.SubPlatforms[subPlatform.SubPlatformId] = subPlatform;
		});
	}

	private void LoadVersion(VistaDBDatabaseProvider db, ProfileDataInstance instance)
	{
		DataTable sqlData = db.GetSqlData(string.Format(_keyValueSql, "Version"));
		instance.ProfileDataVersion = new ProfileDataVersion();
		ConvertToList(sqlData, "LoadVersion", delegate(string key, string value)
		{
			Dictionary<string, object> dictionary = value.ParseLuaValue();
			if (key.Equals("origin_machine", StringComparison.CurrentCultureIgnoreCase))
			{
				instance.ProfileDataVersion.OriginMachine = dictionary.ToString("name");
			}
			if (key.Equals("generated_on", StringComparison.CurrentCultureIgnoreCase))
			{
				instance.ProfileDataVersion.GeneratedOn = dictionary.ToDateTime("date", null).Value;
			}
		});
	}

	private void ExtractImages(VistaDBDatabaseProvider db, ProfileDataInstance instance)
	{
		LogHelper.Instance.Log("ProfileData.ExtractImages - Loading");
		if (!Directory.Exists(_imagesPath))
		{
			Directory.CreateDirectory(_imagesPath);
		}
		DataTable sqlData = db.GetSqlData(" SELECT [NAME] FROM CACHE");
		if (sqlData == null)
		{
			return;
		}
		foreach (DataRow row in sqlData.Rows)
		{
			string name = Convert.ToString(row["NAME"]);
			string path = Path.Combine(_imagesPath, name);
			if (File.Exists(path))
			{
				continue;
			}
			TryCatchData("ProfileData.ExtractImages", name, delegate
			{
				DataTable sqlData2 = db.GetSqlData($"SELECT DATA FROM CACHE WHERE [NAME] = '{name}'", nolog: true);
				if (sqlData2.Rows.Count > 0)
				{
					byte[] array = (byte[])sqlData2.Rows[0]["Data"];
					using FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
					fileStream.Write(array, 0, array.Length);
					fileStream.Close();
				}
			});
		}
	}

	private void ConvertToList(DataTable data, string name, Action<string, string> action)
	{
		LogHelper.Instance.Log("ProfileData.{0} - Loading", name);
		if (data != null)
		{
			int columnIndex = data.Columns.IndexOf("Key");
			int columnIndex2 = data.Columns.IndexOf("Value");
			{
				foreach (DataRow row in data.Rows)
				{
					string arg = Convert.ToString(row[columnIndex]);
					string arg2 = Convert.ToString(row[columnIndex2]);
					try
					{
						action(arg, arg2);
					}
					catch (Exception e)
					{
						LogHelper.Instance.Log($"ProfileData.{name} - Unhandled exception occurred, skipping Key: {arg} Value: {arg2}", e);
					}
				}
				return;
			}
		}
		LogHelper.Instance.Log("Profile.{0} - data is null", name);
	}

	private void TryCatchData(string name, string data, Action action)
	{
		try
		{
			action();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"{name} - Unhandled exception occurred, skipping {data}", e);
		}
	}
}
