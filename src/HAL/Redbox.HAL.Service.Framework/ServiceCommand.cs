using System;
using System.Collections.Generic;
using System.ComponentModel;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Attributes;
using Redbox.HAL.IPC.Framework;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Service.Framework;

[Command("service")]
public sealed class ServiceCommand
{
	[CommandForm(Name = "hardware-status")]
	[Usage("SERVICE hardware-status")]
	[Description("Queries the state of the hardware.")]
	public void Status(CommandContext context)
	{
		context.Errors.Add(Error.NewError("E221", "Deprecated command", "The hardware-status command is deprecated."));
	}

	[CommandForm(Name = "mm-status")]
	[Usage("SERVICE mm-status status: enabled")]
	[Description("")]
	public void ChangeMMStatus(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "status")] bool enabled)
	{
		OnChangeMode(enabled, EngineModes.Maintenance);
	}

	[CommandForm(Name = "merch-mode")]
	[Usage("SERVICE merch-mode status: enabled")]
	[Description("")]
	public void ChangeMerchModeStatus(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "status")] bool enabled)
	{
		OnChangeMode(enabled, EngineModes.Merchandizing);
	}

	[CommandForm(Name = "diagnostic-mode")]
	[Usage("SERVICE diagnostic-mode status: enabled")]
	[Description("")]
	public void ChangeDiagnosticModeStatus(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "status")] bool enabled)
	{
		OnChangeMode(enabled, EngineModes.Diagnostic);
	}

	[CommandForm(Name = "sync-unknowns")]
	[Usage("SERVICE sync-unknowns")]
	[Description("")]
	public void SyncUnknowns(CommandContext context)
	{
		List<ILocation> unknowns = ServiceLocator.Instance.GetService<IInventoryService>().GetUnknowns();
		if (unknowns.Count == 0)
		{
			LogHelper.Instance.Log("Service unknown sync: there are no UNKNOWNS. Nice job!");
			return;
		}
		IExecutionContext executionContext = ServiceLocator.Instance.GetService<IExecutionService>().ScheduleJob("sync-locations", "UNKNOWN inventory sync.", DateTime.Now, ProgramPriority.Low);
		using (new DisposeableList<ILocation>(unknowns))
		{
			executionContext.Push(unknowns);
			LogHelper.Instance.Log("Service unknown sync: {0} locations ( job ID = {1} ).", unknowns.Count, executionContext.ID);
		}
		executionContext.Pend();
		CommandHelper.FormatJob(executionContext, context);
	}

	[CommandForm(Name = "sync-empty")]
	[Usage("SERVICE sync-empty")]
	[Description("")]
	public void SyncEmpty(CommandContext context)
	{
		List<ILocation> emptySlots = ServiceLocator.Instance.GetService<IInventoryService>().GetEmptySlots();
		if (emptySlots.Count == 0)
		{
			LogHelper.Instance.Log("SyncEmpty: there are no empty slots.");
			return;
		}
		IExecutionContext executionContext = ServiceLocator.Instance.GetService<IExecutionService>().ScheduleJob("sync-locations", "EMPTY inventory sync.", DateTime.Now, ProgramPriority.Low);
		using (new DisposeableList<ILocation>(emptySlots))
		{
			executionContext.Push(emptySlots);
			LogHelper.Instance.Log("Service empty sync: {0} locations ( job ID = {1} ).", emptySlots.Count, executionContext.ID);
		}
		executionContext.Pend();
		CommandHelper.FormatJob(executionContext, context);
	}

	[CommandForm(Name = "full-sync")]
	[Usage("SERVICE full-sync")]
	[Description("")]
	public void FullSync(CommandContext context)
	{
		IExecutionContext executionContext = ServiceLocator.Instance.GetService<IExecutionService>().ScheduleJob("sync", "Full inventory sync.", DateTime.Now, ProgramPriority.Low);
		IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
		IDeck deck = service.Last;
		if (deck.IsQlm)
		{
			deck = service.GetByNumber(deck.Number - 1);
		}
		executionContext.Push(deck.NumberOfSlots, StackEnd.Top);
		executionContext.Push(deck.Number, StackEnd.Top);
		IDeck first = service.First;
		executionContext.Push(1, StackEnd.Top);
		executionContext.Push(first.Number, StackEnd.Top);
		LogHelper.Instance.Log("Service executing full sync ( job ID = {0} ).", executionContext.ID);
		executionContext.Pend();
		CommandHelper.FormatJob(executionContext, context);
	}

	[CommandForm(Name = "update-gamp")]
	[Usage("SERVICE update-gamp [backup-directory: 'c:\\backup\\path' ]")]
	[Description("")]
	public void UpdateGamp(CommandContext context, [CommandKeyValue(KeyName = "backup-directory")] string backupDirectory)
	{
		context.Errors.Add(Error.NewError("C999", "Unsupported Client API", "The update-gamp client API is no longer supported."));
	}

	[CommandForm(Name = "last-user-job")]
	[Usage("SERVICE last-user-job")]
	[Description("")]
	public void LastUserJob(CommandContext context)
	{
		context.Messages.Add("NONE");
	}

	[CommandForm(Name = "get-version")]
	[Usage("SERVICE get-version")]
	[Description("")]
	public void GetServiceVersion(CommandContext context)
	{
		context.Messages.Add(typeof(ServiceCommand).Assembly.GetName().Version.ToString());
	}

	[CommandForm(Name = "get-kiosk-id")]
	[Usage("SERVICE get-kiosk-id")]
	[Description("")]
	public void GetKioskID(CommandContext context)
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		context.Messages.Add(service.KioskId);
	}

	[CommandForm(Name = "mark-kiosk-empty")]
	[Usage("SERVICE mark-kiosk-empty")]
	[Description("")]
	public void MarkKioskEmpty(CommandContext context)
	{
		IExecutionContext executionContext = ScheduleHighPriority("mark-kiosk-inventory");
		executionContext.Push("EMPTY", StackEnd.Top);
		executionContext.Pend();
		CommandHelper.FormatJob(executionContext, context);
	}

	[CommandForm(Name = "mark-kiosk-unknown")]
	[Usage("SERVICE mark-kiosk-unknown")]
	[Description("")]
	public void MarkKioskUnknown(CommandContext context)
	{
		IExecutionContext executionContext = ScheduleHighPriority("mark-kiosk-inventory");
		executionContext.Push("UNKNOWN", StackEnd.Top);
		executionContext.Pend();
		CommandHelper.FormatJob(executionContext, context);
	}

	[CommandForm(Name = "dump-inventory-store")]
	[Usage("SERVICE dump-inventory-store")]
	[Description("")]
	public void DumpInventoryStore(CommandContext context)
	{
		IExecutionContext executionContext = ScheduleHighPriority("dump-inventory-store-job");
		executionContext.Pend();
		CommandHelper.FormatJob(executionContext, context);
	}

	[CommandForm(Name = "power-down")]
	[Usage("SERVICE power-down")]
	[Description("")]
	public void PowerDownService(CommandContext context)
	{
		ServiceLocator.Instance.GetService<IExecutionService>().Suspend();
	}

	[CommandForm(Name = "power-up")]
	[Usage("SERVICE power-up")]
	[Description("")]
	public void PowerUpService(CommandContext context)
	{
		ServiceLocator.Instance.GetService<IExecutionService>().Restart();
	}

	[CommandForm(Name = "test-comm")]
	[Usage("SERVICE test-comm")]
	[Description("")]
	public void TestConnection(CommandContext context)
	{
		context.Messages.Add("ACK");
	}

	private IExecutionContext ScheduleHighPriority(string name)
	{
		return ScheduleHighPriority(name, string.Empty);
	}

	private IExecutionContext ScheduleHighPriority(string name, string label)
	{
		return ServiceLocator.Instance.GetService<IExecutionService>().ScheduleJob(name, label, DateTime.Now, ProgramPriority.High);
	}

	private void OnChangeMode(bool enter, EngineModes mode)
	{
		IExecutionService service = ServiceLocator.Instance.GetService<IExecutionService>();
		if (enter)
		{
			service.Enter(mode);
		}
		else
		{
			service.Exit(mode);
		}
	}
}
