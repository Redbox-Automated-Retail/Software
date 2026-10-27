using System.IO;

namespace Redbox.HAL.Component.Model;

public interface IRuntimeService
{
	string KioskId { get; }

	string DataPath { get; }

	string AssemblyDirectory { get; }

	string InstallRoot { get; }

	int PageSize { get; }

	string ExpandRuntimeMacros(string value);

	string ExpandConstantMacros(string value);

	string RuntimePath(string fileName);

	string InstallPath(string subfolder);

	string CreateBackup(string path);

	string CreateBackup(string path, BackupAction action);

	string GenerateUniqueFile(string suffix);

	byte[] ReadToBuffer(string fileName);

	bool SafeCopy(TextWriter writer, string source, string dest);

	bool SafeMove(TextWriter writer, string source, string dest);

	bool SafeDelete(string path);

	Platform GetPlatform();
}
