using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RandomStartMod.Compat
{
    public static class VECoreCompat
    {
        public static bool Running()
        {
            return LoadedModManager.RunningMods.Any((ModContentPack m) => m.Name == "VFECore");
        }

        public static void SetupForKCSG()
        {
            if (Current.Game.Scenario.AllParts.ToList().Any((ScenPart s) => s.def.defName == "VFEC_AddStartingStructure"))
            {
                KCSG.PrepareCarefully_Util.pcScenariosSave.Clear();
                KCSG.ScenPart_AddStartingStructure scenPart_AddStartingStructure = (KCSG.ScenPart_AddStartingStructure)Current.Game.Scenario.AllParts.ToList().Find((ScenPart s) => s.def.defName == "VFEC_AddStartingStructure");
                List<KCSG.StructureLayoutDef> chooseFrom = scenPart_AddStartingStructure.chooseFrom;
                if (chooseFrom != null && chooseFrom.Count > 0)
                {
                    KCSG.PrepareCarefully_Util.pcScenariosSave.Add(scenPart_AddStartingStructure.chooseFrom.RandomElement(), scenPart_AddStartingStructure.nearMapCenter);
                }
            }
        }
    }
}
