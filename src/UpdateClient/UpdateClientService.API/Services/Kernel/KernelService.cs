using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.Configuration;
using UpdateClientService.API.Services.Utilities;

namespace UpdateClientService.API.Services.Kernel;

public class KernelService : IKernelService
{
	private class SyncTimestampAndTimeZoneData
	{
		public DateTime UtcTimestamp { get; set; }

		public string Timezone { get; set; }
	}

	private readonly ICommandLineService _cmd;

	private readonly ILogger<KernelService> _logger;

	private readonly KernelServiceSettings _settings;

	private readonly bool _updateTimestamp;

	private readonly bool _updateTimezone;

	public KernelService(ICommandLineService cmd, ILogger<KernelService> logger, IOptionsMonitor<AppSettings> settings, IOptionsMonitorKioskConfiguration kioskConfiguration)
	{
		_cmd = cmd;
		_logger = logger;
		_settings = settings?.CurrentValue?.KernelService;
		_updateTimestamp = kioskConfiguration.Operations.SyncTimestamp;
		_updateTimezone = kioskConfiguration.Operations.SyncTimezone;
	}

	public bool PerformShutdown(ShutdownType shutdownType)
	{
		_logger.LogInfoWithSource($"Performing {shutdownType}", "PerformShutdown", "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
		if (!_settings.ShutdownEnabled)
		{
			_logger.LogWarningWithSource($"Cannot {shutdownType} because ShutdownEnabled is set to {_settings.ShutdownEnabled}", "PerformShutdown", "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
			return false;
		}
		switch (shutdownType)
		{
		case ShutdownType.Reboot:
			return _cmd.TryExecutePowerShellScript("Restart-Computer -Force");
		case ShutdownType.Shutdown:
			return _cmd.TryExecutePowerShellScript("Stop-Computer -Force");
		default:
			_logger.LogWarningWithSource($"{shutdownType} was unsuccessful", "PerformShutdown", "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
			return false;
		}
	}

	public void SyncTimeAndTimezone(object data)
	{
		try
		{
			string text = data.ToJson();
			_logger.LogInfoWithSource("sync time data: " + text, "SyncTimeAndTimezone", "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
			SyncTimestampAndTimeZoneData syncTimestampAndTimeZoneData = JsonConvert.DeserializeObject<SyncTimestampAndTimeZoneData>(text);
			if (syncTimestampAndTimeZoneData == null)
			{
				_logger.LogErrorWithSource("unable to deserialize sync time data!", "SyncTimeAndTimezone", "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
				return;
			}
			UpdateTimezone(syncTimestampAndTimeZoneData);
			UpdateTime(syncTimestampAndTimeZoneData);
		}
		catch (Exception ex)
		{
			_logger.LogCriticalWithSource(ex, "Exception occured sync'ing kiosk time/timezone!", "SyncTimeAndTimezone", "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
		}
	}

	private void UpdateTimezone(SyncTimestampAndTimeZoneData data)
	{
		if (_updateTimezone)
		{
			if (string.IsNullOrEmpty(data.Timezone))
			{
				_logger.LogErrorWithSource("Empty time zone returned from server!", "UpdateTimezone", "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
				return;
			}
			TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(data.Timezone);
			switch (TimeZoneFunctions.SetTimeZone(timeZoneInfo))
			{
			default:
				_logger.LogErrorWithSource("Unknown error setting the kiosk time zone!", "UpdateTimezone", "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
				break;
			case TimeZoneFunctions.SetTimeZoneResult.Same:
				_logger.LogInfoWithSource("Client and Server time zones match", "UpdateTimezone", "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
				break;
			case TimeZoneFunctions.SetTimeZoneResult.Changed:
				_logger.LogInfoWithSource("Client zone changed to " + timeZoneInfo.DisplayName, "UpdateTimezone", "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
				break;
			case TimeZoneFunctions.SetTimeZoneResult.Errored:
				_logger.LogErrorWithSource("Failed to set TimeZone for the kiosk!", "UpdateTimezone", "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
				break;
			}
		}
		else
		{
			_logger.LogInfoWithSource("Skipping time zone sync; configuration off", "UpdateTimezone", "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
		}
	}

	private void UpdateTime(SyncTimestampAndTimeZoneData data)
	{
		if (_updateTimestamp)
		{
			switch (TimeZoneFunctions.SetTime(data.UtcTimestamp))
			{
			default:
				_logger.LogErrorWithSource("Unknown error setting the kiosk time!", "UpdateTime", "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
				break;
			case TimeZoneFunctions.SetTimeResult.InRange:
				_logger.LogInfoWithSource("Kiosk time is in range", "UpdateTime", "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
				break;
			case TimeZoneFunctions.SetTimeResult.Changed:
				_logger.LogInfoWithSource("Kiosk time changed to match server time", "UpdateTime", "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
				break;
			case TimeZoneFunctions.SetTimeResult.Errored:
				_logger.LogErrorWithSource("Failed to set date/time for the kiosk!", "UpdateTime", "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
				break;
			}
		}
		else
		{
			_logger.LogInfoWithSource("Skipping time sync; configuration off", "UpdateTime", "/sln/src/UpdateClientService.API/Services/Kernel/KernelService.cs");
		}
	}
}
