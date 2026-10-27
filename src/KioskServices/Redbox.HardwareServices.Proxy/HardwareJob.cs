using Redbox.HardwareServices.Proxy.ComponentModel;

namespace Redbox.HardwareServices.Proxy;

internal class HardwareJob : IHardwareJob
{
	public string ID { get; internal set; }

	public string Status { get; internal set; }

	public string Label { get; internal set; }

	public string Priority { get; internal set; }

	public string ProgramName { get; internal set; }

	public string StartTime { get; internal set; }

	public string ExecutionTime { get; internal set; }
}
