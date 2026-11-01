using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Quartz;
using Quartz.Collection;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Util;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.TaskScheduler;

public class Scheduler : ITaskScheduler
{
	private class PersistTask
	{
		public string Name { get; set; }

		public string ScheduleType { get; set; }

		public string Payload { get; set; }

		public string PayloadType { get; set; }

		public string StartOffset { get; set; }

		public string StartDate { get; set; }

		public string StartTime { get; set; }

		public string EndTime { get; set; }

		public string EndDate { get; set; }

		public string CronExpression { get; set; }

		public string RepeatInterval { get; set; }

		public PersistTask()
		{
		}

		public PersistTask(ITask task)
		{
			Name = task.Name;
			ScheduleType = Enum.GetName(typeof(ScheduleType), task.ScheduleType);
			Payload = task.Payload;
			PayloadType = Enum.GetName(typeof(PayloadType), task.PayloadType);
			StartOffset = (task.StartOffset.HasValue ? task.StartOffset.ToString() : null);
			StartTime = (task.StartTime.HasValue ? task.StartTime.Value.ToLongTimeString() : null);
			StartDate = (task.StartTime.HasValue ? task.StartTime.Value.ToLongDateString() : null);
			EndTime = (task.EndTime.HasValue ? task.EndTime.Value.ToLongTimeString() : null);
			EndDate = (task.EndTime.HasValue ? task.EndTime.Value.ToLongDateString() : null);
			CronExpression = task.CronExpression;
			RepeatInterval = (task.RepeatInterval.HasValue ? task.RepeatInterval.Value.ToString() : null);
		}

		public Task Task()
		{
			Task task = new Task();
			ParsePayload(PayloadType, out var parsed);
			TimeSpan? startOffset = null;
			if (TimeSpan.TryParse(StartOffset, out var parsedOffset))
			{
				startOffset = parsedOffset;
			}
			DateTime? startTime;
			if (!string.IsNullOrEmpty(StartDate))
			{
				if (!DateTime.TryParse(StartDate, out var parsedStartDate))
				{
					LogHelper.Instance.Log("Could not parse startDate, skipping task " + Name, LogEntryType.Error);
					return task;
				}
				if (!DateTime.TryParse(StartTime, out var parsedStartTime))
				{
					LogHelper.Instance.Log("Could not parse startDate, skipping task " + Name, LogEntryType.Error);
					return task;
				}
				startTime = new DateTime(parsedStartDate.Year, parsedStartDate.Month, parsedStartDate.Day, parsedStartTime.Hour, parsedStartTime.Minute, parsedStartTime.Second);
			}
			else
			{
				startTime = null;
			}
			DateTime? endTime;
			if (!string.IsNullOrEmpty(EndDate))
			{
				if (!DateTime.TryParse(EndDate, out var parsedEndDate))
				{
					LogHelper.Instance.Log("Could not parse endDate, skipping task " + Name, LogEntryType.Error);
					return task;
				}
				if (!DateTime.TryParse(EndTime, out var parsedEndTime))
				{
					LogHelper.Instance.Log("Could not parse endTime, skipping task " + Name, LogEntryType.Error);
					return task;
				}
				endTime = new DateTime(parsedEndDate.Year, parsedEndDate.Month, parsedEndDate.Day, parsedEndTime.Hour, parsedEndTime.Minute, parsedEndTime.Second);
			}
			else
			{
				endTime = null;
			}
			switch (Enum<Redbox.UpdateManager.ComponentModel.ScheduleType>.ParseIgnoringCase(ScheduleType, Redbox.UpdateManager.ComponentModel.ScheduleType.Unknown))
			{
			case Redbox.UpdateManager.ComponentModel.ScheduleType.Cron:
				task = new Task(Name, Payload, parsed, startOffset, startTime, endTime, CronExpression);
				break;
			case Redbox.UpdateManager.ComponentModel.ScheduleType.Simple:
			{
				if (!TimeSpan.TryParse(RepeatInterval, out var parsedRepeatInterval))
				{
					LogHelper.Instance.Log("Could not parse RepeatInterval, skipping task " + Name, LogEntryType.Error);
				}
				else
				{
					task = new Task(Name, Payload, parsed, startOffset, startTime, endTime, parsedRepeatInterval);
				}
				break;
			}
			}
			return task;
		}
	}

	private const string ScheduleLabel = "config-schedule";

	private const string JobGroupname = "updatemanager";

