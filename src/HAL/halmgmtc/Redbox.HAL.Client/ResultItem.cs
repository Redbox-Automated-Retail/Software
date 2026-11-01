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
		string[] parts = item.Split('(');
		if (parts[0].Equals("UNKNOWN", StringComparison.CurrentCultureIgnoreCase) || parts[0].Equals("EMPTY", StringComparison.CurrentCultureIgnoreCase) || parts[0].Equals("redbox", StringComparison.CurrentCultureIgnoreCase))
		{
			Metadata = parts[0];
		}
		else
		{
			Barcode = parts[0].TrimEnd();
			if (parts.Length > 1)
			{
				int closeIndex = parts[1].IndexOf(')');
				if (closeIndex != -1)
				{
					Metadata = parts[1].Substring(0, closeIndex);
				}
				else
				{
					Metadata = parts[1];
				}
			}
		}
		return true;
	}
}
