# Primary goal: automatic compatibility with minimal setup

The user's priority is broad out-of-the-box compatibility and minimal user fuss.
This takes precedence over adding more optional integrations or randomization
features. Preserve the mod's purpose: press Random, receive a valid randomized
start, and let the installed game/mods perform their normal initialization.

Maintenance cost is a primary constraint: shrink responsibility for other mods'
internals. Do not build an automatic panel importer or a new integration framework.
Preserve Random Start's own configuration; retiring controls that exist solely
for another mod may be necessary and must be documented explicitly.

The [native startup prototype](native-startup-prototype.md) implements the first
development-only milestone. The [Harmony audit](harmony-audit.md) records the
patching decisions and actual pinned-runtime tests.

## Default behavior to work toward

- Run the actual new-game configuration lifecycle, applying a captured random
  plan to its supported inputs.
- Preserve scenario-required pawns, xenotypes, factions, arrival methods, and
  other constraints. Other mods' native settings and validation must participate.
- Automatically use valid existing defaults where no Random Start override was
  requested. Avoid asking users to identify which compatibility switches to set.
- Automatically advance supported steps only after initialization and validation
  complete. Preserve asynchronous work and genuine mod-required decisions.
- Cover automated pages with a progress screen while preserving their normal
  lifecycle and drawing. Reveal pages/dialogs that need input; avoid visible
  setup-page flicker during ordinary automatic advancement.
- Pause only for missing required data or a choice that cannot safely be inferred;
  keep completed random choices when handing control to the user.
- Prefer a shared solution to a class of startup hooks over another package-ID
  exception. Retain a small adapter only for a demonstrated missing capability.

## The remaining architectural gap

Current automatic startup still calls `SetupRandomDifficulty`,
`SetupRandomPlanet`, and `SetupRandomIdeology` itself. It obtains the normal
configuration page chain, but only opens it when the manual setting is enabled
or a non-vanilla page type is detected. Vanilla pages otherwise do not receive
normal open/draw/transition calls.

A mod can patch a vanilla page's `PreOpen`, `PostOpen`, drawing or transition
method without introducing a custom page type. The existing type-based detection
therefore misses an important class of integrations. Calling scenario callbacks
and restoring normal item/research APIs helps, but does not close this gap.

Adding more detection rules is not the intended long-term solution. Automatically
switching every patched page to manual operation would also create unnecessary
user work: patch presence alone does not establish incompatibility.

## Next implementation milestone

Build and validate one native startup path for 1.6 before expanding the named
adapter list. Keep this work separate from the experimental 1.7 port.

1. Enter through `Page_SelectScenario.BeginScenarioConfiguration` so normal
   initialization and mods patching that entry point execute. Bind the pending
   random plan to the game created by that entry point; ordinary New Colony
   starts must remain unaffected.
2. Capture choices once. Apply supported values at the appropriate page/data
   boundaries without clearing data produced by scenario or mod callbacks.
3. Preserve real page opening, rendering where required, validation, and normal
   transitions. The inspected API includes `Page.prev/next/nextAct`,
   `Window.PreOpen/PostOpen`, `Page.CanDoNext/DoNext`, and the window stack.
   Their existence does not establish their exact timing or side effects.
4. Automate the supported vanilla steps and preserve mod hooks attached to them.
   Do not replace their bodies with copies. Do not call arbitrary methods in an
   optional assembly to guess how it initializes.
5. Handle asynchronous transitions and blocking dialogs explicitly. Avoid
   advancing the same step twice, generating a world twice, blindly accepting an
   unfamiliar dialog, or assuming a default `CanDoNext == true` proves an unknown
   page needs no meaningful input.
6. For mod-provided steps, honor a usable native default where readiness can be
   established. Otherwise leave that step available with the existing plan intact.
   Do not restart the entire setup or silently discard requested constraints.
7. Remove manual KCSG/SOS2/planet initialization calls only after their native
   hooks have been traced and verified to run once. Optional feature integrations
   can remain where they provide a genuinely separate function.

The reference metadata confirms useful native APIs and fields, including world
parameter inputs, starting-site selection, ideology pages, and starting-pawn
pages. It contains stripped method bodies and cannot demonstrate frame order,
window behavior, asynchronous completion, or the semantics of patched UI code.
A guessed auto-clicker based on those signatures would not be a verified native
startup implementation.

## Required validation before replacing the default

Trace a normal startup in a running licensed 1.6 game and compare it with the new
path. Verify callback order/count, queued world creation, ideology/pawn timing,
validation warnings, cancellation, and asynchronous page transitions. Then test
representative page-hook, world-generation, scenario, ideology and pawn mods.

Use automated controller/policy tests for sequencing, duplicate-advance guards,
plan preservation and failure handling, alongside those engine checks. The
current cloud contains reference assemblies and test tools, not a running game.
That limits what can be claimed about a replacement for the entire startup flow.

The existing development builds remain intermediate work. The prototype is not
the default startup path and does not yet automate ideology. No complete native
page-flow replacement or runtime verification is claimed. Prioritize finishing
and verifying this path over feature expansion or more guessed exceptions.
