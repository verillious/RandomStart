using System;
using System.Collections.Generic;
using RandomStartMod;
using Xunit;

public class SetupPagePolicyTests
{
    private abstract class Page { }
    private sealed class StartingPawns : Page { }
    private sealed class ReplacementWorldSettings : Page { }
    private sealed class RequiredScenarioChoice : Page { }

    [Fact]
    public void AutomaticStartDoesNotBuildGlobalPagesThatAWorldModWouldReplace()
    {
        int globalPageBuilds = 0;
        var result = SetupPagePolicy.ForStart<Page>(false,
            () => { globalPageBuilds++; return new ReplacementWorldSettings(); },
            new Page[] { new StartingPawns() }, page => page is StartingPawns, "Unsupported scenario");
        Assert.Null(result);
        Assert.Equal(0, globalPageBuilds);
    }

    [Fact]
    public void ExplicitManualSetupKeepsTheLiveReplacementPageAndDoesNotInspectProvidersAgain()
    {
        var replacement = new ReplacementWorldSettings();
        int globalPageBuilds = 0;
        var result = SetupPagePolicy.ForStart<Page>(true,
            () => { globalPageBuilds++; return replacement; },
            UnexpectedEnumeration(), _ => throw new Exception("Manual pages must remain unrestricted"), "Unsupported scenario");
        Assert.Same(replacement, result);
        Assert.Equal(1, globalPageBuilds);
    }

    [Fact]
    public void AutomaticStartStillRejectsAnExtraChoiceSuppliedByTheScenario()
    {
        int globalPageBuilds = 0;
        var error = Assert.Throws<InvalidOperationException>(() => SetupPagePolicy.ForStart<Page>(false,
            () => { globalPageBuilds++; return new ReplacementWorldSettings(); },
            new Page[] { new StartingPawns(), new RequiredScenarioChoice() },
            page => page is StartingPawns, "Scenario requires additional input"));
        Assert.Equal("Scenario requires additional input", error.Message);
        Assert.Equal(0, globalPageBuilds);
    }

    [Fact]
    public void EmptyOrNullScenarioPageEntriesDoNotRequireASetupScreen()
    {
        foreach (var pages in new[] { Array.Empty<Page>(), new Page[] { null, new StartingPawns(), null } })
            Assert.Null(SetupPagePolicy.ForStart<Page>(false,
                () => throw new Exception("An automatic start must not build the full UI"),
                pages, page => page is StartingPawns, "Unsupported scenario"));
    }

    private static IEnumerable<Page> UnexpectedEnumeration()
    {
        yield return new StartingPawns();
        throw new Exception("The native page-chain builder already owns manual page construction");
    }
}
