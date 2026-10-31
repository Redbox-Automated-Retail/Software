"""Copy the original assemblies of one history release out of a rebuilt update store.

Reads RELEASE.md of a commit on a history/* branch (written by decompile_history.py) and copies every assembly
it lists from <work>/blobs into <out>, under its shipped file name, checking each file's SHA-1. The result is a
reference folder for building old projects against the libraries they were compiled with (see
docs/decompilation.md, "Known gaps"). Nothing is written into the repository.

Usage:
  python -I export_release.py --work <dir> --commit <history commit or ref> --out <dir>
"""
import argparse
import hashlib
import os
import re
import shutil
import subprocess
import sys

ROW = re.compile(r"^\| `([^`]+)` \| ([^|]*) \| ([^|]*) \| `([0-9A-F]{40})` \| (\w+) \|$")
REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))


def export(work, commit, out):
    """Copy the release's assemblies into out; returns {file name: assembly name}."""
    text = subprocess.run(["git", "show", commit + ":RELEASE.md"], cwd=REPO, capture_output=True, text=True,
                          encoding="utf-8", check=True).stdout
    os.makedirs(out, exist_ok=True)
    names = {}
    for line in text.splitlines():
        m = ROW.match(line)
        if not m or not m.group(1).lower().endswith((".dll", ".exe")):
            continue
        name, sha = m.group(1), m.group(4)
        src = os.path.join(work, "blobs", sha[:2], sha)
        with open(src, "rb") as f:
            if hashlib.sha1(f.read()).hexdigest().upper() != sha:
                sys.exit(f"SHA-1 mismatch: {src}")
        shutil.copyfile(src, os.path.join(out, name))
        names[name] = m.group(2).strip()
    return names


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--work", required=True, help="work directory of rebuild_store.py (holds blobs/)")
    ap.add_argument("--commit", required=True, help="commit on a history/* branch, e.g. a release's hash")
    ap.add_argument("--out", required=True, help="folder to copy the assemblies into")
    a = ap.parse_args()
    print(f"{len(export(a.work, a.commit, a.out))} assemblies -> {a.out}")


if __name__ == "__main__":
    main()
