# Native startup prototype (RimWorld 1.6)

This is the first implementation milestone toward a smaller Random Start that
sets inputs and lets RimWorld and installed mods perform initialization. It is
not yet the replacement for the normal Random button. It has compiled and its
portable sequencing/Harmony tests pass; no in-game execution is claimed.

## Trying the prototype

Use the development package from this branch's GitHub Actions build. Disable
the Workshop copy, enable Harmony and the development copy, and restart the game.
Configure preferences through the existing Random Start settings. Enable the
game's Development mode and, from the main menu's debug actions, select:

**Random Start → Prototype native startup**

The normal Random button still exercises the existing startup implementation.
No new saved compatibility switch, other-mod DLL reference, or integration API
has been introduced.

## Implemented scope

The prototype captures the existing settings, chooses a copied scenario and
calls the live `Page_SelectScenario.BeginScenarioConfiguration`. A scoped pending
request binds settings to the game created by the native entry, at its scenario
`PreConfigure` boundary. An ordinary New Colony start does not adopt that request.
Item planning happens before the original scenario callback.

Opened pages receive the existing configured choices once:

| Native page | Applied inputs |
| --- | --- |
| Storyteller | Allowed storyteller pool, difficulty/custom values, Anomaly settings and save mode. |
| World parameters | Seed, coverage, climate, population, landmarks, pollution and configured factions, including scenario-declared required factions. |
| Starting site | Existing tile filters, season and map size; synchronize the native world selector. |
| Starting pawns | Retain normal scenario generation and the existing scoped pawn/name/gene hooks. Apply configured faction goodwill once. |

The native page owns validation and transition actions. The controller waits for
a subsequent GUI repaint with no queued long event and no window above the page,
then calls the page's live acceptance handler. It does not recreate its body,
call world generation itself, jump directly to map initialization, or use a
reverse patch. At most one acceptance attempt is made per page instance, even
when the handler queues a dialog or asynchronous generation without closing it.
A rejected attempt leaves the real page available for manual correction; the
controller does not repeatedly submit it.

## Progress screen

Supported automated pages sit behind an opaque, full-screen **Preparing random
start…** window. The normal pages continue to open, initialize and draw underneath
it; their draw methods are not replaced or skipped. The cover uses RimWorld's
window/input routing to block clicks and acceptance keys on hidden controls.
It follows the game's UI scale and screen resolution.

The controller ignores only its own cover when checking for a blocking window,
so the cover does not prevent automatic advancement. A real dialog takes priority:
the cover closes to expose it, and the controller does not answer it automatically.
The cover can return when the dialog continues into queued work or the next
supported page. An acceptance attempt that leaves a page open without queued work
also reveals that page for manual correction. Ideology/unknown-page handover and
errors dismiss the cover.

**Continue setup manually**, or Escape while the cover has input, stops automatic
advancement and removes the cover while retaining the applied choices. Queued
generation is allowed to finish; this does not cancel or restart it. A stale cover
also closes on the UI thread when its game/session ends. It is not serialized and
does not add a new saved setting.

The cover's decision policy has automated tests for rejected validation, blocking
dialogs, queued generation after page closure, manual handover and resuming on
a subsequent page. Actual opacity, focus, input routing, display scaling and frame
order still require a running-game check; the cloud cannot confirm visual behavior.

## Existing integrations

The existing item/research hooks remain scoped to the new game. The prototype
does not manually call KCSG, SOS2, VFE faction, Real Ruins, No Pause, My Little
Planet, Realistic Planets or Prepare Moderately adapters. Their normal hooks and
their own settings must provide their native behavior. Random Start's specific
controls for those integrations are not applied by this prototype. Those adapters
remain available to the existing startup implementation until replacement is
validated; this milestone has not yet reduced their maintained code.

## Deliberately incomplete

Ideology automation needs its own implementation and validation, especially
saved-ideology loading with asynchronous mismatch dialogs. On an ideology page,
an unfamiliar page or a subclass of a supported page, the prototype displays a
handover notice and stops all automatic advancement. Applied choices remain;
later supported pages still receive their configured inputs once. Finish the
remaining pages normally, including ideology. Their being shown is not evidence
that a mod's defaults are safe to accept automatically.

This is therefore not yet a fully automatic configured start with Ideology.
The initial purpose is to exercise native entry, page ownership and transitions
before changing the default or deleting adapters. The configured manual-pages
option continues to control the existing startup path, not this debug action.

Exceptions stop the prototype and clear its per-game override state. The error
dialog asks the tester to cancel that setup. Do not continue a partially failed
prototype as a verified configuration.

## Runtime checks still required

The cloud has stripped reference assemblies, not a runnable licensed game.
Portable tests cannot establish the order and behavior of native UI callbacks.
Before advancing this prototype to the normal Random button:

1. Trace a minimal start without Ideology, then with it. Check `Player.log` entries
   prefixed `[Random Start native prototype]` for opened pages and acceptance
   attempts. Compare with an ordinary New Colony start, including actual
   PreOpen/PostOpen/draw/validation and scenario callback order/count.
2. Verify applied storyteller/world/faction/site/save-mode values survive page
   drawing and transitions. Check one generated world, one pawn initialization
   and one map start, including validation dialogs and long events.
3. Open an extra modal over a page, reject a warning, go back, cancel and start
   again. Confirm no repeated queued action, rerolled page or leaked overrides.
   Check that automated pages stay covered without visible flashes, including
   between world-generation steps. Check the cover at different UI scales and
   after resizing the game window. Verify Enter/mouse input cannot change hidden
   controls, Escape/manual continuation reveals the current page, and genuine
   mod/validation dialogs remain visible and interactive.
4. Implement and verify normal ideology transitions without manually duplicating
   `PostIdeoChosen`, pawn generation, preset loading or dialog continuation.
5. Test representative scenario/page-hook/world mods. Trace KCSG/SOS2/native
   initialization once before deleting their old manual calls.
6. Retire controls that only exist to manipulate another mod's internals, with
   explicit release notes. Do not promise those controls survive without a
   maintained integration contract.

The pure sequencing tests cover one-time choices, identity, blocking, reentrancy,
asynchronous acceptance guards, handover and failure. Real Harmony tests cover
live argument patches, vetoes, `__runOriginal`, and cleanup after original/prefix/
postfix failures. Neither suite is a RimWorld integration test. See the
[Harmony audit](harmony-audit.md) for patch decisions and primary sources.
