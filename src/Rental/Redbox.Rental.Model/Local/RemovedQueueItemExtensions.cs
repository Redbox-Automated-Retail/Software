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
			IInventoryService inventoryService = ServiceLocator.Instance.GetService<IInventoryService>();
			foreach (RemovedQueueItem item in source)
			{
				list.Add(inventoryService.CreateInventoryItem(item.Barcode, null, null, InventoryItemStatus.Removed));
			}
		}
		return list;
	}
}
