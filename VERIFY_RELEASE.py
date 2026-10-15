#!/usr/bin/env python3
"""Offline structural check only; not a malware scan. Standard library only."""
import argparse
import hashlib
import pathlib
import struct
import sys
import zipfile

EXPECTED_EXE = "f06ed9eb07ada7234d2dc1e094ddbf9da8bfa507e5c7ea96ffcccfd4ee930940"
EXPECTED_DLL = "8b29a5bbdc9bb6052206a05ee452265b449ff882613d80974bf1fe404cb36c75"
EXE_NAME = "Rayman2Randomizer_Beta0.1.5_Public.exe"

def sha256(b):
    return hashlib.sha256(b).hexdigest()

def machine(data):
    if len(data) < 512 or data[:2] != b"MZ":
        raise ValueError("Missing MZ header")
    pe = struct.unpack_from("<I", data, 0x3c)[0]
    if pe + 24 > len(data) or data[pe:pe+4] != b"PE\0\0":
        raise ValueError("Invalid PE header")
    return struct.unpack_from("<H", data, pe + 4)[0], struct.unpack_from("<H", data, pe+22)[0]

def main():
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument("release", help="Path to published release ZIP or EXE")
    p.add_argument("--dll", help="Optional path to the original compiled Rayman2Randomizer.dll")
    a = p.parse_args()
    path = pathlib.Path(a.release)
    if zipfile.is_zipfile(path):
        with zipfile.ZipFile(path) as z:
            names = z.namelist()
            print("Archive entries:", ", ".join(names))
            if any(n.lower().endswith((".zip", ".7z", ".rar")) for n in names):
                print("Warning: nested archive detected")
            binary = z.read(EXE_NAME)
    else:
        binary = path.read_bytes()
    print("EXE SHA256:", sha256(binary))
    print("Matches submitted EXE:", sha256(binary) == EXPECTED_EXE)
    print("EXE PE machine/flags:", machine(binary))
    if a.dll:
        dll = pathlib.Path(a.dll).read_bytes()
        print("Native DLL SHA256:", sha256(dll))
        print("Matches submitted DLL:", sha256(dll)==EXPECTED_DLL)
        print("Native DLL PE machine/flags:",machine(dll))
        print("Exact DLL embedded in EXE:", dll in binary)
    else:
        print("For exact embedded DLL check, pass --dll path/to/Rayman2Randomizer.dll")

if __name__ == "__main__":
    main()
