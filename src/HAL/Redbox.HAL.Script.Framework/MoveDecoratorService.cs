using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal struct MoveDecoratorService : IDisposable
{
	private readonly ExecutionContext Context;

	private readonly int DefaultOffset;

	public void Dispose()
	{
	}

	internal ErrorCodes ShowEmptyStuck(ILocation location)
	{
		return ShowEmptyStuck(location, DefaultOffset);
	}

	internal ErrorCodes ShowEmptyStuck(ILocation location, int offset)
	{
		ErrorCodes errorCodes = MoveOffset(location, offset);
		if (errorCodes != ErrorCodes.Success)
		{
			if (!Context.IsImmediate)
			{
				Context.CreateInfoResult("MoveOffsetError", "There was an error moving the offset.");
			}
		}
		else
		{
			IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
			service.SetFinger(GripperFingerState.Rent);
			service.TimedExtend();
			service.SetSensors(on: true);
			if (!Context.IsImmediate)
			{
				Context.CreateInfoResult("MoveOffsetOk", "The damaged disk is located 5 slots left, starting at the disk next to the extended gripper fingers.");
			}
		}
		return errorCodes;
	}

	internal ErrorCodes MoveOffset(ILocation location, int offset)
	{
		IDeck deck = ServiceLocator.Instance.GetService<IDecksService>().GetFrom(location);
		int num = location.Slot - offset;
		if (num <= 0)
		{
			num = deck.NumberOfSlots - Math.Abs(num);
		}
		ErrorCodes result = ServiceLocator.Instance.GetService<IMotionControlService>().MoveTo(location.Deck, num, MoveMode.None, Context.AppLog);
		string msg = $"MOVE-OFFSET Deck = {deck.Number} Slot = {num} returned {result.ToString().ToUpper()}";
		Context.AppLog.Write(msg);
		return result;
	}

	internal MoveDecoratorService(ExecutionContext context)
	{
		this = default(MoveDecoratorService);
		Context = context;
		DefaultOffset = 5;
	}
}
