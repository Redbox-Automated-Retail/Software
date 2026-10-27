using System;

namespace Redbox.UpdateService.Model;

public class ClientConfigFile
{
	public long ConfigFileOID { get; set; }

	public string Name { get; set; }

	public long StagedGenerationId { get; set; }

	public DateTime? StagedGenerationDate { get; set; }

	public long ActiveGenerationId { get; set; }

	public DateTime? ActiveGenerationDate { get; set; }
}
