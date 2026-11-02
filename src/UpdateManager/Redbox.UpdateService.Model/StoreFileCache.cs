using System.Collections.Generic;

namespace Redbox.UpdateService.Model;

public class StoreFileCache
{
	public long Id { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string Name { get; set; } = null!;

	public string Path { get; set; } = null!;

	public List<StoreFileDataCache> StoreFileDataCacheList { get; set; } = null!;

	public List<StoreFileStateCache> StoreFileStateCacheList { get; set; } = null!;

	public Dictionary<string, StoreFileDataStateCache> DataStateCache { get; set; } = null!;

	public void UpdateDataStateCache()
	{
		DataStateCache = new Dictionary<string, StoreFileDataStateCache>();
		StoreFileDataCacheList.ForEach(delegate(StoreFileDataCache sfd)
		{
			DataStateCache[sfd.StoreNumber] = new StoreFileDataStateCache
			{
				DataCache = sfd
			};
		});
		StoreFileStateCacheList.ForEach(delegate(StoreFileStateCache sfs)
		{
			if (DataStateCache.TryGetValue(sfs.StoreNumber, out var value))
			{
				value.StateCache = sfs;
			}
			else
			{
				DataStateCache[sfs.StoreNumber] = new StoreFileDataStateCache
				{
					StateCache = sfs
				};
			}
		});
	}
}
