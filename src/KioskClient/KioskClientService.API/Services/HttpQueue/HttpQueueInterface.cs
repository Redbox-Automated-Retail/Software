using System.Collections.Generic;

namespace KioskClientService.API.Services.HttpQueue;

internal class HttpQueueInterface : IHttpQueueService
{
	public bool Enqueue(HttpQueueServiceRequest request)
	{
		return HttpQueueInternals.Enqueue(request);
	}

	public bool Finalize(HttpQueueServiceRequest request)
	{
		return HttpQueueInternals.Finalize(request);
	}

	public IEnumerable<HttpQueueServiceRequest> List()
	{
		return HttpQueueInternals.List();
	}
}
