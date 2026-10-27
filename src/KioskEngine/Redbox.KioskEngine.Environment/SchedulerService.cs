using System;
using System.Collections.Generic;
using Quartz;
using Quartz.Impl;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class SchedulerService : ISchedulerService
{
	internal sealed class SchedulerEntry : ISchedulerEntry
	{
		public string JobName { get; set; }

		public string Label { get; set; }

		public DateTime StartTime { get; set; }

		public DateTime? EndTime { get; set; }

		public string CronExpression { get; set; }

		public string FunctionName { get; set; }

		public string Program { get; set; }

		public string MisfireInstruction { get; set; }

		public List<object> Parameters { get; set; }

		public SchedulerEntry()
		{
			Parameters = new List<object>();
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (!(obj is SchedulerEntry schedulerEntry))
			{
				return false;
			}
			if (JobName == schedulerEntry.JobName)
			{
				return CronExpression == schedulerEntry.CronExpression;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return JobName.GetHashCode() ^ Program.GetHashCode() ^ CronExpression.GetHashCode();
		}
	}

	internal sealed class SchedulerJob : IJob
	{
		public void Execute(JobExecutionContext context)
		{
			try
			{
				string functionName = context.JobDetail.JobDataMap["function_name"] as string;
				if (string.IsNullOrEmpty(functionName) || !(context.JobDetail.JobDataMap["parameters"] is List<object> list))
				{
					return;
				}
				object[] parameters = new object[list.Count];
				int num = 0;
				foreach (object item in list)
				{
					parameters[num] = item.ToString();
					num++;
				}
				IKernelService kernelService = ServiceLocator.Instance.GetService<IKernelService>();
				if (kernelService != null)
				{
					LogHelper.Instance.Log("Scheduler Service: Execute job '{0}', trigger '{1}', execute function: {2}", context.JobDetail.Name, context.Trigger.Name, functionName);
					ServiceLocator.Instance.GetService<IEngineApplication>()?.ThreadSafeHostUpdate(delegate
					{
						kernelService.ExecuteFunction(functionName, parameters);
					});
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised from SchedulerService+SchedulerJob.Execute.", e);
			}
		}
	}

	private readonly ISchedulerFactory m_schedulerFactory;

	private List<SchedulerEntry> m_entries;

	public static SchedulerService Instance => Singleton<SchedulerService>.Instance;

	internal List<SchedulerEntry> Entries
	{
		get
		{
			if (m_entries != null)
			{
				return m_entries;
			}
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			m_entries = ((service == null) ? new List<SchedulerEntry>() : service.GetValue("Core", "SchedulerEntries", "[]").ToObject<List<SchedulerEntry>>());
			return m_entries ?? new List<SchedulerEntry>();
		}
		set
		{
			m_entries = value;
		}
	}

	public void Clear()
	{
		Entries = new List<SchedulerEntry>();
		SaveEntries();
	}

	public void Reset()
	{
	}

	public void Shutdown()
	{
		m_schedulerFactory.GetScheduler().Shutdown();
	}

	public ErrorList Initialize()
	{
		ErrorList result = new ErrorList();
		ServiceLocator.Instance.AddService(typeof(ISchedulerService), this);
		List<SchedulerEntry> list = new List<SchedulerEntry>();
		foreach (SchedulerEntry entry in Entries)
		{
			if (!ScheduleEntry(entry))
			{
				list.Add(entry);
			}
		}
		list.ForEach(delegate(SchedulerEntry s)
		{
			m_entries.Remove(s);
			LogHelper.Instance.Log("ScheduleEntry failed, Deleting: jobname: {0} label: {1} ", s.JobName, s.Label);
		});
		SaveEntries();
		return result;
	}

	public void Start()
	{
		m_schedulerFactory.GetScheduler().Start();
	}

	public void AddScheduleEntry(string jobname, string label, DateTime startTime, DateTime? endTime, string cronExpression, string functionName, string programName, string misfireInstruction, object[] parms)
	{
		List<SchedulerEntry> entries = Entries;
		if (entries.FindIndex((SchedulerEntry each) => string.Compare(each.JobName, jobname, ignoreCase: true) == 0) == -1)
		{
			SchedulerEntry schedulerEntry = new SchedulerEntry
			{
				JobName = jobname,
				Label = label,
				StartTime = startTime,
				EndTime = endTime,
				CronExpression = cronExpression,
				FunctionName = functionName,
				Program = programName,
				MisfireInstruction = misfireInstruction
			};
			schedulerEntry.Parameters.AddRange(parms);
			entries.Add(schedulerEntry);
			if (ScheduleEntry(schedulerEntry))
			{
				Entries = entries;
				SaveEntries();
			}
			else
			{
				LogHelper.Instance.Log("ScheduleEntry failed, jobname: {0} label: {1} ", jobname, label);
			}
		}
	}

	public bool RemoveScheduleEntry(string jobname)
	{
		SchedulerEntry schedulerEntry = Entries.Find((SchedulerEntry each) => string.Compare(each.JobName, jobname, ignoreCase: true) == 0);
		if (schedulerEntry == null)
		{
			return false;
		}
		Entries.Remove(schedulerEntry);
		m_schedulerFactory.GetScheduler().UnscheduleJob($"{jobname}-trigger", "cron");
		SaveEntries();
		return true;
	}

	public ISchedulerEntry GetScheduleEntry(string jobname)
	{
		return Entries.Find((SchedulerEntry each) => string.Compare(each.JobName, jobname, ignoreCase: true) == 0);
	}

	public List<ISchedulerEntry> GetScheduleEntries()
	{
		List<ISchedulerEntry> list = new List<ISchedulerEntry>();
		foreach (SchedulerEntry entry in Entries)
		{
			list.Add(entry);
		}
		return list;
	}

	internal bool SaveEntries()
	{
		IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
		if (service == null)
		{
			return false;
		}
		service.SetValue("Core", "SchedulerEntries", m_entries.ToJson());
		return true;
	}

	private SchedulerService()
	{
		m_schedulerFactory = new StdSchedulerFactory();
	}

	private bool ScheduleEntry(ISchedulerEntry entry)
	{
		try
		{
			JobDetail jobDetail = new JobDetail(entry.JobName, "kiosk-engine", typeof(SchedulerJob));
			jobDetail.JobDataMap["function_name"] = entry.FunctionName;
			jobDetail.JobDataMap["parameters"] = entry.Parameters;
			CronTrigger cronTrigger = new CronTrigger($"{entry.JobName}-trigger", "cron", entry.JobName, "kiosk-engine", entry.StartTime, entry.EndTime, entry.CronExpression);
			if (entry.MisfireInstruction == "DoNothing")
			{
				cronTrigger.MisfireInstruction = 2;
			}
			else
			{
				cronTrigger.MisfireInstruction = 1;
			}
			m_schedulerFactory.GetScheduler().ScheduleJob(jobDetail, cronTrigger);
		}
		catch (SchedulerException e)
		{
			LogHelper.Instance.Log("Quartz exception, ScheduleEntry failed.", e);
			return false;
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("Unhandled exception, ScheduleEntry failed.", e2);
			return false;
		}
		return true;
	}
}
