using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework.Operations;

internal sealed class GetFromResult : IGetFromResult, IGetResult
{
	internal GetResult GetResult;

	public ErrorCodes MoveResult { get; internal set; }

	public bool Success
	{
		get
		{
			Validate();
			return GetResult.Success;
		}
	}

	public bool IsSlotEmpty
	{
		get
		{
			Validate();
			return GetResult.IsSlotEmpty;
		}
	}

	public bool ItemStuck
	{
		get
		{
			Validate();
			return GetResult.ItemStuck;
		}
	}

	public string Previous
	{
		get
		{
			Validate();
			return GetResult.Previous;
		}
	}

	public ILocation Location
	{
		get
		{
			Validate();
			return GetResult.Location;
		}
	}

	public DateTime? ReturnTime
	{
		get
		{
			Validate();
			return GetResult.ReturnTime;
		}
	}

	public MerchFlags Flags
	{
		get
		{
			Validate();
			return GetResult.Flags;
		}
	}

	public ErrorCodes HardwareError
	{
		get
		{
			Validate();
			return GetResult.HardwareError;
		}
	}

	public void Update(ErrorCodes newError)
	{
		Validate();
		GetResult.Update(newError);
	}

	internal GetFromResult()
	{
		MoveResult = ErrorCodes.Success;
	}

	private void Validate()
	{
		if (GetResult == null)
		{
			throw new InvalidOperationException("No GetResult");
		}
	}
}
