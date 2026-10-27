using System;
using System.ComponentModel;

namespace Redbox.HAL.Management.Console;

public class StringWrapper : INotifyPropertyChanged, IComparable
{
	private string m_value;

	public string Value
	{
		get
		{
			return m_value;
		}
		set
		{
			m_value = value;
			NotifyPropertyChanged("Value");
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public StringWrapper(string value)
	{
		Value = value;
	}

	public int CompareTo(object p_Target)
	{
		StringWrapper stringWrapper = p_Target as StringWrapper;
		return Value.CompareTo(stringWrapper.Value);
	}

	private void NotifyPropertyChanged(string name)
	{
		if (PropertyChanged != null)
		{
			PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}
