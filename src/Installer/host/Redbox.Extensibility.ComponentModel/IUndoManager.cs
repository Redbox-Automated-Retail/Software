using Redbox.Core;

namespace Redbox.Extensibility.ComponentModel;

public interface IUndoManager
{
	bool CanRedo { get; }

	bool CanUndo { get; }

	int LevelsOfUndo { get; set; }

	int GroupingLevel { get; }

	string RedoActionName { get; set; }

	string UndoActionName { get; set; }

	void BeginUndoGroup();

	void EndUndoGroup();

	void RegisterUndo(MethodInvocation invocation);

	void Redo();

	void Undo();

	void RemoveAllActions();
}
