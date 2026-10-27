using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.Environment;

public class StoreInfoService : IStoreInfoService, IPollRequestReply
{
	private int _syncID;

	private StoreInfoChangeSetData _storeInfoChangeSet;

	private string _clientStoreInfoHash;

	public static StoreInfoService Instance => Singleton<StoreInfoService>.Instance;

	public ErrorList GetPollRequest(out List<PollRequest> pollRequestList)
	{
		ErrorList errorList = new ErrorList();
		pollRequestList = new List<PollRequest>();
		try
		{
			ErrorList clientStoreInfo = GetClientStoreInfo(out var clientStoreInfo2);
			errorList.AddRange(clientStoreInfo);
			string data = clientStoreInfo2.ToJson();
			string hash = GetHash(clientStoreInfo2);
			if (!string.IsNullOrEmpty(_clientStoreInfoHash) && _clientStoreInfoHash != hash)
			{
				_syncID = 0;
			}
			SetHash(hash);
			if (_syncID == 0)
			{
				pollRequestList.Add(new PollRequest
				{
					PollRequestType = PollRequestType.StoreInfo,
					SyncId = 0,
					Data = data
				});
			}
			else
			{
				pollRequestList.Add(new PollRequest
				{
					PollRequestType = PollRequestType.StoreInfo,
					SyncId = _syncID
				});
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("SIS0091", "Unhandled exception occurred in StoreInfoModel.GetPollRequest.", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public ErrorList ProcessPollReply(List<PollReply> pollReplyList)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			if ((from pr in pollReplyList.ToList()
				where pr.PollReplyType == PollReplyType.UpdateStoreInfo
				select pr).FirstOrDefault() != null)
			{
				_syncID = 0;
				_clientStoreInfoHash = string.Empty;
				LogHelper.Instance.Log("Store Info server is clearing the sync id as requested by the server.");
				return new ErrorList();
			}
			PollReply pollReply = (from pr in pollReplyList.ToList()
				where pr.PollReplyType == PollReplyType.StoreInfoChangeSet
				select pr).FirstOrDefault();
			if (pollReply != null)
			{
				if (string.IsNullOrEmpty(pollReply.Data))
				{
					errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("CFS0092", "Error, A poll reply contained a type of store info changeset without the changeset.", pollReply.ToJson()));
				}
				else
				{
					StoreInfoChangeSetData changeSet = pollReply.Data.ToObject<StoreInfoChangeSetData>();
					ErrorList collection = ProcessChangeSet(changeSet);
					errorList.AddRange(collection);
				}
				_syncID = pollReply.SyncId;
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("CFS0091", "Unhandled exception occurred in StoreInfoService.ProcessPollReply.", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	private ErrorList ProcessChangeSet(StoreInfoChangeSetData changeSet)
	{
		ErrorList errorList = new ErrorList();
		LogHelper.Instance.Log("Processing StoreInfo ChangeSet: " + changeSet.ToJson(), LogEntryType.Debug);
		try
		{
			_storeInfoChangeSet = changeSet;
			if (_storeInfoChangeSet.CorrectTimeZone && !TimeZoneFunctions.SetTimeZone(TimeZoneInfo.FromSerializedString(_storeInfoChangeSet.TimeZoneInfoString)))
			{
				errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("SIS0100", "Setting timezone failed", "Check the log for further details."));
			}
			if (_storeInfoChangeSet.CorrectDateTime && !TimeZoneFunctions.SetTime(_storeInfoChangeSet.CurrentUTCDateTime))
			{
				errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("SIS0101", "Setting datetime failed", "Check the log for further details."));
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("SIS0089", $"Unhandled exception occurred in StoreInfoService.ProcessChangeSet for store info", e));
		}
		return errorList;
	}

	private void SetHash(string hash)
	{
		_clientStoreInfoHash = hash;
	}

	private ErrorList GetClientStoreInfo(out ClientStoreInfo clientStoreInfo)
	{
		ErrorList errorList = new ErrorList();
		clientStoreInfo = new ClientStoreInfo
		{
			Info = new Dictionary<string, string>()
		};
		try
		{
			TimeZoneInfo.ClearCachedData();
			clientStoreInfo.TimeZoneInfoString = TimeZoneInfo.Local.ToSerializedString();
			clientStoreInfo.UTCDateTime = DateTime.UtcNow;
			clientStoreInfo.Version = Assembly.GetEntryAssembly().GetName().Version.ToString();
			clientStoreInfo.Info.Add("OS Version", System.Environment.OSVersion.Version.ToString());
			clientStoreInfo.Info.Add("OS VersionString", System.Environment.OSVersion.VersionString);
			clientStoreInfo.Info.Add("OS Platform", System.Environment.OSVersion.Platform.ToString());
			clientStoreInfo.Info.Add("OS ServicePack", System.Environment.OSVersion.ServicePack);
			string value = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault((IPAddress a) => a.AddressFamily == AddressFamily.InterNetwork).ToString();
			clientStoreInfo.Info.Add("IP Address", value);
			NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			if (allNetworkInterfaces.Any())
			{
				string dnsSuffix = allNetworkInterfaces[0].GetIPProperties().DnsSuffix;
				clientStoreInfo.Info.Add("DNS Suffix", dnsSuffix);
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("SIS0092", "Unhandled exception occurred in StoreInfoServieGetClientStoreInfo.", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	private string GetHash(ClientStoreInfo clientStoreInfo)
	{
		var obj = new
		{
			TimeZoneInfoString = clientStoreInfo.TimeZoneInfoString,
			Version = clientStoreInfo.Version,
			Info = new Dictionary<string, string>()
		};
		clientStoreInfo.Info.ForEach(delegate(KeyValuePair<string, string> info)
		{
			obj.Info.Add(info.Key, info.Value);
		});
		string s = obj.ToJson();
		return Encoding.ASCII.GetBytes(s).ToASCIISHA1Hash();
	}

	private StoreInfoService()
	{
		if (ServiceLocator.Instance.GetService<IStoreInfoService>() == null)
		{
			ServiceLocator.Instance.AddService(typeof(IStoreInfoService), this);
		}
	}
}
