using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.IoT.Commands;

namespace UpdateClientService.API.Services;

public class ActiveResponsesService : IActiveResponseService
{
	private ConcurrentDictionary<string, Action<IoTCommandModel>> _activeResponses = new ConcurrentDictionary<string, Action<IoTCommandModel>>();

	private ILogger<ActiveResponsesService> _logger;

	public ActiveResponsesService(ILogger<ActiveResponsesService> logger)
	{
		_logger = logger;
	}

	public void AddResponseListener(string requestId, Action<IoTCommandModel> model)
	{
		bool flag = _activeResponses.TryAdd(requestId, model);
		_logger.LogInfoWithSource($"Adding {requestId} to Active Listeners - success = {flag}", "AddResponseListener", "/sln/src/UpdateClientService.API/Services/ActiveResponsesService.cs");
	}

	public Action<IoTCommandModel> GetResponseListenerAction(string requestId)
	{
		if (_activeResponses.TryGetValue(requestId, out var value))
		{
			return value;
		}
		_logger.LogWarningWithSource("Failed To Find RequestId: " + requestId + " in Active Responses.", "GetResponseListenerAction", "/sln/src/UpdateClientService.API/Services/ActiveResponsesService.cs");
		return null;
	}

	public void RemoveResponseListener(string requestId)
	{
		bool flag = _activeResponses.TryRemove(requestId, out var _);
		_logger.LogInfoWithSource($"Removing {requestId} from Active Listeners - success = {flag}", "RemoveResponseListener", "/sln/src/UpdateClientService.API/Services/ActiveResponsesService.cs");
	}
}
