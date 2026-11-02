using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Redbox.Core;
using Redbox.HAL.Component.Model;
using Redbox.HAL.DataStorage;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Data;
using ErrorList = Redbox.HAL.Component.Model.ErrorList;
using IKioskFunctionCheckData = Redbox.HAL.Component.Model.IKioskFunctionCheckData;

namespace Redbox.Data.SqliteParity;

// Usage: Redbox.Data.SqliteParity <work dir> [<folder with sample VistaDB files>]
// Prints one line per result. Lines starting with "#" are build-specific (not compared).
internal static class Program
{
	private static string? s_dataPath;

	private static int Main(string[] args)
	{
		Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
		string work = Path.GetFullPath(args[0]);
		if (Directory.Exists(work))
		{
			Directory.Delete(work, recursive: true);
		}
		Directory.CreateDirectory(work);
		Redbox.Core.ServiceLocator.Instance.AddService(typeof(IEngineApplication), Stub.Create<IEngineApplication>((m, a) => m.Name == "get_DataPath" ? s_dataPath : null));
		Redbox.Core.ServiceLocator.Instance.AddService(typeof(IKernelService), Stub.Create<IKernelService>((m, a) => m.Name == "FormatLuaValue" ? Convert.ToString(a[0], CultureInfo.InvariantCulture) : null));
		Redbox.HAL.Component.Model.ServiceLocator.Instance.AddService<ITableTypeFactory>(new TableTypeFactory());
#if NETFRAMEWORK
		Console.WriteLine("# build: net48 (VistaDB)");
#else
		Console.WriteLine("# build: net10.0-windows (SQLite)");
#endif
		Section("fresh data store", () => DataStore(Path.Combine(work, "fresh-data")));
		Section("fresh queue", () => Queue(Path.Combine(work, "fresh-bin")));
		Section("fresh HAL tables", () => HalFresh(Path.Combine(work, "fresh-hal")));
		if (args.Length > 1)
		{
			string samples = Path.GetFullPath(args[1]);
			string data = Path.Combine(work, "converted", "data");
			string bin = Path.Combine(work, "converted", "bin");
			string hal = Path.Combine(work, "converted", "hal");
			Directory.CreateDirectory(data);
			Directory.CreateDirectory(bin);
			Directory.CreateDirectory(hal);
			foreach (string name in new[] { "Local.data", "cache.data", "profile.data" })
			{
				File.Copy(Path.Combine(samples, name), Path.Combine(data, name));
			}
			File.Copy(Path.Combine(samples, "message.data"), Path.Combine(bin, "message.data"));
			File.Copy(Path.Combine(samples, "HALData.vdb3"), Path.Combine(hal, "HALData.vdb3"));
			File.Copy(Path.Combine(samples, "HALCounters.vdb3"), Path.Combine(hal, "HALCounters.vdb3"));
			Section("converted data stores", () => DataStoreDump(data, "Local", "cache"));
			Section("converted queue", () => QueueDepth(bin));
			Section("converted HAL tables", () => HalDump(hal));
			Section("converted profile", () => Profile(data, Path.Combine(work, "converted", "images")));
#if !NETFRAMEWORK
			Bookkeeping(data, bin, hal);
#endif
		}
		return 0;
	}

