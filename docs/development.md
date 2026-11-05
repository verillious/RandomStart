# Building and testing Random Start

## Requirements

- .NET SDK 8.0.427 (the version selected by `global.json`).
- Python 3.9 or later for asset validation and packaging.
- NuGet access for the first restore. No Steam installation or optional-mod DLLs
  are required to compile the 1.6 project.

The pinned RimRef package provides compilation signatures with Ludeon's
permission. It is not a runnable game. Harmony remains a required installed mod;
the build does not bundle another copy of it.

## Local or cloud checks

Run from the repository root:

```sh
dotnet restore Tests/RandomStartMod.Tests.csproj --locked-mode
python3 tools/validate_xml.py
dotnet test Tests/RandomStartMod.Tests.csproj -c Release --no-restore
dotnet build Source/RandomStartMod.csproj -c Debug --no-restore
python3 tools/package_mod.py
```

The test project builds the real mod, exercises the production reflection
boundary under .NET 8, and inspects the resulting assembly's dependency metadata.
It does not load RimWorld or prove that another mod's current fields and methods
match the integration. Test those integrations in the game.

Build outputs go to `Source/bin/<configuration>/net472/`. Packaging creates
`artifacts/RandomStart-dev.zip` using the newly built Release DLL and leaves
the tracked `1.6/Assemblies/RandomStartMod.dll` unchanged. The package identifies
itself as `Random Start (Development)`, supports only 1.6, and contains no
Workshop publishing ID. Select `--configuration Debug` to package a Debug build.

When intentionally upgrading a package, run restore without `--locked-mode`,
review both the dependency declaration and lockfile changes, then repeat the
checks. Commit both lockfiles. Routine CI restores use locked mode.

## Getting a build from GitHub

The **Build and test** workflow runs on pushes and pull requests and can also
be started manually. After it succeeds, download the
`RandomStart-1.6-development` artifact and extract its contained mod ZIP.
These workflow steps were validated locally; the GitHub-hosted run still needs
to execute after the changes are committed and pushed.

The release workflow now checks out the source, builds and tests it, and packages
the new DLL before attaching it to a published GitHub release. Manual runs
produce development artifacts only. Release packages retain the existing 1.5
binary unchanged; this build does not revalidate or maintain that version.
Nothing here uploads to Steam Workshop.

## First in-game check

1. Extract the `RandomStart` directory from the development ZIP into RimWorld's
   local `Mods` directory. Replace an older development copy rather than nesting
   a second `RandomStart` directory inside it.
2. Disable the Workshop version. Enable Harmony and `Random Start (Development)`
   after Core and the DLCs you are testing. Restart the game after changing DLLs.
3. Initially leave optional integrations out. Check startup logs, open settings
   with the Random button's right-click action, and start a default random game.
4. Verify the map, starting pawns and items; return to the menu and start again.
   Use a disposable game and record the exact RimWorld build and enabled DLCs.
5. Add optional mods one at a time, exercising the integration below. Check
   Player.log for new errors. Share reproduction steps, settings, mod versions,
   and a log with personal information removed when something fails.

| Integration | Minimum manual check |
| --- | --- |
| Vanilla Expanded Framework / KCSG | Start a scenario containing its starting structure; confirm that the structure appears. |
| Save Our Ship 2 | Test both a valid load-ship scenario and start-in-space scenario, plus an ordinary ground start with SOS2 enabled. |
| Real Ruins | Test automatic POI generation with biome filtering on and off; it needs the mod's external services. |
| No Pause Challenge | Check enabled/disabled no-pause and half-speed settings. |
| My Little Planet | Change the subdivision setting and confirm the resulting world size. |
| Realistic Planets / Prepare Moderately | Exercise the existing reflected integrations with their currently installed versions. They were not rewritten in this build change. |

## Optional-mod compatibility boundaries

KCSG, SOS2, Real Ruins, and No Pause integrations now resolve their external
types and members only when called. They no longer create compile-time assembly
dependencies. Missing or changed required API members produce a warning and
disable that integration until restart. A scenario requiring KCSG or SOS2
initialization stops with an explicit error if initialization fails rather than
continuing with an invalid world. Ordinary scenarios can still be used.

This removes the need to supply DLLs for compilation; it does not guarantee
compatibility with arbitrary mod versions. API contracts and gameplay behavior
must still be checked when those mods change.

## Experimental 1.7

The portable project and downloadable development package currently target 1.6.
The .NET 10 SDK and 1.7 reference package are prepared in the cloud, but the 1.7
port remains separate work. See the [maintenance plan](modernization.md) for its
acceptance criteria. Do not run the 1.6 assembly as a claimed 1.7 release.
