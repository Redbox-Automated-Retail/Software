using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.TestData;

public interface IWriteTestDataValueResult
{
	bool Success { get; }

	List<IError> Errors { get; }
}
