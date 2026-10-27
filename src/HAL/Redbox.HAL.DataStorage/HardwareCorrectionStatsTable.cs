using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage;

internal sealed class HardwareCorrectionStatsTable : AbstractVistaDBDataTable<IHardwareCorrectionStatistic>
{
	protected override List<IHardwareCorrectionStatistic> OnLoadEntries()
	{
		List<IHardwareCorrectionStatistic> list = new List<IHardwareCorrectionStatistic>();
		string query = $"SELECT Job, CorrectionTime, Success, Type FROM [{base.Name}]";
		ExectuteSelectQuery(delegate(DbDataReader reader)
		{
			list.Add(new HardwareCorrectionStat
			{
				ProgramName = reader.GetString(0),
				CorrectionTime = reader.GetDateTime(1),
				CorrectionOk = reader.GetBoolean(2),
				Statistic = (HardwareCorrectionStatistic)reader.GetInt32(3)
			});
		}, query);
		return list;
	}

	protected override string CreateStatement()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("CREATE TABLE [{0}] ", base.Name);
		stringBuilder.Append("( [Type] Int NOT NULL,");
		stringBuilder.Append(" [Job] VarChar(100) NOT NULL,");
		stringBuilder.Append(" [CorrectionTime] DateTime NOT NULL,");
		stringBuilder.Append(" [Success] Bit NOT NULL )");
		return stringBuilder.ToString();
	}

	protected override string UpdateStatement(IHardwareCorrectionStatistic obj)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("UPDATE {0} SET", base.Name);
		stringBuilder.AppendFormat(" Success = '{0}'", obj.CorrectionOk ? 1 : 0);
		stringBuilder.AppendFormat(" where Job = '{0}' and CorrectionTime = '{1}' and Type = '{2}'", obj.ProgramName, obj.CorrectionTime.ToString(), (int)obj.Statistic);
		return stringBuilder.ToString();
	}

	protected override string InsertStatement(IHardwareCorrectionStatistic p)
	{
		return $"INSERT INTO [{base.Name}] (Type, Job, CorrectionTime, Success) VALUES ('{(int)p.Statistic}', '{p.ProgramName}', '{p.CorrectionTime}', '{(p.CorrectionOk ? 1 : 0)}')";
	}

	protected override string DeleteStatement(IHardwareCorrectionStatistic obj)
	{
		return $"delete from {base.Name} where Type = '{(int)obj.Statistic}' and CorrectionTime = '{obj.CorrectionTime}' and Job = '{obj.ProgramName}'";
	}

	internal HardwareCorrectionStatsTable(IDataTableDescriptor d)
		: base(d, "HardwareCorrection_v2")
	{
	}
}
