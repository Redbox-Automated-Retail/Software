using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Redbox.Macros;

[Serializable]
public class Location
{
	public static readonly Location UnknownLocation = new Location();

	public string FileName { get; private set; }

	public int LineNumber { get; private set; }

	public int ColumnNumber { get; private set; }

	public Location(string fileName, int lineNumber, int columnNumber)
	{
		Init(fileName, lineNumber, columnNumber);
	}

	public Location(string fileName)
	{
		Init(fileName, 0, 0);
	}

	private Location()
	{
		Init(null, 0, 0);
	}

	private void Init(string fileName, int lineNumber, int columnNumber)
	{
		if (fileName != null)
		{
			try
			{
				fileName = new Uri(fileName).LocalPath;
			}
			catch
			{
				try
				{
					fileName = Path.GetFullPath(fileName);
				}
				catch (ArgumentException)
				{
				}
			}
		}
		FileName = fileName;
		LineNumber = lineNumber;
		ColumnNumber = columnNumber;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder("");
		if (FileName != null)
		{
			stringBuilder.Append(FileName);
			if (LineNumber != 0)
			{
				stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "({0},{1})", LineNumber, ColumnNumber));
			}
			stringBuilder.Append(":");
		}
		return stringBuilder.ToString();
	}
}
