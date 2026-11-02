using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Redbox.HAL.Client;

public sealed class ProgramResult
{
	public Guid ID { get; internal set; }

	public int Deck { get; internal set; }

	public int Slot { get; internal set; }

	// Code and Message are set by the constructor or by FromString.
	public string Code { get; internal set; } = null!;

	public ResultItem? ItemID { get; internal set; }

	public string Message { get; internal set; } = null!;

	public DateTime TimeStamp { get; internal set; }

	public DateTime? ReturnTime { get; internal set; }

	public ResultItem? PreviousItem { get; internal set; }

	public IDictionary<string, object>? MessageData { get; internal set; }

	public ProgramResult(Guid id, DateTime timeStamp, int deck, int slot, string code, string itemId, string message)
	{
		ID = id;
		Deck = deck;
		Slot = slot;
		Code = code;
		ItemID = new ResultItem(itemId);
		Message = message;
		TimeStamp = timeStamp;
	}

	public static ProgramResult? FromString(string value)
	{
		string[] parts = value.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		if (string.Compare(parts[0], "RESULT", ignoreCase: true) == 0 && parts.Length >= 7)
		{
			ProgramResult programResult = new ProgramResult
			{
				ID = new Guid(parts[1]),
				TimeStamp = DateTime.Parse(parts[2]),
				Code = parts[3].Trim()
			};
			string[] location = parts[4].Trim().Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			if (location.Length == 2)
			{
				if (int.TryParse(location[0], out var parsed))
				{
					programResult.Deck = parsed;
				}
				if (int.TryParse(location[1], out parsed))
				{
					programResult.Slot = parsed;
				}
			}
			string itemId = parts[5].Trim();
			if (itemId != "~")
			{
				programResult.ItemID = new ResultItem(itemId);
			}
			programResult.Message = parts[6];
			if (!string.IsNullOrEmpty(programResult.Message) && programResult.Message.StartsWith("{"))
			{
				programResult.MessageData = JsonConvert.DeserializeObject<IDictionary<string, object>>(programResult.Message);
			}
			if (9 == parts.Length)
			{
				string returnTime = parts[7].Trim();
				if (returnTime != "~")
				{
					programResult.ReturnTime = DateTime.Parse(returnTime);
				}
				string previousItem = parts[8].Trim();
				if (previousItem != "~")
				{
					programResult.PreviousItem = new ResultItem(previousItem);
				}
			}
			return programResult;
		}
		return null;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("{0} {1} [{2}", ID, TimeStamp, Code);
		if (Deck != 0 && Slot != 0)
		{
			stringBuilder.AppendFormat("@{0},{1}", Deck, Slot);
		}
		if (ItemID != null)
		{
			stringBuilder.AppendFormat(":{0}", ItemID);
		}
		stringBuilder.AppendFormat("] {0}", Message);
		if (ReturnTime.HasValue)
		{
			stringBuilder.AppendFormat("{0}", ReturnTime);
		}
		if (PreviousItem != null)
		{
			stringBuilder.AppendFormat("{0}", PreviousItem);
		}
		return stringBuilder.ToString();
	}

	private ProgramResult()
	{
	}
}
