using RimWorld;
using Verse;

namespace OskarObnoxious
{
    public class PatchRangeIngestible : PatchRange
    {
        public override bool DefIsPatchable(Def def)
        {
            return def is ThingDef thingDef && thingDef.ingestible.HumanEdible;
        }
    }
}
