using Redbox.KioskEngine.ComponentModel;
using Redbox.REDS.Framework;

namespace Redbox.KioskEngine.Environment;

public static class ErrorListExtensions
{
	public static void CopyToLocalCollection(this Redbox.REDS.Framework.ErrorList source, Redbox.KioskEngine.ComponentModel.ErrorList target)
	{
		foreach (Redbox.REDS.Framework.Error item in source)
		{
			target.Add(item.IsWarning ? Redbox.KioskEngine.ComponentModel.Error.NewWarning(item.Code, item.Description, item.Details) : Redbox.KioskEngine.ComponentModel.Error.NewError(item.Code, item.Description, item.Details));
		}
	}
}
