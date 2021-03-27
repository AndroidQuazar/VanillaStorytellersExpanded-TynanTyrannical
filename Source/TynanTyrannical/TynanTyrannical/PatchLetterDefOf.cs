using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace TynanTyrannical
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
