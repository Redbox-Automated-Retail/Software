namespace Redbox.UpdateManager.ComponentModel;

public interface IConfigFileService
{
	ErrorList ActivateConfigFile(long configFileId);
}
