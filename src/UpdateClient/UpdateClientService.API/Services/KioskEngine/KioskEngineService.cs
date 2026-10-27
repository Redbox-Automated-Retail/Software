using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.KioskEngine;

public class KioskEngineService : IKioskEngineService
{
	private readonly ILogger<KioskEngineService> _logger;

	private readonly IHttpService _http;

	private const string KioskEngineUrl = "http://localhost:9002";

	private int? KioskEngineProcessId { get; set; }

	public KioskEngineService(ILogger<KioskEngineService> logger, IHttpService http)
	{
		_logger = logger;
		_http = http;
	}

	public async Task<KioskEngineStatus> GetStatus()
	{
		if (!KioskEngineProcessId.HasValue)
		{
			KioskEngineProcessId = await GetProcessId();
		}
		if (!KioskEngineProcessId.HasValue)
		{
			return KioskEngineStatus.Unknown;
		}
		if (TryGetProcessById(KioskEngineProcessId, out var _))
		{
			return KioskEngineStatus.Running;
		}
		return KioskEngineStatus.Stopped;
	}

	public async Task<PerformShutdownResponse> PerformShutdown(int timeoutMs, int attempts)
	{
		PerformShutdownResponse response = new PerformShutdownResponse();
		try
		{
			int attempt = 0;
			while (true)
			{
				bool flag = attempt++ < attempts;
				bool flag2 = flag;
				if (flag2)
				{
					flag2 = await GetStatus() != KioskEngineStatus.Stopped;
				}
				if (!flag2)
				{
					break;
				}
				await PerformShutdown(timeoutMs);
			}
			if (await GetStatus() != KioskEngineStatus.Stopped)
			{
				HandleTimeout(timeoutMs, attempts, response);
			}
		}
		catch (Exception e)
		{
			HandleException(response, e);
		}
		response.ProcessId = KioskEngineProcessId;
		_logger.LogInfoWithSource("Result -> " + response.ToJson(), "PerformShutdown", "/sln/src/UpdateClientService.API/Services/KioskEngine/KioskEngineService.cs");
		KioskEngineProcessId = null;
		return response;
	}

	private async Task<bool> PerformShutdown(int timeoutMs)
	{
		CancellationTokenSource cts = new CancellationTokenSource(timeoutMs);
		try
		{
			return await Task.Run(async delegate
			{
				HttpRequestMessage request = _http.GenerateRequest("http://localhost:9002", "api/engine/shutdown", null, HttpMethod.Post);
				APIResponse<PerformShutdownResponse> aPIResponse = await _http.SendRequestAsync<PerformShutdownResponse>(request, null, "PerformShutdown", "/sln/src/UpdateClientService.API/Services/KioskEngine/KioskEngineService.cs", null, logRequest: false, logResponse: false);
				if (!aPIResponse.IsSuccessStatusCode)
				{
					_logger.LogErrorWithSource("Encountered an error while requesting shutdown from Kiosk Engine. Response -> " + aPIResponse.GetErrors(), "PerformShutdown", "/sln/src/UpdateClientService.API/Services/KioskEngine/KioskEngineService.cs");
					return false;
				}
				KioskEngineProcessId = aPIResponse.Response?.ProcessId;
				while (await GetStatus() != KioskEngineStatus.Stopped && !cts.IsCancellationRequested)
				{
					await Task.Delay(3000);
				}
				return !cts.IsCancellationRequested;
			}, cts.Token);
		}
		finally
		{
			if (cts != null)
			{
				((IDisposable)cts).Dispose();
			}
		}
	}

	private async Task<int?> GetProcessId()
	{
		Process process = Process.GetProcessesByName("kioskengine.exe").FirstOrDefault();
		if (process != null)
		{
			return process.Id;
		}
		HttpRequestMessage request = _http.GenerateRequest("http://localhost:9002", "api/engine/processid", null, HttpMethod.Get);
		APIResponse<int?> aPIResponse = await _http.SendRequestAsync<int?>(request, 3000, "GetProcessId", "/sln/src/UpdateClientService.API/Services/KioskEngine/KioskEngineService.cs", null, logRequest: false, logResponse: false);
		return aPIResponse.IsSuccessStatusCode ? aPIResponse.Response : ((int?)null);
	}

	private static bool TryGetProcessById(int? id, out Process process)
	{
		process = null;
		if (!id.HasValue)
		{
			return false;
		}
		try
		{
			process = Process.GetProcessById(id.Value);
			return process != null && !process.HasExited;
		}
		catch
		{
			return false;
		}
	}

	private static void HandleTimeout(int timeoutMs, int attempts, PerformShutdownResponse response)
	{
		response.Error = $"Kiosk Engine did not shutdown given the provided parameters. Timeout: {timeoutMs}ms, Attempts: {attempts}.";
	}

	private static void HandleException(PerformShutdownResponse response, Exception e)
	{
		response.Error = e.Message;
	}
}
