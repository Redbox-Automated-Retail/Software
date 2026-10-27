using System.Collections.Generic;

namespace Redbox.Rental.Model.Local;

public interface ILocalData
{
	ILocalDataInstance LoadData();

	void UpdateBadCard(IBadCard badCard);

	void RemoveBadCard(IBadCard badCard);

	void AddRemovedQueueItem(RemovedQueueItem item);

	List<RemovedQueueItem> GetRemovedQueueItems();

	void DeleteRemovedQueueItem(RemovedQueueItem item);
}
