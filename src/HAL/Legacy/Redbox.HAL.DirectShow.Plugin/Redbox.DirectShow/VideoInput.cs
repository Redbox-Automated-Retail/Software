namespace Redbox.DirectShow;

public class VideoInput
{
	public int Index { get; private set; }

	public PhysicalConnectorType Type { get; private set; }

	public static VideoInput Default => new VideoInput(-1, PhysicalConnectorType.Default);

	internal VideoInput(int index, PhysicalConnectorType type)
	{
		Index = index;
		Type = type;
	}
}
