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
				string[] array = line.Split(splitchar);
				Results.Add(new CortexDecode
				{
					Image = array[0],
					Matrix = array[6],
					MatrixCount = Convert.ToInt32(array[7]),
					SecureCount = Convert.ToInt32(array[8]),
					Location = new Location
					{
						Deck = Convert.ToInt32(array[9]),
						Slot = Convert.ToInt32(array[10])
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
