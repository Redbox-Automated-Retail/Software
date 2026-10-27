namespace Redbox.HAL.Component.Model;

public sealed class DecodeResult : IDecodeResult
{
	private int m_count = 1;

	public string Matrix { get; private set; }

	public int Count => m_count;

	public void IncrementCount()
	{
		m_count++;
	}

	public DecodeResult(string matrix)
	{
		Matrix = matrix;
	}
}
