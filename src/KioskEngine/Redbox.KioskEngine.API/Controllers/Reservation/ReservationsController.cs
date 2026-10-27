using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Reservation;
using Redbox.Rental.Model.UpdateClientService.Reservation;
using Swashbuckle.Swagger.Annotations;

namespace Redbox.KioskEngine.API.Controllers.Reservation;

public class ReservationsController : ApiController
{
	[HttpGet]
	[Route("api/reservation/getreservationservicestatus")]
	[SwaggerResponse(HttpStatusCode.OK, Type = typeof(GetReservationServiceStatusResponse))]
	[SwaggerResponse(HttpStatusCode.InternalServerError)]
	public async Task<HttpResponseMessage> GetReservationServiceStatus()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		GetReservationServiceStatusResponse response = new GetReservationServiceStatusResponse();
		await Task.Run(async delegate
		{
			IReservationService service = ServiceLocator.Instance.GetService<IReservationService>();
			if (service != null)
			{
				response = await service.GetReservationServiceStatus();
			}
			else
			{
				response.StatusCode = HttpStatusCode.InternalServerError;
				response.Errors.Add(new ResponseError
				{
					Code = HttpStatusCode.InternalServerError.ToString(),
					Message = "Unable to find ReservationService."
				});
			}
		});
		response?.SetResponseTime((int?)stopwatch?.ElapsedMilliseconds);
		return base.Request.CreateResponse(response.StatusCode, response, base.Configuration.Formatters.JsonFormatter);
	}

	[HttpGet]
	[Route("api/reservation/getreservations")]
	[SwaggerResponse(HttpStatusCode.OK, Type = typeof(GetReservationsResponse))]
	[SwaggerResponse(HttpStatusCode.NotFound, Description = "No reservations found.", Type = typeof(ApiServiceBaseResponse<object>))]
	[SwaggerResponse(HttpStatusCode.InternalServerError)]
	public async Task<HttpResponseMessage> GetReservations()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		GetReservationsResponse response = new GetReservationsResponse();
		await Task.Run(async delegate
		{
			IReservationService service = ServiceLocator.Instance.GetService<IReservationService>();
			if (service != null)
			{
				response = await service.GetReservations();
			}
			else
			{
				response.StatusCode = HttpStatusCode.InternalServerError;
				response.Errors.Add(new ResponseError
				{
					Code = HttpStatusCode.InternalServerError.ToString(),
					Message = "Unable to find ReservationService."
				});
			}
		});
		response?.SetResponseTime((int?)stopwatch?.ElapsedMilliseconds);
		return base.Request.CreateResponse(response.StatusCode, response, base.Configuration.Formatters.JsonFormatter);
	}

	[HttpGet]
	[Route("api/reservation/getreservabletitles")]
	[SwaggerResponse(HttpStatusCode.OK, Type = typeof(GetReservableTitlesResponse))]
	[SwaggerResponse(HttpStatusCode.NotFound, Description = "No reservable titles found.", Type = typeof(ApiServiceBaseResponse<object>))]
	[SwaggerResponse(HttpStatusCode.InternalServerError)]
	public async Task<HttpResponseMessage> GetReservableTitles()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		GetReservableTitlesResponse response = new GetReservableTitlesResponse();
		await Task.Run(async delegate
		{
			IReservationService service = ServiceLocator.Instance.GetService<IReservationService>();
			if (service != null)
			{
				response = await service.GetReservableTitles();
			}
			else
			{
				response.StatusCode = HttpStatusCode.InternalServerError;
				response.Errors.Add(new ResponseError
				{
					Code = HttpStatusCode.InternalServerError.ToString(),
					Message = "Unable to find ReservationService."
				});
			}
		});
		response?.SetResponseTime((int?)stopwatch?.ElapsedMilliseconds);
		return base.Request.CreateResponse(response.StatusCode, response, base.Configuration.Formatters.JsonFormatter);
	}

	[HttpPost]
	[Route("api/reservation")]
	[SwaggerResponse(HttpStatusCode.OK, Description = "Reservation created", Type = typeof(CreateReservationResponse))]
	[SwaggerResponse(HttpStatusCode.NotFound, Description = "Requested title(s) not available. Reservation not created.", Type = typeof(ApiServiceBaseResponse<object>))]
	[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Invalid Request. Reservation not created.", Type = typeof(ApiServiceBaseResponse<object>))]
	[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal server error. Reservation not created.", Type = typeof(ApiServiceBaseResponse<object>))]
	public async Task<HttpResponseMessage> CreateReservation(ReservationRequest reservationRequest)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		CreateReservationResponse response = new CreateReservationResponse();
		await Task.Run(async delegate
		{
			IReservationService service = ServiceLocator.Instance.GetService<IReservationService>();
			if (service != null)
			{
				response = await service.CreateReservation(reservationRequest);
			}
			else
			{
				response.StatusCode = HttpStatusCode.InternalServerError;
				response.Errors.Add(new ResponseError
				{
					Code = HttpStatusCode.InternalServerError.ToString(),
					Message = "Unable to find ReservationService."
				});
			}
		});
		response?.SetResponseTime((int?)stopwatch?.ElapsedMilliseconds);
		return base.Request.CreateResponse(response.StatusCode, response, base.Configuration.Formatters.JsonFormatter);
	}

	[HttpPost]
	[Route("api/reservation/cancelreservation")]
	[SwaggerResponse(HttpStatusCode.OK, Description = "Reservation cancelled", Type = typeof(CancelReservationResponse))]
	[SwaggerResponse(HttpStatusCode.NotFound, Description = "Reservation not found", Type = typeof(CancelReservationResponse))]
	[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal system error", Type = typeof(CancelReservationResponse))]
	[SwaggerResponse(HttpStatusCode.ServiceUnavailable, Description = "Reservation service is not enabled", Type = typeof(CancelReservationResponse))]
	public async Task<HttpResponseMessage> CancelReservation(string referenceNumber)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		CancelReservationResponse response = new CancelReservationResponse
		{
			Success = false
		};
		await Task.Run(async delegate
		{
			IReservationService service = ServiceLocator.Instance.GetService<IReservationService>();
			if (service != null)
			{
				response = await service.CancelReservation(referenceNumber);
			}
			else
			{
				response.StatusCode = HttpStatusCode.InternalServerError;
				response.Errors.Add(new ResponseError
				{
					Code = HttpStatusCode.InternalServerError.ToString(),
					Message = "Unable to find ReservationService."
				});
			}
		});
		response?.SetResponseTime((int?)stopwatch?.ElapsedMilliseconds);
		return base.Request.CreateResponse(response.StatusCode, response, base.Configuration.Formatters.JsonFormatter);
	}

	[HttpPost]
	[Route("api/reservation/unregisterfrombrokerservice")]
	[SwaggerResponse(HttpStatusCode.OK, Description = "Unregister successful", Type = typeof(UnregisterFromBrokerServiceResponse))]
	[SwaggerResponse(HttpStatusCode.Conflict, Description = "Unable to unregister because of conflicting condition.", Type = typeof(UnregisterFromBrokerServiceResponse))]
	[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal system error", Type = typeof(UnregisterFromBrokerServiceResponse))]
	public async Task<HttpResponseMessage> UnregisterFromBrokerService()
	{
		Stopwatch.StartNew();
		IUnregisterFromBrokerServiceResponse response = new UnregisterFromBrokerServiceResponse
		{
			Success = false
		};
		await Task.Run(async delegate
		{
			IReservationService service = ServiceLocator.Instance.GetService<IReservationService>();
			if (service != null)
			{
				response = await service.UnRegisterFromAWSBrokerServiceAsync(UnRegisterFromBrokerServiceReason.KioskEngineApiCall);
			}
			else
			{
				response.StatusCode = 500;
				response.Errors.Add(new Redbox.Rental.Model.Error
				{
					Code = HttpStatusCode.InternalServerError.ToString(),
					Message = "Unable to find ReservationService."
				});
			}
		});
		return base.Request.CreateResponse((HttpStatusCode)response.StatusCode, response, base.Configuration.Formatters.JsonFormatter);
	}

	[HttpPost]
	[Route("api/reservation/registerwithbrokerservice")]
	[SwaggerResponse(HttpStatusCode.OK, Description = "Register successful", Type = typeof(RegisterWithBrokerServiceResponse))]
	[SwaggerResponse(HttpStatusCode.Conflict, Description = "Unable to register because of conflicting condition.", Type = typeof(RegisterWithBrokerServiceResponse))]
	[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal system error", Type = typeof(RegisterWithBrokerServiceResponse))]
	public async Task<HttpResponseMessage> RegisterWithBrokerService()
	{
		Stopwatch.StartNew();
		IRegisterWithBrokerServiceResponse response = new RegisterWithBrokerServiceResponse
		{
			Success = false
		};
		await Task.Run(async delegate
		{
			IReservationService service = ServiceLocator.Instance.GetService<IReservationService>();
			if (service != null)
			{
				response = await service.RegisterWithAWSBrokerServiceAsync();
			}
			else
			{
				response.StatusCode = 500;
				response.Errors.Add(new Redbox.Rental.Model.Error
				{
					Code = HttpStatusCode.InternalServerError.ToString(),
					Message = "Unable to find ReservationService."
				});
			}
		});
		return base.Request.CreateResponse((HttpStatusCode)response.StatusCode, response, base.Configuration.Formatters.JsonFormatter);
	}
}
