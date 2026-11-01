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
		if (int.TryParse(s, out var number))
		{
			HardwareCommandResult pushResult = hardwareJobWrapper.Push<int>(new object[1] { number });
			CommonFunctions.ProcessCommandResult(pushResult);
			if (!pushResult.Success)
			{
				return false;
			}
			return true;
		}
		string quoted = s.Replace("\"", string.Empty);
		quoted = quoted.Replace("'", "\\'");
		HardwareCommandResult pushStringResult = hardwareJobWrapper.Push<string>(new object[1] { quoted });
		CommonFunctions.ProcessCommandResult(pushStringResult);
		if (!pushStringResult.Success)
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
		List<HardwareJobWrapper> displayable = new List<HardwareJobWrapper>();
		foreach (HardwareJobWrapper jobCombo in JobComboList)
		{
			if (jobCombo.IsDisplayable())
			{
				displayable.Add(jobCombo);
			}
		}
		foreach (HardwareJobWrapper item in displayable)
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
			HardwareCommandResult getJobsResult = ProfileManager.Instance.Service.GetJobs(out var jobs);
			if (!getJobsResult.Success)
			{
				CommonFunctions.ProcessCommandResultError(getJobsResult);
				return false;
			}
			HardwareJob[] serverJobs;
			for (int i = 0; i < JobList.Count; i++)
			{
				bool gone = true;
				serverJobs = jobs;
				foreach (HardwareJob hardwareJob in serverJobs)
				{
					if (hardwareJob.ID == JobList[i].ID)
					{
						gone = false;
						JobList[i].Merge(hardwareJob);
						break;
					}
				}
				if (gone)
				{
					if (JobList[i].ConnectionState == HardwareJobConnectionState.Connected)
					{
						JobList[i].Disconnect();
					}
					JobComboList.Remove(JobList[i]);
					JobList.RemoveAt(i);
				}
			}
			serverJobs = jobs;
			foreach (HardwareJob serverJob in serverJobs)
			{
				bool isNew = true;
				for (int k = 0; k < JobList.Count; k++)
				{
					if (serverJob.ID == JobList[k].ID)
					{
						isNew = false;
						break;
					}
				}
				if (isNew)
				{
					HardwareJobWrapper item = new HardwareJobWrapper(serverJob);
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
			ProgramEvent[] events = new ProgramEvent[newEvent.Value.Count];
			newEvent.Value.CopyTo(events);
			foreach (ProgramEvent item in events.OrderBy((ProgramEvent x) => x.EventTime))
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
		HardwareCommandResult getResultsResult = hardwareJobWrapper.GetResults(out var results);
		if (!getResultsResult.Success)
		{
			CommonFunctions.ProcessCommandResultError(getResultsResult);
			return false;
		}
		ProgramResult[] serverResults;
		for (int i = 0; i < ResultList.Count; i++)
		{
			bool gone = true;
			serverResults = results;
			for (int j = 0; j < serverResults.Length; j++)
			{
				if (serverResults[j].ToString().Equals(m_resultList[i].Value))
				{
					gone = false;
					break;
				}
			}
			if (gone)
			{
				ResultList.RemoveAt(i);
			}
		}
		serverResults = results;
		foreach (ProgramResult programResult in serverResults)
		{
			bool isNew = true;
			for (int k = 0; k < ResultList.Count; k++)
			{
				if (programResult.ToString().Equals(ResultList[k].Value))
				{
					isNew = false;
					break;
				}
			}
			if (isNew)
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
		HardwareCommandResult getSymbolsResult = job.GetSymbols(out var symbols);
		if (!getSymbolsResult.Success)
		{
			CommonFunctions.ProcessCommandResultError(getSymbolsResult);
			return;
		}
		for (int i = 0; i < SymbolList.Count; i++)
		{
			bool gone = true;
			foreach (KeyValuePair<string, string> symbol in symbols)
			{
				if (symbol.Key.Equals(SymbolList[i].Name))
				{
					if (!symbol.Value.Equals(SymbolList[i].Value))
					{
						SymbolList[i].Value = symbol.Value;
					}
					gone = false;
					break;
				}
			}
			if (gone)
			{
				m_symbolList.RemoveAt(i);
			}
		}
		foreach (KeyValuePair<string, string> serverSymbol in symbols)
		{
			bool isNew = true;
			for (int j = 0; j < SymbolList.Count; j++)
			{
				if (serverSymbol.Key.Equals(SymbolList[j].Name))
				{
					isNew = false;
					break;
				}
			}
			if (isNew)
			{
				SymbolList.Add(new Symbol(serverSymbol.Key, serverSymbol.Value));
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
		HardwareCommandResult getStackResult = hardwareJobWrapper.GetStack(out stack);
		if (!getStackResult.Success)
		{
			CommonFunctions.ProcessCommandResultError(getStackResult);
			OutputWindow.Instance.Append("ERROR: Unable to get stack");
			return false;
		}
		while (StackList.Count > stack.Count)
		{
			StackList.RemoveAt(stack.Count);
		}
		for (int i = 0; i < StackList.Count; i++)
		{
			string entry = stack.Pop();
			if (StackList[i].Value != entry)
			{
				StackList[i].Value = entry;
			}
		}
		while (stack.Count > 0)
		{
			string remaining = stack.Pop();
			StackList.Add(new StringWrapper(remaining));
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
		int errorCount = 0;
		foreach (HardwareJobWrapper job in jobs)
		{
			if (job != null && job.IsDisplayable())
			{
				HardwareCommandResult getErrorsResult = job.GetErrors(out var errors);
				if (!getErrorsResult.Success)
				{
					CommonFunctions.ProcessCommandResultError(getErrorsResult);
				}
				else if (errors != null && errors.Count > 0)
				{
					EnvironmentHelper.DisplayErrors(errors, clearPrevious: false, keepOpenOnSuccessfulInstruction: false);
					errorCount += errors.Count;
				}
			}
		}
		if (errorCount == 0)
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
		ToolStripItem garbageItem = collection[JobItems.Garbage.ToString()];
		ToolStripItem viewErrorsItem = collection[JobItems.ViewErrors.ToString()];
		ToolStripItem connectItem = collection[JobItems.Connect.ToString()];
		ToolStripItem disconnectItem = collection[JobItems.Disconnect.ToString()];
		ToolStripItem setLabelItem = collection[JobItems.SetLabel.ToString()];
		ToolStripItem setStartTimeItem = collection[JobItems.SetStartTime.ToString()];
		ToolStripItem resumeItem = collection[JobItems.Resume.ToString()];
		ToolStripItem suspendItem = collection[JobItems.Suspend.ToString()];
		ToolStripItem stopItem = collection[JobItems.Stop.ToString()];
		ToolStripItem trashItem = collection[JobItems.Trash.ToString()];
		ToolStripItem trashLockedItem = collection[JobItems.TrashLocked.ToString()];
		ToolStripItem priorityItem = collection[JobItems.Priority.ToString()];
		garbageItem.Enabled = false;
		viewErrorsItem.Enabled = false;
		connectItem.Enabled = false;
		disconnectItem.Enabled = false;
		setLabelItem.Enabled = false;
		setStartTimeItem.Enabled = false;
		resumeItem.Enabled = false;
		suspendItem.Enabled = false;
		stopItem.Enabled = false;
		if (priorityItem != null)
		{
			priorityItem.Enabled = false;
		}
		if (trashItem != null)
		{
			trashItem.Enabled = false;
		}
		if (trashLockedItem != null)
		{
			trashLockedItem.Enabled = false;
		}
		if (!ProfileManager.Instance.IsConnected)
		{
			return;
		}
		garbageItem.Enabled = true;
		if (list == null || list.Count < 1)
		{
			return;
		}
		foreach (HardwareJobWrapper job in list)
		{
			if (job == null || !job.IsDisplayable())
			{
				continue;
			}
			if (priorityItem != null)
			{
				priorityItem.Enabled = true;
			}
			viewErrorsItem.Enabled = true;
			if (job.ConnectionState == HardwareJobConnectionState.Disconnected)
			{
				connectItem.Enabled = true;
			}
			if (job.ConnectionState == HardwareJobConnectionState.Connected)
			{
				disconnectItem.Enabled = true;
			}
			if (job.Status == HardwareJobStatus.Pending || job.Status == HardwareJobStatus.Running || job.Status == HardwareJobStatus.Suspended)
			{
				setLabelItem.Enabled = true;
				stopItem.Enabled = true;
				if (job.Status != HardwareJobStatus.Running)
				{
					setStartTimeItem.Enabled = true;
				}
			}
			if (job.Status != HardwareJobStatus.Garbage && job.ID != "IMMEDIATE" && !IsSecureJob(job))
			{
				if (trashItem != null)
				{
					trashItem.Enabled = true;
				}
				if (trashLockedItem != null && !EnvironmentHelper.IsLocked)
				{
					trashLockedItem.Enabled = true;
				}
			}
			if (job.Status == HardwareJobStatus.Suspended)
			{
				resumeItem.Enabled = true;
			}
			if (job.Status == HardwareJobStatus.Running || job.Status == HardwareJobStatus.Pending)
			{
				suspendItem.Enabled = true;
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
