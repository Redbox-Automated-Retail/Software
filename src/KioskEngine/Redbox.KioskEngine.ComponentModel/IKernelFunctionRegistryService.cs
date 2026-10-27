using System.Collections.ObjectModel;
using System.Reflection;

namespace Redbox.KioskEngine.ComponentModel;

public interface IKernelFunctionRegistryService
{
	ErrorList Reset();

	void RegisterKernelFunctions(Assembly assembly, string extension);

	ReadOnlyCollection<KernelFunctionInfo> GetKernelFunctions();
}
