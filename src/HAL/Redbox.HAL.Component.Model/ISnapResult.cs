using System;

namespace Redbox.HAL.Component.Model;

public interface ISnapResult : IDisposable
{
	bool SnapOk { get; }

	string Path { get; }
}
