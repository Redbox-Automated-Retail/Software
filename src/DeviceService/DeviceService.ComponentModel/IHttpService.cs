using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DeviceService.ComponentModel;

public interface IHttpService
{
	HttpRequestMessage GenerateRequest(string endpoint, object requestObject, HttpMethod method, string baseUrl, List<Header> headers = null);

	Task<HttpResponseMessage> SendRequest(HttpRequestMessage request, int timeout);
}
