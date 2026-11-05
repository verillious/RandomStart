# Generation bug fixes

This follow-up builds on `cd33284` (the compatibility refactor). The changes are
verified with compilation and automated policy tests; neither this follow-up nor
the preceding compatibility refactor has yet received the user's in-game check.
The original working portable-build branch remains available separately.

## Starting tiles and seasons

Temperature-only filtering now activates the filtered selection path. Biome,
hilliness, and temperature conditions intersect, with inclusive temperature
bounds. Temporarily unavailable biome defs are resolved without deleting saved
preferences. Empty resolved biome/hilliness lists remain neutral.

Matching tiles are sampled randomly without replacement until the game's normal
settlement validation accepts one. This removes the old first-grid-tile bias and
repeated `IndexOf` scans. Cheap filters run in one pass; potentially expensive
settlement checks stop after the first accepted candidate. All random draws use
Verse's RNG. If no usable tile matches, the game selects its normal random start
and the existing fallback is logged.

The regular-season draw now explicitly includes spring, summer, fall, and winter.
The old exclusive upper bound omitted winter. Permanent-season choices remain
available only through the existing explicit setting.

## Genes

- Gene counts are sampled once per pawn and selected root defs are not repeated.
  Prerequisites can increase the total number of added genes beyond that count.
- A missing eligible xenotype pool preserves the scenario's existing xenotypes
  and reports a warning rather than drawing from an empty list.
- Prerequisites are ordered before dependents. Cyclic prerequisite chains are
  skipped with a bounded warning rather than recursing indefinitely.
- The normal `AddGene` API supplies the real gene instance and handles overrides.
  The separate phantom `GeneMaker.MakeGene` object and private override call are
  gone. Existing genes, including inactive ones, are not explicitly deleted.
- When a metabolism floor is enabled, the resulting active gene list is summed
  once after each batch. A rejected batch removes only its newly returned gene
  instances, in reverse dependency order, using the normal removal API. There is
  no manual incremental/double-counted metabolism counter.
- Gene batches use one xenogene/endogene choice for the root and its prerequisites.
  Existing scenario genes remain authoritative; an initially below-floor genome
  can stay unchanged when no single batch satisfies the configured floor.
- Names are derived from actual active genes after accepted changes.

Adding/removing genes invokes real game/mod callbacks. Automated tests cover the
batch contract, identity, ordering, and failure cleanup, not arbitrary mod gene
side effects. Test custom gene providers in-game before treating them as verified.

## Faction goodwill

Each eligible unordered faction pair is processed once. Exclusions apply to
both sides, the player remains eligible, and fixed/non-diplomatic hostility rules
are preserved. Existing relation lists are no longer cleared or reconstructed.
Updates use `TryAffectGoodwillWith`, with messages/letters suppressed, so the game
owns bilateral bookkeeping and any modded goodwill rules. Requested targets are
uniform multiples of ten from -100 to 100; game/mod rules may adjust or reject a
change. A missing relation is initialized through the game's API; a pair still
missing either direction is skipped.

## Checks to run later

1. Enable only temperature filtering, then combine it with biome/hilliness filters.
   Check the actual starting tile and the logged fallback for impossible filters.
2. With Biotech, test faction-restricted xenotypes and custom genes with the
   metabolism floor on/off. Check prerequisites, existing genes, and conflicts;
   repeat with a custom gene mod.
3. Randomize goodwill with an excluded faction. Check player relationships,
   permanent enemies, and NPC-to-NPC relations in developer inspection where
   available. Repeat with a diplomacy mod.
4. Run repeated starts with season randomization and confirm winter is selectable.

The automated tests exhaust the small tile-sampling outcome space to check
uniformity without flaky statistical assertions, check all pair visits and
exclusions, and exercise accepted/rejected/failed gene batches. Those checks do
not execute RimWorld itself.
