using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Kernel;

public class MacroFunctions
{
	[KernelFunction(Name = "kernel.getmacrovalue")]
	internal static string GetMacroValue(string name)
	{
		return ServiceLocator.Instance.GetService<IMacroService>()[name];
	}

	[KernelFunction(Name = "kernel.expandconstantmacro")]
	internal static string ExpandConstantMacro(string value)
	{
		return ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(value);
	}

	[KernelFunction(Name = "kernel.expandruntimemacro")]
	internal static string ExpandRuntimeMacro(string value)
	{
		return ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(value);
	}

	[KernelFunction(Name = "kernel.setmacrovalue")]
	internal static void SetMacroValue(string name, string value)
	{
		ServiceLocator.Instance.GetService<IMacroService>()[name] = value;
	}
}
