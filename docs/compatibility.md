# General mod compatibility audit

The follow-up implementation and in-game checks are tracked in
[compatibility-testing.md](compatibility-testing.md). Findings below describe the
original `a64e7ee` baseline, not the current implementation.
The historical UI-handover proposals below were subsequently rejected; the
current mod has only the one-click path and no setup-screen option.

## Goal and evidence

Random Start should choose the inputs to a new game, then let RimWorld and the
active mods perform initialization. Changing the inputs generally composes better
than replacing the code that consumes them.

This audit examines source at commit `a64e7ee`, the 1.6 baseline the user reports
working in-game. It also checks signatures in the Ludeon-permitted
`Krafs.Rimworld.Ref` package, version `1.6.4871`, and Harmony's maintained
documentation. Reference assemblies establish which APIs exist, not their
implementation or precise execution order. No additional mod combinations have
been tested as part of this audit; the implementation proposals below still need
runtime validation.

The earlier reflection adapters removed build-time dependencies on optional
mods. They still depend on those mods' API names and behavior. Reflection alone
does not provide general compatibility or reduce the number of integration
contracts we maintain.

## Findings and replacements

### 1. Preserve the normal startup lifecycle

`RandomScenario.SetupForRandomPlay` manually creates the game, generates its
world, selects a tile, initializes ideology/pawns, and calls `InitGameStart`.
It does call `WorldGenerator.GenerateWorld`, so mods patching that method can
already participate. The issue is that it bypasses the normal configuration-page
path, where mods can also attach setup behavior.

The 1.6 metadata exposes:

- `Page_SelectScenario.BeginScenarioConfiguration(Scenario, Page)`;
- `Scenario.CopyForEditing()`, `PreConfigure()`, and `GetFirstConfigPage()`;
- `ScenPart.GetConfigPages()`;
- scenario-level `PostWorldGenerate()`, `PostIdeoChosen()`, `PreMapGenerate()`,
  `PostMapGenerate(Map)`, and `PostGameStart()`;
- page lifecycle/transition methods such as `PreOpen()`, `PostOpen()`,
  `CanDoNext()`, and `DoNext()`.

Current source dispatches `ScenPart.PostIdeoChosen()` directly in a loop instead
of calling `Scenario.PostIdeoChosen()`. Patches on that scenario-level wrapper
cannot observe that direct dispatch. Using the wrapper is an immediate candidate
for a small compatibility change.

For the larger refactor, trace the normal configuration path in a licensed game,
then reuse its lifecycle and validation. Do not simply call every callback from
the list above: some may already run inside existing game methods, and calling
them twice can be as harmful as skipping them.

Automate the standard choices, but preserve mod-provided configuration pages.
When a mod requires a decision that cannot safely be inferred, keep the random
choices already made and hand over to its normal setup UI. A guaranteed one-click
start through arbitrary third-party UI is not a realistic universal contract.
Avoid depending on drawing methods or simulating mouse clicks to advance pages.

KCSG and SOS2 adapters are candidates for removal if their normal hooks then run
correctly. That is a hypothesis to test with those mods, not a reason to delete
the adapters immediately.

### 2. Stop replacing the pawn-arrival implementation

`ScenPart_PlayerPawnsArriveMethod_Patch.Prefix` reconstructs pawn/item groups,
calls `DropThingGroupsNear` itself, and returns false when item customization is
enabled. This skips the original arrival method and can also skip other
side-effecting prefixes. Arrival changes, custom possessions, and mod-added setup
are therefore exposed to conflicts even if their mods are unknown to us.

Prefer expressing selected starting items through normal scenario data/starting
item parts. Where a custom scenario part is needed, give it a proper definition,
serialization, and a stable per-start item selection; do not reroll choices every
time an enumerable is inspected. Let the game's arrival implementation deliver
the resulting pawns and things using the scenario's chosen arrival method.

Removal needs explicit semantics: distinguish standard starting-item/scatter
parts, pawn possessions, and custom scenario content. Do not delete arbitrary
mod-provided parts because they happen to create things.

### 3. Separate research additions from research removal

`ResearchUtility_Patch.Prefix` returns false even when the user only requests
additional random research. It reimplements starting grants, and
`FinishProjectOptionalPrequisites` copies a large portion of research completion
instead of calling the normal completion method for the selected project. Mods
patching those normal methods may not see the corresponding operations.

For additions, preserve the original grant method, then apply selected projects
through the game's completion API. Handle already-completed projects and empty
pools explicitly. The option to complete research without prerequisites needs
its own reviewed implementation; do not remove its semantics just to shorten
the patch.

For removal, first define exactly which grants are suppressed. A blanket reset in
a postfix could erase another mod's legitimate grants. Keep this as a separate
opt-in path and investigate a narrow hook; changing every prefix to a postfix is
not automatically a correct fix.

### 4. Let scenarios own their pawn generation

After scenario callbacks, `SetupForRandomPlay` can clear the generated pawn pool
and replace it with ten pawns from `DefaultStartingPawnRequest`. Because
`startingPawnForceViolence` defaults to true, this branch runs in the default
configuration. A custom scenario's counts, pawn kinds, per-slot requests, or
other mods' generation work can be lost.

Preserve the scenario's generated count, pawn kinds, and request parameters.
Apply only the explicitly enabled constraints at a narrow generation boundary,
scoped to the starting-pawn generation for this run. Continue honoring scenario
validation and generation notifications. Do not apply these filters to every
pawn generated anywhere in the game. Verify how regenerating a single pawn
interacts with other mods before adding that behavior.

