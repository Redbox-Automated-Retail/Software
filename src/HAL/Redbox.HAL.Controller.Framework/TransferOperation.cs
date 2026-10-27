using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class TransferOperation : IDisposable, IPutObserver
{
	private readonly IMotionControlService MotionService;

	private readonly IInventoryService InventoryService;

	private readonly IControlSystem ControlSystem;

	private readonly ILocation Source;

	private readonly IControllerService ControllerService;

	private bool m_disposed;

	private IGetResult GetResult;

	internal bool PreserveFlagsOnPut { get; set; }

	public void Dispose()
	{
		DisposeInner(fromDispose: true);
	}

	public void OnSuccessfulPut(IPutResult result, IFormattedLog log)
	{
		OnRestoreState(result.PutLocation, PreserveFlagsOnPut);
	}

	public void OnFailedPut(IPutResult result, IFormattedLog log)
	{
	}

	internal ITransferResult Transfer(IList<ILocation> destinations)
	{
		TransferResult transferResult = new TransferResult();
		FetchSource(transferResult);
		if (transferResult.TransferError != ErrorCodes.Success)
		{
			return transferResult;
		}
		foreach (ILocation destination in destinations)
		{
			MoveAndPut(transferResult, destination);
			if (transferResult.TransferError == ErrorCodes.Success)
			{
				return transferResult;
			}
		}
		ReturnToSource(transferResult);
		return transferResult;
	}

	internal ITransferResult Transfer(IList<ILocation> destinations, IGetObserver observer)
	{
		TransferResult transferResult = new TransferResult();
		FetchSource(transferResult, observer);
		if (transferResult.TransferError != ErrorCodes.Success)
		{
			return transferResult;
		}
		foreach (ILocation destination in destinations)
		{
			MoveAndPut(transferResult, destination);
			if (transferResult.TransferError == ErrorCodes.Success)
			{
				return transferResult;
			}
		}
		ReturnToSource(transferResult);
		return transferResult;
	}

	internal ITransferResult Transfer(ILocation destination)
	{
		TransferResult transferResult = new TransferResult();
		FetchSource(transferResult);
		if (transferResult.TransferError != ErrorCodes.Success)
		{
			return transferResult;
		}
		MoveAndPut(transferResult, destination);
		if (!transferResult.Transferred)
		{
			ReturnToSource(transferResult);
		}
		return transferResult;
	}

	internal TransferOperation(IControllerService service, ILocation source)
	{
		InventoryService = ServiceLocator.Instance.GetService<IInventoryService>();
		MotionService = ServiceLocator.Instance.GetService<IMotionControlService>();
		ControlSystem = ServiceLocator.Instance.GetService<IControlSystem>();
		ControllerService = service;
		Source = source;
	}

	private void MoveAndPut(TransferResult result, ILocation destination)
	{
		ErrorCodes errorCodes = MotionService.MoveTo(destination, MoveMode.Put);
		if (errorCodes != ErrorCodes.Success)
		{
			result.TransferError = errorCodes;
			return;
		}
		ControlSystem.TrackCycle();
		IPutResult putResult = ControllerService.Put(this, GetResult.Previous);
		result.TransferError = putResult.Code;
		if (putResult.Success)
		{
			result.Destination = destination;
		}
	}

	private void ReturnToSource(TransferResult result)
	{
		ErrorCodes errorCodes = MotionService.MoveTo(Source, MoveMode.Put);
		if (errorCodes != ErrorCodes.Success)
		{
			result.TransferError = errorCodes;
			return;
		}
		ControlSystem.TrackCycle();
		if (ControllerService.Put(GetResult.Previous).Success)
		{
			result.ReturnedToSource = true;
			OnRestoreState(Source, preserve: true);
		}
	}

	private void OnRestoreState(ILocation location, bool preserve)
	{
		if (preserve)
		{
			location.Flags = GetResult.Flags;
		}
		location.ReturnDate = GetResult.ReturnTime;
		InventoryService.Save(location);
	}

	private void FetchSource(TransferResult result, IGetObserver observer)
	{
		ErrorCodes errorCodes = MotionService.MoveTo(Source, MoveMode.Get);
		if (errorCodes != ErrorCodes.Success)
		{
			result.TransferError = errorCodes;
			return;
		}
		GetResult = ControllerService.Get(observer);
		result.TransferError = ((!GetResult.Success) ? GetResult.HardwareError : ErrorCodes.Success);
		if (GetResult.Success)
		{
			result.Source = Source;
		}
	}

	private void FetchSource(TransferResult result)
	{
		ErrorCodes errorCodes = MotionService.MoveTo(Source, MoveMode.Get);
		if (errorCodes != ErrorCodes.Success)
		{
			result.TransferError = errorCodes;
			return;
		}
		GetResult = ControllerService.Get();
		result.TransferError = ((!GetResult.Success) ? GetResult.HardwareError : ErrorCodes.Success);
		if (GetResult.Success)
		{
			result.Source = Source;
		}
	}

	private void DisposeInner(bool fromDispose)
	{
		if (!m_disposed)
		{
			m_disposed = true;
		}
	}
}
