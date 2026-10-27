using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal class QlmThinHelper : ThinHelper
{
	protected override MerchandizeResult ThinDiskInner(string id, ILocation source, ILocation notUsed, MerchFlags mf)
	{
		int thinSlot = GetThinSlot();
		IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
		IInventoryService service2 = ServiceLocator.Instance.GetService<IInventoryService>();
		try
		{
			MerchandizeResult merchandizeResult;
			do
			{
				if (thinSlot > service.QlmDeck.NumberOfSlots)
				{
					return MerchandizeResult.QLMFull;
				}
				ILocation target = service2.Get(service.QlmDeck.Number, thinSlot++);
				merchandizeResult = ThinDiskToLocation(source, target, id, mf);
			}
			while (MerchandizeResult.SlotInUse == merchandizeResult);
			return merchandizeResult;
		}
		finally
		{
			base.Context.SetSymbolValue("THIN-SLOT", thinSlot);
		}
	}

	protected override ILocation FindEmptyTarget(MerchFlags notUsed, out MerchandizeResult result)
	{
		int thinSlot = GetThinSlot();
		result = MerchandizeResult.Success;
		IDeck qlmDeck = ServiceLocator.Instance.GetService<IDecksService>().QlmDeck;
		if (thinSlot > qlmDeck.NumberOfSlots)
		{
			result = MerchandizeResult.QLMFull;
			return null;
		}
		return ServiceLocator.Instance.GetService<IInventoryService>().Get(qlmDeck.Number, thinSlot);
	}

	internal QlmThinHelper(ExecutionResult result, ExecutionContext context)
		: base(result, context)
	{
	}

	private int GetThinSlot()
	{
		int result = 1;
		ErrorList errorList = new ErrorList();
		object symbolValue = base.Context.GetSymbolValue("THIN-SLOT", errorList);
		if (errorList.Count == 0)
		{
			result = (int)symbolValue;
		}
		return result;
	}
}
