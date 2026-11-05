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

    public static class SOS2Compat
    {
        private static readonly OptionalModApi Api = new OptionalModApi("Save Our Ship 2", message => Log.Warning(message));

        public static void SetupForStartInSpace()
        {
            // Ordinary scenarios can continue without SOS2 hooks; a space-start
            // scenario must not proceed in a partially initialized state.
            bool requiresInitialization = Find.Scenario.AllParts.Any(part =>
                IsSpaceStart(part.GetType()));
            if (!requiresInitialization)
                return;

            bool initialized = Api.TryRun(() =>
            {
                Type loadShip = OptionalModApi.RequireType("SaveOurShip2.ScenPart_LoadShip");
                Type startInSpace = OptionalModApi.RequireType("SaveOurShip2.ScenPart_StartInSpace");
                MethodInfo hasFilename = OptionalModApi.RequireMethod(loadShip, "HasValidFilename", typeof(bool));
                MethodInfo loadInit = OptionalModApi.RequireMethod(loadShip, "DoEarlyInit", typeof(void));
                MethodInfo spaceInit = OptionalModApi.RequireMethod(startInSpace, "DoEarlyInit", typeof(void));
                foreach (ScenPart part in Find.Scenario.AllParts)
                {
                    if (loadShip.IsInstanceOfType(part) && (bool)hasFilename.Invoke(part, null))
                        loadInit.Invoke(part, null);
                    else if (startInSpace.IsInstanceOfType(part))
                        spaceInit.Invoke(part, null);
                }
            });
            if (!initialized)
                throw new InvalidOperationException("Random Start cannot initialize this space-start scenario with the installed Save Our Ship 2. Choose another scenario or update the integration.");
        }

        private static bool IsSpaceStart(Type type)
        {
            for (; type != null; type = type.BaseType)
                if (type.FullName == "SaveOurShip2.ScenPart_LoadShip"
                    || type.FullName == "SaveOurShip2.ScenPart_StartInSpace")
                    return true;
            return false;
        }
    }

}