	private static void DataStore(string dataPath)
	{
		Directory.CreateDirectory(dataPath);
		s_dataPath = dataPath;
		IDataStoreService store = DataStoreProvider.GetProvider("kv");
		Out("count missing table", store.Count("kv", "T1"));
		Out("get missing table", store.Get("kv", "T1", "a"));
		store.Set("kv", "T1", "alpha", "1");
		store.Set("kv", "T1", "Beta", "{ x = 2 }");
		store.Set("kv", "T1", "BETA", "replaced");
		store.BulkSet("kv", "T1", new Dictionary<string, object> { ["gamma"] = 3, ["delta"] = "four", ["alpha"] = "uno" });
		store.Set("kv", "T2", "k", "v");
		Out("get alpha", store.Get("kv", "T1", "alpha"));
		Out("get beta (case-insensitive key)", store.Get("kv", "T1", "beta"));
		Out("get missing key", store.Get("kv", "T1", "nope"));
		Out("count", store.Count("kv", "T1"));
		Out("keys", Sorted(store.GetKeys("kv", "T1")));
		Out("keys in order", string.Join(", ", store.GetKeys("kv", "T1")));
		Out("all", Dump(store.GetAll("kv", "T1")));
		Out("tables", Sorted(store.GetTables("kv")));
		store.Remove("kv", "T1", "GAMMA");
		Out("after remove (case-insensitive)", Dump(store.GetAll("kv", "T1")));
		store.Clear("kv", "T2");
		Out("T2 after clear", store.Count("kv", "T2"));
		store.Drop("kv", "T2");
		Out("tables after drop", Sorted(store.GetTables("kv")));
		store.CloseAllStores();
		Out("reopened all", Dump(store.GetAll("kv", "T1")));
		store.CloseAllStores();
	}

	private static void DataStoreDump(string dataPath, params string[] stores)
	{
		s_dataPath = dataPath;
		foreach (string name in stores)
		{
			IDataStoreService store = DataStoreProvider.GetProvider(name);
			foreach (string table in Sorted(store.GetTables(name)))
			{
				Dictionary<string, object> all = store.GetAll(name, table);
				Out($"{name}.{table}", $"count={store.Count(name, table)} all={all.Count} keys={store.GetKeys(name, table).Count} sha1={Sha1(Dump(all))}");
				ReadOnlyCollection<string> keys = store.GetKeys(name, table);
				Out($"{name}.{table} first keys in order", string.Join(", ", keys.Take(3)));
				string? first = keys.OrderBy(k => k, StringComparer.Ordinal).FirstOrDefault();
				if (first != null)
				{
					Out($"{name}.{table} get(first key, upper-cased)", Short(store.Get(name, table, first.ToUpperInvariant())));
				}
			}
			store.CloseAllStores();
		}
	}

	private static void Queue(string bin)
	{
		Directory.CreateDirectory(bin);
		IQueueServiceData queue = NewQueue();
		Out("initialize", queue.Initialize(Path.Combine(bin, "message.data")).Count);
		queue.Enqueue(1, "low", new byte[] { 1 });
		queue.Enqueue(5, "high", new byte[] { 2, 2 }, out int highId);
		queue.Enqueue(5, "high", new byte[] { 3 });
		queue.Enqueue(3, "mid", new byte[] { 4 });
		Out("new id", highId);
		Out("order", Messages(queue, null));
		Out("order of type high", Messages(queue, "high"));
		queue.GetDepth(out int depth);
		Out("depth", depth);
		queue.SelectNextMessage(null, out IMessage next);
		Out("next", Describe(next));
		queue.Update(highId, new byte[] { 9, 9, 9 });
		queue.DeleteMessageByType("mid");
		Out("after update and delete by type", Messages(queue, null));
		queue.DeleteMessage(highId);
		queue.GetDepth(out depth);
		Out("depth after delete", depth);
		((IDisposable)queue).Dispose();
		IQueueServiceData reopened = NewQueue();
		Out("reinitialize", reopened.Initialize(Path.Combine(bin, "message.data")).Count);
		Out("persisted", Messages(reopened, null));
		reopened.Clear();
		reopened.GetDepth(out depth);
		Out("depth after clear", depth);
		((IDisposable)reopened).Dispose();
	}

	private static void QueueDepth(string bin)
	{
		IQueueServiceData queue = NewQueue();
		Out("initialize", queue.Initialize(Path.Combine(bin, "message.data")).Count);
		queue.GetDepth(out int depth);
		Out("depth", depth);
		queue.Enqueue(2, "after-conversion", new byte[] { 7 }, out int id);
		Out("enqueue after conversion", Messages(queue, null));
		((IDisposable)queue).Dispose();
	}

	private static IQueueServiceData NewQueue()
	{
#if NETFRAMEWORK
		return new QueueServiceVistaDBData();
#else
		return new QueueServiceSqliteData();
#endif
	}

