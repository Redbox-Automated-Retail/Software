using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
foreach (var path in args) {
  using var fs = File.OpenRead(path);
  using var pe = new PEReader(fs);
  if (!pe.HasMetadata) continue;
  var md = pe.GetMetadataReader();
  var an = md.GetString(md.GetAssemblyDefinition().Name);
  foreach (var th in md.TypeDefinitions) {
    var td = md.GetTypeDefinition(th);
    if (td.GetDeclaringType().IsNil == false) continue;
    var ns = md.GetString(td.Namespace); var n = md.GetString(td.Name);
    if (n == "<Module>") continue;
    var vis = (td.Attributes & System.Reflection.TypeAttributes.VisibilityMask) == System.Reflection.TypeAttributes.Public ? "pub" : "int";
    Console.WriteLine($"{an}\t{ns}\t{n}\t{vis}\t{td.GetMethods().Count}");
  }
}
