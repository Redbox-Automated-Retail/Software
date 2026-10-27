using System.Collections.Generic;

namespace RBA_SDK_Simulator;

/// <summary>State of the simulated Ingenico iUC285: identity, configuration, variables and files.</summary>
public interface ISimulatorDataProvider
{
	/// <summary>Responses to 07 Unit Data, keyed by the P07_RES_* parameter name.</summary>
	IDictionary<string, string> UnitData { get; }

	/// <summary>Responses to 08 Health Stat, keyed by the P08_RES_* parameter name.</summary>
	IDictionary<string, string> HealthData { get; }

	/// <summary>When true, the unit reports a tamper (manufacturing serial number "ALERT").</summary>
	bool Tampered { get; set; }

	/// <summary>When false, Connect fails as if no terminal were attached.</summary>
	bool DeviceAttached { get; set; }

	/// <summary>Card that is used when a command does not specify one.</summary>
	SimulatedCard DefaultCard { get; set; }

	/// <summary>Milliseconds between a request and an unsolicited terminal message.</summary>
	int ResponseDelay { get; set; }

	string ReadConfig(string group, string index);

	void WriteConfig(string group, string index, string value);

	IDictionary<string, string> Configuration { get; }

	string GetVariable(string id);

	void SetVariable(string id, string value);

	IDictionary<string, string> Variables { get; }

	/// <summary>Files on the terminal (written with 62, read with 65).</summary>
	IDictionary<string, string> Files { get; }

	void CountReboot();

	void CountSwipe();
}
