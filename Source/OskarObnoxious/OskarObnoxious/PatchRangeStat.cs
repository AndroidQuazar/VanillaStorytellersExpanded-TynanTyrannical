using System.Linq;
using Verse;
using RimWorld;
using System;

namespace OskarObnoxious
{
    public class PatchRangeStat : PatchRange
    {
        public StatPatchDef StatPatchDef { get; set; }

        public bool ApplyToDef(StatDef statDef)
        {
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                if (!thingDef.statBases.NullOrEmpty() && thingDef.statBases.Any(s => s.stat == statDef))
                {
                    name = "value";
                    StatPatchDef.label = string.Empty;
                    return false;
                }
            }
            return true;
        }
    }
}
