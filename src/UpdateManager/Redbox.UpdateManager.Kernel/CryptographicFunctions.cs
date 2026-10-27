using System.IO;
using Redbox.Core;

namespace Redbox.UpdateManager.Kernel;

public static class CryptographicFunctions
{
	[KernelFunction(Name = "kernel.hashfile")]
	internal static string HashFile(string fileName)
	{
		using FileStream inputStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
		return inputStream.ToASCIISHA1Hash();
	}

	[KernelFunction(Name = "kernel.comparehash")]
	internal static bool CompareHash(string lhs, string rhs)
	{
		return HashFile(lhs) == HashFile(rhs);
	}
}
