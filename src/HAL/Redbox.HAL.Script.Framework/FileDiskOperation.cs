using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal struct FileDiskOperation : IDisposable
{
	private readonly ExecutionContext Context;

	private readonly IFileDiskHandler Handler;

	public void Dispose()
	{
	}

	internal ErrorCodes PutDiscAway(ExecutionResult result, string idInPicker)
	{
		LogHelper.Instance.WithContext("Find an empty slot for item {0}.", idInPicker);
		ErrorCodes errorCodes = OnFileDisk(result, idInPicker);
		if (errorCodes != ErrorCodes.Success)
		{
			LogHelper.Instance.WithContext(LogEntryType.Error, "PutDiskAway ( barcode = {0} ) returned error status {1}.", idInPicker, errorCodes.ToString().ToUpper());
		}
		return errorCodes;
	}

	internal FileDiskOperation(ExecutionContext context)
		: this(new DefaultHandler(), context)
	{
	}

	internal FileDiskOperation(IFileDiskHandler handler, ExecutionContext context)
	{
		Handler = handler;
		Context = context;
	}

	private ErrorCodes OnFileDisk(ExecutionResult result, string idInPicker)
	{
		using IEmptySearchResult emptySearchResult = ServiceLocator.Instance.GetService<IEmptySearchPatternService>().FindEmptyLocations();
		if (emptySearchResult.FoundEmpty == 0)
		{
			Handler.MachineFull(result, idInPicker);
			return ErrorCodes.MachineFull;
		}
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		ServiceLocator.Instance.GetService<IInventoryService>().IsBarcodeDuplicate(idInPicker, out var _);
		int num = ((emptySearchResult.FoundEmpty > 1) ? (emptySearchResult.FoundEmpty - 1) : 0);
		IMotionControlService service2 = ServiceLocator.Instance.GetService<IMotionControlService>();
		int num2 = 0;
		foreach (ILocation emptyLocation in emptySearchResult.EmptyLocations)
		{
			LogHelper.Instance.WithContext("Moving to {0} to put item in picker away.", emptyLocation.ToString());
			ErrorCodes errorCodes = service2.MoveTo(emptyLocation, MoveMode.Put, Context.AppLog);
			if (errorCodes != ErrorCodes.Success)
			{
				FileDiscIterationResult fileDiscIterationResult = Handler.MoveError(result, idInPicker, errorCodes, emptyLocation.Deck, emptyLocation.Slot);
				if (FileDiscIterationResult.Halt == fileDiscIterationResult)
				{
					return ErrorCodes.DiskNotFiled;
				}
				if (FileDiscIterationResult.NextLocation == fileDiscIterationResult)
				{
					continue;
				}
			}
			LogHelper.Instance.WithContext("Putting item in picker into slot.");
			IPutResult putResult = ServiceLocator.Instance.GetService<IControllerService>().Put(idInPicker);
			if (putResult.Success)
			{
				Handler.DiskFiled(result, putResult);
				return ErrorCodes.Success;
			}
			if (putResult.IsSlotInUse && ++num2 == num)
			{
				service2.InitAxes();
			}
			if (FileDiscIterationResult.Halt == Handler.OnFailedPut(result, putResult))
			{
				return ErrorCodes.DiskNotFiled;
			}
			service.TrackCycle();
		}
		Handler.FailedToFileDisk(result, idInPicker);
		return ErrorCodes.DiskNotFiled;
	}
}
