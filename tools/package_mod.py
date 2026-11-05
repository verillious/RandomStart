"""Stage a freshly built 1.6 mod without modifying shipped files or bundling dependencies."""

import argparse
from pathlib import Path
import shutil
import tempfile
import xml.etree.ElementTree as ET
import zipfile


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--configuration", choices=("Debug", "Release"), default="Release")
    parser.add_argument("--output", type=Path, default=Path("artifacts/RandomStart-dev.zip"))
    parser.add_argument("--release-version", help="Produce a release package; otherwise create a 1.6-only development package")
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[1]
    assembly = root / "Source" / "bin" / args.configuration / "net472" / "RandomStartMod.dll"
    if not assembly.is_file():
        parser.error(f"Build the mod first; missing {assembly}")
    output = args.output.resolve()
    output.parent.mkdir(parents=True, exist_ok=True)

    with tempfile.TemporaryDirectory() as temporary:
        stage = Path(temporary) / "RandomStart"
        stage.mkdir()
        for folder in ("About", "1.6"):
            shutil.copytree(root / folder, stage / folder)
        shutil.copyfile(assembly, stage / "1.6" / "Assemblies" / "RandomStartMod.dll")

        # Do not carry the Workshop item's ID into a local test package.
        if not args.release_version:
            (stage / "About" / "PublishedFileId.txt").unlink(missing_ok=True)
        metadata = ET.parse(stage / "About" / "About.xml")
        if args.release_version:
            # 1.5 remains the existing frozen binary; only 1.6 is rebuilt.
            shutil.copytree(root / "1.5", stage / "1.5")
            shutil.copyfile(root / "LoadFolders.xml", stage / "LoadFolders.xml")
            metadata.find("packageId").text = "bogknight.RandomStart"
            metadata.find("modVersion").text = args.release_version
        else:
            metadata.find("name").text = "Random Start (Development)"
            metadata.find("packageId").text = "bogknight.RandomStartDev"
            metadata.find("modVersion").text = "0.0.0-dev"
            versions = metadata.find("supportedVersions")
            versions.clear()
            ET.SubElement(versions, "li").text = "1.6"
            (stage / "LoadFolders.xml").write_text(
                "<loadFolders><v1.6><li>1.6</li></v1.6></loadFolders>\n", encoding="utf-8"
            )
        metadata.write(stage / "About" / "About.xml", encoding="utf-8", xml_declaration=True)

        # Harmony is provided by its required mod. Never ship reference assemblies.
        for dll in stage.rglob("*.dll"):
            if dll.name != "RandomStartMod.dll":
                raise SystemExit(f"Unexpected dependency in package: {dll.relative_to(stage)}")
        with zipfile.ZipFile(output, "w", compression=zipfile.ZIP_DEFLATED) as archive:
            for path in sorted(stage.rglob("*")):
                if path.is_file():
                    archive.write(path, Path("RandomStart") / path.relative_to(stage))
    print(f"Packaged {output}")


if __name__ == "__main__":
    main()
