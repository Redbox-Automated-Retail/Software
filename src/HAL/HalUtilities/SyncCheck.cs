using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Component.Model;

namespace HALUtilities;

internal class SyncCheck
{
	private class SyncLocation
	{
		internal string GetMatrix;

		internal string PutMatrix;

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
			bool flag = statement.IndexOf("PUT") != -1;
			bool flag2 = statement.IndexOf("GET") != -1;
			if (!flag && !flag2)
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
			int num = statement.IndexOf("ID=");
			string text = statement.Substring(num + "ID=".Length);
			if (flag2)
			{
				GetMatrix = text;
			}
			else if (flag)
			{
				PutMatrix = text;
			}
		}

		private void ExtractDeckSlot(string statement)
		{
			int num = statement.IndexOf("Deck =");
			if (-1 == num)
			{
				throw new ArgumentException($"Unrecognized line '{statement}'");
			}
			int result = -1;
			string s = statement.Substring(num + "Deck =".Length + 1, 1);
			Deck = int.Parse(s);
			num = statement.IndexOf("Slot =");
			string text = statement.Substring(num + "Slot =".Length + 1);
			char[] array = text.ToCharArray();
			for (int i = 0; i < array.Length; i++)
			{
				if (char.IsWhiteSpace(array[i]))
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
		ReadData readData = null;
		List<ReadData> list = new List<ReadData>();
		try
		{
			using StreamWriter streamWriter = new StreamWriter(path);
			streamWriter.WriteLine("Analyzing file '{0}'", file);
			using (TextReader textReader = new StreamReader(file))
			{
				while (true)
				{
					string text = textReader.ReadLine();
					if (text == null)
					{
						break;
					}
					if (text.IndexOf("Sync location") != -1)
					{
						string text2 = "Deck = ";
						string text3 = "Slot = ";
						int num = text.IndexOf(text2);
						if (-1 != num)
						{
							int result = -1;
							int deck = int.Parse(text.Substring(num + text2.Length, 1));
							num = text.IndexOf(text3);
							string text4 = text.Substring(num + text3.Length).TrimEnd();
							if (!int.TryParse(text4, out result))
							{
								streamWriter.WriteLine("Unable to parse {0}", text4);
								Environment.Exit(1);
							}
							readData = new ReadData(deck, result);
							list.Add(readData);
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
							readData.IsEmpty = true;
						}
						else
						{
							if (text.Contains("PickerSensors: 1"))
							{
								continue;
							}
							string value = "data (barcode = ";
							int num2 = text.IndexOf(">> Reponse packet");
							if (-1 != num2 && text.IndexOf(value) != -1)
							{
								if (text.Contains("YES") || text.Contains("yes"))
								{
									readData.SecureReadOk();
								}
								else
								{
									readData.ReadOk();
								}
							}
						}
					}
				}
			}
			list.RemoveAll((ReadData each) => each.IsEmpty);
			Console.WriteLine("There are {0} readable disks.", list.Count);
			List<ReadData> list2 = list.FindAll((ReadData each) => each.SecureFound && !each.Read);
			Console.WriteLine(" {0} read only the secure code.", list2.Count);
			list2.ForEach(delegate(ReadData item)
			{
				Console.WriteLine("   YesOnly: deck = {0} slot = {1}", item.Deck, item.Slot);
			});
			List<ReadData> list3 = list.FindAll((ReadData each) => each.Read && !each.SecureFound);
			Console.WriteLine(" {0} read only the std code.", list3.Count);
			list3.ForEach(delegate(ReadData item)
			{
				Console.WriteLine("   MatrixOnly: deck = {0} slot = {1}", item.Deck, item.Slot);
			});
			int count = list.FindAll((ReadData each) => each.Read && each.SecureFound).Count;
			Console.WriteLine(" {0} read both.", count);
			int count2 = list.FindAll((ReadData each) => !each.Read && !each.SecureFound).Count;
			Console.WriteLine(" {0} were no read.", count2);
			list.FindAll((ReadData each) => !each.Read && !each.SecureFound).ForEach(delegate(ReadData item)
			{
				Console.WriteLine("   Noread: deck = {0} slot = {1}", item.Deck, item.Slot);
			});
		}
		finally
		{
			list.Clear();
		}
	}

	private string ParseItem(string statement, int startIdx, int tokLength)
	{
		string text = statement.Substring(startIdx + tokLength);
		char[] array = text.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (char.IsWhiteSpace(array[i]))
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
					string text = textReader.ReadLine();
					if (text == null)
					{
						break;
					}
					try
					{
						SyncLocation newItem = new SyncLocation(text);
						SyncLocation syncLocation = Locations.Find((SyncLocation each) => each.Deck == newItem.Deck && each.Slot == newItem.Slot);
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
