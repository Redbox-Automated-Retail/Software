using System.Collections.Generic;
using System.Linq;

namespace Redbox.UpdateManager.FileSets;

internal class ClientFileSetRevision
{
	public long FileSetId { get; set; }

	// The properties below are filled by the object initializer or the JSON reader.
	public string FileSetName { get; set; } = null!;

	public int RetentionDays { get; set; }

	public int RetentionRevisions { get; set; }

	public long RevisionId { get; set; }

	public string RevisionName { get; set; } = null!;

	public string RevisionVersion { get; set; } = null!;

	public long PatchRevisionId { get; set; }

	public string SetPath { get; set; } = null!;

	public int SetCompressionType { get; set; }

	public string SetFileHash { get; set; } = null!;

	public long SetFileSize { get; set; }

	public string PatchSetPath { get; set; } = null!;

	public int PatchSetCompressionType { get; set; }

	public string PatchSetFileHash { get; set; } = null!;

	public long PatchSetFileSize { get; set; }

	public IEnumerable<ClientFileSetFile> Files { get; set; } = null!;

	public IEnumerable<ClientPatchFileSetFile> PatchFiles { get; set; } = null!;

	public virtual IEnumerable<ClientFileSetRevisionDependency> Dependencies { get; set; } = null!;

	public ClientFileSetRevision Clone()
	{
		ClientFileSetRevision clone = new ClientFileSetRevision
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
		clone.Files = files;
		List<ClientPatchFileSetFile> patchFiles = new List<ClientPatchFileSetFile>();
		if (PatchFiles != null)
		{
			PatchFiles.ToList().ForEach(delegate(ClientPatchFileSetFile item)
			{
				patchFiles.Add(item);
			});
		}
		clone.PatchFiles = patchFiles;
		List<ClientFileSetRevisionDependency> dependencies = new List<ClientFileSetRevisionDependency>();
		if (Dependencies != null)
		{
			Dependencies.ToList().ForEach(delegate(ClientFileSetRevisionDependency item)
			{
				dependencies.Add(item);
			});
		}
		clone.Dependencies = dependencies;
		return clone;
	}
}
