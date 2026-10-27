using System;
using System.Collections.Generic;

namespace Redbox.UpdateManager.ComponentModel;

public interface IUpdateService
{
	ErrorList UploadFile(string path);

	ErrorList StartDownloads();

	ErrorList FinishDownloads();

	ErrorList FinishDownload(Guid id);

	ErrorList ServerPoll();

	ErrorList Poll();

	ErrorList ClearWorkQueue();

	ErrorList DeleteFromWorkQueue(string name);

	ErrorList AddStoreToRepository(string number, string name);

	ErrorList RemoveStoreFromRepository(string number, string name);

	ErrorList AddGroupToRepository(string group, string name);

	ErrorList RemoveGroupFromRepository(string group, string name);

	ErrorList ForcePublish(string name);

	ErrorList GetSubscriptionState(string name, out DateTime lastRun, out SubscriptionState state);

	ErrorList StartInstaller(string repositoryHash, string frontEndVersion, out Dictionary<string, string> response);

	ErrorList FinishInstaller(string guid, Dictionary<string, string> data);

	ErrorList FinishInstaller(string guid, string name, string value);

	ErrorList ExecuteScriptFile(string path, out string result);

	ErrorList ExecuteScript(string script, out string result);

	ErrorList ExecuteScriptFile(string path, out string result, bool reset);

	ErrorList ExecuteScript(string script, out string result, bool reset);

	ErrorList DoInCompleteWork(IEnumerable<Guid> ids);

	string StoreNumber();
}
