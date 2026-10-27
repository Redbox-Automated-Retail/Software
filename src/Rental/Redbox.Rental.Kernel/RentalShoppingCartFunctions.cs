using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Kernel;

public static class RentalShoppingCartFunctions
{
	[KernelFunction(Name = "kernel.abandon_rental_shopping_cart")]
	public static void AbandonRentalShoppingCart(string id, string reason)
	{
		Guid id2 = ((id != null) ? new Guid(id) : Guid.NewGuid());
		(ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.GetRentalShoppingCart(id2))?.Abandon(reason);
	}

	[KernelFunction(Name = "kernel.create_rental_shopping_cart")]
	public static string CreateRentalShoppingCart(string id)
	{
		Guid id2 = ((id != null) ? new Guid(id) : Guid.NewGuid());
		try
		{
			ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CreateNew(id2);
		}
		catch (Exception ex)
		{
			LogHelper.Instance?.LogEntireException("Unhandled exception in RentalShoppingCartFunctions.CreateRentalShoppingCart.", ex, "CreateRentalShoppingCart");
			throw ex;
		}
		return id2.ToString();
	}

	private static bool GetDecimalValue(LuaTable luaTable, string propertyName, out decimal value)
	{
		bool result = false;
		value = 0m;
		if (luaTable != null)
		{
			object obj = luaTable[propertyName];
			if (obj != null && decimal.TryParse(obj.ToString(), out var result2))
			{
				value = result2;
				result = true;
			}
		}
		return result;
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_clear")]
	public static void Clear(string sessionId)
	{
		ProcessRentalShoppingCartAction(sessionId, delegate(IRentalShoppingCart rentalShoppingCart)
		{
			rentalShoppingCart?.Clear();
		});
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_assign_barcodes_to_vend")]
	public static LuaTable AssignBarcodesToVend(string sessionId)
	{
		List<string> barcodeList = null;
		ProcessRentalShoppingCartAction(sessionId, delegate(IRentalShoppingCart rentalShoppingCart)
		{
			barcodeList = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.AssignBarcodesToVend(rentalShoppingCart);
		});
		return barcodeList?.ToLuaTable();
	}

	private static void ProcessRentalShoppingCartAction(string sessionId, Action<IRentalShoppingCart> rentalShoppingCartAction)
	{
		Guid id = new Guid(sessionId);
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.GetRentalShoppingCart(id);
		if (rentalShoppingCart != null)
		{
			if (rentalShoppingCartAction != null)
			{
				rentalShoppingCartAction(rentalShoppingCart);
				return;
			}
			LogHelper.Instance.Log("Error in RentalShoppingCartFunctions.ProcessRentalShoppingCartAction.  Unable to find rental shopping cart with id {0}", sessionId);
		}
	}

	private static T GetValueFromRentalShoppingCartFunction<T>(string sessionId, Func<IRentalShoppingCart, T> rentalShoppingCartFunction)
	{
		Guid id = new Guid(sessionId);
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.GetRentalShoppingCart(id);
		if (rentalShoppingCart != null)
		{
			if (rentalShoppingCartFunction != null)
			{
				return rentalShoppingCartFunction(rentalShoppingCart);
			}
			LogHelper.Instance.Log("Error in RentalShoppingCartFunctions.ProcessRentalShoppingCartAction.  Unable to find rental shopping cart with id {0}", sessionId);
		}
		return default(T);
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_get_pickup_recommendations_shown")]
	public static bool GetPickupRecommendationsShown(string id)
	{
		bool result = false;
		ProcessRentalShoppingCartAction(id, delegate(IRentalShoppingCart rentalShoppingCart)
		{
			result = rentalShoppingCart.PickupRecommendationsShown;
		});
		return result;
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_remove_non_reserved_products")]
	public static void RemoveNonReservedProductsFromRentalShoppingCart(string sessionId)
	{
		if (sessionId != null)
		{
			Guid shoppingCartSessionId = new Guid(sessionId);
			ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.RemoveNonReservedProductsFromRentalShoppingCart(shoppingCartSessionId);
		}
	}

	[KernelFunction(Name = "kernel.current_rental_shopping_cart_item_count")]
	public static int ItemCount()
	{
		int result = 0;
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		if (service != null)
		{
			result = service.CurrentRentalShoppingCart.Count;
		}
		return result;
	}

	[KernelFunction(Name = "kernel.get_rental_shopping_cart_items")]
	public static LuaTable GetRentalShoppingCartItems(string id)
	{
		LuaTable result = null;
		Guid id2 = new Guid(id);
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.GetRentalShoppingCart(id2);
		if (rentalShoppingCart != null)
		{
			List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
			foreach (IRentalShoppingCartTitleItem titleItem in rentalShoppingCart.TitleItems)
			{
				list.Add(CreateLuaTableFromRentalShoppingCartItem(rentalShoppingCart, titleItem));
			}
			result = list.ToLuaTable();
		}
		return result;
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_clear_promo_code")]
	public static void ClearPromoCode(string sessionId)
	{
		ProcessRentalShoppingCartAction(sessionId, delegate(IRentalShoppingCart rentalShoppingCart)
		{
			rentalShoppingCart.ClearPromoCode();
		});
	}

	private static Dictionary<string, object> CreateLuaTableFromRentalShoppingCartItem(IRentalShoppingCart rentalShoppingCart, IRentalShoppingCartTitleItem rentalShoppingCartItem)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (rentalShoppingCartItem != null)
		{
			dictionary["product_id"] = rentalShoppingCartItem.ProductId;
			dictionary["action"] = rentalShoppingCartItem.Action.ToString();
			dictionary["barcode"] = rentalShoppingCartItem.Barcode;
			dictionary["price"] = rentalShoppingCartItem.Price;
			dictionary["product_type"] = ((rentalShoppingCartItem.TitleType == TitleType.Bluray) ? "Blu-ray" : rentalShoppingCartItem.TitleType.ToString());
			dictionary["product_type_id"] = (int)rentalShoppingCartItem.TitleType;
			dictionary["product_family"] = rentalShoppingCartItem.TitleFamily.ToString();
			dictionary["rating_id"] = (int)rentalShoppingCartItem.Rating;
			dictionary["merchandise_date"] = rentalShoppingCartItem.MerchandiseDate.ToString("yyyyMMddHHmmss");
			dictionary["sort_name"] = rentalShoppingCartItem.SortName;
			dictionary["add_sequence"] = rentalShoppingCartItem.Sequence.ToString();
			dictionary["status"] = rentalShoppingCartItem.Status.ToString();
			dictionary["quantity"] = rentalShoppingCartItem.Quantity;
			dictionary["source_type"] = rentalShoppingCartItem.SourceType.ToString();
			IUpsellItem upsellItem = rentalShoppingCart?.UpsellInfo?.Items.FirstOrDefault((IUpsellItem x) => x.OriginalProductId == rentalShoppingCartItem.ProductId || x.UpsellProductId == rentalShoppingCartItem.ProductId);
			Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
			if (upsellItem != null)
			{
				dictionary2.Add("product_group_id", upsellItem.ProductGroupId);
				dictionary2.Add("offer", upsellItem.OfferResponse.ToString());
				dictionary2.Add("title_id", rentalShoppingCartItem.ProductId);
			}
			dictionary["blu_ray_upsell"] = dictionary2.ToLuaTable();
			dictionary["multi_disc"] = rentalShoppingCartItem.MultiDisc.ToString();
			dictionary["pricing_policy"] = ((rentalShoppingCartItem.PricingRecord != null) ? rentalShoppingCartItem.PricingRecord.ToDictionary().ToLuaTable() : new Dictionary<string, object>().ToLuaTable());
		}
		return dictionary;
	}

	private static string GetLuaTableStringValue(LuaTable luaTable, string propertyName)
	{
		string result = null;
		if (luaTable != null && luaTable[propertyName] != null)
		{
			result = luaTable[propertyName].ToString();
		}
		return result;
	}
}
