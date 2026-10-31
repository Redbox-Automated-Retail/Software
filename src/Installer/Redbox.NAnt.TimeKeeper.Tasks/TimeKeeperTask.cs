using NAnt.Core.Attributes;
using Redbox.NAnt.Core;

namespace Redbox.NAnt.TimeKeeper.Tasks;

[TaskName("time-keeper")]
public class TimeKeeperTask : TaskBase
{
	private long _kioskId;

	private string _url;

	private int _timeOut;

	[TaskAttribute("url", Required = true)]
	public string URL
	{
		get
		{
			return _url;
		}
		set
		{
			_url = value;
		}
	}

	[TaskAttribute("timeout", Required = true)]
	public int TimeOut
	{
		get
		{
			return _timeOut;
		}
		set
		{
			_timeOut = value;
		}
	}

	[TaskAttribute("kioskid", Required = true)]
	public long KioskId
	{
		get
		{
			return _kioskId;
		}
		set
		{
			_kioskId = value;
		}
	}

	protected override void ExecuteTask()
	{
		new TimeKeeper(_kioskId, _timeOut, _url, this).Report();
	}
}
