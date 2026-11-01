using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Redbox.Core;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Health;
using Swashbuckle.Swagger.Annotations;

namespace Redbox.KioskEngine.API.Controllers.Health;

public class HealthController : ApiController
{
	[HttpPost]
	[Route("api/health/ping")]
	[SwaggerResponse(HttpStatusCode.OK, Description = "Ping sent", Type = typeof(HealthPingResponse))]
	[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal system error", Type = typeof(HealthPingResponse))]
	public async Task<HealthPingResponse> SendPing()
	{
		Stopwatch.StartNew();
		HealthPingResponse response = new HealthPingResponse();
		await Task.Run(delegate
		{
			IHealthServices healthServices = ServiceLocator.Instance.GetService<IHealthServices>();
			IStoreServices storeServices = ServiceLocator.Instance.GetService<IStoreServices>();
			if (healthServices != null && storeServices != null)
			{
				healthServices.Ping(storeServices.StoreNumberString);
			}
			else
			{
				response.StatusCode = 500;
				response.Errors.Add(new Redbox.Rental.Model.Error
				{
					Code = HttpStatusCode.InternalServerError.ToString(),
					Message = "HealthService is null."
				});
			}
		});
		return response;
	}

	[HttpPost]
	[Route("api/health/alert")]
	[SwaggerResponse(HttpStatusCode.OK, Description = "Alert sent", Type = typeof(HealthAlertResponse))]
	[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal system error", Type = typeof(HealthAlertResponse))]
	public async Task<HealthAlertResponse> SendAlert()
	{
		Stopwatch.StartNew();
		HealthAlertResponse response = new HealthAlertResponse();
		await Task.Run(delegate
		{
			IHealthServices healthServices = ServiceLocator.Instance.GetService<IHealthServices>();
			IStoreServices storeServices = ServiceLocator.Instance.GetService<IStoreServices>();
			if (healthServices != null && storeServices != null)
			{
				healthServices.SendAlert(storeServices.StoreNumberString, "QLM", "TestSubAlertType", "Test Kiosk Alert generated thru the F4 menu", DateTime.Now, null);
			}
			else
			{
				response.StatusCode = 500;
				response.Errors.Add(new Redbox.Rental.Model.Error
				{
					Code = HttpStatusCode.InternalServerError.ToString(),
					Message = "HealthService is null."
				});
			}
		});
		return response;
	}
}
