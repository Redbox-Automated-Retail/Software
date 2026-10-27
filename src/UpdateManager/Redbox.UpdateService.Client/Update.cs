using System;
using System.Collections.Generic;

namespace Redbox.UpdateService.Client;

public class Update
{
	public string Title;

	public Guid Id;

	public List<string> Categories;

	public Uri MoreInfoUrl;

	public bool IsDownloaded;

	public bool IsInstalled;
}
