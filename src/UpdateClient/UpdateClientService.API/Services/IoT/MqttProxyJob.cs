using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;

namespace UpdateClientService.API.Services.IoT;

public class MqttProxyJob : IInvocable
{
	private ILogger<MqttProxyJob> _logger;

	private IMqttProxy _mqttProxy;

	public MqttProxyJob(ILogger<MqttProxyJob> logger, IMqttProxy mqttProxy)
	{
		_logger = logger;
		_mqttProxy = mqttProxy;
	}

	public async Task Invoke()
	{
		await _mqttProxy.CheckConnectionAsync();
	}
}
