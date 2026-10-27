namespace Redbox.HAL.Component.Model;

public interface IControlSystemRevision
{
	bool Success { get; }

	IBoardVersionResponse[] Responses { get; }

	string Revision { get; }
}
