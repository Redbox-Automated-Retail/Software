"""Decompile every recoverable release and commit each one to a history branch.

Input is the work directory of rebuild_store.py (blobs/, index/) and history.py (surfaces.jsonl).
Packages are grouped (HAL, MS HAL Tester and HAL Utilities ship together) and each group gets an orphan
branch `history/<group>` with one commit per release, oldest first, dated with the release date:

  src/<area>/<project>/   first-party assemblies, decompiled like the baseline (ilspycmd -p --decompile-baml
                          --ignore-decompilation-errors) and laid out by tools/decompile/organize.py
  scripts/                Lua (*.lua) and HAL (*.hs) scripts shipped by the package
  bundles/<bundle>/       text entries (Lua, resource XML) of Lua bundles; images, fonts and Flash are left out
  RELEASE.md              release name, package revisions, every file with version and SHA-1

A release is a revision that brought at least one new first-party assembly, script or bundle. Files whose
version at that revision has no content in any snapshot are taken at their last recoverable version and
marked "stale" in RELEASE.md. Each assembly is decompiled against the release's own assemblies plus what every
other package had shipped by then, and the tools/fixes scripts are applied to the result, as on main.

Commit subjects are "<Code Name> (<version of the package's main assembly>)" or "<version> (no code name)";
the store labels are kept in the body. Each release also gets a local annotated tag <group>/<Code-Name>
(<group>/<version> for versions that never had a code name). Nothing is pushed.

Usage:
  python -I decompile_history.py --work <dir> [--group engine ...] [--jobs 8] [--decompile-only] [--list]
"""
import argparse
import collections
import concurrent.futures
import datetime
import hashlib
import json
import os
import re
import shutil
import subprocess
import sys
import zipfile

import _winapi

TOOLS = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
REPO = os.path.dirname(TOOLS)

GROUPS = {
    "engine": ["kop-v2-engine"],
    "hal": ["kop-hal-install", "kop-ms-hal-tester", "kop-tools"],
    "device-service": ["device-service"],
    "kiosk-client": ["kioskclient"],
    "update-client": ["updateclient"],
    "update-manager": ["kop-update-manager"],
    "rental-bundles": ["kop-rental-bundles"],
}
SCRIPT_EXT = (".lua", ".hs")
BUNDLE_SKIP_EXT = (".png", ".jpg", ".jpeg", ".gif", ".bmp", ".ico", ".ttf", ".otf", ".swf", ".flv", ".wav",
                   ".mp3", ".mp4", ".wmv", ".avi")
LABEL_PREFIX = re.compile(r"^(kop-)?(v2-engine|hal-install|ms-hal-tester|tools|rental-bundles|update-manager|"
                          r"scripts|kioskclient|updateclient|device-service)-", re.I)
SECRET = re.compile(r"(password\w*|passwd|pwd|secret\w*|api_?key)(\W{1,4})([^\s'\"<>,;]{4,})", re.I)


def blob(work, sha):
    return os.path.join(work, "blobs", sha[:2], sha)


# Primary assembly per package: its version names a release in commit subjects and tags.
PRIMARY = {
    "kop-v2-engine": ("kioskengine", "Kiosk Engine"),
    "kop-hal-install": ("halsvc-win32", "HAL"),
    "kop-ms-hal-tester": ("MSHALTester", "MS HAL Tester"),
    "kop-tools": ("HalUtilities", "HAL Utilities"),
    "device-service": ("DeviceService.WebApi", "Device Service"),
    "kioskclient": ("KioskClientService.API", "Kiosk Client Service"),
    "updateclient": ("UpdateClientService.API", "Update Client Service"),
    "kop-update-manager": ("updatemgrd", "Update Manager"),
    "kop-rental-bundles": (None, "Rental bundles"),
}
# Labels that are not code names (a build number, a version, "NEW").
NOT_A_NAME = re.compile(r"^(\d[\d.]*|new)$", re.I)
NAME_FIX = {"AI": "Alien", "Caddyshack 1.0": "Caddyshack", "The-Martian": "The Martian"}


