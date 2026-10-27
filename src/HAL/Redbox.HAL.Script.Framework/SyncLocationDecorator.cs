using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal sealed class SyncLocationDecorator : SyncDecorator, IGetObserver
{
	public void OnStuck(IGetResult result)
	{
		HandleStuck(result);
	}

	public bool OnEmpty(IGetResult result)
	{
		using PeekOperation peekOperation = new PeekOperation();
		IPeekResult peekResult = peekOperation.Execute();
		if (!peekResult.TestOk)
		{
			string value = $"Peek Deck={SyncLocation.Deck} Slot={SyncLocation.Slot} returned status {peekResult.Error.ToString().ToUpper()} after GET returned SLOTEMPTY";
			UpdateSymbol("MSTESTER-SYMBOL-GET-ERR-MSG", value);
			Context.CreateResult("MachineError", "An error occurred when attempting to peek the slot.", SyncLocation.Deck, SyncLocation.Slot, null, null, null);
			AddError("There was an error performing a PEEK.");
			HandleStuck(result);
			return false;
		}
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		if (!peekResult.IsFull)
		{
			UpdateSymbol("MSTESTER-SYMBOL-EMPTY-SLOT-MSG", $"The location Deck={SyncLocation.Deck} Slot={SyncLocation.Slot} is empty.");
			SyncLocation.ReturnDate = null;
			service.Save(SyncLocation);
			return true;
		}
		HandleStuck(result, service);
		return false;
	}

	protected override void OnReadDisk(ScanResult scanResult)
	{
		string scannedMatrix = scanResult.ScannedMatrix;
		int num = scanResult.ReadCount;
		TimeSpan timeSpan = scanResult.ExecutionTime;
		if (!scanResult.ReadCode)
		{
			num = 0;
			timeSpan = default(TimeSpan);
		}
		string value = (ServiceLocator.Instance.GetService<IInventoryService>().IsBarcodeDuplicate(scannedMatrix, out var _) ? $"SUCCESS: {scannedMatrix} codes ({num}), time = {timeSpan.ToString()} DUPLICATE" : $"SUCCESS: {scannedMatrix} codes ({num}), time = {timeSpan.ToString()}");
		UpdateSymbol("MSTESTER-SYMBOL-FORMATTED-DETAIL-MSG", value);
	}

	protected override void OnMoveErrorInner(ErrorCodes moveRes)
	{
		UpdateSymbol("MSTESTER-SYMBOL-MOVE-FALURE", $"MOVE Deck={SyncLocation.Deck} Slot={SyncLocation.Slot} returned an error status {moveRes.ToString().ToUpper()}");
	}

	internal override IGetResult GetDisk(IFormattedLog applog)
	{
		return ServiceLocator.Instance.GetService<IControllerService>().Get(this);
	}

	internal override void OnGetFailure(IGetResult gr)
	{
		UpdateSymbol("MSTESTER-SYMBOL-GET-ERR-MSG", $"GET Deck={SyncLocation.Deck} Slot={SyncLocation.Slot} returned error status {gr.ToString()}");
		AddError("There was an error performing a GET.");
	}

	internal override void OnPutError(string id, IPutResult putResult)
	{
		string value = $"PUT Deck={SyncLocation.Deck} Slot={SyncLocation.Slot} ID={id} returned error status {putResult.ToString()}";
		UpdateSymbol("MSTESTER-SYMBOL-PUT-FAILURE", value);
		Context.CreateResult("PutFailure", "The PUT instruction failed.", SyncLocation.Deck, SyncLocation.Slot, id, null, null);
	}

	internal override void OnExcludedLocation()
	{
		string value = (SyncLocation.IsWide ? $"LOCATION DECK={SyncLocation.Deck} SLOT={SyncLocation.Slot} is a wide slot on deck 7, and will not be synced." : $"LOCATION DECK={SyncLocation.Deck} SLOT={SyncLocation.Slot} is excluded");
		UpdateSymbol("MSTESTER-SYMBOL-EXCLUDED-SLOT-MSG", value);
	}

	internal SyncLocationDecorator(ExecutionContext ctx, ExecutionResult r, ILocation loc)
		: base(ctx, r, loc)
	{
	}

	private void HandleStuck(IGetResult result)
	{
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		HandleStuck(result, service);
	}

	private void HandleStuck(IGetResult result, IInventoryService service)
	{
		UpdateSymbol("MSTESTER-SYMBOL-STUCK-DISC-MSG", $"The location Deck={SyncLocation.Deck} Slot={SyncLocation.Slot} has a disc stuck in slot.");
		result.Update(ErrorCodes.ItemStuck);
		service.UpdateEmptyStuck(result.Location);
	}
}