	private const string DefaultScheduleFileName = "default-schedule.lua";

	private static readonly object FileLock = new object();

#if NETFRAMEWORK
	private readonly ISchedulerFactory m_schedulerFactory = new StdSchedulerFactory();
#else
	// Without properties Quartz 2.3 looks for quartz.config through Quartz.Util.FileUtil, which uses System.Web.HttpContext
	// (TypeLoadException on .NET 10). Given the properties it would have found, it skips the lookup: the app config's
	// quartz section, else Quartz's own embedded defaults (what the kiosks run with: there is no quartz.config).
	private readonly ISchedulerFactory m_schedulerFactory = new StdSchedulerFactory(QuartzDefaultProperties());

	private static System.Collections.Specialized.NameValueCollection QuartzDefaultProperties()
	{
		if (System.Configuration.ConfigurationManager.GetSection("quartz") is System.Collections.Specialized.NameValueCollection section)
		{
			return section;
		}
		System.Collections.Specialized.NameValueCollection properties = new System.Collections.Specialized.NameValueCollection();
		using Stream stream = typeof(StdSchedulerFactory).Assembly.GetManifestResourceStream("Quartz.quartz.config");
		using StreamReader reader = new StreamReader(stream);
		string line;
		while ((line = reader.ReadLine()) != null)
		{
			line = line.Trim();
			int equals = line.IndexOf('=');
			if (line.Length > 0 && !line.StartsWith("#") && equals > 0)
			{
				properties[line.Substring(0, equals).Trim()] = line.Substring(equals + 1).Trim();
			}
		}
		return properties;
	}
#endif

	private List<Task> m_tasks = new List<Task>();

	public static Scheduler Instance => Singleton<Scheduler>.Instance;

	public bool IsRunning { get; private set; }

