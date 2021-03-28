using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace TynanTyrannical
{
    [DefOf]
    public static class StorytellerDefOf
    {
        public static StorytellerDef VSE_TynanTyrannical;

        static StorytellerDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(StorytellerDefOf));
        }
    }
}
