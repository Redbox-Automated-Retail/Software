using System;
using System.Collections.ObjectModel;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.UpdateServices.Proxy.ComponentModel;

public interface IUpdateManagerService
{
	ErrorList Initialize();

	ErrorList ForceScheduleEvent(string eventName, out bool isRunning);

	ErrorList GetDownloadState(out ReadOnlyCollection<IDownloadData> downloads);

	ErrorList GetSubscriptionState(out DateTime lastRun, out string subscriptionState);

	ErrorList ForceSubscriptionPublish();

	ErrorList ExecuteCommand(string command, out ReadOnlyCollection<string> messages);
}
