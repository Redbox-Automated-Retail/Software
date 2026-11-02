using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Component.Model;

namespace HALUtilities;

internal class SyncCheck
{
	private class SyncLocation
	{
		internal string? GetMatrix;

		internal string? PutMatrix;

		private const string deckTok = "Deck =";

		private const string slotTok = "Slot =";

		private const string idTok = "ID=";

		internal int Deck { get; private set; }

		internal int Slot { get; private set; }

		internal bool IsEmpty { get; private set; }

		internal bool IsExcluded { get; private set; }

		internal void Merge(SyncLocation loc)
		{
			if (string.IsNullOrEmpty(GetMatrix))
			{
				GetMatrix = loc.GetMatrix;
			}
			else if (string.IsNullOrEmpty(PutMatrix))
			{
				PutMatrix = loc.PutMatrix;
			}
		}

		internal SyncLocation(string statement)
		{
			if (statement.IndexOf("excluded") != -1)
			{
				IsExcluded = true;
				ExtractDeckSlot(statement);
				return;
			}
			bool isPut = statement.IndexOf("PUT") != -1;
			bool isGet = statement.IndexOf("GET") != -1;
			if (!isPut && !isGet)
			{
				throw new ArgumentException($"Unrecognized line '{statement}'");
			}
			ExtractDeckSlot(statement);
			if (statement.IndexOf("SLOTEMPTY") != -1)
			{
				GetMatrix = (PutMatrix = "EMPTY");
				IsEmpty = true;
				return;
			}
			int index = statement.IndexOf("ID=");
			string text = statement.Substring(index + "ID=".Length);
			if (isGet)
			{
				GetMatrix = text;
			}
			else if (isPut)
			{
				PutMatrix = text;
			}
		}

		private void ExtractDeckSlot(string statement)
		{
			int index = statement.IndexOf("Deck =");
			if (-1 == index)
			{
				throw new ArgumentException($"Unrecognized line '{statement}'");
			}
			int result = -1;
			string s = statement.Substring(index + "Deck =".Length + 1, 1);
			Deck = int.Parse(s);
			index = statement.IndexOf("Slot =");
			string text = statement.Substring(index + "Slot =".Length + 1);
			char[] slotChars = text.ToCharArray();
			for (int i = 0; i < slotChars.Length; i++)
			{
				if (char.IsWhiteSpace(slotChars[i]))
				{
					if (!int.TryParse(text.Substring(0, i + 1), out result))
					{
						throw new ArgumentException($"Unrecognized line '{statement}'");
					}
					Slot = result;
					break;
				}
			}
		}
	}

	private class ReadData
	{
		private int Reads;

		private int SecureReads;

		internal int Deck { get; private set; }

		internal int Slot { get; private set; }

		internal bool Read => Reads > 0;

		internal bool SecureFound => SecureReads > 0;

		internal bool IsEmpty { get; set; }

		internal void ReadOk()
		{
			Reads++;
		}

		internal void SecureReadOk()
		{
			SecureReads++;
		}

		internal ReadData(int deck, int slot)
		{
			Deck = deck;
			Slot = slot;
		}
	}

	private readonly string JobID;

	private readonly List<SyncLocation> Locations = new List<SyncLocation>();

	private readonly IRuntimeService RuntimeService;

	internal SyncCheck(string jobId, IRuntimeService rts)
	{
		JobID = jobId;
		RuntimeService = rts;
	}

	internal void Summarize()
	{
		string path = "c:\\Program Files\\Redbox\\KioskLogs\\Sync";
		string text = Path.Combine(path, $"sync-{JobID}.log");
		if (File.Exists(text))
		{
			ParseFile(text);
			return;
		}
		text = Path.Combine(path, $"sync-locations-{JobID}.log");
		if (File.Exists(text))
		{
			ParseFile(text);
		}
	}

