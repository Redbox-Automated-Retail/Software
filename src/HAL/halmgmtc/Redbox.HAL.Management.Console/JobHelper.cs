using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;
using Redbox.HAL.Management.Console.Properties;

namespace Redbox.HAL.Management.Console;

public static class JobHelper
{
	private static AtomicFlag m_jobListLock = new AtomicFlag();

	private static BindingList<ProgramEvent> m_programEventsList;

	private static BindingList<StringWrapper> m_resultList;

	private static SortableBindingList<HardwareJobWrapper> m_jobList;

	private static BindingList<HardwareJobWrapper> m_jobComboList;

	private static BindingList<StringWrapper> m_stackList;

	private static SortableBindingList<Symbol> m_symbolList;

	private static List<string> m_secureJobs;

	private static Dictionary<string, List<ProgramEvent>> m_newEvents = new Dictionary<string, List<ProgramEvent>>();

	public static SortableBindingList<HardwareJobWrapper> JobList
	{
		get
		{
			if (m_jobList == null)
			{
				m_jobList = new SortableBindingList<HardwareJobWrapper>();
			}
			return m_jobList;
		}
	}

	public static BindingList<HardwareJobWrapper> JobComboList
	{
		get
		{
			if (m_jobComboList == null)
			{
				m_jobComboList = new BindingList<HardwareJobWrapper>();
				m_jobComboList.Add(Constants.NullJob);
			}
			return m_jobComboList;
		}
	}

	public static BindingList<StringWrapper> StackList
	{
		get
		{
			if (m_stackList == null)
			{
				m_stackList = new BindingList<StringWrapper>();
			}
			return m_stackList;
		}
	}

	public static SortableBindingList<Symbol> SymbolList
	{
		get
		{
			if (m_symbolList == null)
			{
				m_symbolList = new SortableBindingList<Symbol>();
			}
			return m_symbolList;
		}
	}

	public static BindingList<StringWrapper> ResultList
	{
		get
		{
			if (m_resultList == null)
			{
				m_resultList = new BindingList<StringWrapper>();
			}
			return m_resultList;
		}
	}

	public static BindingList<ProgramEvent> ProgramEventList
	{
		get
		{
			if (m_programEventsList == null)
			{
				m_programEventsList = new BindingList<ProgramEvent>();
			}
			return m_programEventsList;
		}
		set
		{
			m_programEventsList = value;
		}
	}

	public static void DisconnectJobs()
	{
		foreach (HardwareJobWrapper job in JobList)
		{
			if (job.ConnectionState == HardwareJobConnectionState.Connected)
			{
				job.Disconnect();
			}
		}
	}

	public static bool PushStackItem(string s)
	{
		if (!ProfileManager.Instance.IsConnected)
		{
			return false;
		}
		if (!(JobComboBox.Instance.SelectedItem is HardwareJobWrapper hardwareJobWrapper))
		{
			return false;
		}
		if (int.TryParse(s, out var result))
		{
			HardwareCommandResult hardwareCommandResult = hardwareJobWrapper.Push<int>(new object[1] { result });
			CommonFunctions.ProcessCommandResult(hardwareCommandResult);
			if (!hardwareCommandResult.Success)
			{
				return false;
			}
			return true;
		}
		string text = s.Replace("\"", string.Empty);
		text = text.Replace("'", "\\'");
		HardwareCommandResult hardwareCommandResult2 = hardwareJobWrapper.Push<string>(new object[1] { text });
		CommonFunctions.ProcessCommandResult(hardwareCommandResult2);
		if (!hardwareCommandResult2.Success)
		{
			return false;
		}
		return true;
	}

	public static bool ClearJobStack()
	{
		if (!ProfileManager.Instance.IsConnected)
		{
			return false;
		}
		if (!(JobComboBox.Instance.SelectedItem is HardwareJobWrapper hardwareJobWrapper) || !hardwareJobWrapper.IsDisplayable())
		{
			return false;
		}
		HardwareCommandResult hardwareCommandResult = hardwareJobWrapper.ClearStack();
		if (!hardwareCommandResult.Success)
		{
			CommonFunctions.ProcessCommandResultError(hardwareCommandResult);
			return false;
		}
		return true;
	}

