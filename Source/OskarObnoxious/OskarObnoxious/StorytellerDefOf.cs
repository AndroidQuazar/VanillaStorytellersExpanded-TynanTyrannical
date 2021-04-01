using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace OskarObnoxious
{
    [DefOf]
    public static class StorytellerDefOf
    {
        public static StorytellerDef VSE_OskarObnoxious;

        static StorytellerDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(StorytellerDefOf));
        }
    }
}
