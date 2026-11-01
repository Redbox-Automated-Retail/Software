using System;
using System.Reflection;
using Redbox.Core;

namespace Redbox.KioskEngine.ComponentModel.KioskServices;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class ResponseCodeAttribute : Attribute
{
	public string Name { get; set; }

	public static object GetFieldsForResponseCode(Type type, string code)
	{
		FieldInfo[] fields = ConversionHelper.GetNullableTypeIfWrapped(type).GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (GetResponseCodes(fieldInfo, code) != null)
			{
				return fieldInfo.GetValue(null);
			}
		}
		return null;
	}

	private static ResponseCodeAttribute GetResponseCodes(MemberInfo fieldInfo, string code)
	{
		ResponseCodeAttribute[] attributes = (ResponseCodeAttribute[])Attribute.GetCustomAttributes(fieldInfo, typeof(ResponseCodeAttribute));
		foreach (ResponseCodeAttribute responseCodeAttribute in attributes)
		{
			if (string.Compare(responseCodeAttribute.Name, code, ignoreCase: true) == 0)
			{
				return responseCodeAttribute;
			}
		}
		foreach (ResponseCodeAttribute responseCodeAttribute2 in attributes)
		{
			if (responseCodeAttribute2.Name == "*")
			{
				return responseCodeAttribute2;
			}
		}
		return null;
	}
}