def pretty_name(label):
    """Readable code name for a store label: venomSR2 -> Venom SR2, AquamanSR2023 -> Aquaman 2023 SR,
    KISS1.1 -> KISS 1.1; None for labels that are not names."""
    n = LABEL_PREFIX.sub("", label.strip())
    if NOT_A_NAME.match(n):
        return None
    n = NAME_FIX.get(n, n)
    n = re.sub(r"SR(20\d\d)$", r"\1SR", n)
    n = n.replace("VanHalen", "Van Halen")
    n = re.sub(r"(?<=[A-Za-z])(20\d\d)", r" \1", n)
    n = re.sub(r"(?<=[a-z0-9])SR(\d*)$", r" SR\1", n)
    n = re.sub(r"(?<=[A-Za-z])(\d+\.\d+)$", r" \1", n)
    return n[0].upper() + n[1:]


def release_name(labels, versions=None):
    names = [n for n in (pretty_name(l) for l in labels) if n]
    return " / ".join(dict.fromkeys(names)) if names else None


def vkey(version):
    return tuple(int(x) if x.isdigit() else 0 for x in (version or "0").split("."))


class Work:
    def __init__(self, work):
        self.work = work
        self.surfaces = {}
        with open(os.path.join(work, "surfaces.jsonl"), encoding="utf-8") as f:
            for line in f:
                s = json.loads(line)
                self.surfaces[os.path.basename(s["file"])] = s
        self.firstparty = set(open(os.path.join(TOOLS, "decompile", "firstparty.txt"), encoding="utf-8").read().split())
        self.index = {}
        for fn in os.listdir(os.path.join(work, "index")):
            d = json.load(open(os.path.join(work, "index", fn), encoding="utf-8"))
            self.index[d["package"]] = d

    def version(self, sha):
        s = self.surfaces.get(sha)
        return s["version"] if s else None

    def pool(self):
        """[(date, file name, sha)] of every rebuilt managed file in every package, by date."""
        if not hasattr(self, "_pool"):
            self._pool = sorted((rev_date(idx, r), c["name"], c["version"]) for idx in self.index.values()
                                for r in idx["revisions"] for c in r["changes"]
                                if c["status"] == "ok" and c["version"] in self.surfaces)
        return self._pool

    def assembly(self, sha):
        s = self.surfaces.get(sha)
        return s["name"] if s else None

    def is_firstparty(self, sha):
        n = self.assembly(sha)
        return bool(n) and not n.endswith(".resources") and (n in self.firstparty or n.lower().startswith("redbox"))

    def is_code(self, name, sha):
        if self.is_firstparty(sha):
            return True
        n = name.lower()
        return n.endswith(SCRIPT_EXT) or n.endswith(".bundle") and not n.startswith("content-")

    def deletions(self):
        """[(date, path suffix)] for every kernel.filedelete("${VAR}\\...") in any package's Lua scripts.

        The store never lists a file as removed; packages delete obsolete files from their activation scripts
        instead (the engine's afteractivate.lua removes the 2014 ticket services, for example)."""
        if not hasattr(self, "_deletions"):
            self._deletions = []
            for idx in self.index.values():
                for r in idx["revisions"]:
                    for c in r["changes"]:
                        if c["status"] != "ok" or not c["name"].lower().endswith(".lua"):
                            continue
                        with open(blob(self.work, c["version"]), "rb") as f:
                            text = f.read().decode("utf-8", "replace")
                        for p in re.findall(r'kernel\.filedelete\(\s*"([^"]+)"', text):
                            p = p.replace("\\\\", "\\").lower()
                            if p.startswith("${") and "}\\" in p:
                                self._deletions.append((rev_date(idx, r), "\\" + p.split("}\\", 1)[1]))
        return self._deletions


def rev_date(idx, r):
    """When a revision reached the kiosks: its earliest content file, else noon of the first snapshot listing it."""
    dates = sorted(c["patch_date"] for c in r["changes"] if c["status"] == "ok" and c["patch_date"])
    return dates[0] if dates else idx["sources"][r["seen_in"][0]]["date"] + "T12:00:00"


