using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Command;

[Command("repository")]
internal class RepositoryCommand
{
	[CommandForm(Name = "trim-repository")]
	[Usage("repository trim-repository name:")]
	public void TrimRepository(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		ServiceLocator.Instance.GetService<IRepositoryService>().TrimRepository(name);
	}

	[CommandForm(Name = "disable-subscription")]
	[Usage("repository disable-subscription name:")]
	public void DisableSuscription(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		ServiceLocator.Instance.GetService<IRepositoryService>().DisableSubscription(name);
	}

	[CommandForm(Name = "enable-subscription")]
	[Usage("repository enable-subscription name:")]
	public void EnableSuscription(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		ServiceLocator.Instance.GetService<IRepositoryService>().EndableSubscription(name);
	}

	[CommandForm(Name = "subscribed")]
	[Usage("repository subscribed name:")]
	public void Subscribed(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		bool flag = ServiceLocator.Instance.GetService<IRepositoryService>().Subscribed(name);
		context.Messages.Add(flag.ToJson());
	}

	[CommandForm(Name = "activate-to")]
	[Usage("repository activate-to name: hash:")]
	public void ActivateTo(CommandContext context, [CommandKeyValue(IsRequired = true)] string name, [CommandKeyValue(IsRequired = true)] string hash)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = ServiceLocator.Instance.GetService<IRepositoryService>().ActivateTo(name, hash, out var deferredMove);
		if (errorList.ContainsError())
		{
			context.Errors.AddRange(errorList.ToIPCErrors());
		}
		else
		{
			context.Messages.Add(deferredMove.ToJson());
		}
	}

	[CommandForm(Name = "format-stage-file-name")]
	[Usage("repository format-stage-file name:")]
	public void FormatStageFileName(CommandContext context, [CommandKeyValue(IsRequired = true)] string file)
	{
		string instance = ServiceLocator.Instance.GetService<IRepositoryService>().FormatStagedFileName(file.Replace("\\\\", "\\"));
		context.Messages.Add(instance.ToJson());
	}

	[CommandForm(Name = "unpack-change-set")]
	[Usage("repository unpack-change-set name: id: archive:")]
	public void UnpackChangeset(CommandContext context, [CommandKeyValue(IsRequired = true)] string name, [CommandKeyValue(IsRequired = true)] string id, [CommandKeyValue(IsRequired = true)] string archive)
	{
		ServiceLocator.Instance.GetService<IRepositoryService>().UnpackChangeSet(name, new Guid(id), archive.Replace("\\\\", "\\"));
	}

	[CommandForm(Name = "contains-repository")]
	[Usage("repository contains-repository name:")]
	public void ContainsRepository(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		bool flag = ServiceLocator.Instance.GetService<IRepositoryService>().ContainsRepository(name);
		context.Messages.Add(flag.ToJson());
	}

	[CommandForm(Name = "add-repository")]
	[Usage("repository add-repository name:")]
	public void AddRepository(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		ServiceLocator.Instance.GetService<IRepositoryService>().AddRepository(name);
	}

	[CommandForm(Name = "add-revisions")]
	[Usage("repository add-revisions name: revisions:")]
	public void AddRevisions(CommandContext context, [CommandKeyValue(IsRequired = true)] string name, [CommandKeyValue(IsRequired = true)] string revisions)
	{
		ServiceLocator.Instance.GetService<IRepositoryService>().AddRevisions(name, revisions.ToObject<List<string>>());
	}

	[CommandForm(Name = "remove-revisions")]
	[Usage("repository remove-revisions name: revisions:")]
	public void RemoveRepository(CommandContext context, [CommandKeyValue(IsRequired = true)] string name, [CommandKeyValue(IsRequired = true)] string revisions)
	{
		ServiceLocator.Instance.GetService<IRepositoryService>().RemoveRevisions(name, revisions.ToObject<List<string>>());
	}

	[CommandForm(Name = "add-delta")]
	[Usage("repository add-delta name: revision: file: isSeed: isPlaceHolder: contentHash: versionHash:")]
	public void AddDelta(CommandContext context, [CommandKeyValue(IsRequired = true)] string name, [CommandKeyValue(IsRequired = true)] string revision, [CommandKeyValue(IsRequired = true)] string file, [CommandKeyValue(IsRequired = true)] bool isSeed, [CommandKeyValue(IsRequired = true)] bool isPlaceHolder, [CommandKeyValue(IsRequired = true)] string contentHash, [CommandKeyValue(IsRequired = true)] string versionHash)
	{
		ServiceLocator.Instance.GetService<IRepositoryService>().AddDelta(name, revision, file, isSeed, isPlaceHolder, contentHash, versionHash);
	}

	[CommandForm(Name = "list-repositories")]
	[Usage("repository list-repositories name:")]
	public void ListRepositories(CommandContext context)
	{
		List<string> allRepositories = ServiceLocator.Instance.GetService<IRepositoryService>().GetAllRepositories();
		context.Messages.Add(allRepositories.ToJson());
	}

	[CommandForm(Name = "list-unfinished-changes")]
	[Usage("repository list-unfinished-changes name:")]
	public void ListUnfinishedChanges(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		List<IRevLog> unfinishedChanges = ServiceLocator.Instance.GetService<IRepositoryService>().GetUnfinishedChanges(name);
		context.Messages.Add(unfinishedChanges.ToJson());
	}

	[CommandForm(Name = "list-pending-changes")]
	[Usage("repository list-pending-changes name:")]
	public void ListPendingChanges(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		List<IRevLog> pendingChanges = ServiceLocator.Instance.GetService<IRepositoryService>().GetPendingChanges(name);
		context.Messages.Add(pendingChanges.ToJson());
	}

	[CommandForm(Name = "list-pending-activations")]
	[Usage("repository list-pending-activations name:")]
	public void ListPendingActivations(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		List<IRevLog> pendingActivations = ServiceLocator.Instance.GetService<IRepositoryService>().GetPendingActivations(name);
		context.Messages.Add(pendingActivations.ToJson());
	}

	[CommandForm(Name = "list-all-changes")]
	[Usage("repository list-all-changes name:")]
	public void ListAllChanges(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		List<IRevLog> allChanges = ServiceLocator.Instance.GetService<IRepositoryService>().GetAllChanges(name);
		context.Messages.Add(allChanges.ToJson());
	}

	[CommandForm(Name = "list-activated-changes")]
	[Usage("repository list-activated-changes name:")]
	public void ListActivatedChanges(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		List<IRevLog> activatedChanges = ServiceLocator.Instance.GetService<IRepositoryService>().GetActivatedChanges(name);
		context.Messages.Add(activatedChanges.ToJson());
	}

	[CommandForm(Name = "get-head-hash")]
	[Usage("repository get-head-hash name:")]
	public void GetHeadRevision(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		string headRevision = ServiceLocator.Instance.GetService<IRepositoryService>().GetHeadRevision(name);
		context.Messages.Add(headRevision.ToJson());
	}

	[CommandForm(Name = "get-active-revision")]
	[Usage("repository get-active-revision name:")]
	public void GetAcitveRevision(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		string activeRevision = ServiceLocator.Instance.GetService<IRepositoryService>().GetActiveRevision(name);
		context.Messages.Add(activeRevision.ToJson());
	}

	[CommandForm(Name = "get-active-label")]
	[Usage("repository get-active-label name:")]
	public void GetAcitveLabel(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		string activeLabel = ServiceLocator.Instance.GetService<IRepositoryService>().GetActiveLabel(name);
		context.Messages.Add(activeLabel.ToJson());
	}

	[CommandForm(Name = "get-staged-revision")]
	[Usage("repository get-staged-revision name:")]
	public void GetStagedRevision(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		string stagedRevision = ServiceLocator.Instance.GetService<IRepositoryService>().GetStagedRevision(name);
		context.Messages.Add(stagedRevision.ToJson());
	}

	[CommandForm(Name = "list-applied-changes")]
	[Usage("repository list-applied-changes name:")]
	public void ListAppliedChanges(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		List<IRevLog> appliedChanges = ServiceLocator.Instance.GetService<IRepositoryService>().GetAppliedChanges(name);
		context.Messages.Add(appliedChanges.ToJson());
	}

	[CommandForm(Name = "update-to")]
	[Usage("repository update-to name: hash:")]
	public void UpdateTo(CommandContext context, [CommandKeyValue(IsRequired = true)] string name, [CommandKeyValue(IsRequired = true)] string hash)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = ServiceLocator.Instance.GetService<IRepositoryService>().UpdateTo(name, hash);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				context.Errors.Add(e.IsWarning ? Redbox.IPC.Framework.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.IPC.Framework.Error.NewError(e.Code, e.Description, e.Details));
			});
		}
	}

	[CommandForm(Name = "rebuild-to")]
	[Usage("repository rebuild-to name: hash:")]
	public void RebuildToHash(CommandContext context, [CommandKeyValue(IsRequired = true)] string name, [CommandKeyValue(IsRequired = true)] string hash)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = ServiceLocator.Instance.GetService<IRepositoryService>().RebuildTo(name, hash);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				context.Errors.Add(e.IsWarning ? Redbox.IPC.Framework.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.IPC.Framework.Error.NewError(e.Code, e.Description, e.Details));
			});
		}
	}

	[CommandForm(Name = "update-to-head")]
	[Usage("repository update-to-head name:")]
	public void UpdateTo(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = ServiceLocator.Instance.GetService<IRepositoryService>().UpdateToHead(name);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				context.Errors.Add(e.IsWarning ? Redbox.IPC.Framework.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.IPC.Framework.Error.NewError(e.Code, e.Description, e.Details));
			});
		}
	}

	[CommandForm(Name = "rebuild-to-head")]
	[Usage("repository rebuild-to-head name:")]
	public void RebuildTo(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = ServiceLocator.Instance.GetService<IRepositoryService>().RebuildToHead(name);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				context.Errors.Add(e.IsWarning ? Redbox.IPC.Framework.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.IPC.Framework.Error.NewError(e.Code, e.Description, e.Details));
			});
		}
	}

	[CommandForm(Name = "activate-to-head")]
	[Usage("repository activate-to-head name:")]
	public void ActivateToHead(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = ServiceLocator.Instance.GetService<IRepositoryService>().ActivateToHead(name, out var deferredMove);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				context.Errors.Add(e.IsWarning ? Redbox.IPC.Framework.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.IPC.Framework.Error.NewError(e.Code, e.Description, e.Details));
			});
		}
		else
		{
			context.Messages.Add(deferredMove.ToJson());
		}
	}

	[CommandForm(Name = "verify")]
	[Usage("repository verify name:")]
	public void Verify(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = ServiceLocator.Instance.GetService<IRepositoryService>().Verify(name);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				context.Errors.Add(e.IsWarning ? Redbox.IPC.Framework.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.IPC.Framework.Error.NewError(e.Code, e.Description, e.Details));
			});
		}
	}

	[CommandForm(Name = "repair")]
	[Usage("repository repair name:")]
	public void Repair(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		if (ServiceLocator.Instance.GetService<IRepositoryService>().Repair(name, out var deferredMove).ContainsError())
		{
			context.Messages.Add(deferredMove.ToJson());
		}
	}

	[CommandForm(Name = "reset")]
	[Usage("repository reset name:")]
	public void Reset(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		ServiceLocator.Instance.GetService<IRepositoryService>().Reset(name, out var defferedDelete);
		context.Messages.Add(defferedDelete.ToJson());
	}

	[CommandForm(Name = "reset-all")]
	[Usage("repository reset-all")]
	public void ResetAll(CommandContext context)
	{
		ServiceLocator.Instance.GetService<IRepositoryService>().Reset(out var _);
	}

	[CommandForm(Name = "set-label")]
	[Usage("repository set-label name: revision: label:")]
	public void SetLabel(CommandContext context, [CommandKeyValue(IsRequired = true)] string name, [CommandKeyValue(IsRequired = true)] string revision, [CommandKeyValue(IsRequired = true)] string label)
	{
		ServiceLocator.Instance.GetService<IRepositoryService>().SetLabel(name, revision, label);
	}

	[CommandForm(Name = "get-label")]
	[Usage("repository get-label name: revision:")]
	public void GetLabel(CommandContext context, [CommandKeyValue(IsRequired = true)] string name, [CommandKeyValue(IsRequired = true)] string revision)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		context.Messages.Add(service.GetLabel(name, revision).ToJson());
	}
}
