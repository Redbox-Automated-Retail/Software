using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.TestData;

public interface IReadTestDataValueResult
{
	string PropertyValue { get; }

	bool Success { get; }

	List<IError> Errors { get; }

	T GetPropertyValue<T>();
}
