using System;

namespace Redbox.HAL.Common.GUI.Functions;

public interface ISessionUser
{
	string User { get; }

	DateTime SessionStart { get; }
}