	private static string Messages(IQueueServiceData queue, string? type)
	{
		List<string> list = new List<string>();
		if (type == null)
		{
			queue.ForEach(m => list.Add(Describe(m)));
		}
		else
		{
			queue.ForEach(m => list.Add(Describe(m)), type);
		}
		return string.Join("; ", list);
	}

	private static string Describe(IMessage m)
	{
		return m == null ? "null" : $"{m.ID}:{m.Type}:{m.Priority}:{BitConverter.ToString(m.Data)}:{(m.CreatedOn > DateTime.Now.AddDays(-1) ? "recent" : "old")}";
	}

	private static void HalFresh(string dataPath)
	{
		Directory.CreateDirectory(dataPath);
#if NETFRAMEWORK
		// HAL's package installer copies empty VistaDB databases into place; the SQLite build creates the files itself.
		using (VistaDB.DDA.IVistaDBDDA dda = VistaDB.DDA.VistaDBEngine.Connections.OpenDDA())
		{
			foreach (string name in new[] { "HALData.vdb3", "HALCounters.vdb3" })
			{
				dda.CreateDatabase(Path.Combine(dataPath, name), false, null, 0, 0, false).Dispose();
			}
		}
#endif
		Redbox.HAL.Component.Model.ServiceLocator.Instance.RemoveService<IRuntimeService>();
		Redbox.HAL.Component.Model.ServiceLocator.Instance.AddService<IRuntimeService>(Stub.Create<IRuntimeService>((m, a) => m.Name == "get_DataPath" ? dataPath : null));
		DataTableService service = new DataTableService(exclusive: false);
		ITableTypeFactory factory = new TableTypeFactory();
		DateTime t1 = new DateTime(2026, 10, 31, 9, 15, 30);
		DateTime t2 = new DateTime(2026, 10, 31, 18, 5, 0);

		IDataTable<IPersistentCounter> counters = service.GetTable<IPersistentCounter>()!;
		Out("counters exists", counters.Exists);
		Out("counters create", counters.Create());
		Out("counters exists after create", counters.Exists);
		Out("counters insert", counters.Insert(new List<IPersistentCounter> { factory.NewCounter("a", 1), factory.NewCounter("b", 2) }));
		IPersistentCounter b = factory.NewCounter("b", 5);
		Out("counters update", counters.Update(b));
		Out("counters update missing", counters.Update(factory.NewCounter("zzz", 1)));
		Out("counters", string.Join(",", counters.LoadEntries().OrderBy(c => c.Name).Select(c => $"{c.Name}={c.Value}")));
		Out("counters delete", counters.Delete(factory.NewCounter("a", 0)));
		Out("counters row count", counters.GetRowCount());
		Out("counters update table", UpdateTable(counters));
		Out("counters update table again", UpdateTable(counters));

		IDataTable<ILocation> inventory = service.GetTable<ILocation>()!;
		Out("inventory create", inventory.Create());
		Out("inventory update table", UpdateTable(inventory));
		List<ILocation> slots = new List<ILocation>
		{
			factory.NewLocation(1, 1, "EMPTY", null, false, 0, MerchFlags.None),
			factory.NewLocation(1, 2, "ABC123", t1, true, 2, MerchFlags.Thin),
			factory.NewLocation(2, 1, "XYZ", t2, false, 0, MerchFlags.Rebalance)
		};
		Out("inventory insert", inventory.Insert(slots));
		ILocation changed = factory.NewLocation(1, 1, "NEW1", t2, false, 1, MerchFlags.Unload);
		Out("inventory update", inventory.Update(changed));
		Out("inventory delete", inventory.Delete(factory.NewLocation(2, 1)));
		Out("inventory", string.Join("; ", inventory.LoadEntries().OrderBy(l => l.Deck).ThenBy(l => l.Slot).Select(DescribeLocation)));

		IDataTable<IDumpBinInventoryItem> bin = service.GetTable<IDumpBinInventoryItem>()!;
		Out("dumpbin create", bin.Create());
		Out("dumpbin insert", bin.Insert(new List<IDumpBinInventoryItem> { factory.NewBinItem("B1", t1), factory.NewBinItem("B2", t2) }));
		Out("dumpbin delete by barcode+time", bin.Delete(factory.NewBinItem("B1", t1)));
		Out("dumpbin", string.Join("; ", bin.LoadEntries().Select(i => $"{i.ID}@{Stamp(i.PutTime)}")));

		IDataTable<IHardwareCorrectionStatistic> stats = service.GetTable<IHardwareCorrectionStatistic>()!;
		Out("stats create", stats.Create());
		Out("stats insert", stats.Insert(factory.NewStatistic(HardwareCorrectionStatistic.Arcus, "init", false, t1)));
		Out("stats update", stats.Update(factory.NewStatistic(HardwareCorrectionStatistic.Arcus, "init", true, t1)));
		Out("stats", string.Join("; ", stats.LoadEntries().Select(s => $"{s.Statistic}:{s.ProgramName}:{s.CorrectionOk}:{Stamp(s.CorrectionTime)}")));
		Out("stats delete", stats.Delete(factory.NewStatistic(HardwareCorrectionStatistic.Arcus, "init", true, t1)));

		IDataTable<IKioskFunctionCheckData> checks = service.GetTable<IKioskFunctionCheckData>()!;
		Out("kfc create", checks.Create());
		DateTime old = DateTime.Now.AddDays(-20).Date.AddHours(10);
		DateTime recent = DateTime.Now.AddDays(-1).Date.AddHours(10);
		Out("kfc insert", checks.Insert(new List<IKioskFunctionCheckData>
		{
			factory.NewCheckData("ok", "ok", "ok", "ok", "ok", "ok", "ok", old, "tech1"),
			factory.NewCheckData("fail", "ok", "ok", "ok", "ok", "ok", "ok", recent, "tech2")
		}));
		Out("kfc update", checks.Update(factory.NewCheckData("ok", "ok", "ok", "ok", "ok", "ok", "fixed", recent, "tech2")));
		Out("kfc clean (older than 14 days)", checks.Clean());
		Out("kfc", string.Join("; ", checks.LoadEntries().Select(c => $"{c.UserIdentifier}:{c.VerticalSlotTestResult}:{c.CameraDriverTestResult}:{(c.Timestamp == recent ? "recent" : Stamp(c.Timestamp))}")));

		IDataTable<IPersistentOption> options = service.GetTable<IPersistentOption>()!;
		Out("options create", options.Create());
		Out("options insert", options.Insert(factory.NewPersistentOption("RebootTime", "03:00")));
		Out("options update", options.Update(factory.NewPersistentOption("RebootTime", "04:00")));
		Out("options", string.Join(",", options.LoadEntries().Select(o => $"{o.Key}={o.Value}")));
		Out("options delete all", options.DeleteAll());
		Out("counters drop", counters.Drop());
		Out("counters exists after drop", counters.Exists);
	}

