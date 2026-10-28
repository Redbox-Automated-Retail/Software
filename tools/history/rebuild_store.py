"""Rebuild every file version of every update-store package from several store snapshots.

Sources are the archived data packages (`data-MMDDYYYY.zip`, each a copy of a kiosk's
`.store/.repository` folder) and the kiosk image's own `.repository` folder. Revisions from all
sources are merged into one ordered history per package. Every change is then rebuilt into a
content-addressed blob store: seeds are taken as they are, VCDIFF deltas are applied with the
kiosk's own `xdelta3.exe` to an earlier version of the same file, and every result's SHA-1 must
equal the change's `VersionHash` (the check `RepositoryService` does, see docs/update-store.md).

Usage:
  python -I rebuild_store.py --work <dir> --xdelta <xdelta3.exe> [--include-dvd-data]
                             <source> [<source> ...]

A source is a zip or a `.repository` folder. Output in <work>:
  blobs/<sha1[:2]>/<sha1>   every verified file version
  index/<package>.json      merged revision history with the rebuild status of every change
  rebuild.log
"""
import argparse
import datetime
import hashlib
import json
import os
import re
import shutil
import subprocess
import sys
import tempfile
import zipfile

VCDIFF_MAGIC = b"\xd6\xc3\xc4"
ZERO_HASH = "0" * 40


def sha1_file(path):
    h = hashlib.sha1()
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            h.update(chunk)
    return h.hexdigest().upper()


def path_hash(target_path, target_name):
    return hashlib.sha1((target_path + "\\" + target_name).encode("ascii", "replace")).hexdigest().upper()


def source_date(name):
    """data-MMDDYYYY.zip -> date; dates without leading zeros (data-452019) are M D YYYY."""
    m = re.search(r"data-(\d+)\.zip$", name, re.I)
    if not m:
        return None
    d = m.group(1)
    year, md = int(d[-4:]), d[:-4]
    if len(md) == 4:
        month, day = int(md[:2]), int(md[2:])
    elif len(md) == 2:
        month, day = int(md[0]), int(md[1])
    else:  # 3 digits: prefer a valid month in front
        month, day = (int(md[:2]), int(md[2:])) if int(md[:2]) <= 12 else (int(md[0]), int(md[1:]))
    return datetime.date(year, month, day)


class Source:
    """One store snapshot: a zip of `.repository` or the folder itself."""

    def __init__(self, path):
        self.path = path
        self.name = os.path.basename(path.rstrip("\\/"))
        self.is_zip = path.lower().endswith(".zip")
        if self.is_zip:
            self.zip = zipfile.ZipFile(path)
            self.entries = {i.filename: i for i in self.zip.infolist() if not i.is_dir()}
            self.date = source_date(self.name)
        else:
            self.entries = {}
            for root, _, files in os.walk(path):
                for f in files:
                    full = os.path.join(root, f)
                    self.entries[os.path.relpath(full, path).replace("\\", "/")] = full
            # The image's snapshot date: newest file in the store.
            self.date = datetime.date.fromtimestamp(max(os.path.getmtime(p) for p in self.entries.values()))
            self.name = "image"

    def packages(self):
        return sorted({k.split("/")[0] for k in self.entries if "/" in k and not k.startswith("____")})

    def read(self, member):
        if self.is_zip:
            return self.zip.read(member)
        with open(self.entries[member], "rb") as f:
            return f.read()

    def mtime(self, member):
        if self.is_zip:
            return datetime.datetime(*self.entries[member].date_time).isoformat()
        return datetime.datetime.fromtimestamp(os.path.getmtime(self.entries[member])).isoformat(timespec="seconds")

    def head(self, member, n):
        if self.is_zip:
            with self.zip.open(member) as f:
                return f.read(n)
        with open(self.entries[member], "rb") as f:
            return f.read(n)

    def extract(self, member, dest):
        if self.is_zip:
            with self.zip.open(member) as src, open(dest, "wb") as out:
                shutil.copyfileobj(src, out, 1 << 20)
        else:
            shutil.copyfile(self.entries[member], dest)

    def revlog(self, package):
        prefix = package + "/.revlog/"
        nums = sorted(int(k[len(prefix):]) for k in self.entries if k.startswith(prefix) and k[len(prefix):].isdigit())
        return [json.loads(self.read(prefix + str(n)).decode("utf-8-sig")) for n in nums]

    def marker(self, package, name):
        key = package + "/" + name
        if key not in self.entries:
            return None
        return json.loads(self.read(key).decode("utf-8-sig"))

    def patches(self, package):
        out = {}
        for k in self.entries:
            parts = k.split("/")
            if len(parts) == 2 and parts[0] == package and parts[1].endswith(".patch"):
                out[parts[1][:-6].upper()] = k
        return out


def merge_order(lists):
    """Merge several ordered hash lists into one order that keeps every list's relative order."""
    order = []
    for seq in lists:
        last = -1
        for h in seq:
            if h in order:
                last = order.index(h)
            else:
                order.insert(last + 1, h)
                last += 1
    return order


