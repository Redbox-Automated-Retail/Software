using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Command;

[Command("configfile")]
internal class ConfigFileCommand
{
	[CommandForm(Name = "activate")]
	[Usage("configfile activate configfileid:")]
	public void ActivateConfigFile(CommandContext context, [CommandKeyValue(IsRequired = true)] long configfileid)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = ServiceLocator.Instance.GetService<IConfigFileService>().ActivateConfigFile(configfileid);
		if (errorList.ContainsError())
		{
			context.Errors.AddRange(errorList.ToIPCErrors());
		}
	}
}
