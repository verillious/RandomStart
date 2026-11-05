# Archived native startup experiment

The page-driving prototype is preserved on
[`codex/native-startup-prototype`](https://github.com/verillious/RandomStart/tree/codex/native-startup-prototype),
including its controller, progress cover, debug action and test guide. The last
runtime implementation is commit
[`2a192a6`](https://github.com/verillious/RandomStart/commit/2a192a6).

It was removed from the current one-click branch because its manual-page and
dialog handovers did not meet the mod's core requirement, and maintaining another
startup path increased complexity. This branch no longer includes its debug
entry, window observers, controller, cover or dedicated sequencing tests.

The independently useful Harmony execution guards and real Harmony tests remain.
Use the [one-click design](automatic-compatibility.md) and
[simplification review](one-click-simplification.md) for current direction and
remaining compatibility limits. The archived experiment is available for source
investigation; it is not the recommended development build.
