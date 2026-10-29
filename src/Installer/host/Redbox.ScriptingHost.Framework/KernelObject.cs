using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Redbox.ScriptingHost.Framework;

public class KernelObject : IVsaKernelObject, IReflect
{
	private IDictionary<string, object> m_dynamicProperties;

	public object this[string name]
	{
		get
		{
			return GetDynamicProperty(name);
		}
		set
		{
			SetDynamicProperty(name, value);
		}
	}

	public Type UnderlyingSystemType
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	internal IDictionary<string, object> DynamicProperties
	{
		get
		{
			if (m_dynamicProperties == null)
			{
				m_dynamicProperties = new Dictionary<string, object>();
			}
			return m_dynamicProperties;
		}
	}

	public void SetDynamicProperty(string name, object value)
	{
		DynamicProperties[name] = value;
	}

	public object GetDynamicProperty(string name)
	{
		return DynamicProperties.ContainsKey(name) ? DynamicProperties[name] : null;
	}

	public bool RemoveDynamicProperty(string name)
	{
		return DynamicProperties.Remove(name);
	}

	public MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
	{
		MemberInfo[] member = GetType().GetMember(name, bindingAttr);
		if (member != null && member.Length > 0)
		{
			return member;
		}
		return new MemberInfo[1]
		{
			new DynamicFieldInfo(name)
		};
	}

	public object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
	{
		if (string.IsNullOrEmpty(name))
		{
			return ToString();
		}
		return GetType().GetMethod(name, invokeAttr)?.Invoke(target, args);
	}

	public MethodInfo GetMethod(string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers)
	{
		throw new NotImplementedException();
	}

	public MethodInfo GetMethod(string name, BindingFlags bindingAttr)
	{
		throw new NotImplementedException();
	}

	public MethodInfo[] GetMethods(BindingFlags bindingAttr)
	{
		throw new NotImplementedException();
	}

	public FieldInfo GetField(string name, BindingFlags bindingAttr)
	{
		throw new NotImplementedException();
	}

	public FieldInfo[] GetFields(BindingFlags bindingAttr)
	{
		throw new NotImplementedException();
	}

	public PropertyInfo GetProperty(string name, BindingFlags bindingAttr)
	{
		throw new NotImplementedException();
	}

	public PropertyInfo GetProperty(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
	{
		throw new NotImplementedException();
	}

	public PropertyInfo[] GetProperties(BindingFlags bindingAttr)
	{
		throw new NotImplementedException();
	}

	public MemberInfo[] GetMembers(BindingFlags bindingAttr)
	{
		throw new NotImplementedException();
	}
}
