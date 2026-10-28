"""Size every recoverable release in the rebuilt update store (output of rebuild_store.py).

For each package, every revision that brought content (a seed or a delta that was rebuilt) is compared
with the previous recoverable version of each of its files:
  - managed assemblies: assembly version, types and members added/removed (ApiSurface), IL size;
  - zip bundles (`*.bundle`): entries added/removed/changed;
  - anything else: size.
Revisions that only list placeholders (no content in any snapshot) are folded into the next revision
with content.

Usage:
  python -I history.py --work <dir> --apisurface <ApiSurface.exe>
Output in <work>: surfaces.jsonl (cache), timeline.json, timeline.md (draft table).
"""
import argparse
import json
import os
import subprocess
import zipfile


def blob(work, sha):
    return os.path.join(work, "blobs", sha[:2], sha)


def load_surfaces(work, exe):
    cache = os.path.join(work, "surfaces.jsonl")
    known = {}
    if os.path.exists(cache):
        with open(cache, encoding="utf-8") as f:
            for line in f:
                s = json.loads(line)
                known[os.path.basename(s["file"])] = s
    todo = []
    for root, _, files in os.walk(os.path.join(work, "blobs")):
        for name in files:
            if name in known:
                continue
            p = os.path.join(root, name)
            with open(p, "rb") as f:
                if f.read(2) == b"MZ":
                    todo.append(p)
    if todo:
        lst = os.path.join(work, "surfaces.todo")
        with open(lst, "w", encoding="utf-8") as f:
            f.write("\n".join(todo))
        out = subprocess.run([exe, "@" + lst], capture_output=True, text=True, encoding="utf-8").stdout
        with open(cache, "a", encoding="utf-8") as f:
            for line in out.splitlines():
                s = json.loads(line)
                known[os.path.basename(s["file"])] = s
                f.write(line + "\n")
        os.remove(lst)
    return known


def real(names):
    return {n for n in names if "<" not in n}


def zip_entries(path):
    try:
        with zipfile.ZipFile(path) as z:
            return {i.filename: i.CRC for i in z.infolist() if not i.is_dir()}
    except zipfile.BadZipFile:
        return None


