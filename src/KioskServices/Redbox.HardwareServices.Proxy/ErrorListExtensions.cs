using Redbox.HAL.Component.Model;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.HardwareServices.Proxy;

public static class ErrorListExtensions
{
	public static void AddToLocalErrorList(this Redbox.HAL.Component.Model.ErrorList foreginErrorList, Redbox.KioskEngine.ComponentModel.ErrorList errors)
	{
		foreach (Redbox.HAL.Component.Model.Error foreginError in foreginErrorList)
		{
			errors.Add(foreginError.IsWarning ? Redbox.KioskEngine.ComponentModel.Error.NewWarning(foreginError.Code, foreginError.Description, foreginError.Details) : Redbox.KioskEngine.ComponentModel.Error.NewError(foreginError.Code, foreginError.Description, foreginError.Details));
		}
	}
}
