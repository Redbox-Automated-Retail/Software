using System;

namespace Redbox.Rental.Model.Planogram;

public class Planogram
{
	public string Name { get; set; }

	public DateTime StartDate { get; set; }

	public FileType FileType { get; set; }

	public string Filename { get; set; }

	public string Path { get; set; }
}
