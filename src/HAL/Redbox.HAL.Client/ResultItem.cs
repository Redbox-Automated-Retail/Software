using System;

namespace Redbox.HAL.Client;

public sealed class ResultItem
{
	private string m_rawResult;

	public string RawResult
	{
		get
		{
			return m_rawResult;
		}
		private set
		{
			if (value != null)
			{
				m_rawResult = (string)value.Clone();
			}
		}
	}

	public string Barcode { get; private set; }

	public string Metadata { get; private set; }

	public ResultItem(string item)
	{
		Parse(item);
	}

	public bool IsUnknown()
	{
		if (Barcode == null)
		{
			if (!Metadata.ToUpper().Equals("UNKNOWN"))
			{
				return Metadata.ToUpper().Equals("REDBOX");
			}
			return true;
		}
		return false;
	}

	public bool IsEmpty()
	{
		if (Barcode == null)
		{
			return Metadata.Equals("EMPTY");
		}
		return false;
	}

	public override string ToString()
	{
		return RawResult;
	}

	private bool Parse(string item)
	{
		RawResult = item;
		if (RawResult == null)
		{
			return false;
		}
		string[] array = item.Split('(');
		if (array[0].Equals("UNKNOWN", StringComparison.CurrentCultureIgnoreCase) || array[0].Equals("EMPTY", StringComparison.CurrentCultureIgnoreCase) || array[0].Equals("redbox", StringComparison.CurrentCultureIgnoreCase))
		{
			Metadata = array[0];
		}
		else
		{
			Barcode = array[0].TrimEnd();
			if (array.Length > 1)
			{
				int num = array[1].IndexOf(')');
				if (num != -1)
				{
					Metadata = array[1].Substring(0, num);
				}
				else
				{
					Metadata = array[1];
				}
			}
		}
		return true;
	}
}
