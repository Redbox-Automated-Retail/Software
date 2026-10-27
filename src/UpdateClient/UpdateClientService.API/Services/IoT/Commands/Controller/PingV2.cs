using System.Threading.Tasks;

namespace UpdateClientService.API.Services.IoT.Commands.Controller;

public class PingV2 : ICommandIoTController
{
	private readonly IMqttProxy _mqttProxy;

	private readonly IStoreService _store;

	public CommandEnum CommandEnum => CommandEnum.Ping;

	public int Version => 2;

	public PingV2(IMqttProxy mqttProxy, IStoreService store)
	{
		_mqttProxy = mqttProxy;
		_store = store;
	}

	public async Task Execute(IoTCommandModel ioTCommand)
	{
		await _mqttProxy.PublishIoTCommandAsync("redbox/updateservice-instance/" + ioTCommand.SourceId + "/request", new IoTCommandModel
		{
			RequestId = ioTCommand.RequestId,
			Command = CommandEnum,
			Version = Version,
			SourceId = _store.KioskId.ToString(),
			MessageType = MessageTypeEnum.Response,
			Payload = new MqttResponse<object>()
		});
	}
}
