namespace Redbox.HAL.Common.GUI.Functions;

public class ExecuteServiceCommand : ImmediateCommand
{
	protected internal override string Token => "%";

	protected override string Help => "Executes the given HAL Service command.";

	protected override void OnExecute(ImmediateCommandResult result)
	{
		if (base.Arguments.Count != 0 && !string.IsNullOrEmpty(base.Arguments[0]))
		{
			result.CommandResult = ImmediateCommand.Service.ExecuteServiceCommand(base.Arguments[0]);
		}
	}
}