	private static void HalDump(string dataPath)
	{
		Redbox.HAL.Component.Model.ServiceLocator.Instance.RemoveService<IRuntimeService>();
		Redbox.HAL.Component.Model.ServiceLocator.Instance.AddService<IRuntimeService>(Stub.Create<IRuntimeService>((m, a) => m.Name == "get_DataPath" ? dataPath : null));
		DataTableService service = new DataTableService(exclusive: false);
		Out("counters", Summary(service.GetTable<IPersistentCounter>()!.LoadEntries().OrderBy(c => c.Name, StringComparer.Ordinal).Select(c => $"{c.Name}={c.Value}")));
		Out("options", Summary(service.GetTable<IPersistentOption>()!.LoadEntries().OrderBy(o => o.Key, StringComparer.Ordinal).Select(o => $"{o.Key}={o.Value}")));
		Out("inventory", Summary(service.GetTable<ILocation>()!.LoadEntries().OrderBy(l => l.Deck).ThenBy(l => l.Slot).Select(DescribeLocation)));
		Out("dumpbin", Summary(service.GetTable<IDumpBinInventoryItem>()!.LoadEntries().Select(i => $"{i.ID}@{Stamp(i.PutTime)}").OrderBy(s => s, StringComparer.Ordinal)));
		Out("stats rows", service.GetTable<IHardwareCorrectionStatistic>()!.GetRowCount());
		Out("kfc rows", service.GetTable<IKioskFunctionCheckData>()!.GetRowCount());
		IDataTable<ILocation> inventory = service.GetTable<ILocation>()!;
		Out("inventory update table", UpdateTable(inventory));
		ILocation first = inventory.LoadEntries().OrderBy(l => l.Deck).ThenBy(l => l.Slot).First();
		first.ID = "AFTER-CONVERSION";
		Out("inventory update after conversion", inventory.Update(first));
		Out("inventory first after update", DescribeLocation(inventory.LoadEntries().OrderBy(l => l.Deck).ThenBy(l => l.Slot).First()));
	}

