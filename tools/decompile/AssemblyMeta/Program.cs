using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
foreach (var path in args) { try {
  using var fs = File.OpenRead(path);
  using var pe = new PEReader(fs);
  if (!pe.HasMetadata) continue;
  var md = pe.GetMetadataReader();
  if (!md.IsAssembly) continue;
  var asm = md.GetAssemblyDefinition();
  string tfm = "", company = "", fileVer="", infoVer="";
  foreach (var h in asm.GetCustomAttributes()) {
    var ca = md.GetCustomAttribute(h);
    string tname = "";
    if (ca.Constructor.Kind == HandleKind.MemberReference) {
      var mr = md.GetMemberReference((MemberReferenceHandle)ca.Constructor);
      if (mr.Parent.Kind == HandleKind.TypeReference) tname = md.GetString(md.GetTypeReference((TypeReferenceHandle)mr.Parent).Name);
    }
    if (tname is "TargetFrameworkAttribute" or "AssemblyCompanyAttribute" or "AssemblyFileVersionAttribute" or "AssemblyInformationalVersionAttribute") {
      var br = md.GetBlobReader(ca.Value); br.ReadUInt16(); var s = br.ReadSerializedString();
      if (tname=="TargetFrameworkAttribute") tfm=s; else if (tname=="AssemblyCompanyAttribute") company=s; else if (tname=="AssemblyFileVersionAttribute") fileVer=s; else infoVer=s;
    }
  }
  var refs = string.Join(";", md.AssemblyReferences.Select(r => { var a = md.GetAssemblyReference(r); return md.GetString(a.Name)+"/"+a.Version; }));
  var corflags = pe.PEHeaders.CorHeader.Flags;
  var mixed = !corflags.HasFlag(CorFlags.ILOnly);
  Console.WriteLine($"{md.GetString(asm.Name)}\t{asm.Version}\t{tfm}\t{company}\t{infoVer}\t{pe.PEHeaders.CoffHeader.TimeDateStamp:X8}\t{(mixed?"MIXED":"IL")}\t{corflags}\t{path}\t{refs}");
} catch (Exception e) { Console.Error.WriteLine(path+": "+e.Message); } }
