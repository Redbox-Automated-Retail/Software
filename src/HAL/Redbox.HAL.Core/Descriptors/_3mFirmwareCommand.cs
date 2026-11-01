using System;
using Microsoft.Win32.SafeHandles;

namespace Redbox.HAL.Core.Descriptors;

internal sealed class _3mFirmwareCommand : Abstract3MCommand
{
	private readonly byte ResponseSize = 24;

	protected override byte[] OnReadResponse(SafeFileHandle handle)
	{
		return Read(handle, ResponseSize);
	}

	protected override byte OnFillIndex(int idx)
	{
		byte result = 0;
		switch (idx)
		{
		case 0:
			result = 192;
			break;
		case 1:
			result = 10;
			break;
		case 6:
			result = ResponseSize;
			break;
		}
		return result;
	}

	internal string GetFirmwareRevision()
	{
		byte[] response = SendReceive();
		if (response.Length != 0)
		{
			return $"{BitConverter.ToString(response, 3, 1)}.{BitConverter.ToString(response, 4, 1)}";
		}
		return "UNKNOWN";
	}

	internal _3mFirmwareCommand()
	{
	}
}
