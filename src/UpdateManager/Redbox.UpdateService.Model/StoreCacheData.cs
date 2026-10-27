using System;
using System.Collections.Generic;
using System.Linq;

namespace Redbox.UpdateService.Model;

public class StoreCacheData
{
	private readonly List<long> _downloadFileList = new List<long>();

	private readonly object _lock = new object();

	public StoreData Store { get; set; }

	public long KioskClient_ID { get; set; }

	public string City { get; set; }

	public string County { get; set; }

	public string State { get; set; }

	public string Zip { get; set; }

	public Identifier KioskStatus { get; set; }

	public Identifier Vendor { get; set; }

	public Identifier Market { get; set; }

	public Identifier Banner { get; set; }

	public string WindowsTimeZoneID { get; set; }

	public List<ClientRepositoryData> ClientRepositoryData { get; set; }

	public int ClientRepositoryDataSyncID { get; set; }

	public List<ClientConfigFile> ClientConfigFiles { get; set; }

	public int ClientConfigFileSyncID { get; set; }

	public ClientStoreInfo ClientStoreInfo { get; set; }

	public int ClientStoreInfoSyncID { get; set; }

	public List<long> DownloadFileList
	{
		get
		{
			lock (_lock)
			{
				List<long> list = new List<long>();
				_downloadFileList.ForEach(list.Add);
				return list;
			}
		}
	}

	public StoreCacheData()
	{
		ClientConfigFiles = new List<ClientConfigFile>();
	}

	public void UpdateClientConfigFile(List<ClientConfigFile> clientConfigFile)
	{
		lock (_lock)
		{
			ClientConfigFiles.Clear();
			clientConfigFile.ForEach(delegate(ClientConfigFile data)
			{
				ClientConfigFiles.Add(data);
			});
			ClientConfigFileSyncID++;
		}
	}

	public void UpdateClientConfigFile(ConfigFileArgs args)
	{
		lock (_lock)
		{
			ClientConfigFiles.Clear();
			args.ClientConfigFilesData.ForEach(delegate(ClientConfigFile data)
			{
				ClientConfigFiles.Add(data);
			});
			ClientConfigFileSyncID = args.ClientConfigFileSyncID;
		}
	}

	public void UpdateClientStoreInfo(ClientStoreInfo clientStoreInfo)
	{
		lock (_lock)
		{
			ClientStoreInfo = clientStoreInfo;
			ClientStoreInfoSyncID++;
		}
	}

	public void UpdateClientStoreInfo(StoreInfoArgs args)
	{
		lock (_lock)
		{
			ClientStoreInfo = args.ClientStoreInfo;
			ClientStoreInfoSyncID = args.ClientStoreInfoSyncID;
		}
	}

	public void UpdateDownloadFileList(List<long> downloadFileList)
	{
		lock (_lock)
		{
			_downloadFileList.Clear();
			downloadFileList.ForEach(delegate(long data)
			{
				_downloadFileList.Add(data);
			});
		}
	}

	public void UpdateDownloadFileList(DownloadFileArgs args)
	{
		lock (_lock)
		{
			_downloadFileList.Clear();
			args.DownloadFileList.ForEach(delegate(long data)
			{
				_downloadFileList.Add(data);
			});
		}
	}

	public void UpdateStoreData(StoreData sd)
	{
		lock (_lock)
		{
			if (Store == null)
			{
				Store = new StoreData
				{
					Number = sd.Number,
					ID = sd.ID,
					LastCheckIn = sd.LastCheckIn
				};
			}
			else
			{
				Store.Number = sd.Number;
				Store.ID = sd.ID;
				Store.LastCheckIn = sd.LastCheckIn;
			}
		}
	}

	public void UpdateKioskClientData(KioskClientData kc)
	{
		lock (_lock)
		{
			KioskClient_ID = kc.KioskClientID;
			City = kc.City;
			County = kc.County;
			State = kc.State;
			Zip = kc.Zip;
			KioskStatus = (kc.KioskStatusId.HasValue ? new Identifier
			{
				ID = kc.KioskStatusId.Value,
				Reference = kc.KioskStatusName
			} : null);
			Banner = (kc.BannerId.HasValue ? new Identifier
			{
				ID = kc.BannerId.Value,
				Reference = kc.BannerName
			} : null);
			Market = (kc.MarketId.HasValue ? new Identifier
			{
				ID = kc.MarketId.Value,
				Reference = kc.MarketName
			} : null);
			Vendor = (kc.VendorId.HasValue ? new Identifier
			{
				ID = kc.VendorId.Value,
				Reference = kc.VendorName
			} : null);
			if (!string.Equals(WindowsTimeZoneID, kc.WindowsTimeZoneID, StringComparison.CurrentCultureIgnoreCase))
			{
				ClientStoreInfoSyncID = 0;
			}
			WindowsTimeZoneID = kc.WindowsTimeZoneID;
		}
	}

