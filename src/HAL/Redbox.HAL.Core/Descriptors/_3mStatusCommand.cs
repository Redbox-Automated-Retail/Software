using Microsoft.Win32.SafeHandles;

namespace Redbox.HAL.Core.Descriptors;

internal sealed class _3mStatusCommand : Abstract3MCommand
{
	private readonly byte ResponseSize = 20;

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
			result = 6;
			break;
		case 6:
			result = ResponseSize;
			break;
		}
		return result;
	}
}
