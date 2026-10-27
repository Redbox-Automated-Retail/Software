using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class GetResult : IGetResult
{
	public ErrorCodes HardwareError { get; private set; }

	public bool Success => HardwareError == ErrorCodes.Success;

	public bool EmptyOrStuck
	{
		get
		{
			if (!IsSlotEmpty)
			{
				return ItemStuck;
			}
			return true;
		}
	}

	public bool IsSlotEmpty => ErrorCodes.SlotEmpty == HardwareError;

	public bool ItemStuck => ErrorCodes.ItemStuck == HardwareError;

	public ILocation Location { get; private set; }

	public string Previous { get; private set; }

	public DateTime? ReturnTime { get; private set; }

	public MerchFlags Flags { get; private set; }

	public override string ToString()
	{
		return HardwareError.ToString();
	}

	public void Update(ErrorCodes newError)
	{
		HardwareError = newError;
	}

	internal GetResult(ILocation location)
	{
		Location = location;
		Previous = Location.ID;
		Flags = Location.Flags;
		ReturnTime = location.ReturnDate;
		HardwareError = ErrorCodes.Success;
		LogHelper.Instance.Log(LogEntryType.Debug, "[GET Result] Loc = {0} Prev = {1} Flags = {2} r/t = {3}", location.ToString(), Previous, Flags.ToString(), ReturnTime.ToString());
	}
}
