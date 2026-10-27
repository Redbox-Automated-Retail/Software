namespace Redbox.HAL.Core.Descriptors;

internal sealed class _3mHardResetCommand : _3MResetCommand
{
	protected override byte ResetByte => 2;

	protected override byte CompletionByte => 5;

	internal _3mHardResetCommand()
		: base(ResetType_3M.Hard)
	{
	}
}
