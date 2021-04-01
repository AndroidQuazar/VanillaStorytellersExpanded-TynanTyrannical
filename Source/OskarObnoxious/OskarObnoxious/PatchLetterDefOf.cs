using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace OskarObnoxious
{
    [DefOf]
    public static class PatchLetterDefOf
    {
        public static LetterDef PatchLetter;

        static PatchLetterDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(PatchLetterDefOf));
        }
    }
}
