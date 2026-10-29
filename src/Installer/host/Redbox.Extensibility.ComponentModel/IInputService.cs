namespace Redbox.Extensibility.ComponentModel;

public interface IInputService
{
	bool ShowInputBox(string caption, string label, string value, bool isValuerequired, string errorMessage, string inputMask, out string newValue);
}
