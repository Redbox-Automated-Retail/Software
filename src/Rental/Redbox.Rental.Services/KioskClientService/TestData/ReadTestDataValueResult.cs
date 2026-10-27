using System;
using System.Collections.Generic;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService.TestData;

namespace Redbox.Rental.Services.KioskClientService.TestData;

public class ReadTestDataValueResult : IReadTestDataValueResult
{
	public string PropertyValue { get; set; }

	public bool Success { get; set; }

	public List<IError> Errors { get; set; }

	public ReadTestDataValueResult()
	{
		Errors = new List<IError>();
	}

	public static IReadTestDataValueResult CreateFromReadTestDataResponse(ReadTestDataResponse readTestDataResponse)
	{
		ReadTestDataValueResult readTestDataValueResult = null;
		if (readTestDataResponse != null)
		{
			readTestDataValueResult = new ReadTestDataValueResult
			{
				Success = readTestDataResponse.Success,
				PropertyValue = readTestDataResponse.PropertyValue
			};
			if (readTestDataResponse?.Errors != null)
			{
				foreach (Redbox.Rental.Model.Error error in readTestDataResponse.Errors)
				{
					readTestDataValueResult.Errors.Add(new Error
					{
						Code = error.Code,
						Message = error.Message
					});
				}
			}
		}
		return readTestDataValueResult;
	}

	public T GetPropertyValue<T>()
	{
		Type conversionType = typeof(T);
		Type underlyingType = Nullable.GetUnderlyingType(typeof(T));
		if (underlyingType != null)
		{
			conversionType = underlyingType;
		}
		try
		{
			return (T)Convert.ChangeType(PropertyValue, conversionType);
		}
		catch (Exception ex)
		{
			if (underlyingType != null)
			{
				return default(T);
			}
			throw ex;
		}
	}
}