def drop_deleted(w, tree, set_dates, date):
    """The tree without files that a script in effect by `date` deleted after they were last listed."""
    gone = {}
    for d, suffix in w.deletions():
        if d <= date:
            gone[suffix] = max(d, gone.get(suffix, ""))
    out = {}
    for key, v in tree.items():
        low = key.lower()
        hit = [d for s, d in gone.items() if low.endswith(s)]
        if hit and set_dates[key] < max(hit):
            continue
        out[key] = v
    return out


def releases(w, group):
    """Ordered releases of a group: list of dicts with date, name, labels, revisions, tree."""
    events = []
    for order, pkg in enumerate(GROUPS[group]):
        idx = w.index[pkg]
        state = {}     # path -> (sha, rev index) of the version listed at this point
        last_ok = {}   # path -> sha of the last version with content
        committed = {}
        labels = []
        last = None
        pkg_events = []
        set_dates = {}  # path -> date of the revision that last listed it
        # Paths are case-insensitive: the store moved from "redbox\REDS" to "Redbox\REDS" around 2016, and the
        # old spelling must not survive as a second copy of every file.
        canon = {}      # lower-case path -> spelling of its latest listing
        for r in idx["revisions"]:
            for c in r["changes"]:
                key = (c["path"] + "\\" + c["name"]).lower()
                canon[key] = c["path"] + "\\" + c["name"]
                state[key] = c["version"]
                set_dates[key] = rev_date(idx, r)
                if c["status"] == "ok":
                    last_ok[key] = c["version"]
            if r["label"]:
                labels.append(r["label"])
            fresh = [k for k, v in last_ok.items() if committed.get(k) != v and w.is_code(k.rsplit("\\", 1)[1], v)]
            tree = {canon[k]: (last_ok.get(k), v) for k, v in state.items()}
            sets = {canon[k]: d for k, d in set_dates.items()}
            if not fresh:
                # A label on a revision that adds no code names the code already committed (the release was
                # built up over unlabelled revisions and then labelled): move it to the previous release.
                if labels and last:
                    last.update(rev=r["index"], hash=r["hash"], tree=tree, set=sets)
                    last["labels"] += labels
                    labels = []
                continue
            last = {"date": rev_date(idx, r), "order": order, "package": pkg, "rev": r["index"], "hash": r["hash"],
                    "labels": labels, "tree": tree, "set": sets}
            pkg_events.append(last)
            committed = dict(last_ok)
            labels = []
        # Revision order is the truth within a package; dates only place releases among the group's packages.
        # A patch file's date can be much later than the release (content kept only in a later snapshot), so
        # first pull each date back to its successor's, then push early fallback dates up to their predecessor's.
        for a, b in zip(reversed(pkg_events[:-1]), reversed(pkg_events[1:])):
            a["date"] = min(a["date"], b["date"])
        for a, b in zip(pkg_events, pkg_events[1:]):
            b["date"] = max(a["date"], b["date"])
        events += pkg_events
    events.sort(key=lambda e: (e["date"], e["order"]))  # stable: keeps each package's revision order
    # Combined groups: each release carries the latest tree of every package in the group.
    out, trees, revs = [], {}, {}
    for e in events:
        trees[e["package"]] = drop_deleted(w, e["tree"], e["set"], e["date"])
        revs[e["package"]] = (e["rev"], e["hash"])
        out.append({"date": e["date"], "package": e["package"], "labels": e["labels"],
                    "revisions": dict(revs), "trees": {p: dict(t) for p, t in trees.items()}})
    # Keep dates monotonic (a delayed download must not move a release before its predecessor).
    for a, b in zip(out, out[1:]):
        if b["date"] < a["date"]:
            b["date"] = a["date"]
    return out


