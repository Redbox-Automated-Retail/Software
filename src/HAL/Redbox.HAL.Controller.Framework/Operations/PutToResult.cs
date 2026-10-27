using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework.Operations;

internal sealed class PutToResult : IPutToResult, IPutResult
{
	internal PutResult PutResult;

	public ErrorCodes MoveResult { get; internal set; }

	public bool Success
	{
		get
		{
			Validate();
			return PutResult.Success;
		}
	}

	public bool IsSlotInUse
	{
		get
		{
			Validate();
			return PutResult.IsSlotInUse;
		}
	}

	public bool PickerEmpty
	{
		get
		{
			Validate();
			return PutResult.PickerEmpty;
		}
	}

	public bool PickerObstructed
	{
		get
		{
			Validate();
			return PutResult.PickerObstructed;
		}
	}

	public bool IsDuplicate
	{
		get
		{
			Validate();
			return PutResult.IsDuplicate;
		}
	}

	public ILocation PutLocation
	{
		get
		{
			Validate();
			return PutResult.PutLocation;
		}
	}

	public ErrorCodes Code
	{
		get
		{
			Validate();
			return PutResult.Code;
		}
	}

	public string OriginalMatrix
	{
		get
		{
			Validate();
			return PutResult.OriginalMatrix;
		}
	}

	public string StoredMatrix
	{
		get
		{
			Validate();
			return PutResult.StoredMatrix;
		}
	}

	public ILocation OriginalMatrixLocation
	{
		get
		{
			Validate();
			return PutResult.OriginalMatrixLocation;
		}
	}

	internal PutToResult()
	{
		MoveResult = ErrorCodes.Success;
	}

	private void Validate()
	{
		if (PutResult == null)
		{
			throw new InvalidOperationException("No put result");
		}
	}
}
