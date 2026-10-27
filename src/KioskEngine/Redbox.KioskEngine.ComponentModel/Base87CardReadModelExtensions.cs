using System;
using System.Collections.Generic;
using System.Linq;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Responses;

namespace Redbox.KioskEngine.ComponentModel;

public static class Base87CardReadModelExtensions
{
	public static IList<Error> GetTamperedErrors(this Base87CardReadModel source)
	{
		List<Error> result = new List<Error>();
		source.Errors.Where((DeviceService.ComponentModel.Error e) => string.Equals(e.Code, "TAMPERED", StringComparison.CurrentCultureIgnoreCase)).ToList().ForEach(delegate(DeviceService.ComponentModel.Error e)
		{
			result.Add(Error.NewError(e.Code, e.Message, ""));
		});
		return result;
	}
}
