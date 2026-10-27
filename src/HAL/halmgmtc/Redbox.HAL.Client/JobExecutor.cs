using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client;

public abstract class JobExecutor : IDisposable
{
	protected readonly ClientHelper Helper;

	protected readonly HardwareService Service;

	protected internal readonly HardwareCommandResult ScheduleResult;

	private HardwareEvent UserEvent;

	private readonly HardwareJob m_job;

	private HardwareJobStatus m_endStatus = HardwareJobStatus.Errored;

	private bool m_disposed;

	public IDictionary<string, string> Symbols { get; private set; }

	public List<ProgramResult> Results { get; private set; }

	public ErrorList Errors { get; private set; }

	public HardwareJobStatus EndStatus => m_endStatus;

	public string ID { get; private set; }

	public HardwareJob Job => m_job;

	protected abstract string JobName { get; }

	protected virtual string Label => string.Empty;

	protected virtual HardwareJobPriority JobPriority => HardwareJobPriority.Highest;

	public void Dispose()
	{
		if (!m_disposed)
		{
			m_disposed = true;
			DisposeInner();
			Helper.Dispose();
			Results.Clear();
			Results = null;
			Errors.Clear();
			Errors = null;
		}
	}

	public void AddSink(HardwareEvent callback)
	{
		m_job.EventRaised += OnHardwareEvent;
		UserEvent = callback;
	}

	public void Run()
	{
		if (m_job == null)
		{
			Errors.Add(Error.NewError("E785", "Job schedule failure.", "Failed to schedule job."));
			return;
		}
		ID = m_job.ID;
		SetupJob(m_job);
		if (!Helper.WaitForJob(m_job, out m_endStatus))
		{
			Errors.Add(Error.NewError("E786", "Job wait failure.", "Failed to wait for job."));
			return;
		}
		if (!m_job.GetResults(out var results).Success)
		{
			Errors.Add(Error.NewError("E787", "GetResults failure.", "Failed to get program results. I'm sorry!"));
		}
		else
		{
			Results.AddRange(results);
			results = null;
		}
		if (m_job.GetErrors(out var errors).Success)
		{
			Errors.AddRange(errors);
			errors.Clear();
		}
		if (m_job.GetSymbols(out var symbols).Success)
		{
			foreach (string key in symbols.Keys)
			{
				Symbols[key] = symbols[key];
			}
		}
		OnJobCompleted();
		m_job.Trash();
	}

	protected virtual void SetupJob(HardwareJob job)
	{
	}

	protected virtual void DisposeInner()
	{
	}

	protected virtual void OnJobCompleted()
	{
	}

	protected JobExecutor(HardwareService service)
	{
		Service = service;
		Helper = new ClientHelper(service);
		Results = new List<ProgramResult>();
		Errors = new ErrorList();
		Symbols = new Dictionary<string, string>();
		HardwareJobSchedule schedule = new HardwareJobSchedule
		{
			Priority = JobPriority
		};
		ScheduleResult = Service.ScheduleJob(JobName, Label, enableDebugging: false, schedule, out m_job);
		if (!ScheduleResult.Success)
		{
			m_job = null;
		}
	}

	private void OnHardwareEvent(HardwareJob job, DateTime eventTime, string eventMessage)
	{
		if (!eventMessage.StartsWith(">STATUS") && !eventMessage.StartsWith(">IPC"))
		{
			UserEvent(job, eventTime, eventMessage);
		}
	}
}
