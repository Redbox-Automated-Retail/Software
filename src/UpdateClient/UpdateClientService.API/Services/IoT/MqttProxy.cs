using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Publishing;
using MQTTnet.Client.Subscribing;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.IoT.Certificate;
using UpdateClientService.API.Services.IoT.Commands;

namespace UpdateClientService.API.Services.IoT;

public class MqttProxy : IMqttProxy
{
	private const int IoTCoreBrokerPort = 8883;

	private readonly ICertificateService _certService;

	private readonly ILogger<MqttProxy> _logger;

	private readonly IIotCommandDispatch _commandDispatch;

	private readonly IIoTStatisticsService _ioTStatisticsService;

	private readonly IStoreService _store;

	private readonly AppSettings _appSettings;

	private IMqttClient _iotClient;

	private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

	private bool _isSubscribed;

	private bool _isRegistered;

	private int _inConnect;

	private IMqttClientOptions _clientOptions;

	private bool _isTimeSynced;

	private IEnumerable<string> Topics => new List<string>
	{
		$"redbox/kiosk/{_store.KioskId}/command",
		"redbox/kiosk/market/" + _store.Market + "/command",
		"redbox/kiosk/all/command"
	};

	private bool IsMqttConnected => _iotClient?.IsConnected ?? false;

	public MqttProxy(ILogger<MqttProxy> logger, ICertificateService certService, IIotCommandDispatch commandDispatch, IStoreService store, IIoTStatisticsService ioTStatisticsService, IOptions<AppSettings> appSettings)
	{
		_logger = logger;
		_certService = certService;
		_commandDispatch = commandDispatch;
		_store = store;
		_appSettings = appSettings.Value;
		_ioTStatisticsService = ioTStatisticsService;
		MqttFactory mqttFactory = new MqttFactory();
		_iotClient = mqttFactory.CreateMqttClient();
		_iotClient.UseApplicationMessageReceivedHandler(async delegate(MqttApplicationMessageReceivedEventArgs e)
		{
			await MessageReceivedHandler(e);
		});
		_iotClient.UseConnectedHandler(async delegate(MqttClientConnectedEventArgs e)
		{
			await ConnectedHandler(e);
		});
		_iotClient.UseDisconnectedHandler(async delegate(MqttClientDisconnectedEventArgs e)
		{
			await DisconnectedHandler(e);
		});
		_logger.LogInfoWithSource("MqttProxy instantiated", ".ctor", "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
	}

	private async Task ConnectedHandler(MqttClientConnectedEventArgs e)
	{
		_logger.LogInfoWithSource("Connected Result: " + e.ConnectResult.ToJson(), "ConnectedHandler", "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
		if (e.ConnectResult.ResultCode == MqttClientConnectResultCode.Success)
		{
			await _ioTStatisticsService.RecordConnectionSuccess();
		}
	}

	private async Task DisconnectedHandler(MqttClientDisconnectedEventArgs e)
	{
		if (e.Exception == null)
		{
			_logger.LogInfoWithSource($"Received DisconnectedHandler Event Reason: {e.Reason}", "DisconnectedHandler", "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
		}
		else
		{
			_logger.LogErrorWithSource(e.Exception, $"Received DisconnectedHandler Event Reason: {e.Reason}", "DisconnectedHandler", "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
		}
		await _ioTStatisticsService.RecordDisconnection();
	}

	private async Task MessageReceivedHandler(MqttApplicationMessageReceivedEventArgs e)
	{
		byte[] payload = e?.ApplicationMessage?.Payload;
		if (payload == null)
		{
			_logger.LogInfoWithSource($"Message received Topic = {e.ApplicationMessage?.Topic}, Payload Length = 0, QoS = {e?.ApplicationMessage?.QualityOfServiceLevel}, Retain = {e?.ApplicationMessage?.Retain}", "MessageReceivedHandler", "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
		}
		else
		{
			string text = Encoding.UTF8.GetString(payload);
			_logger.LogInfoWithSource($"Message received Topic = {e?.ApplicationMessage?.Topic}, Payload Length = {text?.Length ?? 0}, QoS = {e?.ApplicationMessage?.QualityOfServiceLevel}, Retain = {e?.ApplicationMessage?.Retain}", "MessageReceivedHandler", "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
		}
		Task.Run(async delegate
		{
			await _commandDispatch.Execute(payload, e?.ApplicationMessage?.Topic);
		});
	}

	public async Task<bool> PublishIoTCommandAsync(string topicName, IoTCommandModel request)
	{
		return await PublishMessageAsync(topicName, request.ToJson(), request?.LogRequest ?? true, request.QualityOfServiceLevel);
	}

	public async Task<bool> PublishAwsRuleActionAsync(string ruleName, string payload, bool logMessage = true, QualityOfServiceLevel qualityOfServiceLevel = QualityOfServiceLevel.AtMostOnce)
	{
		return await PublishMessageAsync(ruleName, payload, logMessage, qualityOfServiceLevel);
	}

	private async Task<bool> PublishMessageAsync(string topicName, string message, bool logMessage = true, QualityOfServiceLevel qualityOfServiceLevel = QualityOfServiceLevel.AtLeastOnce)
	{
		bool result = false;
		if (!IsMqttConnected)
		{
			_logger.LogInfoWithSource("Not connected. Cannot publish to topic: " + topicName, "PublishMessageAsync", "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
			return false;
		}
		string text = (logMessage ? ("Message : " + message) : $"Message length: {message?.Length ?? 0}");
		_logger.LogInfoWithSource("Publishing message to topic: " + topicName + " ." + text, "PublishMessageAsync", "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
		try
		{
			MqttApplicationMessage applicationMessage = new MqttApplicationMessageBuilder().WithTopic(topicName).WithPayload(message).WithQualityOfServiceLevel(qualityOfServiceLevel.ToMqttQOS())
				.WithRetainFlag(value: false)
				.Build();
			using CancellationTokenSource cancellationToken = new CancellationTokenSource(5000);
			Stopwatch timer = Stopwatch.StartNew();
			MqttClientPublishResult mqttClientPublishResult = await _iotClient.PublishAsync(applicationMessage, cancellationToken.Token);
			timer.Stop();
			if (mqttClientPublishResult.ReasonCode == MqttClientPublishReasonCode.Success)
			{
				result = true;
				_logger.LogInfoWithSource($"Publish successfully for Packet Identifier: {mqttClientPublishResult.PacketIdentifier}, Topic: {topicName}, QoS Level {qualityOfServiceLevel}, Duration: {timer.ElapsedMilliseconds}", "PublishMessageAsync", "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
			}
			else
			{
				_logger.LogErrorWithSource($"Publish failed for ReasonCode: {mqttClientPublishResult.ReasonCode} - {mqttClientPublishResult.ReasonString}, Topic: {topicName}, QoS Level {qualityOfServiceLevel}, Duration: {timer.ElapsedMilliseconds}", "PublishMessageAsync", "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
				result = false;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while publishing message to topic " + topicName, "PublishMessageAsync", "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
			result = false;
		}
		return result;
	}

	private async Task<bool> SubscribeTopics(IEnumerable<string> topics)
	{
		_logger.LogInfoWithSource("Attempting to subscribe to topics", "SubscribeTopics", "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
		_isSubscribed = false;
		if ((topics != null && topics.Count() == 0) || !IsMqttConnected)
		{
			return false;
		}
		_logger.LogInfoWithSource("calling _iotClient.Subscribe for topics: " + topics.ToJson(), "SubscribeTopics", "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
		MqttClientSubscribeOptionsBuilder builder = new MqttClientSubscribeOptionsBuilder();
		topics.ToList().ForEach(delegate(string t)
		{
			builder.WithTopicFilter(t, MqttQualityOfServiceLevel.AtLeastOnce);
		});
		try
		{
			using CancellationTokenSource cancellationToken = new CancellationTokenSource(5000);
			MqttClientSubscribeResult mqttClientSubscribeResult = await _iotClient.SubscribeAsync(builder.Build(), cancellationToken.Token);
			mqttClientSubscribeResult.Items.ForEach(delegate(MqttClientSubscribeResultItem item)
			{
				_logger.LogInformation($"Subscribe Topic: {item.TopicFilter.Topic} Result: {item.ResultCode}");
			});
			_isSubscribed = mqttClientSubscribeResult.Items.All((MqttClientSubscribeResultItem item) => item.ResultCode <= MqttClientSubscribeResultCode.GrantedQoS2);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception occurred", "SubscribeTopics", "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
		}
		if (_isSubscribed)
		{
			_logger.LogInfoWithSource("Successfully subscribed to Topics: " + topics.ToJson(), "SubscribeTopics", "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
		}
		else
		{
			_logger.LogErrorWithSource("Failed to subscription to Topics: " + topics.ToJson(), "SubscribeTopics", "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
		}
		return _isSubscribed;
	}

	public async Task<bool> CheckConnectionAsync()
	{
		if (Interlocked.CompareExchange(ref _inConnect, 1, 0) == 1)
		{
			_logger.LogError("Already in CheckConnectionAsync, exiting");
			return false;
		}
		try
		{
			return await Connect();
		}
		finally
		{
			_inConnect = 0;
		}
	}

	private async Task<bool> Connect()
	{
		_ = 1;
		try
		{
			if (!IsMqttConnected)
			{
				await ConnectToMqtt();
			}
			if (IsMqttConnected && !_isSubscribed)
			{
				await SubscribeTopics(Topics);
			}
			if (IsMqttConnected && _isSubscribed && !_isRegistered)
			{
				Task.WaitAll(RegisterUCSVersion(), SyncTimestampAndTimezone());
			}
			return IsMqttConnected && _isSubscribed && _isRegistered;
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception occured while attempting to connect MqttClient.", "Connect", "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
			return false;
		}
	}

	public async Task Disconnect()
	{
		try
		{
			await (_iotClient?.DisconnectAsync());
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception occured while attempting to disconnect from MqttClient, this can happen if the connection lost already.", "Disconnect", "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
		}
		Clear();
	}

	private void Clear()
	{
		_isSubscribed = false;
		_isRegistered = false;
	}

	private async Task<bool> Initialize()
	{
		try
		{
			if (_clientOptions != null)
			{
				return true;
			}
			IotCert iotCert = await _certService.GetCertificateAsync();
			if (iotCert == null)
			{
				_logger.LogErrorWithSource("No cert, aborting connection attempt", "Initialize", "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
				return false;
			}
			MqttClientOptionsBuilderTlsParameters mqttClientOptionsBuilderTlsParameters = new MqttClientOptionsBuilderTlsParameters();
			mqttClientOptionsBuilderTlsParameters.Certificates = new List<X509Certificate> { iotCert.RootCa, iotCert.DeviceCertPfx };
			mqttClientOptionsBuilderTlsParameters.SslProtocol = SslProtocols.Tls12;
			mqttClientOptionsBuilderTlsParameters.UseTls = true;
			mqttClientOptionsBuilderTlsParameters.AllowUntrustedCertificates = false;
			_clientOptions = new MqttClientOptionsBuilder().WithTcpServer(_appSettings?.IoTBrokerEndpoint, 8883).WithKeepAlivePeriod(new TimeSpan(0, 0, 0, 300)).WithTls(mqttClientOptionsBuilderTlsParameters)
				.WithProtocolVersion(MqttProtocolVersion.V311)
				.WithClientId(_store.KioskId.ToString())
				.WithCommunicationTimeout(TimeSpan.FromSeconds(60.0))
				.WithCleanSession()
				.Build();
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception during Initialization", "Initialize", "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
			return false;
		}
	}

	private async Task ConnectToMqtt()
	{
		_logger.LogInfoWithSource($"Attempting to connect KioskId: {_store.KioskId} to mqtt", "ConnectToMqtt", "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
		await _ioTStatisticsService.RecordConnectionAttempt();
		try
		{
			Clear();
			if (!(await Initialize()))
			{
				_logger.LogErrorWithSource("Initialization failed", "ConnectToMqtt", "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
				return;
			}
			using CancellationTokenSource cancellationToken = new CancellationTokenSource(20000);
			MqttClientConnectResult mqttClientConnectResult = await _iotClient.ConnectAsync(_clientOptions, cancellationToken.Token);
			_logger.LogInfoWithSource($"MqttClient ConnectAsync completed with KioskId: {_store.KioskId}, and ResultCode: {mqttClientConnectResult.ResultCode}", "ConnectToMqtt", "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception occurred in ConnectToMqtt", "ConnectToMqtt", "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
			await _ioTStatisticsService.RecordConnectionException(ex?.Message + ex?.InnerException?.Message);
			await _certService.Validate();
			_clientOptions = null;
		}
	}

	private async Task<bool> RegisterUCSVersion()
	{
		_isRegistered = false;
		_logger.LogInfoWithSource("Attempting to register ucs", "RegisterUCSVersion", "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
		UCSRegisterMessage message = new UCSRegisterMessage
		{
			KioskId = _store.KioskId,
			Version = "2.0",
			AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString()
		};
		_isRegistered = await PublishMessageAsync("$aws/rules/kioskucsregister", message.ToJson());
		if (_isRegistered)
		{
			_logger.LogInfoWithSource("Register UCS version " + message.Version + " succeeded", "RegisterUCSVersion", "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
		}
		else
		{
			_logger.LogErrorWithSource("Register UCS version " + message.Version + " failed", "RegisterUCSVersion", "/sln/src/UpdateClientService.API/Services/IoT/MqttProxy.cs");
		}
		return _isRegistered;
	}

	private async Task SyncTimestampAndTimezone()
	{
		if (!_isTimeSynced)
		{
			string topicName = "$aws/rules/kioskrestcall";
			IoTCommandModel request = new IoTCommandModel
			{
				RequestId = Guid.NewGuid().ToString(),
				Command = CommandEnum.SyncTimestampAndTimezone,
				MessageType = MessageTypeEnum.Request,
				Version = 1,
				SourceId = _store?.KioskId.ToString(),
				LogRequest = true,
				QualityOfServiceLevel = QualityOfServiceLevel.AtLeastOnce
			};
			await PublishIoTCommandAsync(topicName, request);
			_isTimeSynced = true;
		}
	}
}
