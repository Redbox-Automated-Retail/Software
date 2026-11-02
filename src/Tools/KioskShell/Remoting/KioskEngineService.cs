using System;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.Shell.ComponentModel;

namespace Redbox.Shell.Remoting;

public class KioskEngineService : IKioskEngineService
{
	private string _kioskEnginePath = null!; // set by Initialize

	public static KioskEngineService Instance => Singleton<KioskEngineService>.Instance;

	public Redbox.Shell.ComponentModel.ErrorList IsEngineRunning(out bool isRunning)
	{
		isRunning = false;
		Redbox.Shell.ComponentModel.ErrorList errorList = new Redbox.Shell.ComponentModel.ErrorList();
		try
		{
			isRunning = KioskEngineProxy.IsEngineRunning();
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.Shell.ComponentModel.Error.NewError("K999", "An unhandled exception was raised in KioskEngineService.IsEngineRunning.", e));
		}
		return errorList;
	}

	public Redbox.Shell.ComponentModel.ErrorList ActivateControlPanel()
	{
		Redbox.Shell.ComponentModel.ErrorList errorList = new Redbox.Shell.ComponentModel.ErrorList();
		try
		{
			if (KioskEngineProxy.IsEngineRunning())
			{
				ClientCommandResult? clientCommandResult = KioskEngineProxy.ActiviateControlPanel();
				if (!clientCommandResult!.Success)
				{
					foreach (Redbox.IPC.Framework.Error error in clientCommandResult.Errors)
					{
						errorList.Add(Redbox.Shell.ComponentModel.Error.NewError(error.Code, error.Description));
					}
				}
			}
			else
			{
				errorList.Add(Redbox.Shell.ComponentModel.Error.NewError("K999", "Cannot activate control panel bundle; kiosk engine is not running."));
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.Shell.ComponentModel.Error.NewError("K999", "An unhandled exception was raised in KioskEngineService.ActivateControlPanel.", e));
		}
		return errorList;
	}

	public Redbox.Shell.ComponentModel.ErrorList StartEngineWithControlPanel()
	{
		Redbox.Shell.ComponentModel.ErrorList errorList = new Redbox.Shell.ComponentModel.ErrorList();
		if (KioskEngineProxy.IsEngineRunning())
		{
			errorList.Add(Redbox.Shell.ComponentModel.Error.NewError("K999", "Cannot start kiosk engine; already running"));
		}
		else
		{
			try
			{
				LogHelper.Instance.Log("Starting engine at location: {0}", _kioskEnginePath);
				int launchResult = ShellHelper.StartProcessAsAdministrator(_kioskEnginePath, "--bundle:\"Control Panel Application,*\"");
				if (launchResult < 0)
				{
					LogHelper.Instance.Log("Unable to launch Kiosk Engine using TaskScheduler! result={0:X}", launchResult);
				}
			}
			catch (Exception e)
			{
				errorList.Add(Redbox.Shell.ComponentModel.Error.NewError("K999", "An unhandled exception was raised in KioskEngineService.StartEngineWithControlPanel.", e));
			}
		}
		return errorList;
	}

	public void Initialize(string kioskEnginePath, string kioskEngineUrl)
	{
		LogHelper.Instance.Log("KioskEngineService.Initialize() - kioskEnginePath: " + kioskEnginePath + ", kioskEngineUrl: " + kioskEngineUrl);
		_kioskEnginePath = kioskEnginePath;
		ServiceLocator.Instance.AddService(typeof(IKioskEngineService), this);
		KioskEngineProxy.IpcHostUrl = kioskEngineUrl;
	}

	private KioskEngineService()
	{
	}
}
