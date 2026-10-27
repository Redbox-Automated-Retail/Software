using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Redbox.KioskEngine.ComponentModel;

public class KernelFunctionInfo
{
	private readonly List<KernelFunctionAttribute> m_attributes = new List<KernelFunctionAttribute>();

	public string Extension { get; internal set; }

	public MethodInfo Method { get; internal set; }

	public string Description { get; internal set; }

	public string DeprecationWarning { get; internal set; }

	public ReadOnlyCollection<KernelFunctionAttribute> Attributes => m_attributes.AsReadOnly();

	internal List<KernelFunctionAttribute> InnerAttributes => m_attributes;
}
