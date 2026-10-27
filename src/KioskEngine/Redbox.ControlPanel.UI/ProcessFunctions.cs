using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Xml;
using Microsoft.Win32;
using Redbox.Core;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.IDE;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Health;

namespace Redbox.ControlPanel.UI;

internal static class ProcessFunctions
{
	public delegate void TimerFiredHandler();

	internal struct Item(string p1, int p2, int p3, string p4)
	{
		public string barcode = p1;

		public int deck = p2;

		public int slot = p3;

		public string status = p4;
	}

	private delegate bool EnumWindowsDelegate(IntPtr hwnd, IntPtr lParam);

	private static char[] AdminPassword = new char[10] { 'W', 'i', 'A', 'M', 's', 'o', 'c', 'a', 'P', '?' };

	internal static long? kioskId = ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId;

	internal static WindowsIdentity currentUser = WindowsIdentity.GetCurrent();

	internal static DateTime timeOfCPStart;

	internal static int kioskItemsCount = 0;

	internal static List<IState> tableStates = new List<IState>();

	internal static List<IBanner> tableBanners = new List<IBanner>();

	private const string User32Dll = "user32.dll";

	private const uint WM_CLOSE = 16u;

	internal static int LaunchProcess(string processName, string args, bool hiddenProcess)
	{
		string defaultAdmin = "rbadmin";
		try
		{
			if (new WindowsPrincipal(currentUser).IsInRole(WindowsBuiltInRole.Administrator))
			{
				LogHelper.Instance.Log($"Launch {processName} {args} as user");
				return LaunchProcessInner(processName, args, null, useAdminPassword: false, hiddenProcess);
			}
			List<string> allAdministrators = GetAllAdministrators();
			if (allAdministrators.Count() < 1)
			{
				LogHelper.Instance.Log("No Admin Accounts Dectected - Launching with reduced privileges.");
				return LaunchProcessInner(processName, args, null, useAdminPassword: false, hiddenProcess);
			}
			if (allAdministrators.Find((string x) => x.ToLower() == defaultAdmin) != null)
			{
				LogHelper.Instance.Log($"Launch {processName} {args} as {defaultAdmin}");
				return LaunchProcessInner(processName, args, defaultAdmin, useAdminPassword: true, hiddenProcess);
			}
			LogHelper.Instance.Log($"Launch {processName} {args} as {allAdministrators[0]}");
			return LaunchProcessInner(processName, args, allAdministrators[0], useAdminPassword: false, hiddenProcess);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error in Kernel Function LaunchProcess.", e);
		}
		return -1;
	}

