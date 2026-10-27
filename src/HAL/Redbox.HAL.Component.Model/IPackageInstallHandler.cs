using System.IO;

namespace Redbox.HAL.Component.Model;

public interface IPackageInstallHandler
{
	void OnRegister();

	void OnUpgrade(ErrorList errors, TextWriter writer);

	void OnNewInstall(ErrorList errors, TextWriter writer);
}
