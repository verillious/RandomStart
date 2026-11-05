using System;
using System.Collections.Generic;
using System.Linq;
using RandomStartMod;
using Xunit;

public class GenerationBugTests
{
    [Fact]
    public void TemperatureAloneActivatesStartingTileFiltering()
    {
        var filters = new StartingTileFilters<string, int>(Array.Empty<string>(), Array.Empty<int>(), true, -10, 30);
        Assert.True(filters.Active);
        Assert.True(filters.Matches("any biome", 1, 20));
        Assert.False(filters.Matches("any biome", 1, 40));
    }

    [Theory]
    [InlineData("forest", 2, -10, true)]
    [InlineData("forest", 2, 30, true)]
    [InlineData("forest", 2, 31, false)]
    [InlineData("desert", 2, 20, false)]
    [InlineData("forest", 4, 20, false)]
    public void TileFiltersIntersectAndIncludeTemperatureBoundaries(string biome, int hills, float temp, bool expected)
    {
        var filters = new StartingTileFilters<string, int>(new[] { "forest" }, new[] { 2 }, true, -10, 30);
        Assert.Equal(expected, filters.Matches(biome, hills, temp));
    }

    [Fact]
    public void EmptyResolvedFiltersLeaveNormalSelectionAvailable()
    {
        var filters = new StartingTileFilters<string, int>(Array.Empty<string>(), Array.Empty<int>(), false, -10, 30);
        Assert.False(filters.Active);
        Assert.True(filters.Matches("modded biome", 4, 100));
    }

    [Fact]
    public void CandidateSamplingIsUniformEvenWhenSettlementChecksRejectTiles()
    {
        var totals = new Dictionary<int, int> { [1] = 0, [3] = 0 };
        // Enumerate all 4! equally likely draws without statistical/flaky assertions.
        for (int a = 0; a < 4; a++)
            for (int b = 0; b < 3; b++)
                for (int c = 0; c < 2; c++)
                {
                    var draws = new Queue<int>(new[] { a, b, c, 0 });
                    Assert.True(RandomSelection.TryChooseIndex(4, _ => true, i => i == 1 || i == 3,
                        upper => { int value = draws.Dequeue(); Assert.InRange(value, 0, upper - 1); return value; }, out int chosen));
                    totals[chosen]++;
                }
        Assert.Equal(12, totals[1]);
        Assert.Equal(12, totals[3]);
    }

    [Fact]
    public void CheapFiltersRunBeforeSettlementChecksAndChecksStopAtFirstUsableTile()
    {
        int cheapChecks = 0, placementChecks = 0;
        Assert.True(RandomSelection.TryChooseIndex(1000, i => { cheapChecks++; return i % 2 == 1; },
            i => { placementChecks++; Assert.True(i % 2 == 1); return true; }, _ => 0, out int chosen));
        Assert.Equal(1, chosen);
        Assert.Equal(1000, cheapChecks);
        Assert.Equal(1, placementChecks);
    }

    [Fact]
    public void FailedSettlementChecksExhaustCandidatesOnceWithoutLooping()
    {
        var checkedTiles = new List<int>();
        Assert.False(RandomSelection.TryChooseIndex(4, _ => true, i => { checkedTiles.Add(i); return false; },
            _ => 0, out int chosen));
        Assert.Equal(-1, chosen);
        Assert.Equal(4, checkedTiles.Count);
        Assert.Equal(4, checkedTiles.Distinct().Count());
        Assert.False(RandomSelection.TryChooseIndex(4, _ => false, _ => throw new Exception("No candidates"),
            _ => throw new Exception("Do not draw from an empty pool"), out _));
    }

    [Fact]
    public void FactionPairsAreVisitedExactlyOnce()
    {
        var seen = new List<string>();
        Pairwise.ForEach(new[] { "player", "a", "b", "c" }, _ => false, (a, b) => seen.Add(a + ":" + b));
        Assert.Equal(new[] { "player:a", "player:b", "player:c", "a:b", "a:c", "b:c" }, seen);
        Assert.Equal(6, seen.Distinct().Count());
    }

    [Fact]
    public void ExcludedFactionsAreNeverPassedToEitherSideOfAnUpdate()
    {
        var seen = new List<string>();
        Pairwise.ForEach(new[] { "player", "excluded", "a", "b" }, value => value == "excluded",
            (a, b) => seen.Add(a + ":" + b));
        Assert.Equal(new[] { "player:a", "player:b", "a:b" }, seen);
    }

    [Fact]
    public void GenePrerequisitesAreOrderedOnceBeforeTheirDependents()
    {
        var graph = new Dictionary<string, string[]>
        {
            ["root"] = new[] { "a", "b" }, ["a"] = new[] { "shared" }, ["b"] = new[] { "shared" }, ["shared"] = null
        };
        Assert.True(DependencyGraph.TryOrder("root", key => graph[key], out var result));
        Assert.Equal(4, result.Count);
        foreach (var pair in graph)
            foreach (var dependency in pair.Value ?? Array.Empty<string>())
                Assert.True(result.IndexOf(dependency) < result.IndexOf(pair.Key));
    }

