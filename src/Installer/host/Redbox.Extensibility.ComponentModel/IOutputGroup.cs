namespace Redbox.Extensibility.ComponentModel;

public interface IOutputGroup
{
	string Name { get; }

	void Clear();

	void CopyToClipboard();

	void Load(string fileName);

	void Save(string fileName);

	void Append(string message);

	void AppendLine(string message);

	void AppendFormat(string format, params object[] parms);
}
