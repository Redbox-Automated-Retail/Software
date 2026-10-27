using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Kernel;

internal static class MacroFunctions
{
	[KernelFunction(Name = "kernel.setmacro")]
	internal static void SetMacro(string name, string value)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		if (service != null)
		{
			service[name] = value;
		}
	}

	[KernelFunction(Name = "kernel.getmacro")]
	internal static string GetMacro(string name)
	{
		return ServiceLocator.Instance.GetService<IMacroService>()?[name];
	}

	[KernelFunction(Name = "kernel.expandmacros")]
	internal static string ExpandRuntimeMacros(string value)
	{
		return ServiceLocator.Instance.GetService<IMacroService>()?.ExpandProperties(value);
	}
}