    [Fact]
    public void CyclicGenePrerequisitesReturnNoPartialPlan()
    {
        Assert.False(DependencyGraph.TryOrder("root", gene => new[] { gene == "root" ? "other" : "root" }, out var result));
        Assert.Empty(result);
    }

    private sealed class Gene
    {
        internal readonly string Name;
        internal readonly int Metabolism;
        internal Gene(string name, int metabolism) { Name = name; Metabolism = metabolism; }
    }

    [Fact]
    public void AcceptedGeneBatchUsesReturnedInstancesAndCountsMetabolismOnce()
    {
        var genes = new List<Gene> { new Gene("existing", 1) };
        var added = new Gene("new", -1);
        Assert.True(GeneBatch.TryApply(new[] { "new" }, name => genes.Any(g => g.Name == name),
            name => { genes.Add(added); return added; }, gene => genes.Remove(gene), () => genes.Sum(g => g.Metabolism) >= 0));
        Assert.Same(added, genes[1]);
        Assert.Equal(0, genes.Sum(g => g.Metabolism));
    }

    [Fact]
    public void RejectedGeneBatchRemovesOnlyItsNewInstancesInDependencySafeOrder()
    {
        var existing = new Gene("existing", 0);
        var genes = new List<Gene> { existing };
        var removed = new List<Gene>();
        Assert.False(GeneBatch.TryApply(new[] { "prerequisite", "root" }, name => genes.Any(g => g.Name == name),
            name => { var gene = new Gene(name, name == "root" ? -3 : 1); genes.Add(gene); return gene; },
            gene => { removed.Add(gene); genes.Remove(gene); }, () => genes.Sum(g => g.Metabolism) >= 0));
        Assert.Same(existing, Assert.Single(genes));
        Assert.Equal(new[] { "root", "prerequisite" }, removed.Select(g => g.Name));
    }

    [Fact]
    public void ExistingAndRepeatedGeneDefinitionsAreNotAddedTwice()
    {
        var existing = new Gene("old", 0);
        var genes = new List<Gene> { existing };
        int additions = 0;
        Assert.True(GeneBatch.TryApply(new[] { "old", "new", "new" }, name => genes.Any(g => g.Name == name),
            name => { additions++; var gene = new Gene(name, 0); genes.Add(gene); return gene; },
            gene => genes.Remove(gene), () => true));
        Assert.Equal(1, additions);
        Assert.Same(existing, genes[0]);
    }

    [Fact]
    public void FailedGeneAdditionRollsBackAndPreservesTheOriginalException()
    {
        var genes = new List<Gene>();
        var error = new InvalidOperationException("mod rejected gene");
        var actual = Assert.Throws<InvalidOperationException>(() =>
            GeneBatch.TryApply(new[] { "first", "broken" }, _ => false,
                name => { if (name == "broken") throw error; var gene = new Gene(name, 0); genes.Add(gene); return gene; },
                gene => genes.Remove(gene), () => true));
        Assert.Same(error, actual);
        Assert.Empty(genes);
    }

    [Fact]
    public void GeneValidationFailureAlsoRollsBackTheBatch()
    {
        var genes = new List<Gene>();
        Assert.Throws<InvalidOperationException>(() => GeneBatch.TryApply(new[] { "new" }, _ => false,
            name => { var gene = new Gene(name, 0); genes.Add(gene); return gene; },
            gene => genes.Remove(gene), () => throw new InvalidOperationException("validation failed")));
        Assert.Empty(genes);
    }
    [Fact]
    public void MissingGeneResultRollsBackEarlierAdditions()
    {
        var genes = new List<Gene>();
        Assert.Throws<InvalidOperationException>(() => GeneBatch.TryApply(new[] { "first", "missing" }, _ => false,
            name => { if (name == "missing") return null; var gene = new Gene(name, 0); genes.Add(gene); return gene; },
            gene => genes.Remove(gene), () => true));
        Assert.Empty(genes);
    }

    [Fact]
    public void RollbackFailureReportsBothErrorsInsteadOfHidingTheOriginal()
    {
        var original = new InvalidOperationException("add failed");
        var cleanup = new InvalidOperationException("remove failed");
        var actual = Assert.Throws<AggregateException>(() => GeneBatch.TryApply(new[] { "first", "broken" }, _ => false,
            name => { if (name == "broken") throw original; return new Gene(name, 0); },
            gene => throw cleanup, () => true));
        Assert.Equal(new Exception[] { original, cleanup }, actual.InnerExceptions);
    }

}