	public void UpdateClientRepositoryData(List<ClientRepositoryData> clientRepositoryData, int syncId)
	{
		lock (_lock)
		{
			if (ClientRepositoryData == null)
			{
				ClientRepositoryData = new List<ClientRepositoryData>();
			}
			foreach (ClientRepositoryData clientRepositoryDatum in clientRepositoryData)
			{
				ClientRepositoryData data = clientRepositoryDatum;
				ClientRepositoryData clientRepositoryData2 = ClientRepositoryData.FirstOrDefault((ClientRepositoryData c) => string.Equals(data.RepositoryName, c.RepositoryName, StringComparison.CurrentCultureIgnoreCase));
				if (clientRepositoryData2 == null)
				{
					ClientRepositoryData.Add(clientRepositoryDatum);
					continue;
				}
				clientRepositoryData2.InTransit = clientRepositoryDatum.InTransit;
				clientRepositoryData2.Staged = clientRepositoryDatum.Staged;
				clientRepositoryData2.Active = clientRepositoryDatum.Active;
				clientRepositoryData2.Head = clientRepositoryDatum.Head;
			}
			foreach (ClientRepositoryData item in ClientRepositoryData.Where((ClientRepositoryData crd1) => !clientRepositoryData.Any((ClientRepositoryData clientRepositoryData3) => string.Equals(crd1.RepositoryName, clientRepositoryData3.RepositoryName, StringComparison.CurrentCultureIgnoreCase))))
			{
				item.InTransit = new ClientRepositoryData.Revlog();
				item.Staged = new ClientRepositoryData.Revlog();
				item.Active = new ClientRepositoryData.Revlog();
				item.Head = new ClientRepositoryData.Revlog();
			}
			ClientRepositoryDataSyncID = syncId;
		}
	}

	public void UpdateClientRepositoryData(ClientRepositoryArgs args)
	{
		UpdateClientRepositoryData(args.ClientRepositoryData, args.ClientRepositoryDataSyncID);
	}

	public void UpdateClientRepositoryData(Dictionary<ClientRepositoryData.Repository, ClientRepositoryData.Revlog> assigned)
	{
		lock (_lock)
		{
			if (ClientRepositoryData == null)
			{
				ClientRepositoryData = new List<ClientRepositoryData>();
			}
			foreach (KeyValuePair<ClientRepositoryData.Repository, ClientRepositoryData.Revlog> item2 in assigned)
			{
				KeyValuePair<ClientRepositoryData.Repository, ClientRepositoryData.Revlog> repository1 = item2;
				ClientRepositoryData clientRepositoryData = ClientRepositoryData.FirstOrDefault((ClientRepositoryData c) => string.Equals(repository1.Key.Name, c.RepositoryName, StringComparison.CurrentCultureIgnoreCase));
				if (clientRepositoryData != null)
				{
					clientRepositoryData.Assigned = item2.Value;
					continue;
				}
				ClientRepositoryData item = new ClientRepositoryData
				{
					RepositoryName = item2.Key.Name,
					RepositoryOID = item2.Key.OID,
					Assigned = item2.Value,
					InTransit = new ClientRepositoryData.Revlog(),
					Staged = new ClientRepositoryData.Revlog(),
					Active = new ClientRepositoryData.Revlog(),
					Head = new ClientRepositoryData.Revlog()
				};
				ClientRepositoryData.Add(item);
			}
			ClientRepositoryData.RemoveAll((ClientRepositoryData crd) => !assigned.Any((KeyValuePair<ClientRepositoryData.Repository, ClientRepositoryData.Revlog> a) => string.Equals(crd.RepositoryName, a.Key.Name, StringComparison.CurrentCultureIgnoreCase)));
		}
	}

	public void ClearClientConfigFileSyncId()
	{
		lock (_lock)
		{
			ClientConfigFileSyncID = 0;
		}
	}

	public void ClearClientStoreInfoSyncId()
	{
		lock (_lock)
		{
			ClientStoreInfoSyncID = 0;
		}
	}

	public void ClearClearClientRepositoryDataSyncId()
	{
		lock (_lock)
		{
			ClientRepositoryDataSyncID = 0;
		}
	}
}
