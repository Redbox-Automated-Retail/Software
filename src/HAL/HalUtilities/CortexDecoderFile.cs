using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Client;

namespace HALUtilities;

internal sealed class CortexDecoderFile
{
	internal readonly List<CortexDecode> Results = new List<CortexDecode>();

	private readonly string Source;

	internal bool Read()
	{
		try
		{
			char[] splitchar = new char[1] { ',' };
			Array.ForEach(File.ReadAllLines(Source), delegate(string line)
			{
				string[] parts = line.Split(splitchar);
				Results.Add(new CortexDecode
				{
					Image = parts[0],
					Matrix = parts[6],
					MatrixCount = Convert.ToInt32(parts[7]),
					SecureCount = Convert.ToInt32(parts[8]),
					Location = new Location
					{
						Deck = Convert.ToInt32(parts[9]),
						Slot = Convert.ToInt32(parts[10])
					}
				});
			});
			return true;
		}
		catch (Exception ex)
		{
			Console.WriteLine("Unhandled exception during read");
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			return false;
		}
	}

	internal CortexDecoderFile(string file)
	{
		if (string.IsNullOrEmpty(file) || !File.Exists(file))
		{
			throw new ArgumentException("file");
		}
		Source = file;
	}
}
