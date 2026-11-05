# One-click starts with maintainable compatibility

The core product requirement is: configure preferences beforehand, click Random
once, wait for generation, and enter the game. Successful automatic starts must
not require acknowledging warnings, completing setup pages, or answering another
mod's startup questions. A progress cover alone does not satisfy this requirement.

The priorities are:

1. Preserve the configurable one-click start that defines Random Start.
2. Reduce the code and third-party behavior its maintainer must own.
3. Improve compatibility through normal game data/APIs and scoped, additive hooks
   within those constraints.

This supersedes the earlier plan that accepted manual page/dialog handover as a
normal fallback and treated a native page controller as the required destination.
The user explicitly rejected that change to the core experience.

## Startup contract

- Preserve Random Start's core settings for scenarios, worlds, factions, tiles,
  difficulty, ideology, pawns, items and research.
- Resolve supported choices from saved configuration and valid game/scenario
  constraints during planning. Routine confirmations must not become additional
  clicks in a successful start.
- Prefer live generation APIs, scenario data and normal callbacks over copied
  implementations. Keep overrides local to this game and explicit start phase.
- Do not guess answers to unfamiliar dialogs, click buttons by label, suppress
  arbitrary windows, or replace required mod input with an invented choice.
- A workflow that cannot start without new information has a compatibility limit.
  Treat it as an unsupported automatic workflow, with a clear failure reason,
  rather than quietly converting Random into an interactive setup wizard.
- Preserve deliberate manual setup as an explicitly selected mode. It does not
  count as successful one-click compatibility and must not become the default.
- Actual generation errors still need reporting; eliminating routine prompts does
  not mean hiding failures or continuing with a broken game.

Maximum compatibility with arbitrary mandatory custom UI, no interactions, all
mod-specific features and no maintenance is not a universal contract we can
deliver. Keep the one-click product and document limits instead of accumulating
private-API adapters to promise every combination.

## Implementation direction

Continue simplifying the existing automatic path using game-owned generation and
validation wherever possible. The useful refactors already made include scenario
copying, per-game state, preserved pawn requests, normal item delivery, additive
research completion and native meme generation. Keep these independently useful
changes regardless of the page experiment.

The startup-hook gap remains real: some mods initialize through vanilla pages'
open/draw/transition methods that the direct path skips. Fix a missing lifecycle
boundary when it can be reused correctly and without introducing a setup journey.
Do not manually call every callback indiscriminately; duplicate initialization
can also break a start.

Do not expand an automatic panel importer, a third-party integration framework,
or a dialog-answering system. Retire mod-specific controls where their continuing
support cost exceeds their value, with explicit release notes. In the earlier
audit, roughly 600 adapter lines belonged to Prepare Moderately and Realistic Planets;
removing those optional integrations is a separate opportunity from replacing the
startup architecture. Initialization workarounds require verification before
removal; retiring an optional feature must be described as a feature change.

## Native page experiment: archived

The [native startup prototype](native-startup-prototype.md) remains available in
Git history and on its experimental branch. Its page driver, progress cover and
dedicated Harmony observers are removed from this one-click branch. Its
ideology/manual-dialog handovers fail the one-click acceptance requirement.
See the [first simplification review](one-click-simplification.md) for the code
removed and retained integration contracts.

Do not restore that workflow merely because it exercises more native UI methods.
The [Harmony audit](harmony-audit.md) and its tested execution guards are
independently useful and remain in the current implementation.

Any future replacement must demonstrate, in a running licensed 1.6 game:

1. A complete configured automatic start, including Ideology, with no routine
   page/dialog acknowledgments after clicking Random.
2. Preserved scenario constraints and callback order/count, including queued
   generation, cancellation and failures.
3. Representative compatibility gains with old adapters actually removed.
4. One smaller supported startup implementation after retiring redundant paths;
   avoid maintaining two production startup systems indefinitely.

The cloud's stripped reference assemblies establish signatures, not runtime UI
timing or compatibility. Portable policy/Harmony tests do not prove these gates.
The normal Random button uses the direct automatic implementation. Custom setup
pages are an explicit automatic-start limitation; they now report an unsupported
scenario instead of opening an unrequested setup wizard. Users can explicitly
choose manual setup in advance when they want that different workflow.