	private static void Profile(string dataPath, string images)
	{
		s_dataPath = dataPath;
		ProfileData profile = new ProfileData(dataPath, "images");
		Out("has active", profile.HasActive());
		ProfileDataInstance instance = (ProfileDataInstance)profile.LoadDataInstance();
		Out("markets", Summary(instance.Markets.OrderBy(p => p.Key).Select(p => $"{p.Key}={p.Value.MarketName}")));
		Out("titles", Summary(instance.Titles.OrderBy(p => p.Key).Select(p => $"{p.Key}={p.Value.LongName}|{p.Value.RunningTime}|{string.Join("/", p.Value.Starring ?? new List<string>())}")));
		Out("product families", instance.ProductFamilies.Count);
		Out("product groups", Summary(instance.ProductGroups.OrderBy(p => p.Key).Select(p => p.Key.ToString(CultureInfo.InvariantCulture))));
		Out("title rollup xref", instance.TitleRollupXREF.Count);
		Out("multi disc xref", instance.MultiDiscXREF.Count);
		Out("product types", instance.ProductTypes.Count);
		Out("stores", Summary(instance.Stores.OrderBy(p => p.Key).Select(p => p.Key.ToString(CultureInfo.InvariantCulture))));
		Out("sub platforms", instance.SubPlatforms.Count);
		Out("version", instance.ProfileDataVersion == null ? "null" : $"{instance.ProfileDataVersion.GeneratedOn:o}");
		string[] files = Directory.GetFiles(instance.ImagesPath).OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToArray();
		Out("images extracted", $"{files.Length} files, sha1 {Sha1(string.Join("\n", files.Select(f => Path.GetFileName(f) + ":" + Sha1(File.ReadAllBytes(f)))))}");
		string name = Path.GetFileName(files.FirstOrDefault() ?? string.Empty);
		Out("data cache " + name, Sha1(DataCacheService.Instance.GetContent(DataCacheType.Image, name)));
		Out("data cache missing", DataCacheService.Instance.GetContent(DataCacheType.Image, "no-such-image.jpg") == null);
		DataCacheService.Instance.Shutdown();
	}

#if !NETFRAMEWORK
	private static void Bookkeeping(string data, string bin, string hal)
	{
		foreach (string file in new[] { Path.Combine(data, "Local.data"), Path.Combine(data, "cache.data"), Path.Combine(bin, "message.data"), Path.Combine(hal, "HALData.vdb3"), Path.Combine(hal, "HALCounters.vdb3") })
		{
			Console.WriteLine($"# {Path.GetFileName(file)}: original gone={!File.Exists(file)} backup={File.Exists(file + Redbox.Data.Sqlite.LegacyDatabase.BackupSuffix)} sqlite={File.Exists(Redbox.Data.Sqlite.LegacyDatabase.ToSqlitePath(file))}");
		}
		string profile = Path.Combine(data, "profile.data");
		string sqlite = Redbox.Data.Sqlite.LegacyDatabase.ToSqlitePath(profile);
		DateTime converted = File.GetLastWriteTimeUtc(sqlite);
		Redbox.Data.Sqlite.LegacyDatabase.Resolve(profile, Redbox.Data.Sqlite.LegacyDatabaseKind.ReadOnlySource);
		Console.WriteLine($"# profile.data kept={File.Exists(profile)}; unchanged source not converted again={File.GetLastWriteTimeUtc(sqlite) == converted}");
		File.SetLastWriteTimeUtc(profile, DateTime.UtcNow);
		Redbox.Data.Sqlite.LegacyDatabase.Resolve(profile, Redbox.Data.Sqlite.LegacyDatabaseKind.ReadOnlySource);
		Console.WriteLine($"# changed profile.data converted again={File.GetLastWriteTimeUtc(sqlite) > converted}");
	}
#endif

