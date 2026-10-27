using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.Profile;
using Swashbuckle.Swagger.Annotations;

namespace Redbox.KioskEngine.API.Controllers.ProfileData;

public class ProfileDataController : ApiController
{
	[HttpGet]
	[Route("api/profiledata/kioskproductids")]
	[SwaggerResponse(HttpStatusCode.OK, Type = typeof(GetKioskProductIdsResponse))]
	[SwaggerResponse(HttpStatusCode.InternalServerError)]
	public async Task<HttpResponseMessage> GetKioskProductIds()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		GetKioskProductIdsResponse response = new GetKioskProductIdsResponse();
		await Task.Run(delegate
		{
			IDataService service = ServiceLocator.Instance.GetService<IDataService>();
			if (service != null)
			{
				response.Content = service.GetCachedTitleIds();
			}
			else
			{
				response.StatusCode = HttpStatusCode.InternalServerError;
				response.Errors.Add(new ResponseError
				{
					Code = HttpStatusCode.InternalServerError.ToString(),
					Message = "Unable to get kiosk product ids."
				});
			}
		});
		response?.SetResponseTime((int?)stopwatch?.ElapsedMilliseconds);
		return base.Request.CreateResponse(response.StatusCode, response, base.Configuration.Formatters.JsonFormatter);
	}
}
