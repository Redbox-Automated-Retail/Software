using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.EngineApplication;

namespace Redbox.Rental.Services;

public class EngineApplicationService : IEngineApplicationService
{
	private IEngineApplication EngineApplication => ServiceLocator.Instance.GetService<IEngineApplication>();

	public int ProcessId => Process.GetCurrentProcess().Id;

	public PerformShutdownResponse PerformShutdown()
	{
		try
		{
			int processId = ProcessId;
			Task.Run(async delegate
			{
				await Task.Delay(100);
				if (EngineApplication.CanShutDown())
				{
					LogHelper.Instance.Log("EngineApplicationService.PerformShutdown shutting down KE.");
					EngineApplication.ShutDown();
				}
				else
				{
					LogHelper.Instance.Log("EngineApplicationService.PerformShutdown flagging KE for safe shutdown.");
					EngineApplication.FlagForSafeShutdown();
				}
			});
			LogHelper.Instance.Log($"EngineApplicationService.PerformShutdown attempting to shutdown KE. ProcessId={processId}");
			return new PerformShutdownResponse
			{
				ProcessId = processId
			};
		}
		catch (Exception ex)
		{
			string text = "An unhandled exception occurred while requesting shutdown.";
			LogHelper.Instance.LogException(text, ex);
			return new PerformShutdownResponse
			{
				Error = text + ". Exception -> " + ex.Message
			};
		}
	}
}
