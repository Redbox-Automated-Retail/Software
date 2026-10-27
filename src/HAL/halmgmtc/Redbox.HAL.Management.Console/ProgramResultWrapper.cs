using System;
using System.ComponentModel;
using Redbox.HAL.Client;

namespace Redbox.HAL.Management.Console;

internal class ProgramResultWrapper
{
	[Browsable(false)]
	public ProgramResult Result { get; set; }

	public DateTime TimeStamp => Result.TimeStamp;

	public string Code => Result.Code;

	public int Deck => Result.Deck;

	public int Slot => Result.Slot;

	public string Barcode => Result.ItemID.Barcode;

	public string Message => Result.Message;

	public ProgramResultWrapper(ProgramResult result)
	{
		Result = result;
	}
}