def code_files(w, rel):
    """name -> (sha, listed sha, package, path) for the code files of a release; one entry per file name."""
    files = {}
    for pkg, tree in rel["trees"].items():
        for key, (ok, listed) in tree.items():
            if not ok:
                continue
            name = key.rsplit("\\", 1)[1]
            if not w.is_code(name, ok):
                continue
            ident = w.assembly(ok) if w.is_firstparty(ok) else name.lower()
            # The same assembly in two folders (bin and the 2014 kext copies): keep the higher version.
            if ident in files and vkey(w.version(files[ident][0])) >= vkey(w.version(ok)):
                continue
            files[ident] = (ok, listed, pkg, key)
    return files


def link(src, dst):
    if not os.path.exists(dst):
        try:
            os.link(src, dst)
        except OSError:
            shutil.copyfile(src, dst)


def build_allrefs(w):
    """One copy of every managed file name (newest version) as a fallback reference directory."""
    d = os.path.join(w.work, "allrefs")
    if os.path.isdir(d):
        return d
    os.makedirs(d)
    newest = {}
    for pkg, idx in w.index.items():
        for r in idx["revisions"]:
            for c in r["changes"]:
                if c["status"] == "ok" and c["version"] in w.surfaces:
                    newest[c["name"].lower()] = (c["name"], c["version"])
    for name, sha in newest.values():
        link(blob(w.work, sha), os.path.join(d, name))
    return d


def refs_dir(w, rel):
    """Reference folder for decompiling a release: its own assemblies (highest version of each name), and for
    everything else the newest version any package had shipped by the release date (HAL client, Device
    Service client, shared libraries, third-party DLLs)."""
    key = hashlib.sha1(json.dumps(rel["revisions"], sort_keys=True).encode()).hexdigest()[:12]
    d = os.path.join(w.work, "refs", key)
    if os.path.isdir(d):
        return d
    pick = {}
    for date, name, sha in w.pool():
        if date > rel["date"]:
            break
        pick[name.lower()] = (name, sha)
    own = {}
    for tree in rel["trees"].values():
        for k, (ok, _) in tree.items():
            if ok and ok in w.surfaces:
                name = k.rsplit("\\", 1)[1]
                if name.lower() not in own or vkey(w.version(ok)) > vkey(w.version(own[name.lower()][1])):
                    own[name.lower()] = (name, ok)
    pick.update(own)
    os.makedirs(d + ".tmp", exist_ok=True)
    for name, sha in pick.values():
        link(blob(w.work, sha), os.path.join(d + ".tmp", name))
    os.rename(d + ".tmp", d)
    return d


def decompile(w, sha, name, refs, allrefs):
    out = os.path.join(w.work, "decomp", sha)
    if os.path.exists(os.path.join(out, ".done")):
        return sha, True, ""
    shutil.rmtree(out, ignore_errors=True)
    os.makedirs(out)
    tmp = os.path.join(w.work, "decomp-in", sha)
    os.makedirs(tmp, exist_ok=True)
    asm = os.path.join(tmp, name)
    link(blob(w.work, sha), asm)
    r = subprocess.run(["ilspycmd", "-p", "--decompile-baml", "--ignore-decompilation-errors",
                        "-r", refs, "-r", allrefs, "-o", out, asm], capture_output=True, text=True,
                       encoding="utf-8", errors="replace")
    shutil.rmtree(tmp, ignore_errors=True)
    ok = r.returncode == 0 and any(f.endswith(".csproj") for f in os.listdir(out))
    if ok:
        open(os.path.join(out, ".done"), "w").close()
    return sha, ok, (r.stderr or r.stdout)[-2000:]


def junction(target, path):
    if os.path.lexists(path):
        os.unlink(path)
    _winapi.CreateJunction(target, path)


