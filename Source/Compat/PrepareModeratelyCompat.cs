using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RandomStartMod.Compat
{

    public static class PrepareModeratelyCompat
    {
        private static Type pawnFilterType;
        private static Type pawnFilterListerType;
        private static Assembly prepareModeratelyAssembly;

        static PrepareModeratelyCompat()
        {
            try
            {
                prepareModeratelyAssembly = AppDomain
                    .CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "PrepareModerately");

                if (prepareModeratelyAssembly != null)
                {
                    pawnFilterType = prepareModeratelyAssembly.GetType(
                        "Lakuna.PrepareModerately.Filter.PawnFilter"
                    );
                    pawnFilterListerType = prepareModeratelyAssembly.GetType(
                        "Lakuna.PrepareModerately.Filter.PawnFilterLister"
                    );
                }
            }
            catch (Exception ex)
            {
                Log.Warning(
                    $"[RandomStart] [PrepareModeratelyCompat] Failed to initialize PrepareModerately compatibility: {ex.Message}"
                );
            }
        }

        public static bool IsAvailable =>
            prepareModeratelyAssembly != null && pawnFilterType != null;

        public static List<string> GetAvailableFilterNames()
        {
            if (!IsAvailable || pawnFilterListerType == null)
                return new List<string>();

            try
            {
                var allMethod = pawnFilterListerType.GetMethod(
                    "All",
                    BindingFlags.Public | BindingFlags.Static
                );
                if (allMethod == null)
                    return new List<string>();

                var filters = allMethod.Invoke(null, null) as IEnumerable;
                var filterNames = new List<string>();

                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        var nameProperty = pawnFilterType.GetProperty("Name");
                        if (
                            nameProperty?.GetValue(filter) is string name
                            && !string.IsNullOrEmpty(name)
                        )
                        {
                            filterNames.Add(name);
                        }
                    }
                }

                return filterNames;
            }
            catch (Exception ex)
            {
                Log.Warning(
                    $"[RandomStart] [PrepareModeratelyCompat] Failed to get available filter names: {ex.Message}"
                );
                return new List<string>();
            }
        }

        public static object GetFilterByName(string filterName)
        {
            if (!IsAvailable || string.IsNullOrEmpty(filterName))
                return null;

            try
            {
                var allMethod = pawnFilterListerType.GetMethod(
                    "All",
                    BindingFlags.Public | BindingFlags.Static
                );
                if (allMethod == null)
                    return null;

                var filters = allMethod.Invoke(null, null) as IEnumerable;
                if (filters == null)
                    return null;

                foreach (var filter in filters)
                {
                    var nameProperty = pawnFilterType.GetProperty("Name");
                    if (nameProperty?.GetValue(filter) is string name && name == filterName)
                    {
                        return filter;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(
                    $"[RandomStart] [PrepareModeratelyCompat] Failed to get filter by name '{filterName}': {ex.Message}"
                );
            }

            return null;
        }

        public static string GetFilterDescription(string filterName)
        {
            if (!IsAvailable || string.IsNullOrEmpty(filterName))
                return "";

            try
            {
                var filter = GetFilterByName(filterName);
                if (filter != null)
                {
                    // Try to get Description property first
                    var descriptionProperty = pawnFilterType.GetProperty("Description");
                    if (
                        descriptionProperty?.GetValue(filter) is string description
                        && !string.IsNullOrEmpty(description)
                    )
                    {
                        return description;
                    }

                    // Fallback to Summary property if Description is empty
                    var summaryProperty = pawnFilterType.GetProperty("Summary");
                    if (
                        summaryProperty?.GetValue(filter) is string summary
                        && !string.IsNullOrEmpty(summary)
                    )
                    {
                        return summary;
                    }

                    // If both are empty, return a default message
                    return "No description available";
                }
            }
            catch (Exception ex)
            {
                Log.Warning(
                    $"[RandomStart] [PrepareModeratelyCompat] Failed to get filter description for '{filterName}': {ex.Message}"
                );
            }

            return "";
        }

        public static void SetCurrentFilter(string filterName)
        {
            if (!IsAvailable || string.IsNullOrEmpty(filterName))
                return;

            try
            {
                var filter = GetFilterByName(filterName);
                if (filter != null)
                {
                    var currentProperty = pawnFilterType.GetProperty(
                        "Current",
                        BindingFlags.Public | BindingFlags.Static
                    );
                    currentProperty?.SetValue(null, filter);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(
                    $"[RandomStart] Failed to set current filter to '{filterName}': {ex.Message}"
                );
            }
        }

        public static string GetCurrentFilterName()
        {
            if (!IsAvailable)
                return "";

            try
            {
                var currentProperty = pawnFilterType.GetProperty(
                    "Current",
                    BindingFlags.Public | BindingFlags.Static
                );
                var currentFilter = currentProperty?.GetValue(null);

                if (currentFilter != null)
                {
                    var nameProperty = pawnFilterType.GetProperty("Name");
                    return nameProperty?.GetValue(currentFilter) as string ?? "";
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[RandomStart] Failed to get current filter name: {ex.Message}");
            }

            return "";
        }

        public static void RandomiseStartingPawns()
        {
            if (!IsAvailable)
                return;

            try
            {
                // Check if Find.GameInitData exists
                if (Find.GameInitData == null)
                {
                    Log.Warning(
                        "[RandomStart] Find.GameInitData is null, cannot randomise starting pawns"
                    );
                    return;
                }

                // Check if startingAndOptionalPawns exists
                if (Find.GameInitData.startingAndOptionalPawns == null)
                {
                    Log.Warning(
                        "[RandomStart] Find.GameInitData.startingAndOptionalPawns is null, cannot randomise starting pawns"
                    );
                    return;
                }

                // Get the current filter using reflection
                var currentProperty = pawnFilterType.GetProperty(
                    "Current",
                    BindingFlags.Public | BindingFlags.Static
                );

                if (currentProperty == null)
                {
                    Log.Warning("[RandomStart] Could not find Current property on PawnFilter type");
                    return;
                }

                var currentFilter = currentProperty.GetValue(null);

                if (currentFilter == null)
                {
                    Log.Warning("[RandomStart] PrepareModerately current filter is null");
                    return;
                }
                else
                {
                    var nameProperty = pawnFilterType.GetProperty("Name");
                    var filterName = nameProperty?.GetValue(currentFilter) as string ?? "Unknown";
                    Util.LogMessage($"Using PrepareModerately filter: {filterName}");
                }

                // Get the Matches method from the current filter
                var matchesMethod = pawnFilterType.GetMethod(
                    "Matches",
                    new Type[] { typeof(Pawn) }
                );

                if (matchesMethod == null)
                {
                    Log.Warning("[RandomStart] Could not find Matches method on PawnFilter type");
                    return;
                }

                Util.LogMessage(
                    $"Starting pawn randomization with {Find.GameInitData.startingAndOptionalPawns.Count} pawns"
                );

                for (int i = 0; i < Find.GameInitData.startingAndOptionalPawns.Count; i++)
                {
                    int attemptCount = 0;
                    while (
                        (bool)
                            matchesMethod.Invoke(
                                currentFilter,
                                new object[] { Find.GameInitData.startingAndOptionalPawns[i] }
                            ) == false
                    )
                    {
                        StartingPawnUtility.RegenerateStartingPawnInPlace(i);
                        attemptCount++;
                        if (attemptCount >= 250)
                        {
                            Log.Warning(
                                $"[RandomStart] Pawn at index {i} did not match filter after 250 attempts, moving to next pawn"
                            );
                            break;
                        }
                    }
                }

                Util.LogMessage("Completed pawn randomization with PrepareModerately filter");
            }
            catch (Exception ex)
            {
                Log.Warning(
                    $"[RandomStart] Failed to randomise starting pawns with PrepareModerately filter: {ex.Message}\nStack trace: {ex.StackTrace}"
                );
            }
        }
    }

}
