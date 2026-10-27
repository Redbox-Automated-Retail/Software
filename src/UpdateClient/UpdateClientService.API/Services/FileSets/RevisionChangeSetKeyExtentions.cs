namespace UpdateClientService.API.Services.FileSets;

public static class RevisionChangeSetKeyExtentions
{
	public static string IdentifyingText(this RevisionChangeSetKey revisionChangeSetKey)
	{
		return $"FileSetId: {revisionChangeSetKey.FileSetId}, FileSetRevisionId: {revisionChangeSetKey.RevisionId}, PatchFileSetRevisionId: {revisionChangeSetKey.PatchRevisionId}";
	}
}
