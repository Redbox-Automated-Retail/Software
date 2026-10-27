namespace Redbox.HAL.Controller.Framework;

internal interface ICoreCommandExecutor
{
	CoreResponse ExecuteControllerCommand(CommandType type);

	CoreResponse ExecuteControllerCommand(CommandType type, int? timeout);
}
