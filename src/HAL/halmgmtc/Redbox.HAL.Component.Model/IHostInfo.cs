namespace Redbox.HAL.Component.Model;

public interface IHostInfo
{
	string Product { get; }

	string Version { get; }

	string Copyright { get; }
}
