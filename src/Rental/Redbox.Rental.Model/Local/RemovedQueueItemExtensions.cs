using System.Collections.Generic;
using Redbox.Core;

namespace Redbox.Rental.Model.Local;

public static class RemovedQueueItemExtensions
{
	public static IEnumerable<IInventoryItem> ToInventoryItems(this IEnumerable<RemovedQueueItem> source)
	{
		List<IInventoryItem> list;
		if (source == null)
		{
			list = null;
		}
		else
		{
			list = new List<IInventoryItem>();
			IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
			foreach (RemovedQueueItem item in source)
			{
				list.Add(service.CreateInventoryItem(item.Barcode, null, null, InventoryItemStatus.Removed));
			}
		}
		return list;
	}
}
