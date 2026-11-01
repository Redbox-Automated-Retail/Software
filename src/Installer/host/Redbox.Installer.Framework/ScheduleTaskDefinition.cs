using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml;
using Redbox.Core;
using Redbox.Macros;
using TaskScheduler;

namespace Redbox.Installer.Framework;

[DisplayName("Schedule Task")]
[Element(XPath = "schedule-task")]
public class ScheduleTaskDefinition : Definition
{
	private string m_name;

	private string m_creator;

	private string m_comment;

	private string m_password;

	private string m_arguments;

	private string m_accountName;

	private List<Trigger> m_triggers;

	private string m_applicationName;

	private string m_workingDirectory;

	[ElementProperty(Name = "name", IsRequired = true)]
	public string Name
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_name);
		}
		set
		{
			m_name = value;
		}
	}

	[ElementProperty(Name = "creator")]
	public string Creator
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_creator);
		}
		set
		{
			m_creator = value;
		}
	}

	[ElementProperty(Name = "comment")]
	public string Comment
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_comment);
		}
		set
		{
			m_comment = value;
		}
	}

	[ElementProperty(Name = "password")]
	public string Password
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_password);
		}
		set
		{
			m_password = value;
		}
	}

	[ElementProperty(Name = "flags")]
	public TaskFlags Flags { get; set; }

	[ElementProperty(Name = "arguments")]
	public string Arguments
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_arguments);
		}
		set
		{
			m_arguments = value;
		}
	}

	public string AccountName
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_accountName);
		}
		set
		{
			m_accountName = value;
		}
	}

	[ElementProperty(Name = "max-runtime")]
	public TimeSpan? MaxRuntime { get; set; }

	[ElementProperty(Name = "target-path")]
	public string ApplicationName
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_applicationName);
		}
		set
		{
			m_applicationName = value;
		}
	}

	public List<Trigger> Triggers
	{
		get
		{
			if (m_triggers == null)
			{
				m_triggers = new List<Trigger>();
			}
			return m_triggers;
		}
	}

	[ElementProperty(Name = "working-dir")]
	public string WorkingDirectory
	{
		get
		{
			return MacroContainer.Instance.ExpandRuntimeMacros(m_workingDirectory);
		}
		set
		{
			m_workingDirectory = value;
		}
	}

	[DefaultValue(ProcessPriorityClass.Normal)]
	[ElementProperty(Name = "priority")]
	public ProcessPriorityClass Priority { get; set; }

	[DefaultValue(15)]
	public override int InstallSortOrder { get; set; }

	[DefaultValue(0)]
	public override int UninstallSortOrder { get; set; }

	public ScheduleTaskDefinition()
	{
		Priority = ProcessPriorityClass.Idle;
	}

	public override int GetTaskCount()
	{
		return 1;
	}

	public override string ToExecutionMessage()
	{
		return string.Format("{0} task '{1}' running '{2}'", (InstallerProject.Instance.Mode == InstallMode.Install) ? "Create" : "Remove", Name, ApplicationName);
	}

	protected override void OnInstall(LibraryModule module, Action<Definition> progress, ErrorList errors)
	{
		try
		{
			using ScheduledTasks scheduledTasks = new ScheduledTasks();
			Task task = null;
			try
			{
				task = scheduledTasks.OpenTask(Name) ?? scheduledTasks.CreateTask(Name);
				task.Flags = Flags;
				task.Priority = Priority;
				task.ApplicationName = ApplicationName;
				if (Comment != null)
				{
					task.Comment = Comment;
				}
				if (Creator != null)
				{
					task.Creator = Creator;
				}
				if (AccountName != null)
				{
					if (string.Compare(AccountName, "system", ignoreCase: true) == 0)
					{
						task.SetAccountInformation("", (string)null);
					}
					else
					{
						task.SetAccountInformation(AccountName, Password);
					}
				}
				else
				{
					task.SetAccountInformation("", (string)null);
				}
				if (Arguments != null)
				{
					task.Parameters = Arguments;
				}
				if (MaxRuntime.HasValue)
				{
					task.MaxRunTime = MaxRuntime.Value;
					task.MaxRunTimeLimited = true;
				}
				else
				{
					task.MaxRunTimeLimited = false;
				}
				if (WorkingDirectory != null)
				{
					task.WorkingDirectory = WorkingDirectory;
				}
				task.Triggers.Clear();
				foreach (Trigger trigger in Triggers)
				{
					task.Triggers.Add(trigger);
				}
				task.Save();
			}
			finally
			{
				task?.Dispose();
			}
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I015", "Unable to schedule task due to unhandled exception; see details for exception.", ex.ToString()));
		}
		base.OnInstall(module, progress, errors);
	}

	protected override void OnUninstall(Action<Definition> progress, ErrorList errors)
	{
		try
		{
			using ScheduledTasks scheduledTasks = new ScheduledTasks();
			scheduledTasks.DeleteTask(Name);
		}
		catch (Exception ex)
		{
			errors.Add(Error.NewError("I015", "Unable to remove schedule task due to unhandled exception; see details for exception.", ex.ToString()));
		}
		base.OnUninstall(progress, errors);
	}

	protected override void OnCustomParse(XmlNode node, IConsole console, ErrorList errors)
	{
		XmlNodeList triggerNodes = node.SelectNodes("triggers/*");
		if (triggerNodes == null)
		{
			return;
		}
		foreach (XmlNode triggerNode in triggerNodes)
		{
			Trigger trigger = null;
			switch (triggerNode.Name)
			{
			case "idle-trigger":
				trigger = new OnIdleTrigger();
				break;
			case "daily-trigger":
			{
				short? dailyInterval = null;
				ParseHourAndMinutes(triggerNode, out var hour2, out var minutes2);
				if (ParseInterval(triggerNode, out var interval2))
				{
					dailyInterval = interval2;
				}
				trigger = (((int?)dailyInterval).HasValue ? new DailyTrigger(hour2, minutes2, dailyInterval.Value) : new DailyTrigger(hour2, minutes2));
				break;
			}
			case "monthly-trigger":
			{
				List<int> days = new List<int>();
				MonthsOfTheYear? monthsOfTheYear = null;
				ParseHourAndMinutes(triggerNode, out var hour4, out var minutes4);
				if (!ParseDaysOfYear(triggerNode, days))
				{
					days.Add(1);
				}
				if (ParseMonthsOfTheYear(triggerNode, out var monthsOfYear2))
				{
					monthsOfTheYear = monthsOfYear2;
				}
				trigger = (monthsOfTheYear.HasValue ? new MonthlyTrigger(hour4, minutes4, days.ToArray(), monthsOfTheYear.Value) : new MonthlyTrigger(hour4, minutes4, days.ToArray()));
				break;
			}
			case "monthly-dow-trigger":
			{
				WhichWeek whichWeeks = WhichWeek.FirstWeek;
				ParseHourAndMinutes(triggerNode, out var hour3, out var minutes3);
				ParseDaysOfWeeK(triggerNode, out var daysOfWeek2);
				if (triggerNode.Attributes["target-week"] != null)
				{
					whichWeeks = Enum<WhichWeek>.ParseIgnoringCase(MacroContainer.Instance.ExpandConstantMacros(triggerNode.Attributes["target-week"].Value), WhichWeek.FirstWeek);
				}
				trigger = (ParseMonthsOfTheYear(triggerNode, out var monthsOfYear) ? new MonthlyDOWTrigger(hour3, minutes3, daysOfWeek2, whichWeeks, monthsOfYear) : new MonthlyDOWTrigger(hour3, minutes3, daysOfWeek2, whichWeeks));
				break;
			}
			case "run-once-trigger":
				trigger = new RunOnceTrigger(triggerNode.GetExpandedAttributeValue("begin-date", DateTime.Now));
				break;
			case "weekly-trigger":
			{
				short? weeklyInterval = null;
				ParseHourAndMinutes(triggerNode, out var hour, out var minutes);
				if (ParseInterval(triggerNode, out var interval))
				{
					weeklyInterval = interval;
				}
				ParseDaysOfWeeK(triggerNode, out var daysOfWeek);
				trigger = (((int?)weeklyInterval).HasValue ? new WeeklyTrigger(hour, minutes, daysOfWeek, weeklyInterval.Value) : new WeeklyTrigger(hour, minutes, daysOfWeek));
				break;
			}
			case "logon-trigger":
				trigger = new OnLogonTrigger();
				break;
			case "system-startup-trigger":
				trigger = new OnSystemStartTrigger();
				break;
			default:
				errors.Add(Error.NewError("C004", "The specified trigger node " + triggerNode.Name + " is not recognized.", "Specify a valid trigger node."));
				break;
			}
			if (trigger != null)
			{
				ParseCommonTriggerProperties(trigger, triggerNode, errors);
				if (errors.Count > 0)
				{
					continue;
				}
			}
			Triggers.Add(trigger);
		}
	}

	private static void ParseCommonTriggerProperties(Trigger trigger, XmlNode node, ICollection<Error> errors)
	{
		DateTime? expandedAttributeValue = node.GetExpandedAttributeValue<DateTime?>("begin-date");
		if (!expandedAttributeValue.HasValue)
		{
			errors.Add(Error.NewError("C004", "The begin-date attribute is required and the value must be properly formatted.", "Specify a valid begin-date value."));
			return;
		}
		trigger.BeginDate = expandedAttributeValue.Value;
		expandedAttributeValue = node.GetExpandedAttributeValue<DateTime?>("end-date");
		if (expandedAttributeValue.HasValue)
		{
			trigger.EndDate = expandedAttributeValue.Value;
		}
		bool? expandedAttributeValue2 = node.GetExpandedAttributeValue<bool?>("disabled");
		if (expandedAttributeValue2.HasValue)
		{
			trigger.Disabled = expandedAttributeValue2.Value;
		}
		expandedAttributeValue2 = node.GetExpandedAttributeValue<bool?>("kill-at-duration-end");
		if (expandedAttributeValue2.HasValue)
		{
			trigger.KillAtDurationEnd = expandedAttributeValue2.Value;
		}
		trigger.DurationMinutes = node.GetExpandedAttributeValue<int>("duration-minutes");
		trigger.IntervalMinutes = node.GetExpandedAttributeValue<int>("interval-minutes");
	}

	private static void ParseHourAndMinutes(XmlNode node, out short hour, out short minutes)
	{
		hour = node.GetExpandedAttributeValue<short>("hour");
		minutes = node.GetExpandedAttributeValue<short>("minutes");
	}

	private static bool ParseInterval(XmlNode node, out short interval)
	{
		short? expandedAttributeValue = node.GetExpandedAttributeValue<short?>("interval");
		if (((int?)expandedAttributeValue).HasValue)
		{
			interval = expandedAttributeValue.Value;
			return true;
		}
		interval = 0;
		return false;
	}

	private static bool ParseDaysOfYear(XmlNode node, ICollection<int> daysOfYear)
	{
		if (node.Attributes["days-of-year"] != null)
		{
			foreach (string text in MacroContainer.Instance.ExpandConstantMacros(node.Attributes["days-of-year"].Value).Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
			{
				if (int.TryParse(text.Trim(), out var result) && !daysOfYear.Contains(result))
				{
					daysOfYear.Add(result);
				}
			}
		}
		return daysOfYear.Count > 0;
	}

	private static bool ParseMonthsOfTheYear(XmlNode node, out MonthsOfTheYear monthsOfYear)
	{
		monthsOfYear = (MonthsOfTheYear)0;
		if (node.Attributes["months-of-year"] == null)
		{
			return false;
		}
		object converted = ConversionHelper.ChangeType(MacroContainer.Instance.ExpandConstantMacros(node.Attributes["months-of-year"].Value), typeof(MonthsOfTheYear));
		monthsOfYear = (MonthsOfTheYear)Convert.ToInt16(converted);
		return true;
	}

	private static void ParseDaysOfWeeK(XmlNode node, out DaysOfTheWeek daysOfWeek)
	{
		daysOfWeek = (DaysOfTheWeek)0;
		if (node.Attributes["days-of-week"] != null)
		{
			object converted = ConversionHelper.ChangeType(MacroContainer.Instance.ExpandConstantMacros(node.Attributes["days-of-week"].Value), typeof(DaysOfTheWeek));
			daysOfWeek = (DaysOfTheWeek)Convert.ToInt16(converted);
		}
	}
}
