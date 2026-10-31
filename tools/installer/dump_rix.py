"""Dump the resources of a RIX (Redbox Installer from XML) setup host.

usage: python dump_rix.py <setup.exe> <outdir> [--named]

Writes every PE resource as <outdir>/res/<TYPE>/<NAME>, and the decoded
installer script (PREREQ/SCRIPT, LZMA alone format) as <outdir>/script.xml.
With --named it also copies every embedded <file> to <outdir>/named/<file name
of its source-path>, the layout the Backend repo's
provisioning/installer/tests/rix-unpacked.ps1 expects.
Needs the pefile package. See docs/systems/store-installer.md section 2.1.
"""
import lzma
import ntpath
import os
import shutil
import struct
import sys
import xml.etree.ElementTree as ET

import pefile


def name_of(entry):
    return str(entry.name) if entry.name is not None else str(entry.id)


def lzma_alone(data):
    # Redbox.Compression LZMA: 5 property bytes, 8-byte little-endian size, raw stream.
    props = data[:5]
    size = struct.unpack("<q", data[5:13])[0]
    dec = lzma.LZMADecompressor(lzma.FORMAT_ALONE)
    out = dec.decompress(props + struct.pack("<q", -1) + data[13:])
    return out[:size] if size >= 0 else out


def main(exe, outdir):
    pe = pefile.PE(exe, fast_load=True)
    pe.parse_data_directories([pefile.DIRECTORY_ENTRY["IMAGE_DIRECTORY_ENTRY_RESOURCE"]])
    mm = pe.__data__
    for t in pe.DIRECTORY_ENTRY_RESOURCE.entries:
        tname = name_of(t)
        for n in t.directory.entries:
            nname = name_of(n)
            for lang in n.directory.entries:
                d = lang.data.struct
                off = pe.get_offset_from_rva(d.OffsetToData)
                blob = bytes(mm[off:off + d.Size])
                p = os.path.join(outdir, "res", tname, nname)
                os.makedirs(os.path.dirname(p), exist_ok=True)
                with open(p, "wb") as f:
                    f.write(blob)
                print(f"{tname}/{nname}\t{len(blob)}")
                if tname == "PREREQ" and nname == "SCRIPT":
                    with open(os.path.join(outdir, "script.xml"), "wb") as f:
                        f.write(lzma_alone(blob))


def name_files(outdir):
    # Each <file resource-name="NNNN" source-path="..."> is FILE/NNNN; only margin.bmp is compressed (lzma).
    root = ET.parse(os.path.join(outdir, "script.xml")).getroot()
    os.makedirs(os.path.join(outdir, "named"), exist_ok=True)
    for f in root.iter("file"):
        res = f.get("resource-name")
        if not res:
            continue
        src = os.path.join(outdir, "res", "FILE", res)
        dst = os.path.join(outdir, "named", ntpath.basename(f.get("source-path")))
        if f.get("compression") == "lzma":
            with open(src, "rb") as i, open(dst, "wb") as o:
                o.write(lzma_alone(i.read()))
        else:
            shutil.copyfile(src, dst)
        print(f"{res}\t{os.path.basename(dst)}")


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
    if "--named" in sys.argv[3:]:
        name_files(sys.argv[2])
