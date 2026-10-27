using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal class DefaultHandler : IFileDiskHandler
{
	public void MachineFull(ExecutionResult result, string idInPicker)
	{
	}

	public FileDiscIterationResult MoveError(ExecutionResult result, string idInPicker, ErrorCodes moveResult, int deck, int slot)
	{
		return FileDiscIterationResult.Continue;
	}

	public void DiskFiled(ExecutionResult result, IPutResult putResult)
	{
	}

	public FileDiscIterationResult OnFailedPut(ExecutionResult result, IPutResult putResult)
	{
		return FileDiscIterationResult.Continue;
	}

	public void FailedToFileDisk(ExecutionResult result, string idInPicker)
	{
	}
}
