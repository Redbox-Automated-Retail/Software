using System;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.UpdateServices.KernelExtension;

[Command("engine")]
public class EngineCommand
{
	[CommandForm(Name = "ping")]
	[Usage("engine ping")]
	public void Ping(CommandContext context)
	{
	}

	[CommandForm(Name = "shutdown")]
	[Usage("engine shutdown")]
	public void ShutDown(CommandContext context)
	{
		try
		{
			LogHelper.Instance.Log("Engine Command: ShutDownEngine");
			IEngineApplication service = ServiceLocator.Instance.GetService<IEngineApplication>();
			if (service != null)
			{
				if (service.CanShutDown())
				{
					service.ShutDown();
				}
				else
				{
					service.FlagForSafeShutdown();
				}
			}
			else
			{
				LogHelper.Instance.Log("Unable to locate EngineApplication service");
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error in EngineCommand ShutDownEngine.", e);
		}
	}

	[CommandForm(Name = "load-control-panel")]
	[Usage("engine load-control-panel")]
	public void LoadControlPanel(CommandContext context)
	{
		try
		{
			LogHelper.Instance.Log("Engine Command: load-control-bundle");
			IEngineApplication service = ServiceLocator.Instance.GetService<IEngineApplication>();
			if (service != null)
			{
				if (service.CanRestart())
				{
					LogHelper.Instance.Log("Loading Control Panel Bundle...");
					IResourceBundleService service2 = ServiceLocator.Instance.GetService<IResourceBundleService>();
					if (service2 != null)
					{
						service2.Activate("Control Panel Application", "*");
					}
					else
					{
						LogHelper.Instance.Log("Unable to locate ResourceBundle service");
					}
				}
				else
				{
					LogHelper.Instance.Log("CanRestart() Returned False; Not Restarting Bundle.");
				}
			}
			else
			{
				LogHelper.Instance.Log("Unable to locate EngineApplication service");
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error in EngineCommand ResetBundle.", e);
		}
	}

	[CommandForm(Name = "reset-bundle")]
	[Usage("engine reset-bundle")]
	internal static void ResetBundle(string[] parms)
	{
		try
		{
			LogHelper.Instance.Log("Engine Command: reset-bundle");
			IEngineApplication service = ServiceLocator.Instance.GetService<IEngineApplication>();
			if (service != null)
			{
				if (service.CanRestart())
				{
					LogHelper.Instance.Log("Resetting Bundle...");
					IResourceBundleService service2 = ServiceLocator.Instance.GetService<IResourceBundleService>();
					if (service2 != null)
					{
						service2.Restart();
					}
					else
					{
						LogHelper.Instance.Log("Unable to locate ResourceBundle service");
					}
				}
				else
				{
					LogHelper.Instance.Log("CanRestart() Returned False; Not Restarting Bundle.");
				}
			}
			else
			{
				LogHelper.Instance.Log("Unable to locate EngineApplication service");
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error in EngineCommand ResetBundle.", e);
		}
	}
}