	private static string UpdateTable<T>(IDataTable<T> table)
	{
		StringWriter writer = new StringWriter();
		ErrorList errors = new ErrorList();
		table.UpdateTable(writer, errors);
		return writer.ToString().Trim() + (errors.Count > 0 ? " errors=" + errors.Count : string.Empty);
	}

	private static string DescribeLocation(ILocation l)
	{
		return $"{l.Deck}/{l.Slot}:{l.ID}:{(l.ReturnDate.HasValue ? Stamp(l.ReturnDate.Value) : "null")}:{l.Excluded}:{l.StuckCount}:{l.Flags}";
	}

	private static string Stamp(DateTime value)
	{
		return value.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
	}

	private static void Section(string name, Action action)
	{
		Console.WriteLine("=== " + name);
		try
		{
			action();
		}
		catch (Exception e)
		{
			Console.WriteLine("EXCEPTION " + e.GetType().Name + ": " + e.Message);
			Console.WriteLine("# " + e.ToString().Replace("\n", "\n# "));
		}
	}

	private static void Out(string name, object value)
	{
		Console.WriteLine($"{name}: {Format(value)}");
	}

	private static string? Format(object? value)
	{
		switch (value)
		{
		case null:
			return "<null>";
		case bool b:
			return b ? "true" : "false";
		case string s:
			return s;
		case IEnumerable e:
			return "[" + string.Join(", ", e.Cast<object>().Select(Format)) + "]";
		default:
			return Convert.ToString(value, CultureInfo.InvariantCulture);
		}
	}

	private static List<string> Sorted(IEnumerable<string> items)
	{
		return items.OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();
	}

	private static string Dump(Dictionary<string, object> all)
	{
		return string.Join("|", all.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => p.Key + "=" + Format(p.Value)));
	}

	private static string Short(object value)
	{
		string text = Format(value)!; // Format returns null only if a ToString() does
		return text.Length > 60 ? text.Substring(0, 60) + "..." : text;
	}

	private static string Summary(IEnumerable<string> items)
	{
		List<string> list = items.ToList();
		return $"{list.Count} rows, sha1 {Sha1(string.Join("\n", list))}";
	}

	private static string Sha1(string text)
	{
		return Sha1(Encoding.UTF8.GetBytes(text));
	}

	private static string Sha1(byte[] bytes)
	{
		if (bytes == null)
		{
			return "<null>";
		}
		using SHA1 sha = SHA1.Create();
		return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", string.Empty).Substring(0, 16);
	}
}

// Minimal service stubs for the engine/HAL interfaces the data code looks up.
public class Stub : DispatchProxy
{
	private Func<MethodInfo, object?[], object?> m_handler = null!; // set by Create

	public static T Create<T>(Func<MethodInfo, object?[], object?> handler)
	{
		T proxy = DispatchProxy.Create<T, Stub>();
		((Stub)(object)proxy!).m_handler = handler;
		return proxy;
	}

	protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
	{
		object? result = m_handler(targetMethod!, args!); // both always passed by DispatchProxy
		if (result == null && targetMethod!.ReturnType.IsValueType && targetMethod.ReturnType != typeof(void))
		{
			return Activator.CreateInstance(targetMethod.ReturnType);
		}
		return result;
	}
}
