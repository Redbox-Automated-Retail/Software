using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace HALUtilities;

internal class EmptyStuckAnalyzer
{
	internal static EmptyStuckAnalyzer Instance => Singleton<EmptyStuckAnalyzer>.Instance;

	internal void AnalyzeFile(string file, bool dumpSlots)
	{
		List<string> list = new List<string>();
		try
		{
			list.Add(file);
			Analyze(list, dumpSlots);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("There was an error during analysis.", e);
		}
		finally
		{
			list.Clear();
		}
	}

	internal void AnalyzeByPattern(bool dumpSlots)
	{
		List<string> list = new List<string>();
		try
		{
			string[] files = Directory.GetFiles("c:\\Program Files\\Redbox\\KioskLogs\\Vend", "vend*");
			list.AddRange(files);
			Analyze(list, dumpSlots);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("There was an error during analysis.", e);
		}
		finally
		{
			list.Clear();
		}
	}

	private void Analyze(List<string> fileList, bool dump)
	{
		List<TypeStat> typeStats = new List<TypeStat>();
		List<LookupFailStat> lookupFailStats = new List<LookupFailStat>();
		List<LocStat> locStats = new List<LocStat>();
		try
		{
			typeStats.Add(new TypeStat(StuckType.Empty));
			typeStats.Add(new TypeStat(StuckType.Stuck));
			typeStats.Add(new TypeStat(StuckType.ExtendFail));
			typeStats.Add(new TypeStat(StuckType.LookupFail));
			int sampleCount = 0;
			int vmzCount = 0;
			string deckMarker = "GET Deck=";
			string slotMarker = "Slot=";
			IRuntimeService runtimeService = ServiceLocator.Instance.GetService<IRuntimeService>();
			string path = runtimeService.RuntimePath("EmptyStuckAnalysis.log");
			if (File.Exists(path))
			{
				try
				{
					runtimeService.CreateBackup(path, BackupAction.Move);
				}
				catch (Exception ex)
				{
					Console.WriteLine("Unable to backup analysis file: {0}", ex.Message);
				}
			}
			StreamWriter log = new StreamWriter(path);
			try
			{
				foreach (string file in fileList)
				{
					using TextReader textReader = new StreamReader(file);
					while (true)
					{
						StuckType type = StuckType.None;
						string line = textReader.ReadLine();
						if (line == null)
						{
							break;
						}
						if (line.IndexOf("ITEMSTUCK") != -1)
						{
							type = StuckType.Stuck;
						}
						else if (line.IndexOf("SLOTEMPTY") != -1)
						{
							type = StuckType.Empty;
						}
						else if (line.IndexOf("GRIPPEREXTEND") != -1)
						{
							type = StuckType.ExtendFail;
						}
						else if (line.IndexOf("LOOKUP") != -1)
						{
							type = StuckType.LookupFail;
							sampleCount++;
							typeStats.Find((TypeStat each) => each.Type == type).Increment();
							if (type == StuckType.LookupFail)
							{
								int index = line.IndexOf("item") + 5;
								int matrixEnd = line.IndexOf("returned") - 1;
								string _bc = line.Substring(index, matrixEnd - index);
								LookupFailStat lookupFailStat = lookupFailStats.Find((LookupFailStat each) => each.Matrix == _bc);
								if (lookupFailStat == null)
								{
									lookupFailStat = new LookupFailStat(_bc);
									lookupFailStats.Add(lookupFailStat);
								}
								lookupFailStat.Increment();
								continue;
							}
						}
						if (type == StuckType.None)
						{
							continue;
						}
						int markerIndex = line.IndexOf(deckMarker);
						if (-1 == markerIndex)
						{
							continue;
						}
						sampleCount++;
						typeStats.Find((TypeStat each) => each.Type == type).Increment();
						string s = line.Substring(markerIndex + deckMarker.Length, 1);
						int deck = int.Parse(s);
						markerIndex = line.IndexOf(slotMarker);
						string slotText = line.Substring(markerIndex + slotMarker.Length);
						char[] slotChars = slotText.ToCharArray();
						for (int charIndex = 0; charIndex < slotChars.Length; charIndex++)
						{
							if (char.IsWhiteSpace(slotChars[charIndex]))
							{
								string slotNumberText = slotText.Substring(0, charIndex + 1);
								if (!int.TryParse(slotNumberText, out var _targetSlot))
								{
									log.WriteLine("Unable to parse {0}", slotNumberText);
									Environment.Exit(1);
								}
								LocStat locStat = locStats.Find((LocStat each) => each.Deck == deck && each.Slot == _targetSlot);
								if (locStat == null)
								{
									locStat = new LocStat(deck, _targetSlot);
									locStats.Add(locStat);
								}
								locStat.Increment();
								if (_targetSlot >= 1 && _targetSlot <= 15)
								{
									vmzCount++;
								}
								break;
							}
						}
					}
				}
				log.WriteLine("Analysis: There were {0} total samples.", sampleCount);
				lookupFailStats.ForEach(delegate(LookupFailStat fail)
				{
					log.WriteLine(" There were {0} lookup failures for matrix {1}", fail.Count, fail.Matrix);
				});
				log.WriteLine(" Breakdown by deck: ");
				int _d;
				for (_d = 1; _d <= 8; _d++)
				{
					List<LocStat> deckStats = locStats.FindAll((LocStat each) => each.Deck == _d);
					log.WriteLine("  Deck {0} had {1} unique failures.", _d, deckStats.Count);
					if (dump)
					{
						deckStats.ForEach(delegate(LocStat _s)
						{
							log.WriteLine("   Slot {0} had {1} failures.", _s.Slot, _s.Count);
						});
					}
				}
				log.WriteLine(" VMZ breakdown");
				decimal pct = (decimal)vmzCount / (decimal)sampleCount;
				pct *= 100m;
				log.WriteLine("    Of those, {0} were in the VMZ ( {1}% )", vmzCount, pct.ToString("00.00"));
				log.WriteLine(" Failure type:");
				typeStats.ForEach(delegate(TypeStat t)
				{
					pct = (decimal)t.Count / (decimal)sampleCount * 100m;
					log.WriteLine("  There were {0} instances of {1} ( {2}% )", t.Count, t.Type.ToString(), pct.ToString("00.00"));
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
		finally
		{
			typeStats.Clear();
			lookupFailStats.Clear();
			locStats.Clear();
		}
	}

	private EmptyStuckAnalyzer()
	{
	}
}
