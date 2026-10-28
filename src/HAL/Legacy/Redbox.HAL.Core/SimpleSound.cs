using System.Runtime.InteropServices;

namespace Redbox.HAL.Core;

public static class SimpleSound
{
	public static bool Play(string fileName, SoundFlags flags)
	{
		return PlaySound(fileName, flags);
	}

	public static bool StopPlay()
	{
		return PlaySound(null, SoundFlags.SND_PURGE);
	}

	[DllImport("WinMM.dll", SetLastError = true)]
	internal static extern bool PlaySound(string wfname, SoundFlags fuSound);
}