	public static void ClearJobComboList()
	{
		List<HardwareJobWrapper> list = new List<HardwareJobWrapper>();
		foreach (HardwareJobWrapper jobCombo in JobComboList)
		{
			if (jobCombo.IsDisplayable())
			{
				list.Add(jobCombo);
			}
		}
		foreach (HardwareJobWrapper item in list)
		{
			JobComboList.Remove(item);
		}
	}

	public static void ProcessJobEvent(HardwareJob job, DateTime eventTime, string message)
	{
		if (message == null || !message.StartsWith(">STATUS CHANGE<"))
		{
			ProgramEvent item = new ProgramEvent(eventTime, message);
			if (m_newEvents.ContainsKey(job.ID))
			{
				m_newEvents[job.ID].Add(item);
				return;
			}
			m_newEvents.Add(job.ID, new List<ProgramEvent> { item });
		}
	}

	public static bool RefreshJobList()
	{
		if (!ProfileManager.Instance.IsConnected)
		{
			return false;
		}
		if (!m_jobListLock.Set())
		{
			LogHelper.Instance.Log("Refresh Job List Denied - Locked", LogEntryType.Error);
		}
		try
		{
			HardwareCommandResult jobs = ProfileManager.Instance.Service.GetJobs(out var jobs2);
			if (!jobs.Success)
			{
				CommonFunctions.ProcessCommandResultError(jobs);
				return false;
			}
			HardwareJob[] array;
			for (int i = 0; i < JobList.Count; i++)
			{
				bool flag = true;
				array = jobs2;
				foreach (HardwareJob hardwareJob in array)
				{
					if (hardwareJob.ID == JobList[i].ID)
					{
						flag = false;
						JobList[i].Merge(hardwareJob);
						break;
					}
				}
				if (flag)
				{
					if (JobList[i].ConnectionState == HardwareJobConnectionState.Connected)
					{
						JobList[i].Disconnect();
					}
					JobComboList.Remove(JobList[i]);
					JobList.RemoveAt(i);
				}
			}
			array = jobs2;
			foreach (HardwareJob hardwareJob2 in array)
			{
				bool flag2 = true;
				for (int k = 0; k < JobList.Count; k++)
				{
					if (hardwareJob2.ID == JobList[k].ID)
					{
						flag2 = false;
						break;
					}
				}
				if (flag2)
				{
					HardwareJobWrapper item = new HardwareJobWrapper(hardwareJob2);
					JobList.Add(item);
					JobComboList.Add(item);
				}
			}
		}
		finally
		{
			m_jobListLock.Clear();
		}
		return true;
	}

	public static bool RefreshProgramEventList()
	{
		foreach (KeyValuePair<string, List<ProgramEvent>> newEvent in m_newEvents)
		{
			if (newEvent.Value == null || newEvent.Value.Count <= 0)
			{
				continue;
			}
			HardwareJobWrapper hardwareJobWrapper = null;
			foreach (HardwareJobWrapper job in JobList)
			{
				if (newEvent.Key == job.ID)
				{
					hardwareJobWrapper = job;
					break;
				}
			}
			if (hardwareJobWrapper == null)
			{
				continue;
			}
			ProgramEvent[] array = new ProgramEvent[newEvent.Value.Count];
			newEvent.Value.CopyTo(array);
			foreach (ProgramEvent item in array.OrderBy((ProgramEvent x) => x.EventTime))
			{
				hardwareJobWrapper.Events.Add(item);
				newEvent.Value.Remove(item);
			}
		}
		return true;
	}

