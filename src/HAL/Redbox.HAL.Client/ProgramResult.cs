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

	public string Code { get; internal set; }

	public ResultItem ItemID { get; internal set; }

	public string Message { get; internal set; }

	public DateTime TimeStamp { get; internal set; }

	public DateTime? ReturnTime { get; internal set; }

	public ResultItem PreviousItem { get; internal set; }

	public IDictionary<string, object> MessageData { get; internal set; }

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

	public static ProgramResult FromString(string value)
	{
		string[] array = value.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		if (string.Compare(array[0], "RESULT", ignoreCase: true) == 0 && array.Length >= 7)
		{
			ProgramResult programResult = new ProgramResult
			{
				ID = new Guid(array[1]),
				TimeStamp = DateTime.Parse(array[2]),
				Code = array[3].Trim()
			};
			string[] array2 = array[4].Trim().Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			if (array2.Length == 2)
			{
				if (int.TryParse(array2[0], out var result))
				{
					programResult.Deck = result;
				}
				if (int.TryParse(array2[1], out result))
				{
					programResult.Slot = result;
				}
			}
			string text = array[5].Trim();
			if (text != "~")
			{
				programResult.ItemID = new ResultItem(text);
			}
			programResult.Message = array[6];
			if (!string.IsNullOrEmpty(programResult.Message) && programResult.Message.StartsWith("{"))
			{
				programResult.MessageData = JsonConvert.DeserializeObject<IDictionary<string, object>>(programResult.Message);
			}
			if (9 == array.Length)
			{
				string text2 = array[7].Trim();
				if (text2 != "~")
				{
					programResult.ReturnTime = DateTime.Parse(text2);
				}
				string text3 = array[8].Trim();
				if (text3 != "~")
				{
					programResult.PreviousItem = new ResultItem(text3);
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
