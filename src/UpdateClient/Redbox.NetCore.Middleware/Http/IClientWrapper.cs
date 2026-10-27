using System.Net.Http;
using System.Threading.Tasks;

namespace Redbox.NetCore.Middleware.Http;

public interface IClientWrapper
{
	int Timeout { get; set; }

	Task<BaseResponse> SendRequest(HttpRequestMessage request);
}
