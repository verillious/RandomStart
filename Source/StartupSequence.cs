using System;
using System.Runtime.CompilerServices;

namespace RandomStartMod
{
    // A transition can queue asynchronous work or a dialog without closing its
    // page. Claim it before calling out so subsequent frames cannot repeat it.
    internal sealed class StartupSequence<TStep> where TStep : class
    {
        private sealed class StepState { internal bool AdvanceAttempted; }
        private readonly ConditionalWeakTable<TStep, StepState> steps = new ConditionalWeakTable<TStep, StepState>();
        internal bool Stopped { get; private set; }

        internal bool ConfigureOnce(TStep step, Action configure)
        {
            if (steps.TryGetValue(step, out _)) return false;
            steps.Add(step, new StepState());
            try { configure(); }
            catch { Stop(); throw; }
            return true;
        }

        internal bool TryAdvance(TStep step, bool blocked, Action advance)
        {
            if (Stopped || blocked || !steps.TryGetValue(step, out StepState state) || state.AdvanceAttempted)
                return false;
            state.AdvanceAttempted = true;
            try { advance(); }
            catch { Stop(); throw; }
            return true;
        }

        internal bool ShouldCover(TStep step, bool stepOpen, bool blocked, bool workPending)
        {
            if (Stopped || blocked || step == null || !steps.TryGetValue(step, out StepState state))
                return false;
            // An acceptance that leaves its page open without queued work needs
            // manual correction. Keep that page visible instead of trapping the
            // user behind progress. Dialogs are always revealed by 'blocked'.
            return workPending || (stepOpen && !state.AdvanceAttempted);
        }

        internal void Stop() => Stopped = true;
    }
}
