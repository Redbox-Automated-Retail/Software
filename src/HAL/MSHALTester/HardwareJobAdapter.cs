using System.ComponentModel;
using Redbox.HAL.Client;

namespace Redbox.HAL.MSHALTester;

internal sealed class HardwareJobAdapter : INotifyPropertyChanged
{
	public string ID => Job.ID;

	public string ProgramName => Job.ProgramName;

	public HardwareJobStatus Status => Job.Status;

	public HardwareJobPriority Priority => Job.Priority;

	internal HardwareJob Job { get; private set; }

	internal bool Removable { get; set; }

	public event PropertyChangedEventHandler PropertyChanged;

	public void Merge(HardwareJob job)
	{
		if (Job.Merge(job))
		{
			NotifyPropertyChanged("Status");
			NotifyPropertyChanged("Priority");
		}
	}

	internal HardwareJobAdapter(HardwareJob job)
	{
		Job = job;
	}

	private void NotifyPropertyChanged(string name)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}
