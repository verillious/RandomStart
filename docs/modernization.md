# Random Start maintenance plan

The primary product goal is a [configurable one-click start](automatic-compatibility.md).
Reduce maintenance and improve compatibility within that constraint. An automated
page journey with routine confirmation dialogs does not meet it. The native page
prototype is archived on its experimental branch and removed from the current
one-click implementation. See the [simplification review](one-click-simplification.md).

See the [general compatibility audit](compatibility.md) for a plan to reduce
special-case integrations by preserving the normal game startup, scenario
callbacks, and additive patches.

## Target and current evidence

Maintain a stable RimWorld 1.6 track and, as requested, an experimental 1.7
track. On 2026-11-05, the community
[Krafs.Rimworld.Ref version index](https://api.nuget.org/v3-flatcontainer/krafs.rimworld.ref/index.json)
listed **1.6.4871** as its newest stable version and **1.7.4998-unstable** as its
newest prerelease. This establishes available compilation targets, not the
official current game release. Confirm the target against Ludeon's release notes
before shipping. The repository already declares support for 1.6 and 1.5; do not
advertise 1.7 support until its build and gameplay checks pass.

### Portable 1.6 baseline

The repository now builds using SDK-style PackageReferences, pinned dependencies,
and lockfiles, without a game installation or optional-mod DLLs. KCSG, SOS2,
Real Ruins, and No Pause calls use guarded runtime adapters. Development output
is separate from the shipped assemblies, and GitHub workflows build, test, and
package the new DLL. See [development instructions](development.md).
Compilation and boundary tests pass; real-mod integration/gameplay checks and
the GitHub-hosted workflow run remain outstanding.

### Initial investigation before the portable build

The initial cloud build against 1.6.4871 references failed: optional-mod compile
dependencies are absent and the legacy project does not resolve the Unity
netstandard dependency automatically. An external diagnostic build supplied the
full RimRef reference set, resolving that reference error; compilation still
stops at missing KCSG and VEF namespaces. The 1.7 package instead contains
`ref/net10.0` and declares a `net10.0` dependency group. Applying the legacy
net472 reference path to it fails before it can validate source compatibility.
Its assembly set includes Brutal engine libraries and UnityStubs.dll rather than
the 1.6 Unity module set. This is a framework/engine port, not a metadata update.
A separate .NET 10 SDK-style diagnostic project, using the actual 1.7 reference
directory, restored successfully and reached the compiler. It also stopped at
the missing KCSG and VEF namespaces, so it does not yet establish API compatibility.
No gameplay compatibility claim is made. The
Ludeon-permitted RimRef package provides compilation signatures, not a runnable
game. A licensed game installation is still needed for integration testing.

The cloud has checksum-verified .NET SDKs 8.0.427 and 10.0.403, plus the pinned
reference packages above. Their installation steps are saved in the environment
configuration draft. The early diagnostic projects and outputs outside the
checkout are investigation tools; use the repository project for current 1.6
development.

### Experimental 1.7 acceptance criteria

Pin a specific prerelease build (initial investigation: 1.7.4998-unstable), rather
than silently following prerelease updates. Establish a .NET 10 build for 1.7
and audit Unity-dependent input, drawing, and generation code against that API.
Compile 1.6 and 1.7 independently with separate intermediates and outputs.
Once both compile, inspect changed
Harmony targets and the copied research/pawn-arrival methods against the target
game. Exercise world generation, settlement selection, starting pawns/items,
research, ideology/genes, and the restart action in each game version. Check
optional-mod compatibility independently; a 1.6 integration DLL is not evidence
of 1.7 compatibility. Use a 1.7-specific load folder and binary when APIs differ;
do not overwrite the 1.6 output with an experimental assembly.

The first fix repairs malformed Chinese translation XML and removes five
duplicate hilliness keys, retaining the translations in the starting-tile
section. Validate assets with `python3 tools/validate_xml.py`.

## Workshop report intake

Source: [Workshop comments](https://steamcommunity.com/workshop/filedetails/?id=3257028947).
Access now works, and all 233 accessible comments have been reviewed. See
[Steam report triage](steam-reports.md) for dated links, source-level fixes,
historical closures, feature requests and cases still needing reproduction.
The source-level findings below originated in the earlier audit; they are not
all reproduced Workshop bugs.

For each report, record the comment URL/date, game build, mod version, DLCs,
relevant mods/load order, settings, expected/actual behavior, reproduction steps,
and Player.log or a HugsLib log when available. Remove personal information and
credentials from logs. Distinguish reported, reproduced, fixed, and verified;
keep feature requests separate. Do not require a full mod list before trying a
minimal reproduction.

## Compatibility implementation

The general compatibility refactor implements additive UI changes, per-game
state/settings, scenario copying and callbacks, custom-page handover, standard
pawn generation constraints, scenario-driven item delivery, and narrow research
grant/completion hooks. See [implementation details and in-game checks](compatibility-testing.md).
The original issue table below records the audit baseline. The follow-up
[generation fixes](generation-fixes.md) address tile filtering, gene bookkeeping,
and faction-pair updates; their game/mod behavior still needs manual checks.
Real-mod integration checks and experimental 1.7 validation remain outstanding.

## Prioritized work

These are source-level findings, not reproduced Workshop reports. Each item
should be a small reviewable change with its own validation.

| Priority | Finding and trigger | Next change and validation |
| --- | --- | --- |
| Done; manual verification pending | Portable 1.6 build and optional-mod dependency isolation. | SDK-style PackageReferences, lockfiles, runtime adapters, boundary tests, staged packages, and CI are implemented. Complete the documented real-mod/gameplay checks before shipping. |
| P1 | In `RandomScenario.SetupForRandomPlay`, `hasFilters` ignores the temperature toggle. Enabling only temperature filtering calls unrestricted random tile selection. Filtered selection also takes the first valid tile in grid order. | Include temperature-only filtering, select randomly among valid candidates, and avoid repeated `IndexOf` scans. Test temperature-only, combined filters, no matches, stale biome names, and settlement validity. |
| P1 | `SetupRandomScenario` removes scatter parts directly from a ScenarioLister object. A later start can inherit the modified scenario. Its empty-pool fallback runs after `RandomElement`. | Select from the intended categories without duplicates, explicitly handle empty pools, and copy the selected scenario before modification using the game's supported mechanism. Test repeated starts with remove-items toggled and all scenarios disabled. Confirm ScenarioLister category semantics against the pinned API before changing selection. |
| P1 | `ResearchUtility_Patch.Prefix` removes selected projects from a finite pool without stopping when it empties. Its loop resamples `RandomInRange` on every condition check. | Sample the requested count once, cap it to available candidates, and test a one-project pool with a larger requested count. Check null research tabs and missing DLC data as part of the research pass. |
| P1 | `SetupRandomGenes` removes genes while enumerating `GenesListForReading`; metabolism is incremented both by `GivePawnGene` and its caller. The conflict check constructs a different gene from the one added. | Use the gene returned by the supported add API, remove conflicts safely, and calculate metabolism from actual resulting genes. Test conflicting/prerequisite genes, the minimum-metabolism option, and Biotech disabled. |
| P1 | Goodwill randomization removes/recreates bilateral relations inside an outer faction loop. A reverse relation can be recreated before its faction's old relations are removed. | Randomize each unordered faction pair once using game-supported relation APIs. Verify symmetry, excluded factions, permanent enemies, and repeated starts. |
| Implemented; manual verification pending | Harmony initialization and menu insertion located the mod by display name using `First`. | Both now resolve the typed mod instance so the development package can have a distinct display name. Exercise startup and left/right menu actions in the game. |
| P2 | World generation, settings UI, and compatibility code are concentrated in large classes; research and pawn-arrival patches duplicate game internals. | Separate selection, generation orchestration, settings validation, UI sections, and optional integrations incrementally. Prefer narrow postfixes or supported game APIs where they preserve the feature, and minimize publicized/private API access. Add tests at the extracted logic boundaries. |
| Release | About.xml contains a development package ID and MODVERSION placeholder; 1.5 uses a separate prebuilt DLL. | Confirm the Workshop package identity and release process before changing metadata. Document whether 1.5 is maintained or frozen. Updating 1.6 sources does not update the 1.5 binary. |

## Working rules and release checks

- Preserve saved setting keys and defaults unless a deliberate migration is included.
- Keep optional integrations isolated: the mod must work with Harmony alone and
  with each supported DLC combination. Avoid loading optional assembly types
  before checking availability; diagnose reflection failures with useful context.
- Use Verse's seeded random APIs for game generation. Sample each requested
  count once and handle empty candidate sets explicitly.
- Make the smallest bug fix first; avoid mixing it with a large formatting pass
  or rewrite. Record regression coverage and manual checks with each change.
- Start a clean game, generate several random worlds, return to the menu, and
  start again. Test the game-ended restart action, settings persistence, and
  changed mod/DLC sets. Confirm no unexpected errors in Player.log.
- Validate translations and load folders, compile the pinned target, and run the
  relevant integration cases before replacing a shipped DLL. Source edits alone
  do not change the assembly RimWorld loads.
- Publish to Workshop only after reviewing the release diff and validating the
  packaged output in the game. Keep experimental 1.7 builds separate until the
  target and API changes are confirmed.