class Rebuilder:
    def __init__(self, work, xdelta, log):
        self.work = work
        self.xdelta = xdelta
        self.log = log
        self.blobs = os.path.join(work, "blobs")
        self.tmp = tempfile.mkdtemp(prefix="rbh", dir=work)

    def blob_path(self, sha):
        return os.path.join(self.blobs, sha[:2], sha)

    def has_blob(self, sha):
        return os.path.exists(self.blob_path(sha))

    def add_blob(self, sha, src):
        dest = self.blob_path(sha)
        os.makedirs(os.path.dirname(dest), exist_ok=True)
        os.replace(src, dest)

    def apply_delta(self, source_sha, patch_file, out):
        # xdelta3 3.0u fails on paths over 260 characters, so all paths are short temp paths.
        src = os.path.join(self.tmp, "s")
        shutil.copyfile(self.blob_path(source_sha), src)
        if os.path.exists(out):
            os.remove(out)
        r = subprocess.run([self.xdelta, "-f", "-d", "-s", src, patch_file, out], capture_output=True)
        return r.returncode == 0 and not r.stderr and os.path.exists(out)

    def rebuild_change(self, ch, located, history):
        """located: list of (source, member) holding this change's patch file."""
        want = ch["VersionHash"].upper()
        if self.has_blob(want):
            return "ok"
        if not located:
            return "placeholder" if ch.get("Composite") else "missing"
        src, member = located[0]
        pf = os.path.join(self.tmp, "p")
        src.extract(member, pf)
        if src.head(member, 3) != VCDIFF_MAGIC:
            if sha1_file(pf) == want:
                self.add_blob(want, pf)
                return "ok"
            return "bad-seed"
        out = os.path.join(self.tmp, "o")
        # Try earlier versions of this path, newest first, then any other version of the same file name.
        for cand in history:
            if not self.has_blob(cand):
                continue
            if self.apply_delta(cand, pf, out) and sha1_file(out) == want:
                self.add_blob(want, out)
                return "ok"
        return "no-source"


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--work", required=True)
    ap.add_argument("--xdelta", required=True)
    ap.add_argument("--include-dvd-data", action="store_true")
    ap.add_argument("--package", action="append", help="only these packages")
    ap.add_argument("sources", nargs="+")
    a = ap.parse_args()

    os.makedirs(os.path.join(a.work, "index"), exist_ok=True)
    logf = open(os.path.join(a.work, "rebuild.log"), "a", encoding="utf-8")

    def log(msg):
        print(msg)
        logf.write(msg + "\n")
        logf.flush()

    sources = sorted((Source(p) for p in a.sources), key=lambda s: s.date)
    log("sources: " + ", ".join(f"{s.name} ({s.date})" for s in sources))
    packages = sorted({p for s in sources for p in s.packages()})
    if a.package:
        packages = [p for p in packages if p in a.package]
    elif not a.include_dvd_data:
        packages = [p for p in packages if p != "dvd-data"]

    rb = Rebuilder(a.work, a.xdelta, log)
    try:
        for pkg in packages:
            revs = {}        # hash -> revision json (first seen)
            seen_in = {}     # hash -> [source names]
            lists = []
            patches = {}     # patch key -> [(source, member)]
            state = {}       # source name -> {active, current, label, revisions}
            for s in sources:
                rl = s.revlog(pkg)
                if not rl and not s.patches(pkg):
                    continue
                lists.append([r["Hash"].upper() for r in rl])
                for r in rl:
                    h = r["Hash"].upper()
                    revs.setdefault(h, r)
                    seen_in.setdefault(h, []).append(s.name)
                    if r.get("Label") and not revs[h].get("Label"):
                        revs[h]["Label"] = r["Label"]
                for key, member in s.patches(pkg).items():
                    patches.setdefault(key, []).append((s, member))
                state[s.name] = {"date": str(s.date), "revisions": len(rl),
                                 "active": s.marker(pkg, ".active"), "current": s.marker(pkg, ".current"),
                                 "label": s.marker(pkg, ".label")}
            order = merge_order(lists)

            # Rebuild; repeat while progress is made, since a delta's source can come from a later pass.
            per_path = {}  # path -> [VersionHash in revision order]
            for h in order:
                for ch in revs[h]["Changes"]:
                    p = ch["TargetPath"] + "\\" + ch["TargetName"]
                    per_path.setdefault(p, []).append(ch["VersionHash"].upper())
            status = {}
            for _ in range(5):
                progress = False
                for h in order:
                    for ch in revs[h]["Changes"]:
                        key = (h, ch["TargetPath"], ch["TargetName"])
                        if status.get(key) == "ok":
                            continue
                        p = ch["TargetPath"] + "\\" + ch["TargetName"]
                        located = patches.get(h + "-" + path_hash(ch["TargetPath"], ch["TargetName"]), [])
                        hist = per_path[p]
                        idx = hist.index(ch["VersionHash"].upper())
                        cands = list(reversed(hist[:idx])) + [v for v in hist[idx + 1:]]
                        st = rb.rebuild_change(ch, located, cands)
                        if st == "ok" and status.get(key) != "ok":
                            progress = True
                        status[key] = st
                if not progress:
                    break

            out_revs = []
            counts = {}
            for i, h in enumerate(order):
                r = revs[h]
                changes = []
                for ch in r["Changes"]:
                    key = (h, ch["TargetPath"], ch["TargetName"])
                    located = patches.get(h + "-" + path_hash(ch["TargetPath"], ch["TargetName"]), [])
                    st = status[key]
                    counts[st] = counts.get(st, 0) + 1
                    changes.append({"path": ch["TargetPath"], "name": ch["TargetName"],
                                    "version": ch["VersionHash"].upper(), "seed": ch.get("IsSeed"),
                                    "composite": ch.get("Composite"), "status": st,
                                    "patch_date": min((s.mtime(m) for s, m in located), default=None),
                                    "patch_in": [s.name for s, _ in located]})
                out_revs.append({"index": i, "hash": h, "label": r.get("Label"), "seen_in": seen_in[h],
                                 "changes": changes})
            with open(os.path.join(a.work, "index", pkg + ".json"), "w", encoding="utf-8") as f:
                json.dump({"package": pkg, "sources": state, "revisions": out_revs}, f, indent=1)
            log(f"{pkg}: {len(order)} revisions, changes {counts}")
    finally:
        shutil.rmtree(rb.tmp, ignore_errors=True)


if __name__ == "__main__":
    main()
