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
		IDataService dataService = ServiceLocator.Instance.GetService<IDataService>();
		List<IKioskProduct> products = null;
		dataService?.GetProducts(BrowseViewFilter.ComingSoon, TitleFamily.All, TitleType.All, BrowseSort.MerchandiseDate, null, null, null, null, isADAView: false, null, null, out products);
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
		if (rentalShoppingCart != null && products != null)
		{
			Ratings ratings = (rentalShoppingCart.TitleItems.Any() ? rentalShoppingCart.TitleItems.Max((IRentalShoppingCartTitleItem x) => RatingsProperties.GetMPAARatingEquivalent(x.Rating)) : Ratings.Max);
			bool includeBluray = rentalShoppingCart.TitleItems.Any((IRentalShoppingCartTitleItem x) => x.TitleType == TitleType.Bluray);
			List<ITitleProduct> comingSoonByTypeAndRating = GetComingSoonByTypeAndRating(products, ratings, includeBluray);
			LogHelper.Instance.Log($"FindComingSoonTitles.  Cart Title Item count: {rentalShoppingCart.TitleItems.Count}, Highest Rating: {ratings}");
			StringBuilder stringBuilder = new StringBuilder($"Available coming soon titles: {comingSoonByTypeAndRating.Count};  Selected coming soon titles: ");
			List<ITitleProduct> comingSoonTitles = comingSoonByTypeAndRating.Take(count).ToList();
			comingSoonTitles.ForEach(delegate(ITitleProduct x)
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
				list = CreateComingSoonTitles(comingSoonTitles);
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
					string imageName = text.Substring(6);
					if (OrchestrationService.DataServiceRegisterBitmapImage(imageName, imageName))
					{
						image = ServiceLocator.Instance.GetService<IBitmapCacheService>()?.GetBitmapImage(imageName);
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
					bool blurayValid = false;
					if (includeBluray)
					{
						titleProduct = titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.Bluray);
						blurayValid = IsValidComingSoonTitle(titleProduct, includeBluray, highestRating);
					}
					if (!blurayValid)
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
				int comparison = a.TitleType.CompareTo(b.TitleType) * -1;
				if (comparison == 0)
				{
					comparison = a.MerchandiseDate.CompareTo(b.MerchandiseDate);
				}
				return comparison;
			});
		}
		return list;
	}

	private static bool IsValidComingSoonTitle(ITitleProduct titleProduct, bool includeBluray, Ratings highestRating)
	{
		bool result = false;
		if (titleProduct != null && titleProduct.TitleFamily == TitleFamily.Movies && (((titleProduct.TitleType == TitleType.Bluray) & includeBluray) || titleProduct.TitleType == TitleType.DVD) && titleProduct.Genres.Contains(1020L) && (!titleProduct.DiscNumber.HasValue || titleProduct.DiscNumber == 1) && titleProduct.RatingId.HasValue && Enum.TryParse<Ratings>(titleProduct.RatingId.ToString(), out var rating))
		{
			rating = RatingsProperties.GetMPAARatingEquivalent(rating);
			if (rating != Ratings.None && rating <= highestRating)
			{
				result = true;
			}
		}
		return result;
	}
}
