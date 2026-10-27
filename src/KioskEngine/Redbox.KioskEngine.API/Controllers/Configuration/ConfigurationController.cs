using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.UpdateClientService;
using Redbox.Rental.Model.UpdateClientService.Configuration;
using Swashbuckle.Swagger.Annotations;

namespace Redbox.KioskEngine.API.Controllers.Configuration;

public class ConfigurationController : ApiController
{
	[HttpGet]
	[Route("api/configuration/settingvalues/changes")]
	[SwaggerResponse(HttpStatusCode.OK, Type = typeof(GetConfigurationChangesResponse))]
	[SwaggerResponse(HttpStatusCode.InternalServerError)]
	public async Task<HttpResponseMessage> GetConfigurationChanges()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		GetConfigurationChangesResponse response = new GetConfigurationChangesResponse();
		await Task.Run(async delegate
		{
			IUpdateClientService service = ServiceLocator.Instance.GetService<IUpdateClientService>();
			if (service != null)
			{
				response = await service.Configuration.GetConfigurationChanges();
			}
			else
			{
				response.StatusCode = HttpStatusCode.InternalServerError;
				response.Errors.Add(new ResponseError
				{
					Code = HttpStatusCode.InternalServerError.ToString(),
					Message = "Unable to get configuration changes."
				});
			}
		});
		response?.SetResponseTime((int?)stopwatch?.ElapsedMilliseconds);
		return base.Request.CreateResponse(response.StatusCode, response, base.Configuration.Formatters.JsonFormatter);
	}
}
