#!/usr/bin/env python3
"""Repository hygiene checks for Weld Studio. Runs in CI without Unity.

Checks, over the files tracked by git:
  1. Every asset and folder under Assets/ has a .meta, and every .meta has its asset.
  2. No Resources/ folder exists under Assets/ (content must go through Addressables).
  3. Paid third-party assets (Magica Cloth 2) are never committed.
  4. Every file that .gitattributes routes to Git LFS is stored as an LFS pointer.

Usage: python3 Tools/ci/check_repo.py   (from anywhere inside the repository)
Exit code 0 when everything passes, 1 otherwise.
"""

import subprocess
import sys
from pathlib import PurePosixPath

LFS_POINTER_PREFIX = b"version https://git-lfs.github.com/spec/v1"
FORBIDDEN_PREFIXES = ("Assets/MagicaCloth2/", "Assets/MagicaCloth2.meta")


def git(*args, stdin=None):
    return subprocess.run(["git", *args], input=stdin, capture_output=True, check=True).stdout


def is_ignored_by_unity(path):
    # Unity skips hidden files/folders and names ending with "~" (and does not create .meta for them).
    return any(part.startswith(".") or part.endswith("~") for part in PurePosixPath(path).parts)


def check_meta_files(files):
    errors = []
    assets = [f for f in files if f.startswith("Assets/") and not is_ignored_by_unity(f)]
    present = set(assets)

    expected_metas = set()
    for path in assets:
        if path.endswith(".meta"):
            continue
        expected_metas.add(path + ".meta")
        for parent in PurePosixPath(path).parents:
            if str(parent) in (".", "Assets"):
                break
            expected_metas.add(f"{parent}.meta")

    for meta in sorted(expected_metas - present):
        errors.append(f"missing meta file: {meta}")

    folders = {str(p) for f in assets for p in PurePosixPath(f).parents}
    for meta in sorted(p for p in present if p.endswith(".meta")):
        target = meta[: -len(".meta")]
        if target not in present and target not in folders:
            errors.append(f"orphan meta file (asset was deleted or moved): {meta}")
    return errors


def check_resources_folders(files):
    found = {
        str(parent)
        for f in files
        if f.startswith("Assets/")
        for parent in PurePosixPath(f).parents
        if parent.name == "Resources"
    }
    return [f"Resources folder is forbidden, use Addressables: {folder}" for folder in sorted(found)]


def check_forbidden(files):
    return [f"paid third-party asset must not be committed: {f}" for f in files if f.startswith(FORBIDDEN_PREFIXES)]


def check_lfs_pointers(files):
    if not files:
        return []
    attrs = git("check-attr", "--stdin", "-z", "filter", stdin="\0".join(files).encode()).split(b"\0")
    lfs_files = [attrs[i].decode() for i in range(0, len(attrs) - 2, 3) if attrs[i + 2] == b"lfs"]

    errors = []
    for path in lfs_files:
        # Read the blob stored in the index: in CI the working tree holds the real (smudged) file.
        blob = git("cat-file", "blob", f":{path}")
        if not blob.startswith(LFS_POINTER_PREFIX):
            errors.append(f"binary committed without Git LFS (run `git lfs install` and re-add it): {path}")
    return errors


def main():
    files = [f for f in git("ls-files", "-z").decode().split("\0") if f]
    errors = (
        check_meta_files(files)
        + check_resources_folders(files)
        + check_forbidden(files)
        + check_lfs_pointers(files)
    )
    for error in errors:
        print(f"ERROR: {error}")
    print(f"{len(files)} tracked files checked, {len(errors)} problem(s) found.")
    return 1 if errors else 0


if __name__ == "__main__":
    sys.exit(main())
