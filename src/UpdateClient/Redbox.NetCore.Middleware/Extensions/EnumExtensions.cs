using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Redbox.NetCore.Middleware.Extensions;

public static class EnumExtensions
{
	public static T ParseEnumByNameAndAttributes<T>(this string source) where T : struct, Enum
	{
		if (Enum.TryParse<T>(source, ignoreCase: true, out var result))
		{
			return result;
		}
		Type typeFromHandle = typeof(T);
		string[] names = Enum.GetNames(typeFromHandle);
		foreach (string text in names)
		{
			MemberInfo[] member = typeFromHandle.GetMember(text);
			if (member.Length != 0)
			{
				object[] customAttributes = member[0].GetCustomAttributes(typeof(DisplayAttribute), inherit: false);
				if (customAttributes.Length != 0 && ((DisplayAttribute)customAttributes[0]).Name.Equals(source.Trim(), StringComparison.OrdinalIgnoreCase))
				{
					return Enum.Parse<T>(text);
				}
				object[] customAttributes2 = member[0].GetCustomAttributes(typeof(EnumMemberAttribute), inherit: false);
				if (customAttributes2.Length != 0 && ((EnumMemberAttribute)customAttributes2[0]).Value.Equals(source.Trim(), StringComparison.OrdinalIgnoreCase))
				{
					return Enum.Parse<T>(text);
				}
			}
		}
		return default(T);
	}

	public static string GetDescriptionFromEnumValue(this Enum value)
	{
		if (value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(EnumMemberAttribute), inherit: false)
			.SingleOrDefault() is EnumMemberAttribute enumMemberAttribute)
		{
			return enumMemberAttribute.Value;
		}
		return value.ToString();
	}
}
