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
    public static class VECoreCompat
    {
        private static readonly OptionalModApi Api = new OptionalModApi("Vanilla Expanded Framework", message => Log.Warning(message));

        public static void SetupForKCSG()
        {
            ScenPart part = Current.Game.Scenario.AllParts.FirstOrDefault(
                candidate => candidate.def.defName == "VFEC_AddStartingStructure");
            if (part == null)
                return;

            bool initialized = Api.TryRun(() =>
            {
                Type utility = OptionalModApi.RequireType("KCSG.PrepareCarefully_Util");
                FieldInfo cacheField = OptionalModApi.RequireField(utility, "pcScenariosSave", true, typeof(IDictionary));
                FieldInfo choicesField = OptionalModApi.RequireField(part.GetType(), "chooseFrom", false, typeof(IList));
                FieldInfo nearCenterField = OptionalModApi.RequireField(part.GetType(), "nearMapCenter", false, typeof(bool));
                var cache = (IDictionary)cacheField.GetValue(null);
                var choices = (IList)choicesField.GetValue(part);
                bool nearCenter = (bool)nearCenterField.GetValue(part);
                if (cache == null)
                    throw new InvalidOperationException("KCSG scenario cache is not initialized.");

                // Resolve and validate all members before changing the external cache.
                object chosen = choices != null && choices.Count > 0
                    ? choices[Rand.Range(0, choices.Count)] : null;
                cache.Clear();
                if (chosen != null)
                    cache.Add(chosen, nearCenter);
            });
            if (!initialized)
                throw new InvalidOperationException("Random Start cannot initialize this starting-structure scenario with the installed Vanilla Expanded Framework. Choose another scenario or update the integration.");
        }
    }

}
