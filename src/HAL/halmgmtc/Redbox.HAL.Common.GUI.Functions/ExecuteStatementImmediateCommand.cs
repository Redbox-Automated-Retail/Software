namespace Redbox.HAL.Common.GUI.Functions;

public class ExecuteStatementImmediateCommand : ImmediateCommand
{
	protected override void OnExecute(ImmediateCommandResult result)
	{
		if (base.Arguments.Count != 0 && !string.IsNullOrEmpty(base.Arguments[0]))
		{
			result.CommandResult = ImmediateCommand.Service.ExecuteImmediate(base.Arguments[0], 120000, out var _);
		}
	}
}
