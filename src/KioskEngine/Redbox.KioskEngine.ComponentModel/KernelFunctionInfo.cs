using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Redbox.KioskEngine.ComponentModel;

public class KernelFunctionInfo
{
	private readonly List<KernelFunctionAttribute> m_attributes = new List<KernelFunctionAttribute>();

	// Extension and Method are set by KernelFunctionAttribute.GetKernelFunctionInfos.
	public string Extension { get; internal set; } = null!;

	public MethodInfo Method { get; internal set; } = null!;

	public string? Description { get; internal set; }

	public string? DeprecationWarning { get; internal set; }

	public ReadOnlyCollection<KernelFunctionAttribute> Attributes => m_attributes.AsReadOnly();

	internal List<KernelFunctionAttribute> InnerAttributes => m_attributes;
}
