using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Redbox.Rental.Services.App;

public interface IHttpService
{
	int Timeout { get; set; }

	HttpRequestMessage GenerateRequest(string baseUrl, string endpoint, object requestObject, HttpMethod method, List<Header> headers = null, bool isForm = false);

	Task<APIResponse<T>> SendRequestAsync<T>(string baseUrl, string endpoint, object requestObject, HttpMethod method, List<Header> headers = null, bool isForm = false, int? timeout = null, [CallerMemberName] string callerMethod = "", [CallerFilePath] string callerLocation = "", object originalRequestObject = null, bool logRequest = true, bool logResponse = true);

	Task<APIResponse<T>> SendRequestAsync<T>(HttpRequestMessage request, int? timeout = null, [CallerMemberName] string callerMethod = "", [CallerFilePath] string callerLocation = "", object originalRequestObject = null, bool logRequest = true, bool logResponse = true);

	Task<APIResponse<byte[]>> DownloadFile(string url, int? timeout, [CallerMemberName] string callerMethod = "", [CallerFilePath] string callerLocation = "");
}
