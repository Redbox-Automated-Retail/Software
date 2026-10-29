namespace Redbox.Installer.Framework;

public interface IInstallerPage
{
	bool AllowCancel { get; }

	bool AllowMoveNext { get; }

	bool AllowMovePrevious { get; }

	bool CanMoveNext();

	void Configure(PageDefinition page);

	void Execute(InstallerProject project);

	bool Activated(InstallerProject project);

	void Deactivated(InstallerProject project);
}
