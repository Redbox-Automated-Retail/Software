using System;
using System.Linq;
using System.Reflection;

namespace KioskClientService.API.Models;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class ResponseCodeAttribute : Attribute
{
	public string Name { get; set; }

	public static string GetCodeFromEnum<T>(object enumValue)
	{
		Type enumType = typeof(T);
		MemberInfo[] source = enumType?.GetMember(enumValue?.ToString());
		MemberInfo memberInfo = source.FirstOrDefault((MemberInfo m) => m.DeclaringType == enumType);
		object[] customAttributes = memberInfo.GetCustomAttributes(typeof(ResponseCodeAttribute), inherit: false);
		if (customAttributes.Count() <= 0)
		{
			return null;
		}
		return (customAttributes[0] as ResponseCodeAttribute)?.Name;
	}
}
