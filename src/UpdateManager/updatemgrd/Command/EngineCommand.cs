using System.IO;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.Domain;

namespace Redbox.UpdateManager.Command;

[Command("engine")]
internal class EngineCommand
{
	[CommandForm(Name = "export-queue")]
	[Usage("engine export-queue path:")]
	public void ExportQueue(CommandContext context, [CommandKeyValue(IsRequired = true)] string path)
	{
		if (!Directory.Exists(Path.GetDirectoryName(path)))
		{
			context.Errors.Add(Redbox.IPC.Framework.Error.NewError("D999", $"{Path.GetDirectoryName(path)} does not exists.", "Supply a valid directory and try again."));
			return;
		}
		IKioskEngineService kioskEngineService = ServiceLocator.Instance.GetService<IKioskEngineService>();
		context.Errors.AddRange(kioskEngineService.ExportQueue(path).ToIPCErrors());
	}

	[CommandForm(Name = "export-sessions")]
	[Usage("engine export-sessions path:")]
	public void ExportSession(CommandContext context, [CommandKeyValue(IsRequired = true)] string path)
	{
		if (!Directory.Exists(Path.GetDirectoryName(path)))
		{
			context.Errors.Add(Redbox.IPC.Framework.Error.NewError("D999", $"{Path.GetDirectoryName(path)} does not exists.", "Supply a valid directory and try again."));
			return;
		}
		IKioskEngineService kioskEngineService = ServiceLocator.Instance.GetService<IKioskEngineService>();
		context.Errors.AddRange(kioskEngineService.ExportSessions(path).ToIPCErrors());
	}

	[CommandForm(Name = "load-bundle")]
	[Usage("engine load-bundle bundle:")]
	public void LoadBundle(CommandContext context, [CommandKeyValue(IsRequired = true)] string bundle)
	{
		IKioskEngineService kioskEngineService = ServiceLocator.Instance.GetService<IKioskEngineService>();
		context.Errors.AddRange(kioskEngineService.ChangeBundle(bundle).ToIPCErrors());
	}

	[CommandForm(Name = "show-offline-screen")]
	[Usage("engine show-offline-screen")]
	public void ShowOfflineScreen(CommandContext context)
	{
		IKioskEngineService kioskEngineService = ServiceLocator.Instance.GetService<IKioskEngineService>();
		context.Errors.AddRange(kioskEngineService.ShowOfflineScreen().ToIPCErrors());
	}

	[CommandForm(Name = "reset-bundle")]
	[Usage("engine reset-bundle")]
	public void ResetBundle(CommandContext context)
	{
		IKioskEngineService kioskEngineService = ServiceLocator.Instance.GetService<IKioskEngineService>();
		context.Errors.AddRange(kioskEngineService.ReloadBundle().ToIPCErrors());
	}

	[CommandForm(Name = "bring-to-front")]
	[Usage("engine bring-to-front")]
	public void BringToFront(CommandContext context)
	{
		IKioskEngineService kioskEngineService = ServiceLocator.Instance.GetService<IKioskEngineService>();
		context.Errors.AddRange(kioskEngineService.BringToFront().ToIPCErrors());
	}

	[CommandForm(Name = "shutdown")]
	[Usage("engine shutdown")]
	public void Shutdown(CommandContext context)
	{
		IKioskEngineService kioskEngineService = ServiceLocator.Instance.GetService<IKioskEngineService>();
		kioskEngineService.IsEngineRunning(out var isRunning);
		if (isRunning)
		{
			context.Errors.AddRange(kioskEngineService.Shutdown().ToIPCErrors());
		}
	}

	[CommandForm(Name = "start")]
	[Usage("engine start")]
	public void Start(CommandContext context, string bundle)
	{
		LogHelper.Instance.Log("Starting engine at location: {0}", UpdateManagerService.Instance.KioskEnginePath);
		IKioskEngineService kioskEngineService = ServiceLocator.Instance.GetService<IKioskEngineService>();
		if (string.IsNullOrEmpty(bundle))
		{
			context.Errors.AddRange(kioskEngineService.StartEngine().ToIPCErrors());
		}
		else
		{
			context.Errors.AddRange(kioskEngineService.StartEngine(bundle).ToIPCErrors());
		}
	}

	[CommandForm(Name = "execute-script")]
	[Usage("engine execute-script path:")]
	public void ExecuteScript(CommandContext context, [CommandKeyValue(IsRequired = true)] string path)
	{
		IKioskEngineService kioskEngineService = ServiceLocator.Instance.GetService<IKioskEngineService>();
		context.Errors.AddRange(kioskEngineService.ExecuteScript(path).ToIPCErrors());
	}
}
