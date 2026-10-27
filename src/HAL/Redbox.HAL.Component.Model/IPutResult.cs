namespace Redbox.HAL.Component.Model;

public interface IPutResult
{
	bool Success { get; }

	bool IsSlotInUse { get; }

	bool PickerEmpty { get; }

	bool PickerObstructed { get; }

	bool IsDuplicate { get; }

	ILocation PutLocation { get; }

	ErrorCodes Code { get; }

	string OriginalMatrix { get; }

	string StoredMatrix { get; }

	ILocation OriginalMatrixLocation { get; }
}
