using System;
using Redbox.HAL.Common.GUI.Functions;

namespace Redbox.HAL.MSHALTester;

internal sealed class TesterSession : ISessionUser
{
	public string User { get; private set; }

	public DateTime SessionStart { get; private set; }

	internal TesterSession(string uname)
	{
		SessionStart = DateTime.Now;
		User = uname;
	}
}
