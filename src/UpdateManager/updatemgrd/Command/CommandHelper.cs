using System.Linq;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Command;

internal static class CommandHelper
{
	public static Redbox.IPC.Framework.ErrorList ToIPCErrors(this Redbox.UpdateManager.ComponentModel.ErrorList errors)
	{
		Redbox.IPC.Framework.ErrorList errorList = new Redbox.IPC.Framework.ErrorList();
		errorList.AddRange(errors.Select((Redbox.UpdateManager.ComponentModel.Error each) => (!each.IsWarning) ? Redbox.IPC.Framework.Error.NewError(each.Code, each.Description, each.Details) : Redbox.IPC.Framework.Error.NewWarning(each.Code, each.Description, each.Details)));
		return errorList;
	}
}
