using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class PutResult : IPutResult
{
	public bool Success => Code == ErrorCodes.Success;

	public bool IsSlotInUse => ErrorCodes.SlotInUse == Code;

	public bool PickerEmpty => ErrorCodes.PickerEmpty == Code;

	public bool PickerObstructed => ErrorCodes.PickerObstructed == Code;

	public bool IsDuplicate { get; internal set; }

	public ILocation PutLocation { get; private set; }

	public ErrorCodes Code { get; internal set; }

	public string OriginalMatrix { get; private set; }

	public string StoredMatrix { get; internal set; }

	public ILocation OriginalMatrixLocation { get; internal set; }

	public override string ToString()
	{
		return Code.ToString();
	}

	internal PutResult(string matrix, ILocation putLocation)
	{
		Code = ErrorCodes.Success;
		IsDuplicate = false;
		OriginalMatrix = matrix;
		PutLocation = putLocation;
		StoredMatrix = OriginalMatrix;
	}
}
