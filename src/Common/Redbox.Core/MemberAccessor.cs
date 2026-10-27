using System;
using System.Reflection;

namespace Redbox.Core;

public class MemberAccessor
{
	private readonly FieldInfo m_fieldInfo;

	private readonly MemberInfo m_memberInfo;

	private readonly PropertyInfo m_propertyInfo;

	public Type Type
	{
		get
		{
			if (!(m_fieldInfo != null))
			{
				return m_propertyInfo.PropertyType;
			}
			return m_fieldInfo.FieldType;
		}
	}

	public string Name => m_memberInfo.Name;

	public MemberAccessor(MemberInfo memberInfo)
	{
		m_memberInfo = memberInfo;
		if (m_memberInfo.MemberType == MemberTypes.Field)
		{
			m_fieldInfo = (FieldInfo)m_memberInfo;
			return;
		}
		if (m_memberInfo.MemberType == MemberTypes.Property)
		{
			m_propertyInfo = (PropertyInfo)m_memberInfo;
			return;
		}
		throw new ArgumentException("MemberAccessor only supports fields and properties.");
	}

	public void SetValue(object instance, object value)
	{
		SetValue(instance, value, null);
	}

	public void SetValue(object instance, object value, object[] index)
	{
		if (m_fieldInfo != null)
		{
			m_fieldInfo.SetValue(instance, value);
		}
		else
		{
			m_propertyInfo.SetValue(instance, value, index);
		}
	}

	public object GetValue(object instance)
	{
		return GetValue(instance, null);
	}

	public object GetValue(object instance, object[] index)
	{
		if (!(m_fieldInfo != null))
		{
			return m_propertyInfo.GetValue(instance, index);
		}
		return m_fieldInfo.GetValue(instance);
	}
}
