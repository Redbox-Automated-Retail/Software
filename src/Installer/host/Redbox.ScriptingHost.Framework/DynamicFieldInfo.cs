using System;
using System.Globalization;
using System.Reflection;

namespace Redbox.ScriptingHost.Framework;

internal class DynamicFieldInfo : FieldInfo
{
	private readonly string m_name;

	public override FieldAttributes Attributes => FieldAttributes.PrivateScope;

	public override RuntimeFieldHandle FieldHandle => default(RuntimeFieldHandle);

	public override Type FieldType => typeof(object);

	public override Type ReflectedType => null;

	public override Type DeclaringType => null;

	public override string Name => m_name;

	public DynamicFieldInfo(string name)
	{
		m_name = name;
	}

	public override object GetValue(object obj)
	{
		return (!(obj is KernelObject kernelObject)) ? null : kernelObject.GetDynamicProperty(Name);
	}

	public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
	{
		if (obj is KernelObject kernelObject)
		{
			kernelObject.SetDynamicProperty(Name, value);
		}
	}

	public override object[] GetCustomAttributes(bool inherit)
	{
		return null;
	}

	public override object[] GetCustomAttributes(Type attributeType, bool inherit)
	{
		return null;
	}

	public override bool IsDefined(Type attributeType, bool inherit)
	{
		return false;
	}
}
