# One-click simplification: first implementation pass

Random Start keeps its direct automatic startup and existing configuration. This
pass removes the page-driving experiment and simplifies existing integrations
without retiring their settings or adding new dialogs to a supported start.

## Implemented changes

- Remove the experimental page controller, full-screen cover, pending-entry
  binding, three page/window Harmony observers, sequencing policy and its tests.
  The experiment remains on `codex/native-startup-prototype` and in Git history.
  The current build has one automatic startup implementation.
- Remove the first-use confirmation. Left-click Random starts immediately with
  saved settings or defaults; right-click still opens settings. The obsolete
  `openedSettings` marker is no longer read/written. Actual configuration keys
  and defaults remain intact.
- Remove automatic handover into unfamiliar scenario-provided setup pages. An automatic start
  requiring such a page reports an unsupported scenario before world
  generation. This is a compatibility limit, not a successful randomized start.
  The already-existing manual setup option remains available when explicitly
  selected before starting. Do not claim automatic support for mandatory custom
  setup UI, or blindly acknowledge its questions.
- Combine Empire and Deserters faction rules into `ScenarioFactionRequirements`,
  alongside the generic `ScenPartDef.preventRemovalOfFaction` rule. Remove the
  two duplicated adapter classes and their package-ID checks. Inspect scenario
  parts directly so forks with different IDs can still match the same parts.
  Ensure Empire and Deserters independently: the former early return could leave
  Empire absent when Deserters was already selected. Do not duplicate a required
  faction or add another copy when multiple parts require it. These two explicit
  scenario identities remain an integration contract; this is not a universal
  inference mechanism for other mods' hidden dependencies.
- Route every world through one live `WorldGenerator.GenerateWorld` call in
  `SetupRandomPlanet`. Realistic Planets now only configures its own parameters
  through the existing guarded reflection boundary. Resolve its settings fields
  before writing them, using the captured settings rather than reloading global
  preferences. It no longer requires the unused `Planets_CreateWorldParams` type.
- Remove the Realistic Planets catch-and-regenerate fallback. The old wrapper
  could catch a failure from any patched world generator and call that same
  patched generator again. A second attempt is not an unpatched vanilla fallback
  and can repeat initialization against partially changed state. Generation
  errors now reach the existing outer error handling once.
- Remove Prepare Moderately's unused current-filter getter. Keep its actual
  filtering settings and runtime behavior.
- Retain the earlier Harmony execution guards, per-game settings, copied
  scenarios, scoped pawn constraints, standard item delivery/research completion,
  and native meme generation.

## Measured reduction

At the initial simplification commit (`10bfed4`), production C# source was 6,111 lines:

| Comparison | Removed production lines, net |
| --- | ---: |
| Previous experimental branch (`e4c1e6d`, 6,695 lines) | 584 |
| Before the page experiment (`2dab332`, 6,319 lines) | 208 |

The dedicated compatibility folder falls from 1,002 to 858 lines. This distinguishes
removing our unsuccessful experiment from simplifying the earlier mod itself.
Line count is evidence of less source, not proof of lower runtime support cost.

## What source investigation established

Read public upstream source without executing or building it. These snapshots
are evidence about the inspected declarations/algorithms, not a guarantee about
any particular installed Workshop DLL.

| Integration | Finding | Decision |
| --- | --- | --- |
| KCSG / Vanilla Expanded Framework | Its pawn-page `PreOpen` postfix populates `pcScenariosSave`; its starting-structure `PostMapGenerate` checks that cache before generating a structure. | Keep the small cache adapter while the direct flow skips that page. Deleting it now could omit the structure and its starting content. |
| VFE Empire / Deserters | Their inspected 1.6 scenario XML does not declare `preventRemovalOfFaction` for these parts. Deserters' 1.6 part joins its faction during `PostWorldGenerate`. | Generic declared requirements alone cannot replace the existing pair. Consolidate, remove package-ID assumptions, and fix independent dependency checks. |
| Realistic Planets | Our existing wrapper generates through the ordinary world API; the Workshop source link points to an older Continued repository. It is not evidence for the current 1.6 reflection contract. | Remove duplicate generation/error ownership and the unused page-type dependency. Retain the small settings bridge; do not claim current-version integration verification. |
| Prepare Moderately | The current adapter implements optional filter selection and repeated native pawn regeneration. The guessed public repository was not found. | Remove the repository-unused getter. Retain the feature, and do not claim upstream API verification. |
| SOS2 / Real Ruins / No Pause / My Little Planet | Their existing bridges still perform initialization or expose mod-specific settings. Their full current runtime behavior has not been reverified here. | Preserve them in this pass rather than equating deleted initialization with compatibility. |

