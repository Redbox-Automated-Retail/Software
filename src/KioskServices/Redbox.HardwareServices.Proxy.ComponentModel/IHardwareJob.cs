namespace Redbox.HardwareServices.Proxy.ComponentModel;

public interface IHardwareJob
{
	string ID { get; }

	string Status { get; }

	string Label { get; }

	string Priority { get; }

	string ProgramName { get; }

	string StartTime { get; }

	string ExecutionTime { get; }
}