	internal static int StartProcessWaitForExit(string process, string arguments, int timeout)
	{
		try
		{
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = process,
				Arguments = arguments,
				WindowStyle = ProcessWindowStyle.Hidden,
				CreateNoWindow = true
			};
			Process process2 = new Process
			{
				StartInfo = startInfo
			};
			LogHelper.Instance.Log("Launching {0}, args={1}, timeout={2}.", process, arguments, timeout);
			process2.Start();
			process2.WaitForExit(timeout);
			if (process2.HasExited)
			{
				LogHelper.Instance.Log("Launched successfully, ExitCode={0}.", process2.ExitCode);
				return process2.ExitCode;
			}
			LogHelper.Instance.Log("HasExited=false, possible timeout.");
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in ProcessFunctions.StartProcessWaitForExit.", ex.Message);
		}
		return -1;
	}

	internal static bool KillProcesses(List<int> processIds)
	{
		LogHelper.Instance.Log($"Killing {processIds?.Count() ?? 0} processes.");
		foreach (int processId in processIds)
		{
			try
			{
				KillProcessById(processId);
			}
			catch (Exception ex)
			{
				if (!ex.Message.Contains("is not running"))
				{
					LogHelper.Instance.Log("Error in KillProcesses Function KillProcessId.", ex);
					return false;
				}
			}
		}
		return true;
	}

	internal static Dictionary<int, string> GetRunningProcessIds()
	{
		Dictionary<int, string> dictionary = new Dictionary<int, string>();
		Process[] processes = Process.GetProcesses();
		foreach (Process process in processes)
		{
			dictionary.Add(process.Id, process.ProcessName);
		}
		return dictionary;
	}

	internal static Dictionary<int, ProcessDetails> GetRunningProcesses()
	{
		Dictionary<int, ProcessDetails> dictionary = new Dictionary<int, ProcessDetails>();
		ProcessDetails value = default(ProcessDetails);
		Process[] processes = Process.GetProcesses();
		foreach (Process process in processes)
		{
			value.Name = process.ProcessName;
			value.Memory = $"{process.WorkingSet64 / 1000:#,###}K";
			value.WindowTitle = "";
			value.StartTime = "";
			value.Pid = process.Id;
			dictionary.Add(process.Id, value);
		}
		return dictionary;
	}

	internal static void ShowTaskManager(List<string> processNamesToIgnore, KillHandler killHandler)
	{
		string KilledProcesses = "The following processes were killed: ";
		TaskManagerForm form = new TaskManagerForm();
		if (processNamesToIgnore != null)
		{
			form.ProcessNamesToIgnore = processNamesToIgnore;
		}
		form.Kill += delegate
		{
			Dictionary<int, ProcessDetails> dictionary = new Dictionary<int, ProcessDetails>();
			ProcessDetails value = default(ProcessDetails);
			foreach (Process selectedProcess in form.SelectedProcesses)
			{
				value.Name = selectedProcess.ProcessName;
				KilledProcesses = KilledProcesses + selectedProcess.ProcessName + ", ";
				value.Memory = $"{selectedProcess.WorkingSet64 / 1000:#,###}K";
				value.WindowTitle = selectedProcess.MainWindowTitle;
				value.StartTime = selectedProcess.StartTime.ToString();
				dictionary.Add(selectedProcess.Id, value);
			}
			List<int> processes = dictionary.Keys.ToList();
			if (killHandler != null && killHandler(processes))
			{
				string userName = ServiceLocator.Instance.GetService<IControlPanelService>()?.LoggedInUser.CheckNotAvailable();
				InnerSendAuditEvent(kioskId.ToString(), "CP_Process_Killed", userName, "CP Bundle", KilledProcesses);
				form.RefreshList();
			}
		};
		form.Show();
	}

	internal static object GetServicePath(string serviceName)
	{
		string name = string.Format("{0}\\{1}", "System\\CurrentControlSet\\Services", serviceName);
		RegistryKey registryKey = null;
		try
		{
			registryKey = Registry.LocalMachine.OpenSubKey(name);
			if (registryKey == null)
			{
				return null;
			}
			return Path.GetDirectoryName(((string)registryKey.GetValue("ImagePath")).TrimStart('"').TrimEnd('"'));
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log($"Caught an exception reading service information: {ex.Message}");
			return null;
		}
		finally
		{
			registryKey?.Close();
		}
	}

	internal static string GetFullPath(string path)
	{
		if (InvalidPath(path))
		{
			return null;
		}
		return Path.GetFullPath(path);
	}

	internal static string GetFileName(string path)
	{
		if (InvalidPath(path))
		{
			return null;
		}
		return Path.GetFileName(path);
	}

	internal static void ReplaceInFile(string filePath, string pattern, string token, string newvalue)
	{
		if (File.Exists(filePath))
		{
			new RegExFileEditor(filePath, backup: true, "saved").Replace(pattern, token, newvalue);
		}
	}

	internal static string GetMachineSettingValue(string path, string key, string defaultValue)
	{
		return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue(path, key, defaultValue);
	}

	internal static void SetMachineSettingValue(string path, string key, string value)
	{
		ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue(path, key, value);
	}

	internal static ErrorList HardwareNotifyMaintModeState(bool goingIntoMM)
	{
		return ServiceLocator.Instance.GetService<IHardwareService>()?.NotifyMaintModeState(goingIntoMM);
	}

	internal static void EnqueueStatistic(string name, string value)
	{
		ServiceLocator.Instance.GetService<IHealthServices>()?.EnqueueStatistic(name, value);
	}

	internal static string GetServiceStatus(string serviceName)
	{
		try
		{
			using ServiceController serviceController = new ServiceController(serviceName);
			return serviceController?.Status.ToString();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unexpected exception was raised in Win32ServiceFunctions.GetServiceStatus.", e);
			return null;
		}
	}

	internal static bool DoesFileExist(string path)
	{
		if (InvalidPath(path))
		{
			return false;
		}
		return File.Exists(path);
	}

	internal static bool StartService(string serviceName)
	{
		try
		{
			using ServiceController serviceController = new ServiceController(serviceName);
			serviceController.Start();
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unexpected exception was raised in Win32ServiceFunctions.StartService.", e);
			return false;
		}
	}

	internal static bool StopService(string serviceName)
	{
		try
		{
			using ServiceController serviceController = new ServiceController(serviceName);
			serviceController.Stop();
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unexpected exception was raised in Win32ServiceFunctions.StopService.", e);
			return false;
		}
	}

	internal static void RemoveTimer(string name)
	{
		ServiceLocator.Instance.GetService<ITimerService>().RemoveTimer(name);
	}

	internal static void CreateTimer(string name, object dueTime, object period, TimerFiredHandler timerHandler)
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		int? dueTime2 = null;
		if (dueTime != null)
		{
			dueTime2 = Convert.ToInt32(dueTime);
		}
		int? period2 = null;
		if (period != null)
		{
			period2 = Convert.ToInt32(period);
		}
		service.CreateTimer(name, dueTime2, period2, delegate
		{
			timerHandler();
		});
	}

	internal static void StopTimer(string name)
	{
		ServiceLocator.Instance.GetService<ITimerService>().GetTimer(name)?.Stop();
	}

	internal static void StartTimer(string name)
	{
		ServiceLocator.Instance.GetService<ITimerService>().GetTimer(name)?.Start();
	}

	internal static bool DoesDirectoryExist(string path)
	{
		if (InvalidPath(path))
		{
			return false;
		}
		return Directory.Exists(path);
	}

	internal static string CombinePaths(string first, string second)
	{
		if (InvalidPath(first) || InvalidPath(second))
		{
			return null;
		}
		return Path.Combine(first, second);
	}

	internal static void ToggleTouchScreen()
	{
		IEnvironmentNotificationService service = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		try
		{
			service.ToggleTouchScreen();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was thrown in ToggleTouchScreen.", e);
		}
	}

	internal static string TouchScreenStatus()
	{
		IEnvironmentNotificationService service = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		try
		{
			return service.TouchScreenStatus().ToString();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was thrown in TouchScreenStatus.", e);
		}
		return TouchScreenState.Unavailable.ToString();
	}

	internal static string TouchScreenDriver()
	{
		IEnvironmentNotificationService service = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		try
		{
			return service.TouchScreenDriver();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was thrown in TouchScreenDriver.", e);
		}
		return string.Empty;
	}

	internal static void LaunchSecureBrowser(Dictionary<string, object> approvedUrls, NavigateUrlCallback navigateUrlCallback)
	{
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		bool value = false;
		bool flag = service?.TryGetValue<bool>("system", "General", "DisableFieldVisitForm", out value) ?? false;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		for (int i = 0; i <= approvedUrls.Keys.Count - 1; i++)
		{
			KeyValuePair<string, object> keyValuePair = approvedUrls.ElementAt(i);
			if (!(flag & value) || !(keyValuePair.Key == "------------Field Visit Form------------"))
			{
				string value2 = ((!(keyValuePair.Key == "------------Field Visit Form------------") && !(keyValuePair.Key == "------------Install Check List------------")) ? ((string)keyValuePair.Value) : keyValuePair.Value.ToString().Replace("${StoreNumber}", kioskId.ToString()));
				dictionary[keyValuePair.Key] = value2;
			}
		}
		ServiceLocator.Instance.GetService<IEnvironmentNotificationService>().LaunchSecureBrowser(dictionary, navigateUrlCallback);
	}

	internal static Dictionary<string, object> GetTable(string name)
	{
		XmlNode xml = ServiceLocator.Instance.GetService<IResourceBundleService>().GetXml(name);
		if (xml == null)
		{
			return null;
		}
		XmlNodeList xmlNodeList = xml.SelectNodes("row");
		if (xmlNodeList == null)
		{
			return null;
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		foreach (XmlNode item in xmlNodeList)
		{
			XmlNodeList xmlNodeList2 = item.SelectNodes("column");
			if (xmlNodeList2 == null)
			{
				continue;
			}
			string key = string.Empty;
			object value = new object();
			foreach (XmlNode item2 in xmlNodeList2)
			{
				string attributeValue = item2.GetAttributeValue<string>("name");
				if (attributeValue == "title")
				{
					key = item2.GetNodeValue(string.Empty);
				}
				if (attributeValue == "url")
				{
					value = item2.GetNodeValue(string.Empty);
				}
				string attributeValue2 = item2.GetAttributeValue<string>("type");
				if (!string.IsNullOrEmpty(attributeValue2))
				{
					Type type = attributeValue2.ToType();
					if (type != null)
					{
						value = ConversionHelper.ChangeType(value, type);
					}
				}
			}
			dictionary.Add(key, value);
		}
		return dictionary;
	}

	internal static void SendAudit(string browseType, string url)
	{
		string userName = ServiceLocator.Instance.GetService<IControlPanelService>()?.LoggedInUser.CheckNotAvailable();
		InnerSendAuditEvent(kioskId.ToString(), browseType, userName, "CP_SecureBrowser", url);
	}

	internal static void SendAuditEvent(string storeNumber, string eventType, string userName, string source, string message)
	{
		InnerSendAuditEvent(storeNumber, eventType, userName, source, message);
	}

	internal static void RenameComputer(string name)
	{
		using ManagementClass managementClass = new ManagementClass("Win32_ComputerSystem");
		using ManagementObjectCollection managementObjectCollection = managementClass.GetInstances();
		foreach (ManagementObject item in managementObjectCollection)
		{
			using ManagementBaseObject managementBaseObject = item.GetMethodParameters("Rename");
			managementBaseObject.SetPropertyValue("Name", name);
			item.InvokeMethod("Rename", managementBaseObject, new InvokeMethodOptions());
		}
	}

	internal static void GetOpsMarket(string storeNumber, RemoteServiceCallback completeCallback)
	{
		IStoreServices service = ServiceLocator.Instance.GetService<IStoreServices>();
		if (service != null && ServiceLocator.Instance.GetService<ICallbackService>() != null)
		{
			service.GetOpsMarket(storeNumber, completeCallback);
		}
	}

	internal static List<IHardwareResult> HardwareGetProgramResults(string jobId)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		ReadOnlyCollection<IHardwareResult> hardwareResults = null;
		List<IHardwareResult> result = null;
		service.GetProgramResults(jobId, out hardwareResults);
		if (hardwareResults != null)
		{
			result = hardwareResults.ToList();
		}
		return result;
	}

	internal static void DeleteFileFromRelativePath(string relativeFilePath)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		if (service != null)
		{
			string path = Path.Combine(service.ExpandProperties("${RunningPath}\\..\\"), relativeFilePath);
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}
	}

	internal static void LogErrorsFromList(ErrorList errors)
	{
		if (errors == null)
		{
			return;
		}
		foreach (Redbox.KioskEngine.ComponentModel.Error error in errors)
		{
			LogHelper.Instance.Log(error.Description);
		}
	}

	internal static void InnerSnapshot(string storeNumber, List<Item> items, List<IInventoryItem> inventoryItems)
	{
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		if (service == null || items == null)
		{
			return;
		}
		int num = 0;
		foreach (Item item in items)
		{
			int deck = item.deck;
			int? num2 = null;
			num2 = Convert.ToInt32(deck);
			int slot = item.slot;
			int? num3 = null;
			num3 = Convert.ToInt32(slot);
			string barcode = item.barcode;
			if (string.IsNullOrEmpty(barcode))
			{
				LogHelper.Instance.Log($"A null or empty barcode was passed into InventoryFunctions.Snapshot where deck = {num2} and slot = {num3}; skipping this item.", LogEntryType.Error);
				num++;
			}
			else
			{
				InventoryItemStatus status = Enum<InventoryItemStatus>.ParseIgnoringCase(item.status, InventoryItemStatus.In);
				inventoryItems.Add(service.CreateInventoryItem(barcode, num2, num3, status));
				num++;
			}
		}
	}

	internal static void GetPendingStates()
	{
		IStoreServices service = ServiceLocator.Instance.GetService<IStoreServices>();
		if (service == null)
		{
			LogHelper.Instance.Log("Unexpected error, unable to get StoreServices!");
		}
		RemoteServiceCallback completeCallback = GetPendingStateResult;
		service.GetPendingStates(completeCallback);
	}

	public static void GetPendingStateResult(IRemoteServiceResult result)
	{
		tableStates = (List<IState>)result.Properties["states"];
		if (tableStates.Count() == 0)
		{
			LogHelper.Instance.Log("No Pending States were returned!");
			return;
		}
		tableStates = tableStates.OrderBy((IState x) => x.Name).ToList();
	}

	internal static void GetPendingBanners(int stateId, RemoteServiceCallback resultCallback)
	{
		IStoreServices service = ServiceLocator.Instance.GetService<IStoreServices>();
		if (service == null)
		{
			LogHelper.Instance.Log("No Pending Banners were returned, unable to get StoreServices!");
		}
		new List<IBanner>();
		service.GetPendingBanners(stateId, resultCallback);
	}

	internal static void GetPendingStores(int bannerId, int stateId, RemoteServiceCallback resultCallback)
	{
		IStoreServices service = ServiceLocator.Instance.GetService<IStoreServices>();
		if (service == null)
		{
			LogHelper.Instance.Log("No Stores were returned, unable to get StoreServices!");
		}
		new List<IStore>();
		service.GetPendingStores(bannerId, stateId, resultCallback);
	}

	private static void TaskKill(int processId)
	{
		try
		{
			LaunchProcess("TaskKill.exe", $"/f /pid {processId}", hiddenProcess: true);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error in Function KillProcessById.", e);
		}
	}

	private static void KillProcessById(int processId)
	{
		using Process process = Process.GetProcessById(processId);
		if (process != null && !process.HasExited)
		{
			try
			{
				process.Kill();
				process.WaitForExit();
				return;
			}
			catch (Exception)
			{
				TaskKill(processId);
				return;
			}
		}
	}

	private static int LaunchProcessInner(string programPath, string args, string userName, bool useAdminPassword, bool hiddenProcess)
	{
		try
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo
			{
				FileName = programPath,
				Arguments = args,
				WindowStyle = (hiddenProcess ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal),
				CreateNoWindow = hiddenProcess
			};
			if (useAdminPassword)
			{
				processStartInfo.UserName = userName;
				SecureString pass = new SecureString();
				AdminPassword.ForEach(delegate(char x)
				{
					pass.AppendChar(x);
				});
				processStartInfo.Password = pass;
				processStartInfo.UseShellExecute = false;
				if (string.IsNullOrEmpty(Path.GetExtension(processStartInfo.FileName)))
				{
					processStartInfo.FileName += ".exe";
				}
				if (!File.Exists(Path.Combine(processStartInfo.WorkingDirectory, processStartInfo.FileName)))
				{
					string text = Environment.GetEnvironmentVariable("SystemRoot");
					if (string.IsNullOrEmpty(text))
					{
						text = "c:\\windows";
					}
					string text2 = Path.Combine(text, processStartInfo.FileName);
					if (File.Exists(text2))
					{
						processStartInfo.FileName = text2;
					}
				}
			}
			Process process = Process.Start(processStartInfo);
			if (process != null)
			{
				return process.Id;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error in Kernel Function LaunchProcess.", e);
		}
		return -1;
	}

	private static List<string> GetAllAdministrators()
	{
		List<string> list = new List<string>();
		DirectoryEntry directoryEntry = new DirectoryEntry("WinNT://" + Environment.MachineName).Children.Find("administrators", "group");
		try
		{
			foreach (object item in (IEnumerable)directoryEntry.Invoke("members", null))
			{
				DirectoryEntry directoryEntry2 = new DirectoryEntry(item);
				if (directoryEntry2.SchemaClassName == "User")
				{
					list.Add(directoryEntry2.Name);
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Failed to get list of administrators", e, LogEntryType.Error);
		}
		return list.Distinct().ToList();
	}

	private static bool HasInvalidChars(string path)
	{
		char[] invalidPathChars = Path.GetInvalidPathChars();
		foreach (char value in invalidPathChars)
		{
			if (path.IndexOf(value) >= 0)
			{
				return true;
			}
		}
		return false;
	}

	private static bool InvalidPath(string path)
	{
		if (!string.IsNullOrEmpty(path))
		{
			return HasInvalidChars(path);
		}
		return true;
	}

	internal static void InnerSendAuditEvent(string storeNumber, string eventType, string userName, string source, string message)
	{
		ServiceLocator.Instance.GetService<IHealthServices>()?.SendAuditEvent(storeNumber, eventType, DateTime.Now, userName, message, source, null);
	}

	internal static void PerformActivityLogAndSendAuditEvent(string tag)
	{
		string userName = ServiceLocator.Instance.GetService<IControlPanelService>()?.LoggedInUser.CheckNotAvailable();
		LogHelper.Instance.LogTo("ControlPanelActionLog", tag);
		InnerSendAuditEvent(kioskId.ToString(), tag, userName, "CP Bundle", "Activate Control Panel function.");
	}

	private static bool EnumWindowsCallback(IntPtr hwnd, IntPtr lParam)
	{
		IntPtr lpdwProcessId = default(IntPtr);
		GetWindowThreadProcessId(hwnd, out lpdwProcessId);
		if (Process.GetProcessById(lpdwProcessId.ToInt32()).ProcessName == "explorer")
		{
			StringBuilder stringBuilder = new StringBuilder(255);
			RealGetWindowClass(hwnd, stringBuilder, 255u);
			if (stringBuilder.ToString().Equals("CabinetWClass"))
			{
				SendMessage(hwnd, 16u, IntPtr.Zero, IntPtr.Zero);
			}
		}
		return true;
	}

	internal static void CloseExploreWindows()
	{
		EnumWindows(EnumWindowsCallback, IntPtr.Zero);
	}

	[DllImport("user32.dll")]
	private static extern bool EnumWindows(EnumWindowsDelegate lpEnumFunc, IntPtr lParam);

	[DllImport("user32.dll")]
	private static extern uint RealGetWindowClass(IntPtr hwnd, StringBuilder pszType, uint cchType);

	[DllImport("user32.dll")]
	private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out IntPtr lpdwProcessId);

	[DllImport("user32.dll")]
	private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
}
