using Microsoft.Win32.SafeHandles;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core.Descriptors;

internal abstract class _3MResetCommand : Abstract3MCommand
{
	protected enum ResetType_3M
	{
		Soft,
		Hard
	}

	private readonly ResetType_3M Type;

	protected abstract byte ResetByte { get; }

	protected abstract byte CompletionByte { get; }

	protected override byte[] OnReadResponse(SafeFileHandle handle)
	{
		return new byte[1];
	}

	protected override byte OnFillIndex(int idx)
	{
		byte result = 0;
		switch (idx)
		{
		case 0:
			result = 64;
			break;
		case 1:
			result = 7;
			break;
		case 2:
			result = ResetByte;
			break;
		}
		return result;
	}

	protected _3MResetCommand(ResetType_3M type)
	{
		Type = type;
	}

	internal bool Reset()
	{
		if (SendReceive().Length == 0)
		{
			return false;
		}
		int pause = ((Type == ResetType_3M.Soft) ? 750 : 15000);
		return GetStatus(pause);
	}

	private bool GetStatus(int pause)
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		bool result = false;
		_3mStatusCommand _3mStatusCommand2 = new _3mStatusCommand();
		for (int i = 0; i < 5; i++)
		{
			service.Wait(pause);
			byte[] array = _3mStatusCommand2.SendReceive();
			if (array.Length == 0)
			{
				break;
			}
			if (array[3] == CompletionByte)
			{
				result = true;
				break;
			}
		}
		return result;
	}
}
