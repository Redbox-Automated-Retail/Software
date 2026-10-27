using System;
using System.Collections.Generic;

namespace Redbox.UpdateManager.ComponentModel;

public interface IRepositoryService : IDisposable
{
	void TrimRepository(string name);

	ErrorList ActivateTo(string name, string hash, out bool deferredMove);

	string FormatStagedFileName(string file);

	void UnpackChangeSet(string name, Guid id, string archive);

	bool ContainsRepository(string name);

	void AddRepository(string name);

	void AddRevisions(string name, List<string> revisions);

	void SetLabel(string name, string revision, string label);

	void RemoveRevisions(string name, List<string> revisions);

	void AddDelta(string name, List<DeltaItem> deltaItemList);

	void AddDelta(string name, string revision, string file, bool isSeed, bool isPlaceHolder, string contentHash, string versionHash);

	string GetLabel(string name, string revision);

	List<string> GetAllRepositories();

	List<IRevLog> GetUnfinishedChanges(string name);

	List<IRevLog> GetPendingChanges(string name);

	List<IRevLog> GetPendingActivations(string name);

	List<IRevLog> GetAllChanges(string name);

	List<IRevLog> GetActivatedChanges(string name);

	string GetHeadRevision(string name);

	string GetActiveRevision(string name);

	string GetStagedRevision(string name);

	string GetActiveLabel(string name);

	List<IRevLog> GetAppliedChanges(string name);

	ErrorList UpdateTo(string name, string targetHash);

	ErrorList RebuildTo(string name, string targetHash);

	ErrorList RebuildToHead(string name);

	ErrorList ActivateToHead(string name, out bool deferredMove);

	ErrorList UpdateToHead(string name);

	ErrorList Verify(string name);

	ErrorList Repair(string name, out bool deferredMove);

	void Reset(string name, out bool defferedDelete);

	void Reset(out bool defferedDelete);

	bool Subscribed(string name);

	void EndableSubscription(string name);

	void DisableSubscription(string name);
}
