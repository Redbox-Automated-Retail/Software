using System;

namespace Redbox.HAL.Component.Model;

public interface IDecksService
{
	IDeck QlmDeck { get; }

	IDeck First { get; }

	IDeck Last { get; }

	int DeckCount { get; }

	IDeck GetByNumber(int number);

	IDeck GetFrom(ILocation location);

	bool IsValidLocation(ILocation loc);

	void Add(IDeck deck);

	void ForAllDecksDo(Predicate<IDeck> predicate);

	void ForAllReverseDecksDo(Predicate<IDeck> predicate);
}
