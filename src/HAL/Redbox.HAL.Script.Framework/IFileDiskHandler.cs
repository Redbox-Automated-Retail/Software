using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal interface IFileDiskHandler
{
	void MachineFull(ExecutionResult result, string idInPicker);

	FileDiscIterationResult MoveError(ExecutionResult result, string idInPicker, ErrorCodes moveResult, int deck, int slot);

	void DiskFiled(ExecutionResult result, IPutResult putResult);

	FileDiscIterationResult OnFailedPut(ExecutionResult result, IPutResult putResult);

	void FailedToFileDisk(ExecutionResult result, string idInPicker);
}
