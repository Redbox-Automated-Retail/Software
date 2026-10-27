using Redbox.HAL.Common.GUI.Functions;

namespace Redbox.HAL.MSHALTester;

internal sealed class TesterSessionImplemtation : ISessionUserService
{
	private readonly ISessionUser CurrentSession;

	public ISessionUser GetCurrentSession()
	{
		return CurrentSession;
	}

	internal TesterSessionImplemtation(ISessionUser user)
	{
		CurrentSession = user;
	}
}
