// Dumps the metadata surface of managed binaries as one JSON object per line:
// assembly name/version, referenced assemblies, every type, and every method/property/field/event name.
// Used by history.py to size the change between two versions of an assembly without decompiling.
// Usage: ApiSurface <file> ...   (or "@listfile" with one path per line)
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.Json;

var files = args.SelectMany(a => a.StartsWith("@") ? File.ReadAllLines(a[1..]) : new[] { a });
foreach (var path in files) {
  try {
    using var fs = File.OpenRead(path);
    using var pe = new PEReader(fs);
    if (!pe.HasMetadata) continue;
    var md = pe.GetMetadataReader();
    if (!md.IsAssembly) continue;
    var ad = md.GetAssemblyDefinition();
    var types = new List<string>();
    var members = new List<string>();
    int ilBytes = 0;
    foreach (var th in md.TypeDefinitions) {
      var td = md.GetTypeDefinition(th);
      var name = TypeName(md, td);
      if (name == "<Module>") continue;
      var pub = IsPublic(md, td);
      types.Add((pub ? "+" : "-") + name);
      foreach (var mh in td.GetMethods()) {
        var m = md.GetMethodDefinition(mh);
        var mp = (m.Attributes & MethodAttributes.MemberAccessMask) is MethodAttributes.Public or MethodAttributes.Family or MethodAttributes.FamORAssem;
        members.Add($"{(pub && mp ? "+" : "-")}{name}::{md.GetString(m.Name)}/{m.GetParameters().Count}");
        if (m.RelativeVirtualAddress != 0) {
          try { ilBytes += pe.GetMethodBody(m.RelativeVirtualAddress).Size; } catch { }
        }
      }
      foreach (var fh in td.GetFields()) {
        var f = md.GetFieldDefinition(fh);
        var fp = (f.Attributes & FieldAttributes.FieldAccessMask) is FieldAttributes.Public or FieldAttributes.Family;
        members.Add($"{(pub && fp ? "+" : "-")}{name}.{md.GetString(f.Name)}");
      }
      foreach (var ph in td.GetProperties())
        members.Add($"{(pub ? "+" : "-")}{name}#{md.GetString(md.GetPropertyDefinition(ph).Name)}");
      foreach (var eh in td.GetEvents())
        members.Add($"{(pub ? "+" : "-")}{name}!{md.GetString(md.GetEventDefinition(eh).Name)}");
    }
    var refs = md.AssemblyReferences.Select(r => md.GetAssemblyReference(r))
      .Select(r => $"{md.GetString(r.Name)} {r.Version}").OrderBy(x => x).ToList();
    var resources = md.ManifestResources.Select(r => md.GetString(md.GetManifestResource(r).Name)).OrderBy(x => x).ToList();
    Console.WriteLine(JsonSerializer.Serialize(new {
      file = path, name = md.GetString(ad.Name), version = ad.Version.ToString(),
      refs, resources, ilBytes, types, members
    }));
  } catch (BadImageFormatException) { }
}

static string TypeName(MetadataReader md, TypeDefinition td) {
  var n = md.GetString(td.Name);
  var dt = td.GetDeclaringType();
  if (!dt.IsNil) return TypeName(md, md.GetTypeDefinition(dt)) + "/" + n;
  var ns = md.GetString(td.Namespace);
  return ns.Length == 0 ? n : ns + "." + n;
}

static bool IsPublic(MetadataReader md, TypeDefinition td) {
  var v = td.Attributes & TypeAttributes.VisibilityMask;
  if (v == TypeAttributes.Public) return true;
  if (v is TypeAttributes.NestedPublic or TypeAttributes.NestedFamily or TypeAttributes.NestedFamORAssem)
    return IsPublic(md, md.GetTypeDefinition(td.GetDeclaringType()));
  return false;
}
