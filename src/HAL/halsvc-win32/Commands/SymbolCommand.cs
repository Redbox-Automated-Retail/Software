using System.ComponentModel;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Attributes;
using Redbox.HAL.IPC.Framework;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Service.Win32.Commands;

[Command("symbol")]
public sealed class SymbolCommand
{
	[CommandForm(Name = "show")]
	[Usage("SYMBOL show job: job-id")]
	[Description("Dumps a diagnostic list of the symbol list contents.")]
	public void Show(CommandContext context, [CommandKeyValue(KeyName = "job", IsRequired = true)] string jobId)
	{
		CommandHelper.GetJob(jobId, context.Errors)?.ForeachSymbol(delegate(IContextSymbol symbol)
		{
			context.Messages.Add($"{symbol.Key}: {symbol.Value}");
		});
	}

	[CommandForm(Name = "clear")]
	[Usage("SYMBOL clear job: job-id")]
	[Description("Clears the execution engine's symbol list.")]
	public void Clear(CommandContext context, [CommandKeyValue(KeyName = "job", IsRequired = true)] string jobId)
	{
		CommandHelper.GetJob(jobId, context.Errors)?.ClearSymbolTable();
	}
}
