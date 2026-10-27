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
		IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
		context.Errors.AddRange(service.ExportQueue(path).ToIPCErrors());
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
		IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
		context.Errors.AddRange(service.ExportSessions(path).ToIPCErrors());
	}

	[CommandForm(Name = "load-bundle")]
	[Usage("engine load-bundle bundle:")]
	public void LoadBundle(CommandContext context, [CommandKeyValue(IsRequired = true)] string bundle)
	{
		IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
		context.Errors.AddRange(service.ChangeBundle(bundle).ToIPCErrors());
	}

	[CommandForm(Name = "show-offline-screen")]
	[Usage("engine show-offline-screen")]
	public void ShowOfflineScreen(CommandContext context)
	{
		IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
		context.Errors.AddRange(service.ShowOfflineScreen().ToIPCErrors());
	}

	[CommandForm(Name = "reset-bundle")]
	[Usage("engine reset-bundle")]
	public void ResetBundle(CommandContext context)
	{
		IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
		context.Errors.AddRange(service.ReloadBundle().ToIPCErrors());
	}

	[CommandForm(Name = "bring-to-front")]
	[Usage("engine bring-to-front")]
	public void BringToFront(CommandContext context)
	{
		IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
		context.Errors.AddRange(service.BringToFront().ToIPCErrors());
	}

	[CommandForm(Name = "shutdown")]
	[Usage("engine shutdown")]
	public void Shutdown(CommandContext context)
	{
		IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
		service.IsEngineRunning(out var isRunning);
		if (isRunning)
		{
			context.Errors.AddRange(service.Shutdown().ToIPCErrors());
		}
	}

	[CommandForm(Name = "start")]
	[Usage("engine start")]
	public void Start(CommandContext context, string bundle)
	{
		LogHelper.Instance.Log("Starting engine at location: {0}", UpdateManagerService.Instance.KioskEnginePath);
		IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
		if (string.IsNullOrEmpty(bundle))
		{
			context.Errors.AddRange(service.StartEngine().ToIPCErrors());
		}
		else
		{
			context.Errors.AddRange(service.StartEngine(bundle).ToIPCErrors());
		}
	}

	[CommandForm(Name = "execute-script")]
	[Usage("engine execute-script path:")]
	public void ExecuteScript(CommandContext context, [CommandKeyValue(IsRequired = true)] string path)
	{
		IKioskEngineService service = ServiceLocator.Instance.GetService<IKioskEngineService>();
		context.Errors.AddRange(service.ExecuteScript(path).ToIPCErrors());
	}
}
