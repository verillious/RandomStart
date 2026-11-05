using System;
using System.Collections.Generic;
using System.Linq;
using RandomStartMod;
using Xunit;

public class CompatibilityPoliciesTests
{
    [Fact]
    public void MenuInsertionPreservesForeignEntriesAndIsIdempotent()
    {
        var options = new List<string> { "continue", "multiplayer", "new", "quit" };
        int created = 0;
        Action insert = () => AdditiveOptions.InsertBefore(options, item => item == "new",
            item => item == "ours", () => { created++; return "ours"; });
        insert(); insert();
        Assert.Equal(new[] { "continue", "multiplayer", "ours", "new", "quit" }, options);
        Assert.Equal(1, created);
    }

    [Fact]
    public void MenuWithoutAnchorIsLeftAlone()
    {
        var options = new List<string> { "custom-menu" };
        AdditiveOptions.InsertBefore(options, item => item == "new", item => item == "ours",
            () => throw new Exception("Must not create an option without an anchor"));
        Assert.Equal(new[] { "custom-menu" }, options);
    }

    [Fact]
    public void LetterChoicesAreLazyAndPreserveOtherModsOptions()
    {
        int created = 0;
        var original = new[] { "continue", "other-mod", "close" };
        var result = AdditiveOptions.AppendOnce(original, item => item == "ours",
            () => { created++; return "ours"; });
        Assert.Equal(0, created);
        Assert.Equal(new[] { "continue", "other-mod", "close", "ours" }, result);
        Assert.Equal(1, created);
        Assert.Equal(new[] { "continue", "other-mod", "close" }, original);
    }

    [Fact]
    public void WrappingLetterChoicesTwiceDoesNotDuplicateOurOption()
    {
        var once = AdditiveOptions.AppendOnce(new[] { "foreign" }, item => item == "ours", () => "ours");
        var twice = AdditiveOptions.AppendOnce(once, item => item == "ours", () => "ours");
        Assert.Equal(new[] { "foreign", "ours" }, twice);
    }

    [Fact]
    public void StartOverridesCannotLeakIntoAnotherGame()
    {
        var sessions = new StartSession<object, string>();
        var firstGame = new object(); var secondGame = new object();
        sessions.Begin(firstGame, "first");
        Assert.Null(sessions.For(secondGame));
        sessions.Begin(secondGame, "second");
        Assert.Null(sessions.For(firstGame));
        Assert.Equal("second", sessions.For(secondGame));
        sessions.End(firstGame);
        Assert.Equal("second", sessions.For(secondGame));
        sessions.End(secondGame);
        Assert.Null(sessions.For(secondGame));
    }

    [Fact]
    public void ScopesRestoreAfterFailureAndCanBeDisposedByPostfixAndFinalizer()
    {
        var value = new ScopedValue<bool>();
        var outer = value.Enter(true);
        Assert.Throws<InvalidOperationException>((Action)(() =>
        {
            using (value.Enter(false)) throw new InvalidOperationException();
        }));
        Assert.True(value.Value);
        outer.Dispose(); outer.Dispose();
        Assert.False(value.Value);
    }

    [Fact]
    public void DependencyClosureHandlesSharedPrerequisitesCyclesAndNullLeaves()
    {
        var graph = new Dictionary<string, string[]>
        {
            ["selected"] = new[] { "a", "b" }, ["a"] = new[] { "shared" },
            ["b"] = new[] { "shared" }, ["shared"] = new[] { "selected", "leaf" }, ["leaf"] = null
        };
        var closure = DependencyGraph.Descendants("selected", node => graph[node]);
        Assert.True(closure.SetEquals(new[] { "a", "b", "shared", "leaf" }));
        Assert.DoesNotContain("selected", closure);
        Assert.Equal(new[] { "a", "b" }, graph["selected"]);
    }

    [Theory]
    [InlineData(75, 5, 100, 20)]
    [InlineData(75, 5, 1000, 75)]
    [InlineData(75, 5, 4, 0)]
    [InlineData(75, 5, 5, 1)]
    [InlineData(75, 0, 0, 75)]
    [InlineData(0, 5, 100, 0)]
    [InlineData(75, -1, 100, 0)]
    [InlineData(75, 5, -1, 0)]
    public void StartingItemStacksRespectTheirBudget(int stack, float price, float budget, int expected)
    {
        Assert.Equal(expected, StartingItemBudget.AffordableCount(stack, price, budget));
    }

    [Fact]
    public void StartingItemBudgetRejectsInvalidPricesAndHandlesUnlimitedBudget()
    {
        Assert.Equal(75, StartingItemBudget.AffordableCount(75, 5, float.PositiveInfinity));
        Assert.Equal(0, StartingItemBudget.AffordableCount(75, float.NaN, 100));
        Assert.Equal(0, StartingItemBudget.AffordableCount(75, float.PositiveInfinity, 100));
        Assert.Equal(0, StartingItemBudget.AffordableCount(75, 5, float.NaN));
    }
}