	public static bool RefreshResultList()
	{
		if (!(JobComboBox.Instance.SelectedItem is HardwareJobWrapper hardwareJobWrapper) || !hardwareJobWrapper.IsDisplayable())
		{
			return false;
		}
		HardwareCommandResult results = hardwareJobWrapper.GetResults(out var results2);
		if (!results.Success)
		{
			CommonFunctions.ProcessCommandResultError(results);
			return false;
		}
		ProgramResult[] array;
		for (int i = 0; i < ResultList.Count; i++)
		{
			bool flag = true;
			array = results2;
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j].ToString().Equals(m_resultList[i].Value))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				ResultList.RemoveAt(i);
			}
		}
		array = results2;
		foreach (ProgramResult programResult in array)
		{
			bool flag2 = true;
			for (int k = 0; k < ResultList.Count; k++)
			{
				if (programResult.ToString().Equals(ResultList[k].Value))
				{
					flag2 = false;
					break;
				}
			}
			if (flag2)
			{
				ResultList.Add(new StringWrapper(programResult.ToString()));
			}
		}
		return true;
	}

	public static void RefreshSymbolList()
	{
		string jobID = JobComboBox.Instance.GetJobID();
		if (string.IsNullOrEmpty(jobID) || !(JobComboBox.Instance.SelectedItem as HardwareJobWrapper).IsDisplayable())
		{
			return;
		}
		HardwareJob job = null;
		ProfileManager.Instance.Service.GetJob(jobID, out job);
		HardwareCommandResult symbols = job.GetSymbols(out var symbols2);
		if (!symbols.Success)
		{
			CommonFunctions.ProcessCommandResultError(symbols);
			return;
		}
		for (int i = 0; i < SymbolList.Count; i++)
		{
			bool flag = true;
			foreach (KeyValuePair<string, string> item in symbols2)
			{
				if (item.Key.Equals(SymbolList[i].Name))
				{
					if (!item.Value.Equals(SymbolList[i].Value))
					{
						SymbolList[i].Value = item.Value;
					}
					flag = false;
					break;
				}
			}
			if (flag)
			{
				m_symbolList.RemoveAt(i);
			}
		}
		foreach (KeyValuePair<string, string> item2 in symbols2)
		{
			bool flag2 = true;
			for (int j = 0; j < SymbolList.Count; j++)
			{
				if (item2.Key.Equals(SymbolList[j].Name))
				{
					flag2 = false;
					break;
				}
			}
			if (flag2)
			{
				SymbolList.Add(new Symbol(item2.Key, item2.Value));
			}
		}
	}

	public static bool RefreshStackList()
	{
		if (!(JobComboBox.Instance.SelectedItem is HardwareJobWrapper hardwareJobWrapper) || !hardwareJobWrapper.IsDisplayable())
		{
			return false;
		}
		Stack<string> stack = new Stack<string>();
		HardwareCommandResult stack2 = hardwareJobWrapper.GetStack(out stack);
		if (!stack2.Success)
		{
			CommonFunctions.ProcessCommandResultError(stack2);
			OutputWindow.Instance.Append("ERROR: Unable to get stack");
			return false;
		}
		while (StackList.Count > stack.Count)
		{
			StackList.RemoveAt(stack.Count);
		}
		for (int i = 0; i < StackList.Count; i++)
		{
			string text = stack.Pop();
			if (StackList[i].Value != text)
			{
				StackList[i].Value = text;
			}
		}
		while (stack.Count > 0)
		{
			string value = stack.Pop();
			StackList.Add(new StringWrapper(value));
		}
		return true;
	}

	public static void ViewErrors(List<HardwareJobWrapper> jobs)
	{
		if (jobs == null)
		{
			return;
		}
		ErrorListView.Instance.Clear();
		int num = 0;
		foreach (HardwareJobWrapper job in jobs)
		{
			if (job != null && job.IsDisplayable())
			{
				HardwareCommandResult errors = job.GetErrors(out var errors2);
				if (!errors.Success)
				{
					CommonFunctions.ProcessCommandResultError(errors);
				}
				else if (errors2 != null && errors2.Count > 0)
				{
					EnvironmentHelper.DisplayErrors(errors2, clearPrevious: false, keepOpenOnSuccessfulInstruction: false);
					num += errors2.Count;
				}
			}
		}
		if (num == 0)
		{
			MessageBox.Show("No Errors To Display", "Hal Management Console", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
		}
	}

	public static void Connect(HardwareJobWrapper job)
	{
		if (job != null && job.IsDisplayable())
		{
			HardwareCommandResult hardwareCommandResult = job.Connect();
			CommonFunctions.ProcessCommandResult(hardwareCommandResult);
			if (hardwareCommandResult.Success)
			{
				job.EventRaised += ProcessJobEvent;
			}
		}
	}

	public static void ConnectJobs(List<HardwareJobWrapper> jobs)
	{
		foreach (HardwareJobWrapper job in jobs)
		{
			Connect(job);
		}
	}

	public static void Disconnect(HardwareJobWrapper job)
	{
		if (job != null && job.IsDisplayable())
		{
			job.EventRaised -= ProcessJobEvent;
			job.Disconnect();
		}
	}

	public static void DisconnectJobs(List<HardwareJobWrapper> jobs)
	{
		foreach (HardwareJobWrapper job in jobs)
		{
			Disconnect(job);
		}
	}

	public static void SetLabel(HardwareJobWrapper job, string value)
	{
		if (job != null && job.IsDisplayable() && !IsCompleted(job))
		{
			CommonFunctions.ProcessCommandResult(job.SetLabel(value));
		}
	}

	public static void SetLabels(List<HardwareJobWrapper> jobs)
	{
		InputBoxForm inputBoxForm = new InputBoxForm
		{
			Text = "Set Job Label",
			Label = "Enter the label for this job:"
		};
		if (inputBoxForm.ShowDialog() == DialogResult.Cancel)
		{
			return;
		}
		foreach (HardwareJobWrapper job in jobs)
		{
			SetLabel(job, inputBoxForm.Value);
		}
	}

	public static void SetPriority(HardwareJobWrapper job, HardwareJobPriority priority)
	{
		if (job != null && !IsCompleted(job))
		{
			CommonFunctions.ProcessCommandResult(job.SetPriority(priority));
		}
	}

	public static void SetPriorities(List<HardwareJobWrapper> jobs, HardwareJobPriority priority)
	{
		foreach (HardwareJobWrapper job in jobs)
		{
			SetPriority(job, priority);
		}
	}

	public static void SetStartTime(HardwareJobWrapper job, DateTime value)
	{
		if (job != null || job.IsDisplayable() || !IsCompleted(job))
		{
			CommonFunctions.ProcessCommandResult(job.SetStartTime(value));
		}
	}

	public static void SetStartTimes(List<HardwareJobWrapper> jobs)
	{
		StartTimeForm startTimeForm = new StartTimeForm();
		if (startTimeForm.ShowDialog() == DialogResult.Cancel || jobs == null)
		{
			return;
		}
		foreach (HardwareJobWrapper job in jobs)
		{
			SetStartTime(job, startTimeForm.StartTime.Value);
		}
	}

	public static void ResumeJob(HardwareJobWrapper job)
	{
		if (job != null && job.IsDisplayable() && !IsCompleted(job))
		{
			CommonFunctions.ProcessCommandResult(job.Resume());
		}
	}

	public static void ResumeJobs(List<HardwareJobWrapper> jobs)
	{
		foreach (HardwareJobWrapper job in jobs)
		{
			ResumeJob(job);
		}
	}

	public static void SuspendJob(HardwareJobWrapper job)
	{
		if (job != null && job.IsDisplayable())
		{
			CommonFunctions.ProcessCommandResult(job.Suspend());
		}
	}

	public static void SuspendJobs(List<HardwareJobWrapper> jobs)
	{
		foreach (HardwareJobWrapper job in jobs)
		{
			SuspendJob(job);
		}
	}

	public static void TerminateJob(HardwareJobWrapper job)
	{
		if (job != null && job.IsDisplayable())
		{
			CommonFunctions.ProcessCommandResult(job.Terminate());
		}
	}

	public static void TerminateJobs(List<HardwareJobWrapper> jobs)
	{
		foreach (HardwareJobWrapper job in jobs)
		{
			TerminateJob(job);
		}
	}

	public static void TrashJob(HardwareJobWrapper job)
	{
		if (job != null && job.IsDisplayable() && !(job.ID == "IMMEDIATE") && (!EnvironmentHelper.IsLocked || !IsSecureJob(job)))
		{
			CommonFunctions.ProcessCommandResult(job.Trash());
		}
	}

	public static void TrashJobs(List<HardwareJobWrapper> jobs)
	{
		foreach (HardwareJobWrapper job in jobs)
		{
			TrashJob(job);
		}
	}

	public static void CollectGarbage(bool force)
	{
		CommonFunctions.ProcessCommandResult(ProfileManager.Instance.Service.CollectGarbage(force));
	}

	public static void UpdateButtons(ToolStripItemCollection collection, List<HardwareJobWrapper> list)
	{
		ToolStripItem toolStripItem = collection[JobItems.Garbage.ToString()];
		ToolStripItem toolStripItem2 = collection[JobItems.ViewErrors.ToString()];
		ToolStripItem toolStripItem3 = collection[JobItems.Connect.ToString()];
		ToolStripItem toolStripItem4 = collection[JobItems.Disconnect.ToString()];
		ToolStripItem toolStripItem5 = collection[JobItems.SetLabel.ToString()];
		ToolStripItem toolStripItem6 = collection[JobItems.SetStartTime.ToString()];
		ToolStripItem toolStripItem7 = collection[JobItems.Resume.ToString()];
		ToolStripItem toolStripItem8 = collection[JobItems.Suspend.ToString()];
		ToolStripItem toolStripItem9 = collection[JobItems.Stop.ToString()];
		ToolStripItem toolStripItem10 = collection[JobItems.Trash.ToString()];
		ToolStripItem toolStripItem11 = collection[JobItems.TrashLocked.ToString()];
		ToolStripItem toolStripItem12 = collection[JobItems.Priority.ToString()];
		toolStripItem.Enabled = false;
		toolStripItem2.Enabled = false;
		toolStripItem3.Enabled = false;
		toolStripItem4.Enabled = false;
		toolStripItem5.Enabled = false;
		toolStripItem6.Enabled = false;
		toolStripItem7.Enabled = false;
		toolStripItem8.Enabled = false;
		toolStripItem9.Enabled = false;
		if (toolStripItem12 != null)
		{
			toolStripItem12.Enabled = false;
		}
		if (toolStripItem10 != null)
		{
			toolStripItem10.Enabled = false;
		}
		if (toolStripItem11 != null)
		{
			toolStripItem11.Enabled = false;
		}
		if (!ProfileManager.Instance.IsConnected)
		{
			return;
		}
		toolStripItem.Enabled = true;
		if (list == null || list.Count < 1)
		{
			return;
		}
		foreach (HardwareJobWrapper item in list)
		{
			if (item == null || !item.IsDisplayable())
			{
				continue;
			}
			if (toolStripItem12 != null)
			{
				toolStripItem12.Enabled = true;
			}
			toolStripItem2.Enabled = true;
			if (item.ConnectionState == HardwareJobConnectionState.Disconnected)
			{
				toolStripItem3.Enabled = true;
			}
			if (item.ConnectionState == HardwareJobConnectionState.Connected)
			{
				toolStripItem4.Enabled = true;
			}
			if (item.Status == HardwareJobStatus.Pending || item.Status == HardwareJobStatus.Running || item.Status == HardwareJobStatus.Suspended)
			{
				toolStripItem5.Enabled = true;
				toolStripItem9.Enabled = true;
				if (item.Status != HardwareJobStatus.Running)
				{
					toolStripItem6.Enabled = true;
				}
			}
			if (item.Status != HardwareJobStatus.Garbage && item.ID != "IMMEDIATE" && !IsSecureJob(item))
			{
				if (toolStripItem10 != null)
				{
					toolStripItem10.Enabled = true;
				}
				if (toolStripItem11 != null && !EnvironmentHelper.IsLocked)
				{
					toolStripItem11.Enabled = true;
				}
			}
			if (item.Status == HardwareJobStatus.Suspended)
			{
				toolStripItem7.Enabled = true;
			}
			if (item.Status == HardwareJobStatus.Running || item.Status == HardwareJobStatus.Pending)
			{
				toolStripItem8.Enabled = true;
			}
		}
	}

	public static void OnConnect(object sender, EventArgs e)
	{
		List<HardwareJobWrapper> list = ((sender as ToolStripItem).Owner as IJobStrip).UpdateSource();
		if (list != null)
		{
			ConnectJobs(list);
		}
	}

	public static void OnDisconnect(object sender, EventArgs e)
	{
		List<HardwareJobWrapper> list = ((sender as ToolStripItem).Owner as IJobStrip).UpdateSource();
		if (list != null)
		{
			DisconnectJobs(list);
		}
	}

	public static void OnGarbage(object sender, EventArgs e)
	{
		CollectGarbage(force: false);
	}

	public static void OnResume(object sender, EventArgs e)
	{
		List<HardwareJobWrapper> list = ((sender as ToolStripItem).Owner as IJobStrip).UpdateSource();
		if (list != null)
		{
			ResumeJobs(list);
		}
	}

	public static void OnSuspend(object sender, EventArgs e)
	{
		List<HardwareJobWrapper> list = ((sender as ToolStripItem).Owner as IJobStrip).UpdateSource();
		if (list != null)
		{
			SuspendJobs(list);
		}
	}

	public static void OnTerminate(object sender, EventArgs e)
	{
		List<HardwareJobWrapper> list = ((sender as ToolStripItem).Owner as IJobStrip).UpdateSource();
		if (list != null)
		{
			TerminateJobs(list);
		}
	}

	public static void OnTrash(object sender, EventArgs e)
	{
		List<HardwareJobWrapper> list = ((sender as ToolStripItem).Owner as IJobStrip).UpdateSource();
		if (list != null)
		{
			TrashJobs(list);
		}
	}

	public static void OnSetLabel(object sender, EventArgs e)
	{
		List<HardwareJobWrapper> list = ((sender as ToolStripItem).Owner as IJobStrip).UpdateSource();
		if (list != null)
		{
			SetLabels(list);
		}
	}

	public static void OnSetStartTimes(object sender, EventArgs e)
	{
		List<HardwareJobWrapper> list = ((sender as ToolStripItem).Owner as IJobStrip).UpdateSource();
		if (list != null)
		{
			SetStartTimes(list);
		}
	}

	public static void OnViewErrors(object sender, EventArgs e)
	{
		List<HardwareJobWrapper> list = ((sender as ToolStripItem).Owner as IJobStrip).UpdateSource();
		if (list != null)
		{
			ViewErrors(list);
		}
	}

	private static void ExecuteCommand()
	{
	}

	private static bool IsSecureJob(HardwareJobWrapper job)
	{
		if (!EnvironmentHelper.IsLocked)
		{
			return false;
		}
		if (m_secureJobs == null)
		{
			m_secureJobs = new List<string>();
			string secureJob = Settings.Default.SecureJob;
			m_secureJobs.AddRange(secureJob.Split(','));
		}
		if (m_secureJobs.Find((string x) => x == job.ProgramName) != null)
		{
			return true;
		}
		return false;
	}

	private static bool IsCompleted(HardwareJobWrapper job)
	{
		if (job.Status != HardwareJobStatus.Completed && job.Status != HardwareJobStatus.Errored && job.Status != HardwareJobStatus.Garbage)
		{
			return job.Status == HardwareJobStatus.Stopped;
		}
		return true;
	}
}
