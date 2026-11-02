namespace RBA_SDK_Simulator;

public enum SimulatorCommandType
{
	/// <summary>Magnetic stripe swipe (card source M).</summary>
	Swipe,
	/// <summary>Contact chip insert (card source S); the card stays inserted until <see cref="RemoveCard" />.</summary>
	InsertChip,
	/// <summary>Contactless EMV tap (card source E).</summary>
	Tap,
	/// <summary>Contactless magnetic-stripe-data tap (card source C).</summary>
	TapMsd,
	/// <summary>Removes an inserted chip card (09 message, status R).</summary>
	RemoveCard,
	/// <summary>Customer pressed cancel on the terminal (exit type 2).</summary>
	Cancel,
	/// <summary>Unreadable card (exit type 1).</summary>
	BadRead
}

public class SimulatorCommand
{
	public SimulatorCommandType Type { get; set; }

	public SimulatedCard? Card { get; set; }

	public override string ToString()
	{
		return Card == null ? Type.ToString() : $"{Type} ({Card.Name} {Card.Pan.Substring(0, 6)}...{Card.Pan.Substring(Card.Pan.Length - 4)})";
	}
}
