using System.Drawing;
using Redbox.Installer.Framework;

namespace Redbox.Installer.Host;

public interface IWizardView
{
	Size Size { get; set; }

	string Text { get; set; }

	Image BannerImage { get; set; }

	Image MarginImage { get; set; }

	PageDefinition SelectedPageDefinition { get; }

	void Close();

	ErrorList ConfigurePages();
}
