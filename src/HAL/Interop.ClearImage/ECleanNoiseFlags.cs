using System.Runtime.InteropServices;

namespace ClearImage;

[ComVisible(true)]
public enum ECleanNoiseFlags
{
	ciCnxBlackNoise = 1,
	ciCnxWhiteNoise = 2,
	ciCnxMarginsNoise = 8
}
