namespace Redbox.Extensibility.ComponentModel;

public interface IProgressService
{
	ICommandBarItem ProgressItem { get; set; }

	void ResetProgress();

	void IncrementProgress();

	void EndProgressSession();

	void BeginProgressSession(int maximum);
}
