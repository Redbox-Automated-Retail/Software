# Tools

## decompile/

The scripts that produced the first commit of this repository from the kiosk image. They are kept for
reproducibility; you do not need them to build.

1. `AssemblyMeta` (`dotnet run -- <files>`) dumps name/version/target framework/references of every
   managed binary to a TSV (`meta.tsv`). `AssemblyTypes` dumps top-level type lists, which were used to
   compare ILMerged copies of libraries with the standalone DLLs.
2. Every assembly in `firstparty.txt` was decompiled with
   `ilspycmd -p --decompile-baml --ignore-decompilation-errors -r allrefs -o raw/<name> <assembly>`,
   where `allrefs/` holds one copy of every assembly in the image (`.staged_` copies preferred).
3. `organize.py` strips merged code, extracts merged-only libraries, flattens namespace folders and writes
   `src/`; `gen_csproj.py` writes the project files; `gen_docs.py` writes `docs/assemblies.md`.

Set `REDBOX_DECOMP_WORK` to the work directory containing `raw/`, `allrefs/`, `meta.tsv` and
`firstparty.txt`. `organize.py` **deletes and rewrites `src/`**.

## fixes/

Mechanical fixes for decompiler output that does not compile. Each was run once over `src/` and committed;
they are idempotent.

| Script | Fixes |
|---|---|
| `fix_com_event_providers.py` | tlbimp `*_EventProvider` classes: `add_X`/`remove_X` methods wrapped in a real `event` |
| `fix_xaml_markup.py` | BAML-decompiled markup extensions with unquoted `FallbackValue`s containing `,` `=` or quotes |
| `remove_wpf_generated_members.py` | `[GeneratedCode("PresentationBuildTasks")]` members left in WPF code-behind |
