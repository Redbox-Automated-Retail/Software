using System.Text;

namespace Redbox.HAL.Common.GUI.Functions;

public class HelpImmediateCommand : ImmediateCommand
{
	protected internal override string Token => "?";

	protected override string Help => "Provides a list of immediate commands and what active they perform.";

	protected override void OnExecute(ImmediateCommandResult result)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (ImmediateCommand value in ImmediateCommand.Commands.Values)
		{
			if (!string.IsNullOrEmpty(value.Token))
			{
				stringBuilder.AppendLine(value.ToString());
			}
		}
		result.Message = stringBuilder.ToString();
	}
}
