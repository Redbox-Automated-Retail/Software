using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage;

internal sealed class LegacyHardwareCorrectionStatsTable : AbstractVistaDBDataTable<IHardwareCorrectionStatistic>
{
	protected override List<IHardwareCorrectionStatistic> OnLoadEntries()
	{
		throw new NotImplementedException("LoadEntries");
	}

	protected override string CreateStatement()
	{
		throw new NotImplementedException("Create");
	}

	protected override string UpdateStatement(IHardwareCorrectionStatistic obj)
	{
		throw new NotImplementedException("Update");
	}

	protected override string InsertStatement(IHardwareCorrectionStatistic p)
	{
		throw new NotImplementedException("Insert");
	}

	protected override string DeleteStatement(IHardwareCorrectionStatistic obj)
	{
		throw new NotImplementedException("Delete");
	}

	internal LegacyHardwareCorrectionStatsTable(IDataTableDescriptor d)
		: base(d, "HardwareCorrection")
	{
	}
}
