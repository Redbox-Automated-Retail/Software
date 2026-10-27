using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Kernel;

internal static class TweenFunctions
{
	[KernelFunction(Name = "kernel.tweencreate")]
	internal static void CreateTween(string name, string type, float begin, float finish, float duration, TweenChange changeHandler, TweenEnd endHandler)
	{
		ServiceLocator.Instance.GetService<ITweenService>().CreateTween(name, Enum<TweenType>.ParseIgnoringCase(type, TweenType.Linear), begin, finish, duration, changeHandler, endHandler);
	}

	[KernelFunction(Name = "kernel.tweenstop")]
	internal static void StopTween(string name)
	{
		ServiceLocator.Instance.GetService<ITweenService>().StopTween(name);
	}

	[KernelFunction(Name = "kernel.tweenstart")]
	internal static void StartTween(string name)
	{
		ServiceLocator.Instance.GetService<ITweenService>().StartTween(name);
	}

	[KernelFunction(Name = "kernel.tweenremove")]
	internal static void RemoveTween(string name)
	{
		ServiceLocator.Instance.GetService<ITweenService>().RemoveTween(name);
	}

	internal static void Reset()
	{
		ServiceLocator.Instance.GetService<ITweenService>().Reset();
	}
}
