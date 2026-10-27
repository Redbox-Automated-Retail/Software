using System.Collections.Generic;
using System.Net;

namespace KioskClientService.API.Services.ProxyApi;

public abstract class PersonalizationBaseResponse : IHttpResponse
{
	public string HostName { get; set; }

	public bool IsSuccess { get; set; }

	public List<PersonalizationResponseError> Errors { get; set; }

	public HttpStatusCode HttpStatusCode { get; set; }
}
