using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskClientService.Inventory;

namespace Redbox.Rental.Services.KioskClientService.Inventory;

public class KcsUpdateMerchStatusMessageProcessorService : BaseMessageProcessorService
{
	public override List<string> SupportedMessageTypes => new List<string> { "UpdateMerchandizingStatus" };

	public override ProxyDelegate ProxyMethod => KioskClientService.Instance.Inventory.ProcessQueuedMerchandStatusUpdates;

	public override MessageProcessedSuccessfullyDelegate MessageProcessedSuccessfully => HandleMessageResults;

	private void HandleMessageResults(IMessage message, IDictionary<string, object> result, object clientData)
	{
		if ((bool)result["success"])
		{
			List<MerchStatusUpdate> list = result["FailedUpdates"] as List<MerchStatusUpdate>;
			if (list != null && list.Count > 0)
			{
				LogHelper.Instance.Log($"KcsUpdateMerchStatusMessageProcessorService.MessageProcessed: {list.Count} updates failed on server, re-adding to queue.");
				ServiceLocator.Instance.GetService<IQueueService>().Enqueue(105, "UpdateMerchandizingStatus", list);
			}
			else if (list == null)
			{
				LogHelper.Instance.Log("KcsUpdateMerchStatusMessageProcessorService.MessageProcessed: null list of failed updates received.");
			}
			else
			{
				LogHelper.Instance.Log("KcsUpdateMerchStatusMessageProcessorService.MessageProcessed: No failed updates to requeue.");
			}
		}
		else
		{
			LogHelper.Instance.Log("KcsUpdateMerchStatusMessageProcessorService.MessageProcessed: Send updates call failed. Original message will still be in queue.");
		}
	}
}
