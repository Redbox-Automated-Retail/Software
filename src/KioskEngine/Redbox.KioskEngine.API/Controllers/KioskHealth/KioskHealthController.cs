using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Redbox.Core;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskHealth;
using Redbox.Rental.Model.UpdateClientService.KioskHealth;
using Swashbuckle.Swagger.Annotations;

namespace Redbox.KioskEngine.API.Controllers.KioskHealth;

public class KioskHealthController : ApiController
{
	[HttpPost]
	[Route("api/kioskhealth/ping")]
	[SwaggerResponse(HttpStatusCode.OK, Description = "Ping sent", Type = typeof(PingResponse))]
	[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal system error", Type = typeof(PingResponse))]
	public async Task<PingResponse> SendPing()
	{
		Stopwatch.StartNew();
		PingResponse response = new PingResponse();
		await Task.Run(async delegate
		{
			IPingService service = ServiceLocator.Instance.GetService<IPingService>();
			if (service != null)
			{
				response = await service.SendPing();
			}
			else
			{
				response.StatusCode = 500;
				response.Errors.Add(new Redbox.Rental.Model.Error
				{
					Code = HttpStatusCode.InternalServerError.ToString(),
					Message = "PingService is null."
				});
			}
		});
		return response;
	}

	[HttpGet]
	[Route("api/kioskhealth/getpingserviceinfo")]
	[SwaggerResponse(HttpStatusCode.OK, Description = "Success", Type = typeof(PingServiceInfo))]
	[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal system error", Type = typeof(PingServiceInfo))]
	public async Task<PingServiceInfo> GetPingServiceInfo()
	{
		LogHelper.Instance.Log("KioskHealthController.GetPingServiceInfo");
		PingServiceInfo response = null;
		await Task.Run(delegate
		{
			IPingService service = ServiceLocator.Instance.GetService<IPingService>();
			response = new PingServiceInfo
			{
				PingInterval = (service?.PingInterval ?? 0),
				Enabled = (service?.Enabled ?? false)
			};
		});
		return response;
	}
}
