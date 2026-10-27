namespace Redbox.HAL.Component.Model;

public sealed class RedboxHardwareErrorThreshold
{
	private readonly int Threshold;

	private int m_currentCount;

	public bool Exceeded => m_currentCount >= Threshold;

	public int CurrentCount => m_currentCount;

	public int Increment()
	{
		return ++m_currentCount;
	}

	public void Reset()
	{
		m_currentCount = 0;
	}

	public RedboxHardwareErrorThreshold(int threshold, int current)
	{
		Threshold = threshold;
		m_currentCount = current;
	}

	public RedboxHardwareErrorThreshold(int threshold)
		: this(threshold, 0)
	{
	}
}
