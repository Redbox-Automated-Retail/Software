using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.IoT.Commands.Controller;

public class ExecuteHttpRequestV2 : ICommandIoTController
{
	private readonly IHttpService _httpService;

	private readonly ILogger<ExecuteHttpRequestV2> _logger;

	private readonly IMqttProxy _mqttProxy;

	private readonly IStoreService _store;

	public CommandEnum CommandEnum => CommandEnum.ExecuteHttpRequest;

	public int Version => 2;

	public ExecuteHttpRequestV2(IHttpService httpService, ILogger<ExecuteHttpRequestV2> logger, IMqttProxy mqttProxy, IStoreService store)
	{
		_httpService = httpService;
		_logger = logger;
		_mqttProxy = mqttProxy;
		_store = store;
	}

	public async Task Execute(IoTCommandModel ioTCommandRequest)
	{
		KioskHttpRequest kioskHttpRequest = JsonConvert.DeserializeObject<KioskHttpRequest>(ioTCommandRequest.Payload.ToJson());
		StringContent httpContent = ((kioskHttpRequest != null && kioskHttpRequest.JsonBody != null) ? new StringContent(kioskHttpRequest.JsonBody, Encoding.UTF8, "application/json") : null);
		HttpRequestMessage request = _httpService.GenerateRequest(null, kioskHttpRequest.Url, httpContent, new HttpMethod(kioskHttpRequest.HttpMethod));
		APIResponse<object> aPIResponse = await _httpService.SendRequestAsync<object>(request, null, "Execute", "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/ExecuteHttpRequestV2.cs", null, ioTCommandRequest != null && ioTCommandRequest.LogRequest == true, ioTCommandRequest != null && ioTCommandRequest.LogResponse == true);
		MqttResponse<string> mqttResponse = new MqttResponse<string>();
		mqttResponse.Data = aPIResponse?.Response?.ToJson();
		string text;
		string text2;
		if (aPIResponse == null || aPIResponse.Response == null)
		{
			mqttResponse.Data = aPIResponse?.ResponseContent;
			if (aPIResponse == null || !aPIResponse.IsSuccessStatusCode)
			{
				if ((aPIResponse?.Errors?.Count).GetValueOrDefault() > 0)
				{
					mqttResponse.Error = "Errors: " + string.Join(',', aPIResponse.Errors.Select((Error x) => x.Message));
				}
				text = ((ioTCommandRequest != null && ioTCommandRequest.LogResponse == true) ? ("HttpResponse: " + aPIResponse?.HttpResponse?.ToJson()) : string.Empty);
				if (ioTCommandRequest != null && ioTCommandRequest.LogResponse == true)
				{
					if (ioTCommandRequest != null && ioTCommandRequest.LogRequest == true)
					{
						text2 = "Received ioTCommand: " + ioTCommandRequest.ToJson() + ".";
						goto IL_02f9;
					}
				}
				text2 = string.Empty;
				goto IL_02f9;
			}
		}
		goto IL_037b;
		IL_02f9:
		string text3 = text2;
		string text4 = ((ioTCommandRequest != null && ioTCommandRequest.LogResponse == true) ? $"{mqttResponse}" : string.Empty);
		_logger.LogErrorWithSource("ExecuteHttpRequestV2 has failed. Result: " + text4 + "." + text + text3, "Execute", "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/ExecuteHttpRequestV2.cs");
		goto IL_037b;
		IL_037b:
		string text5 = ((ioTCommandRequest != null && ioTCommandRequest.LogResponse == true) ? $"Received response: {aPIResponse?.Response}" : string.Empty);
		_logger.LogInfoWithSource("Executed http v2 request." + text5, "Execute", "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/ExecuteHttpRequestV2.cs");
		await _mqttProxy.PublishIoTCommandAsync("redbox/updateservice-instance/" + ioTCommandRequest.SourceId + "/request", new IoTCommandModel
		{
			RequestId = ioTCommandRequest.RequestId,
			Command = CommandEnum,
			MessageType = MessageTypeEnum.Response,
			Version = Version,
			SourceId = _store?.KioskId.ToString(),
			Payload = mqttResponse.ToJson(),
			LogRequest = true
		});
	}
}