def run_organize(w, group, files, src_out):
    """Lay the release's decompiled assemblies out like src/ with tools/decompile/organize.py."""
    owork = os.path.join(w.work, "organize", group)
    raw = os.path.join(owork, "raw")
    if os.path.isdir(raw):
        for x in os.listdir(raw):
            os.unlink(os.path.join(raw, x))
    os.makedirs(raw, exist_ok=True)
    for ident, (sha, _, _, _) in files.items():
        if w.is_firstparty(sha):
            junction(os.path.join(w.work, "decomp", sha), os.path.join(raw, ident))
    env = dict(os.environ, REDBOX_DECOMP_WORK=owork, REDBOX_SRC_OUT=src_out, REDBOX_FIRSTPARTY_FROM_RAW="1")
    os.makedirs(src_out, exist_ok=True)
    r = subprocess.run([sys.executable, "-I", os.path.join(TOOLS, "decompile", "organize.py")], env=env,
                       capture_output=True, text=True, encoding="utf-8", errors="replace")
    for dp, _, fn in os.walk(src_out):  # the decompile cache's completion markers are not source
        if ".done" in fn:
            os.remove(os.path.join(dp, ".done"))
    # The same mechanical fixes for decompiler artifacts that src/ on main had (tools/fixes); they are
    # idempotent and apply to every release alike, so diffs between releases stay clean.
    for fix in ("fix_com_event_providers.py", "fix_xaml_markup.py", "remove_wpf_generated_members.py"):
        subprocess.run([sys.executable, "-I", os.path.join(TOOLS, "fixes", fix), src_out], capture_output=True)
    return r.returncode == 0, (r.stderr or r.stdout)[-3000:]


def write_text_entries(zip_path, dest):
    with zipfile.ZipFile(zip_path) as z:
        for i in z.infolist():
            if i.is_dir() or i.filename.lower().endswith(BUNDLE_SKIP_EXT):
                continue
            data = z.read(i)
            try:
                data.decode("utf-8")
            except UnicodeDecodeError:
                continue
            p = os.path.join(dest, *i.filename.split("/"))
            os.makedirs(os.path.dirname(p), exist_ok=True)
            with open(p, "wb") as f:
                f.write(data)


def git(cwd, *args, env=None):
    r = subprocess.run(["git", *args], cwd=cwd, capture_output=True, text=True, encoding="utf-8", env=env)
    if r.returncode != 0:
        raise RuntimeError(f"git {' '.join(args)}: {r.stderr}")
    return r.stdout


