using System;

namespace Redbox.UpdateService.Model;

public class ClientConfigFile
{
	public long ConfigFileOID { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string Name { get; set; } = null!;

	public long StagedGenerationId { get; set; }

	public DateTime? StagedGenerationDate { get; set; }

	public long ActiveGenerationId { get; set; }

	public DateTime? ActiveGenerationDate { get; set; }
}
