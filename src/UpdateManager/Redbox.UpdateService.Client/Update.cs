using System;
using System.Collections.Generic;

namespace Redbox.UpdateService.Client;

public class Update
{
	// The fields below are filled by the JSON reader or by the sender's object initializer.
	public string Title = null!;

	public Guid Id;

	public List<string> Categories = null!;

	public Uri? MoreInfoUrl;

	public bool IsDownloaded;

	public bool IsInstalled;
}
