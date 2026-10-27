namespace Redbox.HAL.Component.Model;

public interface IExecutionContext
{
	IFormattedLog ContextLog { get; }

	string ID { get; }

	string ProgramName { get; }

	void CreateResult(string code, string msg, string id);
}
