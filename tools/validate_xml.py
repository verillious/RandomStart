"""Validate mod XML and reject duplicate keyed translations.

Run from any directory with Python 3: python3 tools/validate_xml.py
No game installation or third-party packages are required.
"""

from pathlib import Path
import subprocess
import sys
import xml.etree.ElementTree as ET


def main():
    root = Path(__file__).resolve().parents[1]
    files = subprocess.check_output(
        ["git", "-C", str(root), "ls-files", "--cached", "--others", "--exclude-standard",
         "-z", "--", "*.xml", "*.csproj", "*.config"]
    ).decode().split("\0")
    deleted = set(subprocess.check_output(
        ["git", "-C", str(root), "ls-files", "--deleted", "-z"]
    ).decode().split("\0"))
    failures = []
    translations = {}
    checked = 0

    for name in (name for name in files if name and name not in deleted):
        path = root / name
        checked += 1
        try:
            document = ET.parse(path).getroot()
        except (ET.ParseError, OSError) as error:
            failures.append(f"{name}: {error}")
            continue

        if "Languages" not in path.parts or "Keyed" not in path.parts:
            continue
        if document.tag != "LanguageData":
            failures.append(f"{name}: expected LanguageData root, got {document.tag}")
            continue

        # Separate game versions and languages, but check across files within each.
        language = path.parts[: path.parts.index("Keyed")]
        keys = translations.setdefault(language, {})
        for entry in document:
            if entry.tag in keys:
                failures.append(f"{name}: duplicate key {entry.tag} (first in {keys[entry.tag]})")
            else:
                keys[entry.tag] = name

    if not checked:
        failures.append("No XML/project/config files found")
    for failure in failures:
        print(failure, file=sys.stderr)
    if failures:
        print(f"XML validation failed: {len(failures)} error(s)", file=sys.stderr)
        return 1
    print(f"Validated {checked} XML/project/config files; translation keys are unique.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
