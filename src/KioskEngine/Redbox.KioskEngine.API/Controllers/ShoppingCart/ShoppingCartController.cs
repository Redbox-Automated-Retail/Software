using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Loyalty;
using Redbox.Rental.Model.ShoppingCart;
using Swashbuckle.Swagger.Annotations;

namespace Redbox.KioskEngine.API.Controllers.ShoppingCart;

public class ShoppingCartController : ApiController
{
	[HttpGet]
	[Route("api/shoppingcart/loyaltytestshoppingcartitems")]
	[SwaggerResponse(HttpStatusCode.OK, Type = typeof(GetLoyaltyTestShoppingCartItemsResponse))]
	[SwaggerResponse(HttpStatusCode.InternalServerError)]
	public async Task<HttpResponseMessage> GetLoyaltyTestShoppingCartItems()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		GetLoyaltyTestShoppingCartItemsResponse response = new GetLoyaltyTestShoppingCartItemsResponse();
		await Task.Run(delegate
		{
			IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
			if (service != null)
			{
				IRentalShoppingCart currentRentalShoppingCart = service.CurrentRentalShoppingCart;
				List<LoyaltyTestShoppingCartItem> list = new List<LoyaltyTestShoppingCartItem>();
				foreach (IRentalShoppingCartTitleItem eachRentalShoppingCartTitleItem in currentRentalShoppingCart.TitleItems)
				{
					list.Add(new LoyaltyTestShoppingCartItem
					{
						ProductId = eachRentalShoppingCartTitleItem.ProductId,
						Name = eachRentalShoppingCartTitleItem.SortName,
						HasPromo = (currentRentalShoppingCart?.Promotion?.PromotionItems?.FirstOrDefault((IPromotionItem y) => y.RentalShoppingCartItem.ProductId == eachRentalShoppingCartTitleItem.ProductId) != null),
						IsRedboxIsRedboxPlusFreeOneNightRental = (currentRentalShoppingCart?.TitleItems.FirstOrDefault((IRentalShoppingCartTitleItem y) => y.ProductId == eachRentalShoppingCartTitleItem.ProductId && (y?.IsRedboxPlusFreeOneNightRental ?? false)) != null)
					});
				}
				response.StatusCode = HttpStatusCode.OK;
				response.Success = true;
				response.Content = list;
			}
			else
			{
				response.StatusCode = HttpStatusCode.InternalServerError;
				response.Errors.Add(new ResponseError
				{
					Code = HttpStatusCode.InternalServerError.ToString(),
					Message = "Unable to find RentalShoppingCartService."
				});
			}
		});
		response?.SetResponseTime((int?)stopwatch?.ElapsedMilliseconds);
		return base.Request.CreateResponse(response.StatusCode, response, base.Configuration.Formatters.JsonFormatter);
	}
}
