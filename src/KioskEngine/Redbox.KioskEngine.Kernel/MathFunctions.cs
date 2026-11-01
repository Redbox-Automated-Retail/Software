using System;
using System.Security.Cryptography;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Kernel;

internal static class MathFunctions
{
	[KernelFunction(Name = "kernel.round")]
	internal static decimal Round(decimal value, int places)
	{
		return decimal.Round(value, places, MidpointRounding.AwayFromZero);
	}

	[KernelFunction(Name = "kernel.random")]
	internal static int GetRandomNumber(int min, int max)
	{
		RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider();
		byte[] buffer = new byte[4];
		rNGCryptoServiceProvider.GetBytes(buffer);
		int randomValue = BitConverter.ToInt32(buffer, 0);
		return min + Math.Abs(randomValue % (max - min));
	}

	[KernelFunction(Name = "kernel.formatasbytesize")]
	internal static string FormatAsByteSize(long size)
	{
		return size.FormatAsByteSize();
	}
}
