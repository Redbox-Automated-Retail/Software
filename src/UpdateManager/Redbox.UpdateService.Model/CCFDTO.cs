using System;
using System.Collections.Generic;

namespace Redbox.UpdateService.Model;

public class CCFDTO
{
	public long Id { get; set; }

	public string Name { get; set; }

	public long SGId { get; set; }

	public DateTime? SGD { get; set; }

	public long AGId { get; set; }

	public DateTime? AGD { get; set; }

	public static List<ClientConfigFile> GetClientConfigFileDataList(List<CCFDTO> dtoList)
	{
		List<ClientConfigFile> list = new List<ClientConfigFile>();
		foreach (CCFDTO dto in dtoList)
		{
			ClientConfigFile clientConfigFile = new ClientConfigFile();
			list.Add(clientConfigFile);
			clientConfigFile.ConfigFileOID = dto.Id;
			clientConfigFile.Name = dto.Name;
			clientConfigFile.StagedGenerationId = dto.SGId;
			clientConfigFile.StagedGenerationDate = dto.SGD;
			clientConfigFile.ActiveGenerationId = dto.AGId;
			clientConfigFile.ActiveGenerationDate = dto.AGD;
		}
		return list;
	}

	public static List<CCFDTO> GetCCFDTOList(List<ClientConfigFile> list)
	{
		List<CCFDTO> list2 = new List<CCFDTO>();
		foreach (ClientConfigFile item in list)
		{
			CCFDTO cCFDTO = new CCFDTO();
			list2.Add(cCFDTO);
			cCFDTO.Id = item.ConfigFileOID;
			cCFDTO.Name = item.Name;
			cCFDTO.SGId = item.StagedGenerationId;
			cCFDTO.SGD = item.StagedGenerationDate;
			cCFDTO.AGId = item.ActiveGenerationId;
			cCFDTO.AGD = item.ActiveGenerationDate;
		}
		return list2;
	}
}
