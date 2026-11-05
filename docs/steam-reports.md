# Steam report triage

Reviewed on 2026-11-05: **all 233 publicly accessible comments** on the
[Random Start Workshop item](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947),
retrieved in three read-only pages of 100, 100, and 33 comments. This count includes
questions, praise, author replies, feature requests, and historical reports;
it does not mean 233 bugs. Dates below use Europe/London. Pagination can shift as
new comments arrive; comment IDs identify the cited reports.

No Steam replies, Discord messages, or external log uploads were made. Off-site
logs/screenshots linked by commenters were not inspected, and old reports do not
establish that a bug still exists in the current branch.

**Status language:** a source-level fix means a concrete code path was corrected.
Automated tests cover the existing policy/boundary suite, not UI rendering,
fluid ideology progression, saved-file compatibility, or real optional mods.
None of the new report-driven changes is claimed gameplay-verified.

## Reports and status

| Report cluster | Representative comments | Assessment / action |
| --- | --- | --- |
| Sanguophage/special starting xenotypes become baseliners | [2026-06-19](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=1#comment_565911724530847647), [2026-03-18](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=1#comment_797838866430104604) | Earlier compatibility refactor preserves scenario-owned generation and request data. Strong candidate cause addressed; Sanguophage/other special starts still need in-game verification. |
| Fluid ideology becomes fixed or cannot reform | [2026-04-25](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=1#comment_797842245107645797), [2026-02-06](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=2#comment_766309862211773344), [2025-11-30](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=2#comment_691994366768720289) | This branch explicitly sets Fluid and initializes development, using the vanilla initial-fluid meme target and generation API. Source-level fix; runtime/reform progression not verified. |
| Saved ideology ignored or stops loading on subsequent runs | [2025-09-11](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=3#comment_604166953732562916), [2025-08-09](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=4#comment_601911983260092588), [2025-07-17](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=5#comment_601910081412526990) | This branch stops explicitly for missing files/failed loads instead of silently falling back or dereferencing null. Version/mod-list mismatch dialogs and compatible-file reload behavior still need runtime tests; not a universal load repair. |
| Faction trash button removes the wrong faction | [2025-07-24](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=4#comment_601910715107874860) | Confirmed filtered-display-index versus saved-list-index mismatch. This branch removes the clicked defName and updates the visible cache. Equivalent biome/meme/precept rows also corrected. UI verification pending. |
| Scenario settings GUI throws NullReferenceException | [2025-09-02](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=3#comment_601914224408827680), [2026-04-24](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=1#comment_797842031906016393) | This branch handles missing source-mod metadata/icons generically. Plausible error path hardened; original full stack trace/mod list still needed to confirm these reports. |
| My Little Planet size applies only after opening settings | [2024-07-29](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=12#comment_4511002532257240932), [2025-07-01](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=6#comment_601908674063053231) | Confirmed application in the drawing method only. This branch applies the saved subdivision setting before automatic world generation. Real-mod test pending. |
| Duplicate Random buttons | [2025-09-06](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=3#comment_601914519534768174), [2025-08-17](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=3#comment_524225618711460176), [2024-08-23](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=11#comment_4423184732110940249) | Earlier idempotent insertion addresses repeated insertion by this patch. Verify one installed mod copy and inspect other menu patches if it persists; not every possible duplicate cause is proven fixed. |
| Random faction range produces unexpectedly low counts | [2025-04-07](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=7#comment_601901034049165495) | Earlier refactor samples the count once rather than resampling every loop condition. Available/unique faction limits still constrain the result. Game verification pending. |
| Starting/scattered items ignore count/value settings | [2025-07-19](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=5#comment_601910394159628661), [2024-08-27](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=10#comment_4423184908209597125) | Earlier scenario-based item delivery/removal and budget checks address source-level issues. Budget covers random additions, and removal preserves custom mod-owned parts. Confirm the exact toggles/scenario before classifying remaining items as a bug. |
| Second random start fails until restarting RimWorld | [2025-04-07](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=8#comment_601901034049142149), [2024-07-11](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=12#comment_4406292251593385373) | Earlier cloned scenarios and game-scoped cleanup address plausible retained state. Current logs and a repeat-start reproduction remain required. |
| Permanent scenario condition absent on initial map | [2025-04-07](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=7#comment_601901034049142576) | Unresolved. Needs current scenario/DLC details and lifecycle tracing; do not assume the callback changes have fixed it. |
| VFE Tribals/Wild Men get ideology before Culture research | [2025-01-07](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=8#comment_595137443379890133), [2024-06-29](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=14#comment_4406291166509452872), [2026-03-22](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=1#comment_797839165142205414) | Unresolved integration behavior. Use normal setup screens is a candidate workaround, not a verified fix. Avoid adding a guessed faction-ID exception. |
| Combat Extended ammo/gear causes generation errors | [2024-08-31](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=10#comment_4767710741170569246), [2024-08-31](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=10#comment_4767710741170578832) | Historical dev-build report; normal arrival delivery now restored. Needs current CE version, settings, scenario and fresh log to reproduce. |
| Real Ruins background generation hangs | [2024-08-22](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=11#comment_4423184558852191414) | Unresolved real-mod/service interaction. Needs a current reproduction and log; no speculative timeout or disabled assertions added. |
| Menu buttons extend below the screen with Dev/Continue | [2025-12-24](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=2#comment_694248493352543497) | Unresolved layout report. Requires display dimensions/UI scale and current menu contents; additive insertion alone does not solve overflow. |
| Biomes! Caverns never produces cave starts | [2024-09-12](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=10#comment_4755326933237182397), [2024-12-22](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=8#comment_601891362926819842) | Needs current mod/version and expected layer/biome behavior. Surface-only selection and explicit legacy exclusions may be relevant; no unverified layer-generation rewrite. |
| Non-English text fails to load | [2024-08-04](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=12#comment_4511003232654866634) | Old unspecific report. Malformed Chinese XML and duplicate keys were fixed earlier. Current language/log details needed if other localization failures persist. |
| Ideology save reload breaks crafting | [2024-06-03](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=17#comment_4339860800345608716), [2024-06-04](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=15#comment_4333105589461820869) | Historical: author reported a v0.11.2 fix in the thread. Keep a modern save/load smoke test; do not count the old report as a confirmed current regression. |
| Unique faction mode removes saved preferences | [2024-06-05](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=14#comment_4333105589463680183), [2024-06-05](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=14#comment_4333105589464689643) | Historical author-confirmed fix. Current generation uses a per-run pool and preserves saved missing faction IDs; retain repeat-start coverage. |
| Goodwill changes even when disabled | [2024-07-03](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=13#comment_4406291470272175644), [2024-07-04](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=13#comment_4406291673455400249) | Historical author-confirmed v0.12.4 fix. Current code retains the explicit setting gate and pairwise game-API updates. |
| Other map-generation crashes without actionable logs | [2025-04-24](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=7#comment_601902470979974207), [2025-04-11](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=7#comment_530969587393575607), [2025-06-28](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=6#comment_601908461606822074) | Unresolved/insufficient evidence. Need exact game/mod versions, scenario/settings and fresh Player.log before assigning a cause. |

| Cannibal/bloodfeeder memes conflict with generated precepts | [2024-05-31](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=21#comment_4339860450392459303) | Historical report. This branch supplies the selected meme plan to the normal ideology generator before initialization, instead of replacing memes afterwards. Consistent precepts still require an in-game check; the original base-game attribution is not verified. |
| Genes do not grant their advertised traits | [2024-08-13](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=11#comment_4511004055390514101) | Historical report. Earlier gene fixes removed the untracked phantom gene and use real AddGene/RemoveGene objects. Need the specific gene defs/mod versions and current trait/active-gene inspection before claiming this report fixed. |
| Ideology remains uneditable with an evolution option enabled | [2025-11-30](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=2#comment_691994366768720289) | Insufficient details to identify the mod/system or whether reformation conditions were met. Fluid initialization is corrected, but this broader report remains unverified. |

## Feature requests kept separate

These are scope decisions, not confirmed defects. Prefer normal setup/data APIs
and a small supported feature set rather than promising another private integration
for every request.

- Exclude planetkillers/permanent events: [2026-10-29](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=1#comment_571553273173173723), [2024-11-21](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=9#comment_4625854789815729108).
- Bell-curve randomization: [2025-12-28](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=2#comment_694248809816968063).
- River/coast/road/landform filters: [2025-11-09](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=2#comment_660467372238399283), [2024-06-11](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=14#comment_4333106094377321051).
- Character/Pawn Editor presets: [2025-12-22](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=2#comment_694248165029831093), [2024-06-22](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=14#comment_4343240135702651488).
- World tech limits, Faction Control, Worldbuilder, custom map sizes: [2025-09-11](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=3#comment_604166953732597946), [2025-08-28](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=3#comment_601913886224023705), [2025-07-27](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=4#comment_601910715108155540), [2025-07-19](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=4#comment_601910394159643588).
- Specific/custom xenotype pools: [2026-01-20](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=2#comment_737034982498935264), [2025-07-09](https://steamcommunity.com/sharedfiles/filedetails/?id=3257028947&ctp=5#comment_601909292780612727).

## Next verification priorities

1. Core + Harmony + Ideology: fluid flag/reform progression, fixed meme ranges,
   required/forced memes, and saved ideology success/missing/mismatched files.
2. Core + Harmony + Biotech: Sanguophage start and a scenario with required xenotypes,
   with random xenotypes/genes disabled.
3. UI list containing a disabled-mod def followed by two active defs: delete each
   visible selection and verify the actual saved identity, then reopen settings.
4. My Little Planet: fresh launch, do not open Random Start settings, generate a
   world with the stored size and repeat with another value.
5. Real Ruins, CE, VFE Tribals and menu overflow as independent reproductions with
   current logs/versions. Keep unverified integrations out of a stable claim.
