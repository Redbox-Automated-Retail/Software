using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Kernel;

internal static class SoundFunctions
{
	[KernelFunction(Name = "kernel.soundstop")]
	internal static void StopSound()
	{
		ServiceLocator.Instance.GetService<ISoundService>().StopPlay();
	}

	[KernelFunction(Name = "kernel.ttsrepeat")]
	internal static void TTSRepeat()
	{
		ServiceLocator.Instance.GetService<ITextToSpeechService>()?.TTSRepeatSequence();
	}

	[KernelFunction(Name = "kernel.soundplaysync")]
	internal static void PlaySoundSync(string resourceName)
	{
		ISoundService service = ServiceLocator.Instance.GetService<ISoundService>();
		byte[] sound = ServiceLocator.Instance.GetService<IResourceBundleService>().GetSound(resourceName);
		if (sound != null)
		{
			service.PlaySync(sound);
		}
	}

	[KernelFunction(Name = "kernel.soundplayasync")]
	internal static void PlaySoundAsync(string resourceName)
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		ISoundService service2 = ServiceLocator.Instance.GetService<ISoundService>();
		byte[] sound = service.GetSound(resourceName);
		if (sound != null)
		{
			service2.PlayAsync(sound);
		}
	}
}
