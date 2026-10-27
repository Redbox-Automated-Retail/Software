using System;
using Redbox.Core;

namespace Redbox.IPC.Framework;

[Command("script", Filter = "System|Script")]
public class ScriptCommand
{
	[CommandForm(Name = "execute")]
	[Usage("SCRIPT execute path: 'C:\\temp\\my-script.txt'")]
	public void Execute(CommandContext context, [CommandKeyValue(IsRequired = true)] string path)
	{
		try
		{
			BatchCommandRunner.ExecuteFile(path);
		}
		catch (Exception e)
		{
			context.Errors.Add(Error.NewError("S999", "An unhandled exception was raised in ScriptCommmand.Execute.", e));
		}
	}
}
