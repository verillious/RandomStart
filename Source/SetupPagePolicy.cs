using System;
using System.Collections.Generic;

namespace RandomStartMod
{
    internal static class SetupPagePolicy
    {
        internal static TPage ForStart<TPage>(bool manualSetup, Func<TPage> fullSetup,
            IEnumerable<TPage> scenarioPages, Func<TPage, bool> supportedScenarioPage,
            string unsupportedMessage) where TPage : class
        {
            // Global setup pages may be replaced by a world-generation mod.
            // Only build that UI chain when the user actually requests it.
            if (manualSetup) return fullSetup();

            foreach (TPage page in scenarioPages)
                if (page != null && !supportedScenarioPage(page))
                    throw new InvalidOperationException(unsupportedMessage);
            return null;
        }
    }
}
