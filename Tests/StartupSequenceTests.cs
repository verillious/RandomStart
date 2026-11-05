using System;
using RandomStartMod;
using Xunit;

public class StartupSequenceTests
{
    [Fact]
    public void ReopeningTheSamePagePreservesItsOriginalChoices()
    {
        var sequence = new StartupSequence<object>();
        var page = new object();
        int choices = 0;
        Assert.True(sequence.ConfigureOnce(page, () => choices++));
        Assert.False(sequence.ConfigureOnce(page, () => choices++));
        Assert.Equal(1, choices);
    }

    [Fact]
    public void ABlockingDialogOrWorldGenerationDelaysWithoutConsumingTheAttempt()
    {
        var sequence = new StartupSequence<object>();
        var page = new object();
        sequence.ConfigureOnce(page, () => { });
        Assert.False(sequence.TryAdvance(page, true, () => throw new Exception("Blocked")));
        int advances = 0;
        Assert.True(sequence.TryAdvance(page, false, () => advances++));
        Assert.Equal(1, advances);
    }

    [Fact]
    public void ReentrantAndLaterFramesCannotStartTheSameAsyncOperationAgain()
    {
        var sequence = new StartupSequence<object>();
        var page = new object();
        sequence.ConfigureOnce(page, () => { });
        int worldsQueued = 0;
        Assert.True(sequence.TryAdvance(page, false, () =>
        {
            worldsQueued++;
            Assert.False(sequence.TryAdvance(page, false, () => worldsQueued++));
        }));
        Assert.False(sequence.TryAdvance(page, false, () => worldsQueued++));
        Assert.Equal(1, worldsQueued);
    }

    [Fact]
    public void UnconfiguredPagesNeverAdvance()
    {
        var sequence = new StartupSequence<object>();
        Assert.False(sequence.TryAdvance(new object(), false, () => throw new Exception("Unknown page")));
    }

    [Fact]
    public void HandoverStopsAutomationButRetainsConfigurationForLaterPages()
    {
        var sequence = new StartupSequence<object>();
        sequence.Stop();
        var laterPage = new object();
        int choices = 0;
        Assert.True(sequence.ConfigureOnce(laterPage, () => choices++));
        Assert.Equal(1, choices);
        Assert.False(sequence.TryAdvance(laterPage, false, () => throw new Exception("Manual handover")));
    }

    [Fact]
    public void FailedTransitionStopsFurtherAutomationAndPreservesTheException()
    {
        var sequence = new StartupSequence<object>();
        var page = new object();
        sequence.ConfigureOnce(page, () => { });
        var error = new InvalidOperationException("Mod rejected transition");
        Assert.Same(error, Assert.Throws<InvalidOperationException>(() => sequence.TryAdvance(page, false, () => throw error)));
        Assert.True(sequence.Stopped);
        var nextPage = new object();
        sequence.ConfigureOnce(nextPage, () => { });
        Assert.False(sequence.TryAdvance(nextPage, false, () => throw new Exception("Must remain stopped")));
    }

    [Fact]
    public void FailedConfigurationIsNotRetriedOnARepeatedOpenNotification()
    {
        var sequence = new StartupSequence<object>();
        var page = new object();
        Assert.Throws<InvalidOperationException>(() => sequence.ConfigureOnce(page, () => throw new InvalidOperationException()));
        Assert.True(sequence.Stopped);
        Assert.False(sequence.ConfigureOnce(page, () => throw new Exception("Must not reapply a partial configuration")));
    }

    private sealed class EqualPage
    {
        public override bool Equals(object obj) => obj is EqualPage;
        public override int GetHashCode() => 1;
    }

    [Fact]
    public void DifferentPageInstancesRemainIndependentEvenIfTheirEqualityIsOverridden()
    {
        var sequence = new StartupSequence<EqualPage>();
        int applied = 0;
        Assert.True(sequence.ConfigureOnce(new EqualPage(), () => applied++));
        Assert.True(sequence.ConfigureOnce(new EqualPage(), () => applied++));
        Assert.Equal(2, applied);
    }
}
