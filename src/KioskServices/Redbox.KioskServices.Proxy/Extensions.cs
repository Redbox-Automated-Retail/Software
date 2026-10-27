using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;

namespace Redbox.KioskServices.Proxy;

internal static class Extensions
{
	public static ErrorList ToKEComponentModelErrors(this List<Redbox.Rental.Model.Error> source)
	{
		ErrorList errorList = null;
		if (source != null && source.Count > 0)
		{
			errorList = new ErrorList();
			foreach (Redbox.Rental.Model.Error item in source)
			{
				errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError(item.Code, item.Message, (string)null));
			}
		}
		return errorList;
	}
}