def commit_group(w, group, rels, log):
    branch = "history/" + group
    wt = os.path.join(w.work, "git", group)
    if not os.path.isdir(wt):
        git(REPO, "worktree", "add", "--orphan", "-b", branch, wt)
    try:
        head = git(wt, "log", "--format=%(trailers:key=Release-Key,valueonly)")
        done = set(head.split())
    except RuntimeError:
        done = set()
    prev = {}
    for n, rel in enumerate(rels):
        key = hashlib.sha1(json.dumps(rel["revisions"], sort_keys=True).encode()).hexdigest()[:12]
        files = code_files(w, rel)
        changed = [(i, w.surfaces[v[0]]["version"] if v[0] in w.surfaces else v[0][:8]) for i, v in sorted(files.items())
                   if prev.get(i) != v[0]]
        prev = {i: v[0] for i, v in files.items()}
        if key in done:
            continue
        for x in os.listdir(wt):
            if x != ".git":
                p = os.path.join(wt, x)
                shutil.rmtree(p) if os.path.isdir(p) else os.remove(p)
        notes = []
        if any(w.is_firstparty(v[0]) for v in files.values()):
            ok, out = run_organize(w, group, files, os.path.join(wt, "src"))
            if not ok:
                notes.append("organize.py failed; raw ILSpy output is under src/Raw.")
                log(f"{group} #{n}: organize failed: {out}")
                shutil.rmtree(os.path.join(wt, "src"), ignore_errors=True)
                for ident, (sha, _, _, _) in files.items():
                    if w.is_firstparty(sha):
                        shutil.copytree(os.path.join(w.work, "decomp", sha), os.path.join(wt, "src", "Raw", ident),
                                        ignore=shutil.ignore_patterns(".done"))
        for ident, (sha, listed, pkg, fkey) in files.items():
            name = fkey.rsplit("\\", 1)[1]
            if name.lower().endswith(SCRIPT_EXT):
                d = os.path.join(wt, "scripts", pkg)
                os.makedirs(d, exist_ok=True)
                shutil.copyfile(blob(w.work, sha), os.path.join(d, name))
            elif name.lower().endswith(".bundle"):
                write_text_entries(blob(w.work, sha), os.path.join(wt, "bundles", name[:-7]))
        # Scripts and bundles: replace anything that looks like a credential. (Decompiled code is kept as it is,
        # like src/ on main.)
        for top in ("scripts", "bundles"):
            for root, _, fns in os.walk(os.path.join(wt, top)):
                for fn in fns:
                    p = os.path.join(root, fn)
                    t = open(p, encoding="utf-8", errors="replace").read()
                    t2 = SECRET.sub(r"\1\2<redacted>", t)
                    if t2 != t:
                        open(p, "w", encoding="utf-8").write(t2)
                        notes.append(f"Redacted a credential-like value in `{os.path.relpath(p, wt)}`.")
        name = release_name(rel["labels"])
        versions = sorted((w.assembly(v[0]), w.surfaces[v[0]]["version"]) for v in files.values() if w.is_firstparty(v[0]))
        primary, product = PRIMARY[rel["package"]]
        main_ver = next((v for a, v in versions if a == primary), None)
        ref = main_ver or rel["date"][:10]
        if name:
            title = f"{name} ({ref})"
        elif main_ver:
            title = f"{main_ver} (no code name)"
        else:
            title = f"{product} revision {rel['revisions'][rel['package']][0]} (no code name)"
        lines = [f"# {title}", "", f"- Product: {product}" + (f" {main_ver}" if main_ver else ""),
                 f"- Date (first download in the snapshots): {rel['date'][:10]}",
                 f"- Store labels: {', '.join(l.strip() for l in rel['labels']) or 'none'}", "- Package revisions:"]
        for pkg, (ri, rh) in rel["revisions"].items():
            lines.append(f"  - `{pkg}` revision {ri} (`{rh}`)")
        lines += ["", "| File | Assembly | Version | SHA-1 | Status |", "|---|---|---|---|---|"]
        for ident, (sha, listed, pkg, fkey) in sorted(files.items(), key=lambda kv: kv[1][3].lower()):
            s = w.surfaces.get(sha)
            status = "exact" if sha == listed else f"stale (listed version {listed[:10]}… has no content)"
            lines.append(f"| `{fkey.rsplit(chr(92), 1)[1]}` | {s['name'] if s else ''} | {s['version'] if s else ''} | `{sha}` | {status} |")
        if notes:
            lines += ["", "## Notes", ""] + [f"- {x}" for x in dict.fromkeys(notes)]
        lines += ["", "Generated by `tools/history/decompile_history.py`; see `docs/version-history.md` on main."]
        with open(os.path.join(wt, "RELEASE.md"), "w", encoding="utf-8") as f:
            f.write("\n".join(lines) + "\n")
        git(wt, "add", "-A")
        msg = (f"{title}\n\nStore labels: {', '.join(l.strip() for l in rel['labels']) or 'none'}\n"
               f"Package revisions: " + ", ".join(f"{p} {ri}" for p, (ri, _) in rel["revisions"].items())
               + "\n\nChanged: " + ", ".join(f"{i} {v}" for i, v in changed) + "\n\nAll assemblies:\n"
               + "\n".join(f"{a} {v}" for a, v in versions)
               + f"\n\nRelease-Product: {product}\nRelease-Name: {name or '-'}\nRelease-Version: {main_ver or '-'}"
               f"\nRelease-Key: {key}\n")
        date = rel["date"][:19]
        env = dict(os.environ, GIT_AUTHOR_DATE=date, GIT_COMMITTER_DATE=date)
        git(wt, "commit", "-q", "--allow-empty", "-m", msg, env=env)
        log(f"{group} #{n} {rel['date'][:10]} {title}: committed")


def tag_slug(name):
    s = re.sub(r"\s*/\s*", "+", name)
    return re.sub(r"[^A-Za-z0-9.+-]+", "-", s).strip("-")


