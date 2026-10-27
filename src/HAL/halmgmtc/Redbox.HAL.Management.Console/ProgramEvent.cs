using System;
using System.ComponentModel;

namespace Redbox.HAL.Management.Console;

public class ProgramEvent : INotifyPropertyChanged, IComparable
{
	private DateTime m_eventTime;

	private string m_message;

	public DateTime EventTime
	{
		get
		{
			return m_eventTime;
		}
		set
		{
			m_eventTime = value;
			NotifyPropertyChanged("EventTime");
		}
	}

	public string Message
	{
		get
		{
			return m_message;
		}
		set
		{
			m_message = value;
			NotifyPropertyChanged("Message");
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public ProgramEvent(DateTime time, string msg)
	{
		EventTime = time;
		Message = msg;
	}

	public int CompareTo(object p_Target)
	{
		ProgramEvent programEvent = p_Target as ProgramEvent;
		return EventTime.CompareTo(programEvent.EventTime);
	}

	public bool Equals(ProgramEvent item)
	{
		if (item == null)
		{
			return false;
		}
		if (item.EventTime != EventTime && item.Message != Message)
		{
			return false;
		}
		return true;
	}

	private void NotifyPropertyChanged(string name)
	{
		if (PropertyChanged != null)
		{
			PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}
