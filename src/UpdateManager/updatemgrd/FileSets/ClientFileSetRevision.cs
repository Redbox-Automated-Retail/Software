using System.Collections.Generic;
using System.Linq;

namespace Redbox.UpdateManager.FileSets;

internal class ClientFileSetRevision
{
	public long FileSetId { get; set; }

	public string FileSetName { get; set; }

	public int RetentionDays { get; set; }

	public int RetentionRevisions { get; set; }

	public long RevisionId { get; set; }

	public string RevisionName { get; set; }

	public string RevisionVersion { get; set; }

	public long PatchRevisionId { get; set; }

	public string SetPath { get; set; }

	public int SetCompressionType { get; set; }

	public string SetFileHash { get; set; }

	public long SetFileSize { get; set; }

	public string PatchSetPath { get; set; }

	public int PatchSetCompressionType { get; set; }

	public string PatchSetFileHash { get; set; }

	public long PatchSetFileSize { get; set; }

	public IEnumerable<ClientFileSetFile> Files { get; set; }

	public IEnumerable<ClientPatchFileSetFile> PatchFiles { get; set; }

	public virtual IEnumerable<ClientFileSetRevisionDependency> Dependencies { get; set; }

	public ClientFileSetRevision Clone()
	{
		ClientFileSetRevision obj = new ClientFileSetRevision
		{
			FileSetId = FileSetId,
			FileSetName = FileSetName,
			RetentionDays = RetentionDays,
			RetentionRevisions = RetentionRevisions,
			RevisionId = RevisionId,
			RevisionName = RevisionName,
			RevisionVersion = RevisionVersion,
			PatchRevisionId = PatchRevisionId,
			SetPath = SetPath,
			SetCompressionType = SetCompressionType,
			SetFileHash = SetFileHash,
			SetFileSize = SetFileSize,
			PatchSetPath = PatchSetPath,
			PatchSetCompressionType = PatchSetCompressionType,
			PatchSetFileHash = PatchSetFileHash,
			PatchSetFileSize = PatchSetFileSize
		};
		List<ClientFileSetFile> files = new List<ClientFileSetFile>();
		if (Files != null)
		{
			Files.ToList().ForEach(delegate(ClientFileSetFile item)
			{
				files.Add(item);
			});
		}
		obj.Files = files;
		List<ClientPatchFileSetFile> patchFiles = new List<ClientPatchFileSetFile>();
		if (PatchFiles != null)
		{
			PatchFiles.ToList().ForEach(delegate(ClientPatchFileSetFile item)
			{
				patchFiles.Add(item);
			});
		}
		obj.PatchFiles = patchFiles;
		List<ClientFileSetRevisionDependency> dependencies = new List<ClientFileSetRevisionDependency>();
		if (Dependencies != null)
		{
			Dependencies.ToList().ForEach(delegate(ClientFileSetRevisionDependency item)
			{
				dependencies.Add(item);
			});
		}
		obj.Dependencies = dependencies;
		return obj;
	}
}
