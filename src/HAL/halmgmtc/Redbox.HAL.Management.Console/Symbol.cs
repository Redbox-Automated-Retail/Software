using System;
using System.ComponentModel;

namespace Redbox.HAL.Management.Console;

public class Symbol : INotifyPropertyChanged, IComparable
{
	private string m_name;

	private string m_value;

	public string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
			NotifyPropertyChanged("Name");
		}
	}

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

	public Symbol(string Name, string value)
	{
		this.Name = Name;
		Value = value;
	}

	public int CompareTo(object p_Target)
	{
		Symbol symbol = p_Target as Symbol;
		return Name.CompareTo(symbol.Name);
	}

	private void NotifyPropertyChanged(string name)
	{
		if (PropertyChanged != null)
		{
			PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}
