using System.Net;
using System.Net.Http;
using System.Web.Http;
using Redbox.Core;
using Redbox.Rental.Model.EngineApplication;
using Swashbuckle.Swagger.Annotations;

namespace Redbox.KioskEngine.API.Controllers.EngineApplication;

public class EngineApplicationController : ApiController
{
	[HttpPost]
	[Route("api/engine/shutdown")]
	[SwaggerResponse(HttpStatusCode.Accepted, "Kiosk Engine is processing the shutdown request", typeof(PerformShutdownResponse))]
	[SwaggerResponse(HttpStatusCode.InternalServerError, "Kiosk Engine encountered an error while processing the shutdown request", typeof(PerformShutdownResponse))]
	public HttpResponseMessage PerformShutdown()
	{
		PerformShutdownResponse performShutdownResponse = new PerformShutdownResponse();
		IEngineApplicationService engineApplicationService = ServiceLocator.Instance.GetService<IEngineApplicationService>();
		if (engineApplicationService == null)
		{
			performShutdownResponse.Error = "Unable to find FileSetsService.";
		}
		else
		{
			performShutdownResponse = engineApplicationService.PerformShutdown();
		}
		return base.Request.CreateResponse(string.IsNullOrWhiteSpace(performShutdownResponse.Error) ? HttpStatusCode.Accepted : HttpStatusCode.InternalServerError, performShutdownResponse, base.Configuration.Formatters.JsonFormatter);
	}

	[HttpGet]
	[Route("api/engine/processid")]
	[SwaggerResponse(HttpStatusCode.OK, "Kiosk Engine successfully returned its ProcessId", typeof(int?))]
	[SwaggerResponse(HttpStatusCode.InternalServerError, "Kiosk Engine encountered an error while processing the request", typeof(int?))]
	public HttpResponseMessage GetProcessId()
	{
		IEngineApplicationService engineApplicationService = ServiceLocator.Instance.GetService<IEngineApplicationService>();
		if (engineApplicationService == null)
		{
			return base.Request.CreateResponse<int?>(HttpStatusCode.InternalServerError, null, base.Configuration.Formatters.JsonFormatter);
		}
		return base.Request.CreateResponse(HttpStatusCode.OK, engineApplicationService.ProcessId, base.Configuration.Formatters.JsonFormatter);
	}
}
