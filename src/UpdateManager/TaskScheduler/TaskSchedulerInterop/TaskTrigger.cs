namespace TaskSchedulerInterop;

public struct TaskTrigger
{
	public ushort TriggerSize;

	public ushort Reserved1;

	public ushort BeginYear;

	public ushort BeginMonth;

	public ushort BeginDay;

	public ushort EndYear;

	public ushort EndMonth;

	public ushort EndDay;

	public ushort StartHour;

	public ushort StartMinute;

	public uint MinutesDuration;

	public uint MinutesInterval;

	public uint Flags;

	public TaskTriggerType Type;

	public TriggerTypeData Data;

	public ushort Reserved2;

	public ushort RandomMinutesInterval;
}
