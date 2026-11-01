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
		string[] parts = fileName.Split('_');
		string name = parts[0];
		if (parts.Length > 2)
		{
			logger.LogErrorWithSource("Too many underscores for planogram: " + fileName, "Create");
			return null;
		}
		string[] nameParts = parts[1].Split('.');
		if (nameParts.Length > 2)
		{
			logger.LogErrorWithSource("Too many periods for planogram: " + fileName, "Create");
			return null;
		}
		DateTime? dateTime = ParseStartDate(nameParts[0], logger);
		if (!dateTime.HasValue)
		{
			logger.LogErrorWithSource("Invalid start date for planogram: " + fileName, "Create");
			return null;
		}
		string extension = nameParts[1].ToLower();
		if (extension != "rtf" && extension != "xps")
		{
			logger.LogErrorWithSource("Invalid file type for planogram: " + fileName, "Create");
			return null;
		}
		return new Planogram(path, fileName, name, dateTime.Value, extension);
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
		if (!int.TryParse(dateString.Substring(3), out var day))
		{
			return null;
		}
		int year = DateTime.Now.Year;
		if (abbreviatedMonthNumber == 1 && DateTime.Now.Month == 12)
		{
			year++;
		}
		else if (abbreviatedMonthNumber > 10 && DateTime.Now.Month < 3)
		{
			year--;
		}
		if (day < 1 || day > DateTime.DaysInMonth(year, abbreviatedMonthNumber))
		{
			return null;
		}
		return new DateTime(year, abbreviatedMonthNumber, day);
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