This should reduce the need for special handling of pawn mods that already
participate in standard generation. A separate editor with its own workflow may
still require the normal UI handover or a small documented adapter.

### 5. Keep changes local to the current run

`SetupRandomScenario` currently removes parts from a scenario obtained from the
scenario list. `SetupRandomDifficulty` writes into the selected `DifficultyDef`
and changes `StorytellerDef.tutorialMode`. These are shared objects that other
mods and later starts can also use.

Copy the selected scenario using the game's copy mechanism and check that custom
parts retain their serialized state. Apply difficulty overrides to the current
game's difficulty instance rather than modifying the shared definition. Avoid
changing a storyteller definition merely to select it.

Bind temporary customization state to the current game/start phase, with cleanup
on success and failure, rather than relying on the process-wide
`RandomStartData.startedFromRandom` flag. The flag is not evidence that an
arbitrary later map-generation/research call belongs to initial setup.

When a saved def is absent, resolve an available candidate for this run and
report the missing selection once. Do not delete the saved selection simply
because a mod or DLC is temporarily disabled: faction generation currently
removes missing faction names from settings and writes them back.

Use defs from active mods and the game's eligibility/validation rules. The code
already enumerates modded storytellers, biomes, research, and other defs in many
places; retain that approach. Avoid filtering by translated labels (such as the
research-tab string `anomaly`) or assuming vanilla faction lists are exhaustive.
Continue respecting deliberately configured inclusion/exclusion pools.

### 6. Make UI patches additive

`ChoiceLetter_GameEnded_Patch` currently replaces `ChoiceLetter.OpenLetter` for
the game-ended letter and reconstructs its dialog. The 1.6 reference confirms
that `ChoiceLetter_GameEnded` overrides the `Choices` getter, returning
`IEnumerable<DiaOption>`. Append the Random Start choice to that result and let
the normal dialog open. Preserve existing options and their ordering. This is a
good first refactor because it removes a full-method replacement without
changing world generation.

The main-menu patch currently guesses an insertion index from the Continue
Button package ID. Locate the existing New Colony entry and insert relative to
that entry instead, preserving all other entries. Make insertion idempotent in
case the same list is drawn again. Use an identity/type for our own option rather
than treating any other mod's translated `Random` label as ours.

## Proposed sequence

1. Replace the game-ended dialog override with an additive choice, and remove
   the main-menu package-ID special case. Verify existing choices/menu entries
   survive before testing with UI mods.
2. Isolate per-run data and stop modifying shared scenarios/defs or deleting
   temporarily unavailable settings. Establish repeat-start regression checks.
3. Trace and reuse the configuration lifecycle, including the scenario-level
   callbacks and mod-provided pages. Preserve the existing normal-game behavior
   while testing which integration adapters become unnecessary.
4. Replace the pawn-arrival override with scenario-driven item customization and
   make additional research an additive operation.
5. Preserve scenario-owned pawn generation, then remove each redundant adapter
   only after testing its real mod. Keep the experimental 1.7 port separate.

Do not introduce a large new integration framework now. If multiple remaining
mods genuinely need the same missing extension point, add a small documented
hook at that stage. Discovering an assembly is not permission to invoke arbitrary
methods in it. Prefer a mod's supported API to reflected private fields.

## What compatibility should mean in tests

- With a customization disabled, the original behavior and other mods' hooks
  still execute; an installed Random Start should not suppress unrelated work.
- Existing menu/letter choices survive and our option appears once.
- Required scenario callbacks execute once, in their correct order; custom
  configuration pages are not silently skipped.
- A custom scenario's pawn count/kinds and extra parts survive unless the user
  deliberately overrides the corresponding feature.
- Two starts in the same session, a failed start followed by a normal start, and
  temporarily disabling/re-enabling a content mod do not leak settings/state.
- Additions preserve other mods' existing items/research; removal behavior is
  tested separately rather than assumed to compose with every overhaul.
- Test representative mods by behavior: content/defs, world generation, starting
  scenarios, pawn generation/editing, arrival/items, research, and UI. Full setup
  overhauls remain explicit integration tests, not a universal promise.

For diagnosis, a bounded debug report can include the active generation phase,
selected scenario, enabled overrides, integration status, and Harmony patch
owners on the small set of methods we actually touch. Patch presence alone does
not prove a conflict. Do not automatically unpatch another mod or force global
patch priorities to hide an unexplained failure.

## Sources

- [Harmony prefix documentation](https://harmony.pardeike.net/articles/patching-prefix.html):
  skipping the original also skips remaining side-effecting prefixes; Harmony
  recommends avoiding that for small changes.
- [Harmony postfix documentation](https://harmony.pardeike.net/articles/patching-postfix.html):
  pass-through postfixes can extend enumerable results without replacing their
  producer. Postfixes are still not a substitute for validating state/order.
- [RimRef 1.6.4871](https://www.nuget.org/packages/Krafs.Rimworld.Ref/1.6.4871):
  signatures checked for the scenario/page APIs and game-ended choices getter.
- Repository files: `Source/RandomScenario.cs`, `Source/Harmony/*.cs`,
  `Source/RandomStartData.cs`, and `Source/Compat/*.cs`.

This historical audit does not itself establish that any mod pairing works.
See the implementation status and manual test matrix linked above.
