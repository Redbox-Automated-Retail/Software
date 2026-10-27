using System.Collections.Generic;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService.TestData;

namespace Redbox.Rental.Services.KioskClientService.TestData;

public class WriteTestDataValueResult : IWriteTestDataValueResult
{
	public bool Success { get; set; }

	public List<IError> Errors { get; }

	public WriteTestDataValueResult()
	{
		Errors = new List<IError>();
	}

	public static IWriteTestDataValueResult CreateFromWriteTestDataResponse(WriteTestDataResponse readTestDataResponse)
	{
		WriteTestDataValueResult writeTestDataValueResult = null;
		if (readTestDataResponse != null)
		{
			writeTestDataValueResult = new WriteTestDataValueResult
			{
				Success = readTestDataResponse.Success
			};
			if (readTestDataResponse?.Errors != null)
			{
				foreach (Redbox.Rental.Model.Error error in readTestDataResponse.Errors)
				{
					writeTestDataValueResult.Errors.Add(new Error
					{
						Code = error.Code,
						Message = error.Message
					});
				}
			}
		}
		return writeTestDataValueResult;
	}
}
