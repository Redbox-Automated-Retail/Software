using System;

namespace Redbox.HAL.Component.Model;

public interface IConfigurationFileService
{
	IConfigurationFile Get(SystemConfigurations config);

	void DoForEach(Predicate<IConfigurationFile> a);

	void BackupTo(IConfigurationFile f, string targetDir, ErrorList errors);

	void Restore(IConfigurationFile f, string fromDir, ErrorList errors);

	bool Backup(IConfigurationFile f, ErrorList errors);
}
