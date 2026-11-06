# Compatibility development build

This is a broad 1.6 compatibility refactor following the working portable-build
baseline. Keep that baseline ZIP available when comparing behavior. Automated
policy tests and successful compilation do not replace the in-game checks below.
The changes do not advertise support for 1.7.

## Implemented changes

- Menu insertion follows the existing New Colony entry and is idempotent. It no
  longer detects Continue Button by package ID.
- The game-ended letter's existing choices are extended; its dialog/open method
  is no longer replaced.
- A copied scenario and a snapshot of settings belong to one game/start.
  Difficulty overrides affect its runtime difficulty, not the shared definition.
  The storyteller definition is not modified. Missing faction preferences stay
  saved, and scenario-declared required factions are included generically.
- Startup calls scenario `PreConfigure` and the scenario-level `PostIdeoChosen`
  wrapper, then proceeds directly through configured world/ideology/pawn setup
  into map generation. It does not construct or inspect UI configuration pages.
  The removed setup-screen flag is not read from old settings files. A mod's
  replacement world-settings page cannot itself block a start. The direct path
  does not emulate page lifecycle hooks, so missing mod initialization still
  requires verification. Ordinary New Colony selection clears a cancelled
  Random Start session before the game's own setup begins.
- The scenario generates its own pawn pool, counts, kinds, and per-slot requests.
  Optional human-pawn constraints are applied only to standard starting-pawn
  requests during the selected scenario's pawn-generation callback. Fixed
  scenario ages/genders, developmental-stage restrictions, existing validators,
  nonhuman pawns, and unrelated pawn generation are preserved. Disjoint age
  ranges leave the scenario restriction authoritative. Custom generators that
  bypass the standard utility are not forcibly replaced.
- Item additions are serialized normal starting-item parts, chosen once during
  the scenario's pre-map callback after world/pawn setup and delivered by the
  game's own arrival method. The old `GenerateIntoMap` replacement and hard-coded
  drop-pod delivery have been removed. Counts are sampled once, and real stacks
  are used. The value limit applies to the **random additions**, not other mods'
  existing items. Removal deletes standard game item/scatter parts and clears
  starting possession lists, but preserves scenario parts implemented by other
  mods. Explicit removal can still conflict with a scenario's required gear.
- The original starting-research method always runs. Additional projects use
  `ResearchManager.FinishProject` and a finite candidate pool. Removal suppresses
  only matching faction starting grants/techprints during that call, preserving
  ideology grants and their prerequisites. Explicit scenario research outside
  that grant scope is preserved. Other mods granting the same tagged project
  inside that scope are subject to the user's removal setting too; provenance
  cannot be inferred automatically. The no-prerequisite option narrowly blocks
  prerequisite completion during the selected project's normal completion call.
- Scoped flags restore on exceptions; start state expires after successful or
  failed `Game.InitNewGame`. New games cannot inherit another game's overrides.
  State holds the owning game weakly, so a cancelled setup does not retain an
  abandoned game. Cancel and retry behavior still needs the runtime checks below.

## In-game verification

Use disposable games and the exact game/mod versions in your report. Install the
new development artifact as described in [development.md](development.md), with
only one Random Start copy enabled. Restart RimWorld after replacing the DLL.

| Case | Expected behavior |
| --- | --- |
| Harmony alone; several vanilla scenarios | Random starts finish; selected scenario's starting pawn counts/kinds and normal arrival are preserved. |
| Default pawn controls; then age/sex/violence/work restrictions | Constraints apply to eligible standard human starting requests; scenario-fixed or incompatible constraints are not silently discarded. Check a child-only/nonhuman custom start if available. |
| Custom names and optional gene controls | Names use existing generated slots; nonhuman pawns without genes do not enter the gene-customization path. Existing gene algorithms otherwise remain unchanged. |
| Both item toggles off | Vanilla/custom arrival code and existing possessions still run. |
| Add only / remove only / remove and add | Check items, stacks and possessions for each combination; added-item budget is respected. Repeat with normal landing, drop pods and an Odyssey gravship scenario where available. |
| Research toggles off | Normal research and other mods' grants are unchanged. |
| Add research, prerequisite completion on/off | Root projects respect selection settings; ordinary completion hooks run. With prerequisites off, unrelated prerequisites stay unfinished. Small candidate pools and zero eligible projects do not fail. |
| Remove research with/without additions | Faction grants/techprints are withheld; ideology and explicit scenario grants remain. Check with a research mod as well as vanilla. |
| Continue Button or another menu mod | Existing options retain order; Random appears once immediately before New Colony. |
| Game-ended letter, including a mod-added choice | Original and mod-added options remain; Random Start appears once and starts a new disposable game. |
| Upgrade from settings with the retired setup-screen flag enabled | The option is absent, other preferences remain, and Random proceeds directly into generation without showing configuration pages. |
| Mod replaces the global world-settings page (reported with Realistic Planets) | Automatic starts do not instantiate the full setup chain solely to inspect it, and do not reject the scenario just because the global page was replaced. Verify The Sanguophage and another vanilla scenario. |
| Custom scenario with saved starting data | Configured starting data and normal scenario callbacks are used; no configuration page is opened or rejected by page type. Test actual mod initialization and missing-data failures separately. |
| Two random starts, then an ordinary New Colony start | No scenario parts, difficulty definitions, or temporary constraints carry over. |
| Fail/cancel a start, then start normally | No active constraints or research suppression remain for the other game. |
| Disable/re-enable a faction/content mod | Saved faction selections return; an unavailable selection is skipped for that run without deleting it. |
| Scenario declares `preventRemovalOfFaction` | That required faction appears without adding another package-ID-specific rule. |

## Integrations retained for testing

KCSG, SOS2, Real Ruins, No Pause, Realistic Planets, Prepare Moderately, and the
existing VFE faction bridges have not been deleted on the assumption that the new
callbacks cover them. Test each real mod before removing its adapter. Private
reflection contracts still need maintenance when their owners change APIs.

Do not blindly answer unknown pages or silently substitute choices for missing
scenario data. Random Start has one startup path and no UI handover. Do not
disable another mod's patches or force load order to hide an unexplained failure.
Record Player.log and the failing stage/settings.
