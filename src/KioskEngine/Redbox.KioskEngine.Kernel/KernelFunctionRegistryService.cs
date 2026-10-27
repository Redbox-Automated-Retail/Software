using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Kernel;

public class KernelFunctionRegistryService : IKernelFunctionRegistryService
{
	private readonly List<KernelFunctionInfo> m_kernelFunctions = new List<KernelFunctionInfo>();

	public static KernelFunctionRegistryService Instance => Singleton<KernelFunctionRegistryService>.Instance;

	public ErrorList Reset()
	{
		ErrorList result = new ErrorList();
		m_kernelFunctions.Clear();
		RegisterKernelFunctions(typeof(KernelService).Assembly, "Core Server Engine");
		return result;
	}

	public ReadOnlyCollection<KernelFunctionInfo> GetKernelFunctions()
	{
		return m_kernelFunctions.AsReadOnly();
	}

	public void RegisterKernelFunctions(Assembly assembly, string extension)
	{
		ReadOnlyCollection<KernelFunctionInfo> kernelFunctionInfos = KernelFunctionAttribute.GetKernelFunctionInfos(assembly, extension);
		m_kernelFunctions.AddRange(kernelFunctionInfos);
	}

	private KernelFunctionRegistryService()
	{
	}
}
