#!/usr/bin/env python3
# Appends link definitions for all merge requests ([!NNN][NNN]) and work items ([#NNN][NNN]) referenced in the given
# release notes file.
#
# Usage: scripts/add_release_notes_mr_links.py release-notes/v0.10.0.md

import re
import sys

MR_URL = "https://gitlab.perlang.org/perlang/perlang/merge_requests/{}"
WORK_ITEM_URL = "https://gitlab.perlang.org/perlang/perlang/-/work_items/{}"

path = sys.argv[1]

with open(path, encoding="utf-8") as f:
    text = f.read()

referenced = {int(n): MR_URL for n in re.findall(r"\[!(\d+)\]\[\1\]", text)}
referenced |= {int(n): WORK_ITEM_URL for n in re.findall(r"\[#(\d+)\]\[\1\]", text)}
defined = {int(n) for n in re.findall(r"^\[(\d+)\]:", text, re.MULTILINE)}
missing = sorted(referenced.keys() - defined)

if missing:
    text = text.rstrip("\n") + "\n\n"
    text += "".join(f"[{n}]: {referenced[n].format(n)}\n" for n in missing)

    with open(path, "w", encoding="utf-8") as f:
        f.write(text)
