# Maintaining the mod

## Code layout

- `Source/RandomScenario.cs`: start orchestration, scenario choice and pawn setup.
- `Source/Generation/`: world, difficulty, ideology and gene operations, kept as
  partials of the same runtime type so existing entry points remain stable.
- `Source/Settings/RandomStartMod.cs`: mod entry point, UI state and tab routing.
- `Source/Settings/Tabs/`: one file per settings tab; `SettingsWidgets.cs` holds
  shared UI helpers. These remain partials of the existing mod type.
- `Source/Compat/`: one class/file per existing integration and its reflection
  boundary. No new service container or integration framework is needed.
- `Source/Harmony/`: narrow game hooks; reusable policy code stays outside patches.
- `Tests/`: portable policy/metadata tests. They do not run RimWorld.

The structural move preserved 130 existing method signatures and bodies, with
inherited trailing whitespace trimmed afterwards. Static field initialization
order and settings/runtime type names were retained. Report-driven behavior
changes are separate from that move in Git history.

## Actual simplification

The copied meme-generation algorithm is removed. The pinned game already exposes
`IdeoUtility.GenerateRandomMemes(int, IdeoGenerationParms)`. Select memes through
that API, then pass the resulting plan to the game's ideology generator so it
initializes the ideology consistently. Explicitly initialize fluid state and
its development tracker for newly generated fluid ideologies. Fluid starts use
its initial-fluid meme count; the configurable range remains for fixed ideologies.
Forced/faction-required memes are still honored. Saved ideologies retain their
own loaded mode.

The central settings file is 185 lines rather than 2,777; the start controller is
232 rather than 853. More files make responsibilities easier to locate. The
important reduction in maintained behavior comes from delegating generation to
the game, not merely moving text between files.

## Standards and constraints

- Use the SDK-style build, pinned dependencies, lockfiles, CI and existing tests.
  Keep the 1.6 runtime target at net472; .NET 10 belongs to the separate 1.7 port.
- `.editorconfig` defines formatting for touched files. Correctness analyzers
  catch stack-destroying rethrows (CA2200) and exceptions from finally blocks
  (CA2219). Broader analyzer/nullability adoption should be incremental.
- Preserve saved settings keys, serialized type names and defaults. Avoid a
  cosmetic mass rewrite that obscures behavior changes.
- Treat defName as identity. A visible row index is not an index into a saved
  list containing temporarily unavailable mod definitions.
- Apply runtime settings during initialization, not only from drawing code.
  My Little Planet now applies the stored size before automatic world generation.
- Handle optional source metadata/icons with an unknown-source fallback. Runtime
  defs should not crash the settings window because they have no source package.
- Missing/failed saved ideology loads stop with a useful message. Version/mod-list
  mismatches are still validated by the existing game loader; normal setup screens
  are available when interactive decisions are needed. Do not silently substitute
  a random ideology or clear user settings to hide an error.
- Keep general mod cooperation in normal lifecycle/data APIs. Remove an existing
  adapter only after its real-mod behavior has been verified.

## Reviewing reports

Use [steam-reports.md](steam-reports.md) for the reviewed comments and statuses.
Record exact versions, settings, reproduction steps and a sanitized log. Keep
source fixes, automated checks and in-game confirmation distinct. Some old
comments have author-confirmed fixes; others are feature requests or missing
information. Do not claim a report is resolved solely because a related refactor
compiled.

Further useful work is targeted runtime verification, investigating unresolved
reports, and gradually reducing private API access. Keep new integrations and
optional features out of that maintenance pass unless their benefit justifies
the continuing test/support cost.
