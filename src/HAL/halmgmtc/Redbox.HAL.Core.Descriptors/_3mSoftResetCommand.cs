namespace Redbox.HAL.Core.Descriptors;

internal sealed class _3mSoftResetCommand : _3MResetCommand
{
	protected override byte ResetByte => 1;

	protected override byte CompletionByte => 4;

	internal _3mSoftResetCommand()
		: base(ResetType_3M.Soft)
	{
	}
}
