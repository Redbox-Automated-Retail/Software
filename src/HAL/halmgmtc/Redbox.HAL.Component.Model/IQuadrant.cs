namespace Redbox.HAL.Component.Model;

public interface IQuadrant
{
	int Offset { get; }

	bool IsExcluded { get; }

	IRange<int> Slots { get; }
}