	internal void AnalyzeCortex(string file)
	{
		string path = RuntimeService.RuntimePath($"Cortex-{JobID}-analysis.log");
		if (File.Exists(path))
		{
			try
			{
				RuntimeService.CreateBackup(path, BackupAction.Move);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Unable to backup analysis file: {0}", ex.Message);
			}
		}
		ReadData? readData = null;
		List<ReadData> readDataList = new List<ReadData>();
		try
		{
			using StreamWriter streamWriter = new StreamWriter(path);
			streamWriter.WriteLine("Analyzing file '{0}'", file);
			using (TextReader textReader = new StreamReader(file))
			{
				while (true)
				{
					string? text = textReader.ReadLine();
					if (text == null)
					{
						break;
					}
					if (text.IndexOf("Sync location") != -1)
					{
						string deckMarker = "Deck = ";
						string slotMarker = "Slot = ";
						int index = text.IndexOf(deckMarker);
						if (-1 != index)
						{
							int result = -1;
							int deck = int.Parse(text.Substring(index + deckMarker.Length, 1));
							index = text.IndexOf(slotMarker);
							string slotText = text.Substring(index + slotMarker.Length).TrimEnd();
							if (!int.TryParse(slotText, out result))
							{
								streamWriter.WriteLine("Unable to parse {0}", slotText);
								Environment.Exit(1);
							}
							readData = new ReadData(deck, result);
							readDataList.Add(readData);
						}
					}
					else
					{
						if (text.Contains("ReadDisc"))
						{
							continue;
						}
						if (text.Contains("GET: no disk in picker after pull."))
						{
							readData!.IsEmpty = true;
						}
						else
						{
							if (text.Contains("PickerSensors: 1"))
							{
								continue;
							}
							string value = "data (barcode = ";
							int responseIndex = text.IndexOf(">> Reponse packet");
							if (-1 != responseIndex && text.IndexOf(value) != -1)
							{
								if (text.Contains("YES") || text.Contains("yes"))
								{
									readData!.SecureReadOk();
								}
								else
								{
									readData!.ReadOk();
								}
							}
						}
					}
				}
			}
			readDataList.RemoveAll((ReadData each) => each.IsEmpty);
			Console.WriteLine("There are {0} readable disks.", readDataList.Count);
			List<ReadData> secureOnly = readDataList.FindAll((ReadData each) => each.SecureFound && !each.Read);
			Console.WriteLine(" {0} read only the secure code.", secureOnly.Count);
			secureOnly.ForEach(delegate(ReadData item)
			{
				Console.WriteLine("   YesOnly: deck = {0} slot = {1}", item.Deck, item.Slot);
			});
			List<ReadData> matrixOnly = readDataList.FindAll((ReadData each) => each.Read && !each.SecureFound);
			Console.WriteLine(" {0} read only the std code.", matrixOnly.Count);
			matrixOnly.ForEach(delegate(ReadData item)
			{
				Console.WriteLine("   MatrixOnly: deck = {0} slot = {1}", item.Deck, item.Slot);
			});
			int count = readDataList.FindAll((ReadData each) => each.Read && each.SecureFound).Count;
			Console.WriteLine(" {0} read both.", count);
			int count2 = readDataList.FindAll((ReadData each) => !each.Read && !each.SecureFound).Count;
			Console.WriteLine(" {0} were no read.", count2);
			readDataList.FindAll((ReadData each) => !each.Read && !each.SecureFound).ForEach(delegate(ReadData item)
			{
				Console.WriteLine("   Noread: deck = {0} slot = {1}", item.Deck, item.Slot);
			});
		}
		finally
		{
			readDataList.Clear();
		}
	}

	private string ParseItem(string statement, int startIdx, int tokLength)
	{
		string text = statement.Substring(startIdx + tokLength);
		char[] chars = text.ToCharArray();
		for (int i = 0; i < chars.Length; i++)
		{
			if (char.IsWhiteSpace(chars[i]))
			{
				return text.Substring(0, i + 1);
			}
		}
		return string.Empty;
	}

	private void ParseFile(string file)
	{
		string path = RuntimeService.RuntimePath($"Sync-{JobID}-analysis.log");
		if (File.Exists(path))
		{
			try
			{
				RuntimeService.CreateBackup(path, BackupAction.Move);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Unable to backup analysis file: {0}", ex.Message);
			}
		}
		StreamWriter log = new StreamWriter(path);
		try
		{
			log.WriteLine("Analyzing file '{0}'", file);
			using (TextReader textReader = new StreamReader(file))
			{
				while (true)
				{
					string? text = textReader.ReadLine();
					if (text == null)
					{
						break;
					}
					try
					{
						SyncLocation newItem = new SyncLocation(text);
						SyncLocation? syncLocation = Locations.Find((SyncLocation each) => each.Deck == newItem.Deck && each.Slot == newItem.Slot);
						if (syncLocation == null)
						{
							Locations.Add(newItem);
						}
						else
						{
							syncLocation.Merge(newItem);
						}
					}
					catch (ArgumentException ex2)
					{
						Console.WriteLine("Ignoring line '{0}'", text);
						Console.WriteLine(" Exception: {0}", ex2.Message);
					}
				}
			}
			log.WriteLine("{0} requested locations to sync.", Locations.Count);
			log.WriteLine("  There are {0} empty slots.", Locations.FindAll((SyncLocation each) => each.IsEmpty).Count);
			log.WriteLine("  There are {0} excluded slots.", Locations.FindAll((SyncLocation each) => each.IsExcluded).Count);
			Locations.ForEach(delegate(SyncLocation loc)
			{
				if (loc.PutMatrix != loc.GetMatrix)
				{
					log.WriteLine("The location {0}, {1} went from {2} -> {3}", loc.Deck, loc.Slot, loc.GetMatrix, loc.PutMatrix);
				}
			});
		}
		finally
		{
			if (log != null)
			{
				((IDisposable)log).Dispose();
			}
		}
	}
}
