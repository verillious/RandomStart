using System;

namespace RandomStartMod
{
    internal static class StartingItemBudget
    {
        internal static int AffordableCount(int stackLimit, float unitValue, float remaining)
        {
            if (stackLimit <= 0 || float.IsNaN(unitValue) || float.IsInfinity(unitValue) || unitValue < 0 || float.IsNaN(remaining) || remaining < 0)
                return 0;
            if (unitValue == 0 || float.IsPositiveInfinity(remaining)) return stackLimit;
            return (int)Math.Min(stackLimit, Math.Floor(remaining / unitValue));
        }
    }
}
