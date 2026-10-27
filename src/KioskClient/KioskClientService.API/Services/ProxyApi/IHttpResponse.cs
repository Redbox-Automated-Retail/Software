using System.Net;

namespace KioskClientService.API.Services.ProxyApi;

public interface IHttpResponse
{
	HttpStatusCode HttpStatusCode { get; set; }
}
