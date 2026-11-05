# Harmony audit for the smaller startup design

Reviewed the official Harmony documentation, its pinned `v2.3.6.0` implementation
(`e1354cb9a537be07182158f2d0b265ca969be402`), and this repository's patches.
The objective is the smallest intervention that preserves normal game/mod work,
not use of every Harmony feature. This audit does not certify every installed
mod combination or RimWorld's UI lifecycle.

## Findings applied in this change

- `PatchAll` now receives the mod assembly explicitly. Harmony's parameterless
  overload discovers its caller with a stack trace, which its own source warns
  can be affected by inlining.
- Setup scopes check `__runOriginal` before entering. Pawn/research completion
  postfixes also check it, so an original skipped by another prefix does not
  automatically trigger our completion work. A later prefix can still veto after
  our prefix; finalizers dispose any scope already entered.
- An important pinned-version detail: a prefix with only `__state` output is
  **not automatically skipped** after a veto. `PrefixAffectsOriginal` explicitly
  excludes `__state`. A scope can therefore enter even after the original was
  skipped unless the prefix checks `__runOriginal`. The new tests demonstrate
  that behavior using the actual Harmony package, not a homemade dispatcher.
- Native prototype page observers check `__runOriginal` and actual page opening.
  They operate only while that prototype owns the current game's startup state.
  Normal starts do not receive automatic page configuration/advancement.
- Cleanup finalizers return `void`, retain null-safe/idempotent disposal and
  preserve the original exception. They do not return `null` to suppress errors.

## Existing patch choices

| Area | Current approach and reason |
| --- | --- |
| Main menu | Modify the option list and retain normal drawing. Preserve foreign entries and insert ours once. |
| Game-ended letter | Pass-through enumerable postfix appends an option; preserve normal letter rendering and other choices. |
| Starting pawn requests | Scoped argument prefix changes explicitly configured constraints, retaining scenario requests and validators. |
| Pawn/research setup | Prefix/`__state`/postfix/finalizer scopes, now with original-execution guards. |
| Starting items | Scenario data plus narrow callbacks; normal delivery/arrival remains intact. |
| Research additions | Run normal starting grants, then use the normal completion API. |
| Research removal / no-prerequisite completion | Two narrow boolean prefixes intentionally suppress selected grants during a scoped operation. These remain behavioral conflict points to test; they are not universal compatibility guarantees. |
| Session lifetime | Current-game ownership; cleanup after `Game.InitNewGame` success or failure. |
| Native prototype | Live native entry/page methods, gated observers and a one-attempt controller. No copied transition body. |

The scopes are not proof that unrelated mod work inside the same call is safe
to remove. In particular, research overrides retain their existing semantics
and need representative gameplay checks.

## Capabilities considered

**Transpilers** can be appropriate for a small change inside an original method
when input/postfix changes cannot express it. They must match real method IL and
preserve labels, exception blocks and other transpilers' changes. The available
RimWorld references have stripped bodies: designing an IL matcher from them would
be guesswork. No new transpiler is justified by this milestone.

**Reverse patches** provide a private original/snapshot, excluding normal
prefixes/postfixes/finalizers and potentially later transpilers. That is unsuitable
for a startup flow whose purpose is to execute other mods' live patches. Calling
the native method normally preserves participation.

**Private-field injection / AccessTools field references / delegates** avoid some
reflection boilerplate. They do not make another mod's private fields a stable
contract. The prototype uses the existing publicized core reference build to
access a finite set of native page inputs; this still needs version maintenance.
Replacing every old reflection adapter with Harmony accessors would not remove
the dependency on other mods' internals.

**Priorities and before/after ordering** should address a demonstrated ordering
requirement. They do not fix skipped callbacks or conflicting ownership. This
change adds no priority arms race, forced load-order list or unpatching of other
mods. The isolated tests remove only their own owner ID's fake-target patches.

**Patch inspection** (`Harmony.GetPatchInfo`) can identify owners and ordering
when tracing a reported conflict. Patch presence alone is not an incompatibility
detector and must not trigger automatic manual mode or disabled mods.

## Validation and limits

`HarmonyCompositionTests` runs the pinned runtime against an isolated fake method
under .NET 8. It checks live argument modification, state-only prefixes after a
veto, guarded state entry, postfix original-execution reporting, and finalizer
cleanup with the same exception after failures in the original or foreign
prefix/postfix. This verifies the patterns we rely on, not game initialization
or Mono runtime integration.

The production mod continues targeting net472 and relies on the installed
Harmony mod. The runtime added to the test project is not bundled in the mod ZIP.
Native UI ordering, cancellation and actual mod combinations remain the runtime
checks described in [the prototype guide](native-startup-prototype.md).

## Primary sources (pinned version)

- [Harmony entry points and explicit-assembly PatchAll](https://github.com/pardeike/Harmony/blob/v2.3.6.0/Harmony/Public/Harmony.cs)
- [Execution order](https://github.com/pardeike/Harmony/blob/v2.3.6.0/Harmony/Documentation/articles/execution.md)
- [Prefixes and shared state](https://github.com/pardeike/Harmony/blob/v2.3.6.0/Harmony/Documentation/articles/patching-prefix.md)
- [Postfixes](https://github.com/pardeike/Harmony/blob/v2.3.6.0/Harmony/Documentation/articles/patching-postfix.md)
- [Injected values, including __runOriginal](https://github.com/pardeike/Harmony/blob/v2.3.6.0/Harmony/Documentation/articles/patching-injections.md)
- [Finalizers](https://github.com/pardeike/Harmony/blob/v2.3.6.0/Harmony/Documentation/articles/patching-finalizer.md)
- [Transpilers](https://github.com/pardeike/Harmony/blob/v2.3.6.0/Harmony/Documentation/articles/patching-transpiler.md)
- [Reverse patches](https://github.com/pardeike/Harmony/blob/v2.3.6.0/Harmony/Documentation/articles/reverse-patching.md)
- [Priorities](https://github.com/pardeike/Harmony/blob/v2.3.6.0/Harmony/Documentation/articles/priorities.md)
- [Pinned implementation: PrefixAffectsOriginal, state and finalizer handling](https://github.com/pardeike/Harmony/blob/v2.3.6.0/Harmony/Internal/MethodPatcher.cs)
