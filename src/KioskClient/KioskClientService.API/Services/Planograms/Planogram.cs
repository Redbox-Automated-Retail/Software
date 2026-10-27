using System;
using System.IO;
using KioskClientService.API.App;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Planograms;

public class Planogram
{
	public string Name { get; set; }

	public DateTime StartDate { get; set; }

	public string FileType { get; set; }

	public string Filename { get; set; }

	public string Path { get; set; }

	public static Planogram Create<T>(string fullFilename, ILogger<T> logger)
	{
		string fileName = System.IO.Path.GetFileName(fullFilename);
		string path = fullFilename.Substring(0, fullFilename.IndexOf(fileName));
		if (string.IsNullOrEmpty(fileName) || fileName.IndexOf('_') < 0 || fileName.IndexOf('.') < 0)
		{
			logger.LogErrorWithSource("Invalid filename for planogram: " + fileName, "Create");
			return null;
		}
		string[] array = fileName.Split('_');
		string name = array[0];
		if (array.Length > 2)
		{
			logger.LogErrorWithSource("Too many underscores for planogram: " + fileName, "Create");
			return null;
		}
		string[] array2 = array[1].Split('.');
		if (array2.Length > 2)
		{
			logger.LogErrorWithSource("Too many periods for planogram: " + fileName, "Create");
			return null;
		}
		DateTime? dateTime = ParseStartDate(array2[0], logger);
		if (!dateTime.HasValue)
		{
			logger.LogErrorWithSource("Invalid start date for planogram: " + fileName, "Create");
			return null;
		}
		string text = array2[1].ToLower();
		if (text != "rtf" && text != "xps")
		{
			logger.LogErrorWithSource("Invalid file type for planogram: " + fileName, "Create");
			return null;
		}
		return new Planogram(path, fileName, name, dateTime.Value, text);
	}

	private static DateTime? ParseStartDate<T>(string dateString, ILogger<T> logger)
	{
		if (dateString.Length < 4 || dateString.Length > 5)
		{
			return null;
		}
		int abbreviatedMonthNumber = dateString.Substring(0, 3).GetAbbreviatedMonthNumber();
		if (abbreviatedMonthNumber < 1)
		{
			return null;
		}
		if (!int.TryParse(dateString.Substring(3), out var result))
		{
			return null;
		}
		int num = DateTime.Now.Year;
		if (abbreviatedMonthNumber == 1 && DateTime.Now.Month == 12)
		{
			num++;
		}
		else if (abbreviatedMonthNumber > 10 && DateTime.Now.Month < 3)
		{
			num--;
		}
		if (result < 1 || result > DateTime.DaysInMonth(num, abbreviatedMonthNumber))
		{
			return null;
		}
		return new DateTime(num, abbreviatedMonthNumber, result);
	}

	private Planogram(string path, string filename, string name, DateTime startDate, string type)
	{
		Path = path;
		Filename = filename;
		Name = name;
		StartDate = startDate;
		FileType = type;
	}
}
