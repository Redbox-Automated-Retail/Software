using System.Collections.Generic;

namespace KioskClientService.API.Services.HttpQueue;

public interface IHttpQueueService
{
	bool Enqueue(HttpQueueServiceRequest request);

	bool Finalize(HttpQueueServiceRequest request);

	IEnumerable<HttpQueueServiceRequest> List();
}
