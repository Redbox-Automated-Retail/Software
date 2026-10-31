using System.Runtime.InteropServices;

namespace Redbox.Core;

internal static class Redbox_002EMacros1309_002ESimpleSound
{
	public static bool Play(byte[] buffer, Redbox_002EMacros1309_002ESoundFlags flags)
	{
		return PlaySound(buffer, flags);
	}

	public static bool Play(string fileName, Redbox_002EMacros1309_002ESoundFlags flags)
	{
		return PlaySound(fileName, flags);
	}

	public static bool StopPlay()
	{
		return PlaySound((string)null, Redbox_002EMacros1309_002ESoundFlags.SND_PURGE);
	}

	[DllImport("WinMM.dll", SetLastError = true)]
	internal static extern bool PlaySound(string wfname, Redbox_002EMacros1309_002ESoundFlags fuSound);

	[DllImport("WinMM.dll", SetLastError = true)]
	internal static extern bool PlaySound(byte[] buffer, Redbox_002EMacros1309_002ESoundFlags fuSound);
}