	public ErrorList ForceTask(string name, out bool success)
	{
		ErrorList errorList = new ErrorList();
		success = false;
		try
		{
			Task task = m_tasks.Find((Task t) => t.Name == name);
			if (task != null)
			{
				task.Execute(null);
				success = true;
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "Unhandled exception while forcing task: " + name, e));
		}
		return errorList;
	}

	public ErrorList ScheduleCronJob(string name, string payload, string payloadTypeString, TimeSpan? startOffset, DateTime? startTime, DateTime? endTime, string cronExpression)
	{
		ErrorList errorList = new ErrorList();
		errorList.AddRange(ParsePayload(payloadTypeString, out var parsed));
		if (!errorList.ContainsError())
		{
			errorList.AddRange(ScheduleCronJob(name, payload, parsed, startOffset, startTime, endTime, cronExpression));
		}
		return errorList;
	}

	public ErrorList ScheduleCronJob(string name, string payload, PayloadType payloadType, TimeSpan? startOffset, DateTime? startTime, DateTime? endTime, string cronExpression)
	{
		ErrorList errors = new ErrorList();
		if (!CronExpression.IsValidExpression(cronExpression))
		{
			errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E428", "The cron expression is not valid.", $"The expression '{cronExpression}' is not valid, please specify a valid cron expression."));
		}
		else
		{
			errors.AddRange(ReadWriteFile(delegate(List<Task> tasks)
			{
				Task item = new Task(name, payload, payloadType, startOffset, startTime, endTime, cronExpression);
				if (!tasks.Any((Task task) => task.Name == name))
				{
					tasks.Add(item);
				}
				else
				{
					errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("S489", "Task with same name already scheduled", "Please use a unique name."));
				}
				return tasks;
			}));
		}
		return errors;
	}

	public ErrorList ScheduleSimpleJob(string name, string payload, string payloadTypeString, TimeSpan? startOffset, DateTime? startTime, DateTime? endTime, TimeSpan repeatInterval)
	{
		ErrorList errorList = new ErrorList();
		errorList.AddRange(ParsePayload(payloadTypeString, out var parsed));
		if (!errorList.ContainsError())
		{
			errorList.AddRange(ScheduleSimpleJob(name, payload, parsed, startOffset, startTime, endTime, repeatInterval));
		}
		return errorList;
	}

	public ErrorList ScheduleSimpleJob(string name, string payload, PayloadType payloadType, TimeSpan? startOffset, DateTime? startTime, DateTime? endTime, TimeSpan repeatInterval)
	{
		ErrorList errors = new ErrorList();
		errors.AddRange(ReadWriteFile(delegate(List<Task> tasks)
		{
			Task item = new Task(name, payload, payloadType, startOffset, startTime, endTime, repeatInterval);
			if (!tasks.Any((Task task) => task.Name == name))
			{
				tasks.Add(item);
			}
			else
			{
				errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("S489", "Task with same name already scheduled", "Please use a unique name."));
			}
			return tasks;
		}));
		return errors;
	}

	public ErrorList Stop()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			LogHelper.Instance.Log("Stopping scheduled events.", LogEntryType.Info);
			IScheduler scheduler = m_schedulerFactory.GetScheduler();
			if (scheduler.IsStarted)
			{
				Quartz.Collection.ISet<JobKey> jobKeys = scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals("updatemanager"));
				scheduler.DeleteJobs((IList<JobKey>)((IEnumerable<JobKey>)jobKeys).ToList());
				Quartz.Collection.ISet<TriggerKey> triggerKeys = scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupEquals("updatemanager"));
				scheduler.UnscheduleJobs((IList<TriggerKey>)((IEnumerable<TriggerKey>)triggerKeys).ToList());
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "Unhandled exception throw in stop.", e));
		}
		finally
		{
			IsRunning = false;
		}
		return errorList;
	}

	public ErrorList Start()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			if (IsRunning)
			{
				return new ErrorList { Redbox.UpdateManager.ComponentModel.Error.NewError("E987", "Scheduler is already running.", "") };
			}
			IsRunning = true;
			IScheduler scheduler = m_schedulerFactory.GetScheduler();
			if (!scheduler.IsStarted)
			{
				scheduler.Start();
			}
			LogHelper.Instance.Log("Starting scheduled events.", LogEntryType.Info);
			errorList.AddRange(ReadFile(out m_tasks));
			foreach (Task task in m_tasks)
			{
				try
				{
					LogHelper.Instance.Log("Scheduling task: " + task.ToString());
					IJobDetail jobDetail = JobBuilder.Create<Task>().WithIdentity(task.Name, "updatemanager").Build();
					((DirtyFlagMap<string, object>)(object)jobDetail.JobDataMap)["Task"] = task;
					DateTime dateTime = (task.StartTime.HasValue ? task.StartTime.Value.ToUniversalTime() : DateTime.UtcNow);
					if (task.StartOffset.HasValue)
					{
						dateTime = dateTime.Add(task.StartOffset.Value);
					}
					if (task.ScheduleType == ScheduleType.Cron)
					{
						ITrigger cronTrigger = CronScheduleTriggerBuilderExtensions.WithCronSchedule(TriggerBuilder.Create().WithIdentity(task.Name, "updatemanager").ForJob(jobDetail), task.CronExpression, (Action<CronScheduleBuilder>)delegate(CronScheduleBuilder x)
						{
							x.WithMisfireHandlingInstructionDoNothing();
						}).StartAt((DateTimeOffset)dateTime).Build();
						scheduler.ScheduleJob(jobDetail, cronTrigger);
					}
					if (task.ScheduleType == ScheduleType.Simple && task.RepeatInterval.HasValue)
					{
						ITrigger simpleTrigger = SimpleScheduleTriggerBuilderExtensions.WithSimpleSchedule(TriggerBuilder.Create().WithIdentity(task.Name, "updatemanager").ForJob(jobDetail), (Action<SimpleScheduleBuilder>)delegate(SimpleScheduleBuilder x)
						{
							x.WithIntervalInSeconds(Convert.ToInt32(task.RepeatInterval.Value.TotalSeconds)).RepeatForever();
						}).StartAt((DateTimeOffset)dateTime).Build();
						scheduler.ScheduleJob(jobDetail, simpleTrigger);
					}
				}
				catch (Exception e)
				{
					errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("SCH899", "Unhandled exception throw when scheduling a task in scheduler.start.", e));
				}
			}
		}
		catch (Exception e2)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E929", "Unhandled exception throw in start.", e2));
		}
		return errorList;
	}

	public ErrorList Restart()
	{
		ErrorList errorList = new ErrorList();
		errorList.AddRange(Stop());
		errorList.AddRange(Start());
		return errorList;
	}

	public ErrorList Delete(string name)
	{
		return ReadWriteFile(delegate(List<Task> tasks)
		{
			tasks.RemoveAll((Task task) => task.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
			return tasks;
		});
	}

	public ErrorList List(out List<ITask> outTasks)
	{
		ErrorList result = ReadFile(out var tasks);
		outTasks = tasks.Cast<ITask>().ToList();
		return result;
	}

	public void Initialize()
	{
		ServiceLocator.Instance.AddService(typeof(ITaskScheduler), this);
		lock (FileLock)
		{
			GetRaw();
		}
	}

	ErrorList ITaskScheduler.Clear()
	{
		return Clear();
	}

	public static ErrorList Clear()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			IDataStoreService dataStoreService = ServiceLocator.Instance.GetService<IDataStoreService>();
			lock (FileLock)
			{
				dataStoreService.Delete("config-schedule");
				RunDefaultSchedule();
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E949", "Unhandled exception when deleting the schedule file.", e));
		}
		return errorList;
	}

	private Scheduler()
	{
	}

	private static string GetRaw()
	{
		IDataStoreService dataStoreService = ServiceLocator.Instance.GetService<IDataStoreService>();
		string raw = dataStoreService.GetRaw("config-schedule");
		if (string.IsNullOrEmpty(raw))
		{
			RunDefaultSchedule();
			raw = dataStoreService.GetRaw("config-schedule");
		}
		else
		{
			try
			{
				raw.ToObject<List<PersistTask>>();
			}
			catch (Exception)
			{
				dataStoreService.Delete("config-schedule");
				RunDefaultSchedule();
				raw = dataStoreService.GetRaw("config-schedule");
			}
		}
		return raw;
	}

	private static ErrorList ReadFile(out List<Task> tasks)
	{
		ErrorList errorList = new ErrorList();
		tasks = new List<Task>();
		try
		{
			string raw;
			lock (FileLock)
			{
				raw = GetRaw();
			}
			if (!string.IsNullOrEmpty(raw))
			{
				tasks = PersistTasksToTasks(raw.ToObject<List<PersistTask>>());
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E990", "Unhandled exception when reading the schedule file.", e));
		}
		return errorList;
	}

	private static ErrorList ReadWriteFile(Func<List<Task>, List<Task>> operation)
	{
		try
		{
			IDataStoreService dataStoreService = ServiceLocator.Instance.GetService<IDataStoreService>();
			lock (FileLock)
			{
				string raw = GetRaw();
				List<PersistTask> persistTasks = new List<PersistTask>();
				if (!string.IsNullOrEmpty(raw))
				{
					persistTasks = raw.ToObject<List<PersistTask>>();
				}
				List<Task> tasks = PersistTasksToTasks(persistTasks);
				tasks = operation(tasks);
				persistTasks = TasksToPersistTasks(tasks);
				dataStoreService.Set("config-schedule", persistTasks);
			}
		}
		catch (Exception e)
		{
			return new ErrorList { Redbox.UpdateManager.ComponentModel.Error.NewError("E909", "Unhandled exception when accessing the schedule file.", e) };
		}
		return new ErrorList();
	}

	private static void RunDefaultSchedule()
	{
		try
		{
			string path = Path.Combine(ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties("${Scripts}"), "default-schedule.lua");
			IDataStoreService dataStoreService = ServiceLocator.Instance.GetService<IDataStoreService>();
			if (!File.Exists(path))
			{
				LogHelper.Instance.Log("RunDefaultSchedule: Script: {0} not found.", path);
				return;
			}
			dataStoreService.Set("config-schedule", new object());
			IKernelService kernelService = ServiceLocator.Instance.GetService<IKernelService>();
			string chunk = File.ReadAllText(path);
			LogHelper.Instance.Log("RunDefaultSchedule: Executing lua script at {0}", path);
			kernelService.ExecuteChunk(chunk);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unknown Error in RunDefaultSchedule", e);
		}
	}

	private static List<Task> PersistTasksToTasks(IEnumerable<PersistTask> persistTasks)
	{
		return persistTasks.Select((PersistTask persistTask) => persistTask.Task()).ToList();
	}

	private static List<PersistTask> TasksToPersistTasks(IEnumerable<Task> tasks)
	{
		return tasks.Select((Task task) => new PersistTask(task)).ToList();
	}

	private static ErrorList ParsePayload(string text, out PayloadType parsed)
	{
		ErrorList errorList = new ErrorList();
		parsed = Enum<PayloadType>.ParseIgnoringCase(text, PayloadType.Unknown);
		if (parsed == PayloadType.Unknown)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("P001", "Cannot Parse PayloadType", "Cannot parse PayloadType " + text));
		}
		return errorList;
	}
}