def tag_group(group, log):
    """One annotated tag per release on history/<group>, named <group>/<Code-Name> (or <group>/<version> when
    a version never got a code name). A release built up over several commits is tagged at its last one;
    commits without a code name are tagged only when no named release has their version."""
    branch = "history/" + group
    fmt = "%H%x01%ad%x01%(trailers:key=Release-Name,valueonly)%x01%(trailers:key=Release-Version,valueonly)" \
          "%x01%(trailers:key=Release-Product,valueonly)%x02"
    rows = []
    for rec in git(REPO, "log", "--reverse", "--date=short", f"--format={fmt}", branch).split("\x02"):
        if rec.strip():
            h, date, name, ver, product = (x.strip() for x in rec.strip().split("\x01"))
            rows.append((h, date, "" if name == "-" else name, "" if ver == "-" else ver, product))
    named_versions = {v for _, _, n, v, _ in rows if n and v}
    last = {}  # tag label -> last commit carrying it (HAL, MS HAL Tester and Utilities share code names)
    for h, date, name, ver, product in rows:
        if name:
            last[name] = (h, date, name, ver, product, name)
        elif ver and ver not in named_versions:
            last[ver] = (h, date, name, ver, product, ver)
    picks = list(last.values())
    for t in git(REPO, "tag", "--list", group + "/*").split():
        git(REPO, "tag", "-d", t)
    used = set()
    for h, date, name, ver, product, label in picks:
        tag = f"{group}/{tag_slug(label)}"
        if tag in used:
            tag += "-" + (ver or date)
        used.add(tag)
        what = f"{product} {ver}".strip() if ver else product
        msg = f"{name or ver}\n\n{what}, {date}" + ("" if name else " (no code name)") + "\n"
        env = dict(os.environ, GIT_COMMITTER_DATE=date + "T12:00:00")
        git(REPO, "tag", "-a", tag, h, "-m", msg, env=env)
    log(f"{group}: {len(picks)} tags")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--work", required=True)
    ap.add_argument("--group", action="append")
    ap.add_argument("--jobs", type=int, default=8)
    ap.add_argument("--decompile-only", action="store_true")
    ap.add_argument("--list", action="store_true")
    a = ap.parse_args()
    w = Work(a.work)
    groups = a.group or list(GROUPS)
    logf = open(os.path.join(a.work, "decompile_history.log"), "a", encoding="utf-8")

    def log(m):
        print(m, flush=True)
        logf.write(m + "\n")
        logf.flush()

    plan = {g: releases(w, g) for g in groups}
    if a.list:
        for g, rels in plan.items():
            for n, rel in enumerate(rels):
                files = code_files(w, rel)
                print(g, n, rel["date"][:10], release_name(rel["labels"], files) or "-", len(files))
        return

    allrefs = build_allrefs(w)
    jobs = {}
    for g, rels in plan.items():
        for n, rel in enumerate(rels):
            for ident, (sha, _, _, key) in code_files(w, rel).items():
                if w.is_firstparty(sha) and sha not in jobs:
                    jobs[sha] = (key.rsplit("\\", 1)[1], refs_dir(w, rel))
    todo = {s: j for s, j in jobs.items() if not os.path.exists(os.path.join(a.work, "decomp", s, ".done"))}
    log(f"decompile: {len(jobs)} assemblies, {len(todo)} to do")
    failed = []
    with concurrent.futures.ThreadPoolExecutor(a.jobs) as ex:
        futs = [ex.submit(decompile, w, s, n, r, allrefs) for s, (n, r) in todo.items()]
        for i, f in enumerate(concurrent.futures.as_completed(futs), 1):
            sha, ok, err = f.result()
            if not ok:
                failed.append(sha)
                log(f"FAILED {sha} {w.assembly(sha)}: {err}")
            if i % 25 == 0:
                log(f"decompiled {i}/{len(todo)}")
    log(f"decompile done, {len(failed)} failed")
    if a.decompile_only:
        return
    for g, rels in plan.items():
        commit_group(w, g, rels, log)
        tag_group(g, log)


if __name__ == "__main__":
    main()