Inspected source snapshots:

- Framework `e38a1b0118079e09b4c098143449251dcede955d`:
  [pawn-page initialization](https://github.com/Vanilla-Expanded/VanillaExpandedFramework/blob/e38a1b0118079e09b4c098143449251dcede955d/Source/KCSG/Harmony/Postfix_Page_ConfigureStartingPawns_PreOpen.cs)
  and [starting-structure generation](https://github.com/Vanilla-Expanded/VanillaExpandedFramework/blob/e38a1b0118079e09b4c098143449251dcede955d/Source/KCSG/ScenPart/ScenPart_AddStartingStructure.cs).
  The source directory is shared/versionless; compare with an installed 1.6 DLL
  before treating it as a binary-version match.
- Empire `2d8c9f2f73337d011a8ed6e0ee538b582a73c8be`:
  [1.6 scenario definitions](https://github.com/Vanilla-Expanded/VanillaFactionsExpanded-Empire/blob/2d8c9f2f73337d011a8ed6e0ee538b582a73c8be/1.6/Defs/Scenarios/Scenarios_Classic.xml).
- Deserters `611904562d3d2ca0b7c70721bc8afd1a699ac48a`:
  [1.6 part definitions](https://github.com/Vanilla-Expanded/VanillaFactionsExpanded-Deserters/blob/611904562d3d2ca0b7c70721bc8afd1a699ac48a/1.6/Defs/Scenarios/ScenPartDefs.xml)
  and [1.6 world callback](https://github.com/Vanilla-Expanded/VanillaFactionsExpanded-Deserters/blob/611904562d3d2ca0b7c70721bc8afd1a699ac48a/1.6/Source/VFED/ScenParts/ScenPart_StartDeserting.cs).
- Realistic Planets' [Workshop page](https://steamcommunity.com/sharedfiles/filedetails/?id=3225521979)
  links [the older Continued source](https://github.com/Xeraster/RW_Realistic_Planets_Continued_1.3/tree/ff234e1ba7f001e748ec2304eb29e12171cd884b).

## Validation and remaining work

The initial simplification suite had 53 tests: existing policies, reflection/dependency
checks and actual pinned-Harmony composition tests. The 12 deleted tests belonged
only to the removed page controller/cover; this is not removal of existing
one-click regression coverage. Locked restore, Release tests, Debug compilation,
XML validation and package checks are required for this pass.

No running game is available in this cloud. In-game validation still needs:

- A first-use left-click start, right-click configuration and another start in the
  same session, including ideology, pawn/item/research settings and cleanup.
- The custom-page error in automatic mode and explicit manual setup when selected.
- VFE starts with only Deserters preselected, only Empire preselected, neither,
  and both; verify each required faction exists once without changing deliberately
  repeated ordinary faction selections. Check KCSG structure/pawn/item arrival.
- Realistic Planets with and without My Little Planet, including a generation
  failure: one generator invocation, preserved original error, and no second world
  initialization. Other world-generator patches must still participate.
- Saved settings loading after removal of the obsolete first-use marker.

## Follow-up: world-page replacement regression

A supplied local-test log on RimWorld 1.6.4871 shows The Sanguophage failing
before generation. Realistic Planets replaces the standard global
`Page_CreateWorldParams` through its `PageUtility.StitchedPages` postfix. Our
first simplification incorrectly treated every foreign page in that full chain
as additional scenario input and threw `CustomSetupRequired`. The failing check
was ours; the constructor/replacement stack traces in this log are diagnostic
messages, not evidence of a separate generation exception.

Automatic startup now inspects only `ScenPart.GetConfigPages()` providers for
additional scenario steps. It never calls `Scenario.GetFirstConfigPage()` merely
to inspect the global setup UI. This avoids rejecting a normal scenario because
a world mod replaces its standard settings screen. Explicit manual setup still
uses the live full chain, including replacements, with its existing null/cycle
checks. No package-ID allowlist or new dialog automation is added.

Four portable regression tests verify that automatic starts do not invoke the
global UI factory, explicit manual mode preserves the returned replacement and
does not re-enumerate scenario providers, genuine unfamiliar scenario steps are
still rejected, and empty/null entries do not force a UI. The suite now has
57 tests. These tests verify routing policy; the reported mod combination still
needs an in-game retest to establish what happens after this blocker is removed.

The largest remaining code savings involve retiring optional mod-specific
features (especially Prepare Moderately and Realistic Planets controls). That
changes available configuration, so it must be explicit rather than presented
as a free compatibility improvement. Keep normal APIs and one-click behavior as
constraints on any further initialization simplification.
