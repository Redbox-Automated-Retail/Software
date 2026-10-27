namespace Redbox.ControlPanel.UI;

internal struct ProcessDetails
{
	public string Name { get; set; }

	public string Memory { get; set; }

	public string WindowTitle { get; set; }

	public string StartTime { get; set; }

	public int Pid { get; set; }
}
