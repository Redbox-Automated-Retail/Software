using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Redbox.Rental.Model.KioskProduct;

public static class TitleFamilyEnumExtensions
{
	public static IEnumerable<TitleType> GetAllTitleTypesOfTitleFamily(TitleFamily val)
	{
		List<TitleType> list = new List<TitleType>();
		FieldInfo[] fields = typeof(TitleType).GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			TitleFamilyAttribute titleFamilyAttribute = fieldInfo.GetCustomAttributes(inherit: false).OfType<TitleFamilyAttribute>().FirstOrDefault();
			if (titleFamilyAttribute != null && titleFamilyAttribute.TitleFamily == val)
			{
				list.Add((TitleType)fieldInfo.GetValue(null));
			}
		}
		return list;
	}

	public static TitleFamily GetTitleFamily(this Enum val)
	{
		return (val.GetType().GetMember(val.ToString()).FirstOrDefault()?.GetCustomAttribute<TitleFamilyAttribute>(inherit: false)?.TitleFamily).GetValueOrDefault();
	}
}
