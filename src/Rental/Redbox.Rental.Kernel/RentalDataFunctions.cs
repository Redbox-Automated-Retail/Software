using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.Profile;

namespace Redbox.Rental.Kernel;

public static class RentalDataFunctions
{
	[KernelFunction(Name = "kernel.rental_get_default_culture")]
	internal static string GetDefaultCulture()
	{
		IStoreManager service = ServiceLocator.Instance.GetService<IStoreManager>();
		if (service == null)
		{
			LogHelper.Instance.Log("IStoreManager interface has not been added to the service locater.");
			return null;
		}
		string defaultCulture = service.GetDefaultCulture();
		LogHelper.Instance.Log("StoreManager.GetDefaultCulture({0})", defaultCulture);
		return defaultCulture;
	}

	[KernelFunction(Name = "kernel.rental_get_alternate_culture")]
	internal static string GetAlternateCulture()
	{
		IStoreManager service = ServiceLocator.Instance.GetService<IStoreManager>();
		if (service == null)
		{
			LogHelper.Instance.Log("IStoreManager interface has not been added to the service locater.");
			return null;
		}
		string alternateCulture = service.GetAlternateCulture();
		LogHelper.Instance.Log("StoreManager.GetAlternateCulture({0})", alternateCulture);
		return alternateCulture;
	}

	[KernelFunction(Name = "kernel.get_can_sell_used")]
	internal static bool GetCanSellUsed()
	{
		try
		{
			return ServiceLocator.Instance.GetService<IStoreManager>().CanSellUsed;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in RentalDataFunctions.GetCanSellUsed", e);
			return false;
		}
	}

	[KernelFunction(Name = "kernel.get_all_unmatched_multi_disc_barcodes")]
	internal static LuaTable GetAllUnmatchedMultiDiscBarcodes()
	{
		try
		{
			LuaTable result = null;
			IDataService service = ServiceLocator.Instance.GetService<IDataService>();
			if (service != null)
			{
				result = service.GetAllUnmatchedMultiDiscBarcodes().ToLuaTable();
			}
			return result;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in RentalDataFunctions.GetAllUnmatchedMultiDiscBarcodes", e);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.get_product_type")]
	internal static LuaTable GetProductType(long productTypeId)
	{
		try
		{
			LuaTable result = null;
			ProductType productType = (ServiceLocator.Instance.GetService<IDataService>()?.GetProductTypes())?.FirstOrDefault((ProductType x) => x.ProductTypeId == productTypeId);
			if (productType != null)
			{
				result = new Dictionary<string, object>
				{
					["product_type_id"] = productType.ProductTypeId,
					["product_type_name"] = productType.ProductTypeName,
					["product_family_id"] = productType.ProductFamilyId
				}.ToLuaTable();
			}
			return result;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in RentalDataFunctions.GetProductType", e);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.get_product_catalog_title")]
	internal static LuaTable GetProductCatalogTitle(long productId)
	{
		try
		{
			LuaTable result = null;
			Title title = ServiceLocator.Instance.GetService<IDataService>()?.GetProductCatalogTitle(productId);
			if (title != null)
			{
				result = new Dictionary<string, object>
				{
					["product_id"] = title.ProductId,
					["merchandise_date"] = title.MerchandiseDate,
					["coming_soon_days"] = title.ComingSoonDays,
					["long_name"] = title.LongName,
					["do_not_rent_date"] = title.DoNotRentDate,
					["sort_name"] = title.SortName,
					["product_type_id"] = title.ProductTypeId,
					["rating_id"] = title.RatingId,
					["disc_number"] = title.DiscNumber
				}.ToLuaTable();
			}
			return result;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in RentalDataFunctions.GetProductCatalogTitle", e);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.get_coming_soon_product_ids")]
	internal static LuaTable GetComingSoonProductIds()
	{
		try
		{
			LuaTable result = null;
			Dictionary<long, Title> dictionary = ServiceLocator.Instance.GetService<IDataService>()?.GetProductCatalogTitles();
			if (dictionary != null)
			{
				List<long> list = new List<long>();
				foreach (KeyValuePair<long, Title> item in dictionary)
				{
					Title value = item.Value;
					if (value.ComingSoonDays > 0 && value.MerchandiseDate.HasValue)
					{
						double totalDays = (value.MerchandiseDate.Value - DateTime.Today).TotalDays;
						if (!(totalDays < 0.0) && totalDays <= (double)value.ComingSoonDays)
						{
							list.Add(value.ProductId);
						}
					}
				}
				result = list.ToLuaTable();
			}
			return result;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in RentalDataFunctions.GetComingSoonProductIds", e);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.get_store")]
	internal static LuaTable GetStore(long kioskId)
	{
		try
		{
			LuaTable result = null;
			Store store = ServiceLocator.Instance.GetService<IDataService>()?.GetStore(kioskId);
			if (store != null)
			{
				result = new Dictionary<string, object>
				{
					["store_id"] = store.StoreId,
					["zip"] = store.Zip,
					["return_time"] = store.ReturnTime,
					["state"] = store.State
				}.ToLuaTable();
			}
			return result;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in RentalDataFunctions.GetStore", e);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.get_profile_version")]
	internal static LuaTable GetProfileVersion()
	{
		try
		{
			LuaTable result = null;
			IDataService service = ServiceLocator.Instance.GetService<IDataService>();
			if (service?.ProfileDataVersion != null)
			{
				result = new Dictionary<string, object>
				{
					["origin_machine"] = service.ProfileDataVersion.OriginMachine,
					["generated_on"] = service.ProfileDataVersion.GeneratedOn
				}.ToLuaTable();
			}
			return result;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in RentalDataFunctions.GetStore", e);
			return null;
		}
	}
}
