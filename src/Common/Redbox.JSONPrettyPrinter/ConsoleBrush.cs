using System;

namespace Redbox.JSONPrettyPrinter;

[Serializable]
internal struct ConsoleBrush
{
	private readonly ConsoleColor foreground;

	private readonly ConsoleColor background;

	public ConsoleColor Foreground => foreground;

	public ConsoleColor Background => background;

	public static ConsoleBrush Current => new ConsoleBrush(Console.ForegroundColor, Console.BackgroundColor);

	public ConsoleBrush(ConsoleColor color)
		: this(color, color)
	{
	}

	public ConsoleBrush(ConsoleColor foreground, ConsoleColor background)
	{
		this.foreground = foreground;
		this.background = background;
	}

	public ConsoleBrush ResetForeground(ConsoleColor value)
	{
		return new ConsoleBrush(value, Background);
	}

	public ConsoleBrush ResetBackground(ConsoleColor value)
	{
		return new ConsoleBrush(Foreground, value);
	}

	public void Apply()
	{
		Console.BackgroundColor = Background;
		Console.ForegroundColor = Foreground;
	}

	public override string ToString()
	{
		return Foreground.ToString() + " on " + Background;
	}
}
