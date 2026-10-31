# System.Web.Script.Serialization for .NET 10

`JavaScriptSerializer` is part of .NET Framework's `System.Web.Extensions` and does not exist on .NET (Core).
`Redbox.Core`'s `ToJson()` / `ToObject()` are built on it, and their output is a wire format (for example every
KioskServices message), so the .NET 10 build of `Redbox.Core` compiles Microsoft's own implementation instead of a
look-alike.

- The `*.cs` files here (except `Support.cs`) are copied from
  [microsoft/referencesource](https://github.com/microsoft/referencesource) (`System.Web.Extensions/Script/Serialization`,
  branch `main`), MIT licensed, see `LICENSE.txt`. One change, marked `modernize branch`: doubles and floats are
  formatted with .NET Framework's "R" rules, because .NET Core 3.0+ changed `ToString("r")`.
- `Support.cs` (written here) supplies the few `System.Web` internals they use: the `AtlasWeb` / `SR` messages (the
  text is taken from the .NET Framework 4.8 resources), `aspnet:MaxJsonDeserializerMembers` (read from the app's
  `appSettings` as System.Web does outside ASP.NET; default `Int32.MaxValue`), the UTF-16 validation that
  replaces lone surrogates with U+FFFD, `SecurityUtils` (plain reflection calls) and `Debug.Assert`.
- They are compiled only for `net10.0-windows`; the `net48` build references the real `System.Web.Extensions`.
- `tests/Redbox.Core.JsonParity` checks that both builds produce the same JSON.