def diff_file(work, surfaces, name, old, new):
    """Describe old -> new (sha1s; old may be None)."""
    s_new = surfaces.get(new)
    size_new = os.path.getsize(blob(work, new))
    d = {"file": name, "old": old, "new": new, "size": size_new}
    if s_new:
        d["kind"] = "assembly"
        d["assembly"] = s_new["name"]
        d["version"] = s_new["version"]
        d["types"] = len(real(s_new["types"]))
        d["members"] = len(real(s_new["members"]))
        d["il"] = s_new["ilBytes"]
        s_old = surfaces.get(old) if old else None
        if s_old:
            d["old_version"] = s_old["version"]
            # Compiler-generated types and members (closures, state machines, backing fields) are left out.
            ot, nt = real(s_old["types"]), real(s_new["types"])
            om, nm = real(s_old["members"]), real(s_new["members"])
            d["types_added"] = sorted(nt - ot)
            d["types_removed"] = sorted(ot - nt)
            d["members_added"] = len(nm - om)
            d["members_removed"] = len(om - nm)
            d["il_delta"] = s_new["ilBytes"] - s_old["ilBytes"]
            d["refs_added"] = sorted(set(r.split()[0] for r in s_new["refs"]) - set(r.split()[0] for r in s_old["refs"]))
        return d
    entries = zip_entries(blob(work, new)) if name.endswith(".bundle") or name.endswith(".zip") else None
    if entries is not None:
        d["kind"] = "bundle"
        d["entries"] = len(entries)
        if old:
            oe = zip_entries(blob(work, old)) or {}
            d["entries_added"] = sorted(set(entries) - set(oe))
            d["entries_removed"] = sorted(set(oe) - set(entries))
            d["entries_changed"] = sorted(k for k in entries if k in oe and oe[k] != entries[k])
        return d
    d["kind"] = "file"
    if old:
        d["old_size"] = os.path.getsize(blob(work, old))
    return d


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--work", required=True)
    ap.add_argument("--apisurface", required=True)
    a = ap.parse_args()
    surfaces = load_surfaces(a.work, a.apisurface)

    timeline = []
    for fn in sorted(os.listdir(os.path.join(a.work, "index"))):
        idx = json.load(open(os.path.join(a.work, "index", fn), encoding="utf-8"))
        pkg = idx["package"]
        last = {}          # path -> last recoverable sha1
        pending = []       # placeholder-only revisions folded into the next entry
        for r in idx["revisions"]:
            ok = [c for c in r["changes"] if c["status"] == "ok"]
            fresh = [c for c in ok if last.get(c["path"] + "\\" + c["name"]) != c["version"]]
            if not fresh:
                pending.append(r)
                for c in ok:
                    last[c["path"] + "\\" + c["name"]] = c["version"]
                continue
            files = []
            for c in fresh:
                key = c["path"] + "\\" + c["name"]
                files.append(diff_file(a.work, surfaces, c["name"], last.get(key), c["version"]))
                last[key] = c["version"]
            dates = sorted(c["patch_date"] for c in ok if c["patch_date"])
            labels = [p["label"] for p in pending if p["label"]] + ([r["label"]] if r["label"] else [])
            timeline.append({"package": pkg, "index": r["index"], "hash": r["hash"], "labels": labels,
                             "folded": len(pending), "date": dates[0][:10] if dates else None,
                             "snapshot": idx["sources"][r["seen_in"][0]]["date"],
                             "seen_in": r["seen_in"][0], "files": files})
            pending = []
    with open(os.path.join(a.work, "timeline.json"), "w", encoding="utf-8") as f:
        json.dump(timeline, f, indent=1)

    # One line per release: first-party assemblies only (names in tools/decompile/firstparty.txt or Redbox.*),
    # plus code bundles; third-party libraries and content-* bundles are only counted.
    fp_list = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "decompile", "firstparty.txt")
    firstparty = set(open(fp_list, encoding="utf-8").read().split())

    def is_fp(d):
        return d["kind"] == "assembly" and (d["assembly"] in firstparty or d["assembly"].lower().startswith("redbox"))

    with open(os.path.join(a.work, "summary.tsv"), "w", encoding="utf-8") as f:
        f.write("package\trev\tdate\tlabels\tfolded\tfp_changed\tfp_new\ttypes+\ttypes-\tmembers+\tmembers-\til\tother\tdetail\n")
        for e in timeline:
            fp = [d for d in e["files"] if is_fp(d)]
            new = [d for d in fp if d.get("old_version") is None]
            chg = [d for d in fp if d.get("old_version") is not None]
            ta = sum(len(d["types_added"]) for d in chg)
            tr = sum(len(d["types_removed"]) for d in chg)
            ma = sum(d["members_added"] for d in chg)
            mr = sum(d["members_removed"] for d in chg)
            il = sum(d["il_delta"] for d in chg)
            code_bundles = [d for d in e["files"] if d["kind"] == "bundle" and not d["file"].startswith("content-")]
            other = len(e["files"]) - len(fp) - len(code_bundles)
            detail = [f"{d['assembly']} {d['old_version']}->{d['version']} (t+{len(d['types_added'])}/-{len(d['types_removed'])} m+{d['members_added']}/-{d['members_removed']})"
                      for d in sorted(chg, key=lambda d: -(d["members_added"] + d["members_removed"]))
                      if d["members_added"] + d["members_removed"] or d["old_version"] != d["version"]]
            detail += [f"NEW {d['assembly']} {d['version']} ({d['types']}t)" for d in new]
            for d in code_bundles:
                if d["old"]:
                    detail.append(f"{d['file']} +{len(d['entries_added'])}/-{len(d['entries_removed'])}/~{len(d['entries_changed'])}")
                else:
                    detail.append(f"NEW {d['file']} ({d['entries']} entries)")
            f.write(f"{e['package']}\t{e['index']}\t{e['date']}\t{' '.join(e['labels'])}\t{e['folded']}\t{len(chg)}\t{len(new)}\t"
                    f"{ta}\t{tr}\t{ma}\t{mr}\t{il}\t{other}\t{'; '.join(detail)}\n")

    with open(os.path.join(a.work, "timeline.md"), "w", encoding="utf-8") as f:
        for e in timeline:
            f.write(f"\n### {e['package']} r{e['index']} {e['date']} {' '.join(e['labels'])} (+{e['folded']} folded)\n")
            for d in e["files"]:
                if d["kind"] == "assembly":
                    if d.get("old_version") is not None:
                        f.write(f"- {d['file']} {d['old_version']} -> {d['version']}: types +{len(d['types_added'])}/-{len(d['types_removed'])}, "
                                f"members +{d['members_added']}/-{d['members_removed']}, IL {d['il_delta']:+d}"
                                f"{' refs+' + ','.join(d['refs_added']) if d['refs_added'] else ''}"
                                f"{' NEW TYPES: ' + ', '.join(t[1:] for t in d['types_added'][:12]) if d['types_added'] else ''}\n")
                    else:
                        f.write(f"- {d['file']} {d['version']} (new/first: {d['types']} types, {d['members']} members)\n")
                elif d["kind"] == "bundle":
                    if d["old"]:
                        f.write(f"- {d['file']}: entries +{len(d['entries_added'])}/-{len(d['entries_removed'])}/~{len(d['entries_changed'])}"
                                f"{' ADDED: ' + ', '.join(d['entries_added'][:10]) if d['entries_added'] else ''}\n")
                    else:
                        f.write(f"- {d['file']} (first: {d['entries']} entries)\n")
                else:
                    f.write(f"- {d['file']} {d.get('old_size', '-')} -> {d['size']} bytes\n")


if __name__ == "__main__":
    main()
