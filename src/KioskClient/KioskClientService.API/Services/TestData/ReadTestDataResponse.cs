using System;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.TestData;

public class ReadTestDataResponse : BaseResponse
{
	public string PropertyValue { get; set; }

	public ReadTestDataResponse()
		: base(200)
	{
		base.Success = true;
	}

	public T GetPropertyValue<T>()
	{
		Type conversionType = typeof(T);
		Type underlyingType = Nullable.GetUnderlyingType(typeof(T));
		if (underlyingType != null)
		{
			conversionType = underlyingType;
			if (PropertyValue == null)
			{
				return default(T);
			}
		}
		if (typeof(T).IsEnum)
		{
			return (T)Enum.Parse(typeof(T), PropertyValue);
		}
		T result;
		try
		{
			return (T)Convert.ChangeType(PropertyValue, conversionType);
		}
		catch (Exception ex)
		{
			if (!(underlyingType != null))
			{
				throw ex;
			}
			result = default(T);
		}
		return result;
	}
}
