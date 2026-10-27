using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class ComingSoonViewController : BaseViewController
{
	private static List<IComingSoonTitle> _cachedComingSoonTitles = new List<IComingSoonTitle>();

	public static void ConfigureComingSoonView()
	{
		if (OrchestrationService.InvokeIsVendJobCompletedCallback())
		{
			OrchestrationService.ViewGoBack("Coming Soon View: Vend job finished.", popOnly: false);
		}
		else
		{
			if (!(BaseViewController.GetViewUserControl() is ComingSoonViewUserControl comingSoonViewUserControl))
			{
				return;
			}
			if (BundleFunctions.GetCulture() == "es-MX")
			{
				comingSoonViewUserControl.ComingSoonTextStyle = "font_target_larger";
				comingSoonViewUserControl.RentalsAreOnTheirWayStyle = "font_target_large_overlays";
			}
			string coming_soon_coming_soon = Resources.coming_soon_coming_soon;
			string rentals_coming_soon = Resources.rentals_coming_soon;
			comingSoonViewUserControl.ComingSoonText = coming_soon_coming_soon;
			comingSoonViewUserControl.RentalsAreOnTheirWayText = rentals_coming_soon;
			foreach (IComingSoonTitle cachedComingSoonTitle in _cachedComingSoonTitles)
			{
				comingSoonViewUserControl.AddComingSoonTitle(cachedComingSoonTitle);
			}
		}
	}

	public static bool HasComingSoonTitles()
	{
		List<IComingSoonTitle> list = FindComingSoonTitles();
		if (list != null)
		{
			return list.Count > 0;
		}
		return false;
	}

	private static List<IComingSoonTitle> FindComingSoonTitles()
	{
		int count = 3;
		List<IComingSoonTitle> list = new List<IComingSoonTitle>();
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		List<IKioskProduct> products = null;
		service?.GetProducts(BrowseViewFilter.ComingSoon, TitleFamily.All, TitleType.All, BrowseSort.MerchandiseDate, null, null, null, null, isADAView: false, null, null, out products);
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
		if (rentalShoppingCart != null && products != null)
		{
			Ratings ratings = (rentalShoppingCart.TitleItems.Any() ? rentalShoppingCart.TitleItems.Max((IRentalShoppingCartTitleItem x) => RatingsProperties.GetMPAARatingEquivalent(x.Rating)) : Ratings.Max);
			bool includeBluray = rentalShoppingCart.TitleItems.Any((IRentalShoppingCartTitleItem x) => x.TitleType == TitleType.Bluray);
			List<ITitleProduct> comingSoonByTypeAndRating = GetComingSoonByTypeAndRating(products, ratings, includeBluray);
			LogHelper.Instance.Log($"FindComingSoonTitles.  Cart Title Item count: {rentalShoppingCart.TitleItems.Count}, Highest Rating: {ratings}");
			StringBuilder stringBuilder = new StringBuilder($"Available coming soon titles: {comingSoonByTypeAndRating.Count};  Selected coming soon titles: ");
			List<ITitleProduct> list2 = comingSoonByTypeAndRating.Take(count).ToList();
			list2.ForEach(delegate(ITitleProduct x)
			{
				if (!Enum.TryParse<Ratings>(x.RatingId?.ToString(), out var result))
				{
					result = Ratings.None;
				}
				stringBuilder.Append($"ProductId: {x.ProductId}, Rating: {result}, TitleType: {x.TitleType}, Name: {x.LongName};  ");
			});
			LogHelper.Instance.Log(stringBuilder.ToString());
			if (comingSoonByTypeAndRating.Count > 0)
			{
				list = CreateComingSoonTitles(list2);
			}
		}
		_cachedComingSoonTitles = list;
		return list;
	}

	private static List<IComingSoonTitle> CreateComingSoonTitles(List<ITitleProduct> products)
	{
		List<IComingSoonTitle> list = new List<IComingSoonTitle>();
		if (products != null)
		{
			foreach (ITitleProduct product in products)
			{
				string imageFile = product.ImageFile;
				string text = null;
				if (!string.IsNullOrEmpty(imageFile) && OrchestrationService.DataServiceRegisterImage(imageFile, imageFile))
				{
					text = $"cache:{imageFile}";
				}
				BitmapImage image = null;
				if (!string.IsNullOrEmpty(text) && text.StartsWith("cache:"))
				{
					string text2 = text.Substring(6);
					if (OrchestrationService.DataServiceRegisterBitmapImage(text2, text2))
					{
						image = ServiceLocator.Instance.GetService<IBitmapCacheService>()?.GetBitmapImage(text2);
					}
				}
				IComingSoonTitle item = new ComingSoonTitle
				{
					TitleName = product.LongName,
					ReleaseDate = product.MerchandiseDate.ToShortDateString(),
					Image = image
				};
				list.Add(item);
			}
		}
		return list;
	}

	private static List<ITitleProduct> GetComingSoonByTypeAndRating(List<IKioskProduct> comingSoonProducts, Ratings highestRating, bool includeBluray)
	{
		List<ITitleProduct> list = new List<ITitleProduct>();
		if (comingSoonProducts != null)
		{
			foreach (IKioskProduct comingSoonProduct in comingSoonProducts)
			{
				ITitleProduct titleProduct = comingSoonProduct as ITitleProduct;
				if (comingSoonProduct is ITitleRollupProduct titleRollupProduct)
				{
					bool flag = false;
					if (includeBluray)
					{
						titleProduct = titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.Bluray);
						flag = IsValidComingSoonTitle(titleProduct, includeBluray, highestRating);
					}
					if (!flag)
					{
						titleProduct = titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.DVD);
					}
				}
				if (IsValidComingSoonTitle(titleProduct, includeBluray, highestRating))
				{
					list.Add(titleProduct);
				}
			}
			list.Sort(delegate(ITitleProduct a, ITitleProduct b)
			{
				int num = a.TitleType.CompareTo(b.TitleType) * -1;
				if (num == 0)
				{
					num = a.MerchandiseDate.CompareTo(b.MerchandiseDate);
				}
				return num;
			});
		}
		return list;
	}

	private static bool IsValidComingSoonTitle(ITitleProduct titleProduct, bool includeBluray, Ratings highestRating)
	{
		bool result = false;
		if (titleProduct != null && titleProduct.TitleFamily == TitleFamily.Movies && (((titleProduct.TitleType == TitleType.Bluray) & includeBluray) || titleProduct.TitleType == TitleType.DVD) && titleProduct.Genres.Contains(1020L) && (!titleProduct.DiscNumber.HasValue || titleProduct.DiscNumber == 1) && titleProduct.RatingId.HasValue && Enum.TryParse<Ratings>(titleProduct.RatingId.ToString(), out var result2))
		{
			result2 = RatingsProperties.GetMPAARatingEquivalent(result2);
			if (result2 != Ratings.None && result2 <= highestRating)
			{
				result = true;
			}
		}
		return result;
	}
}
