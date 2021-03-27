using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace TynanTyrannical
{
    [DefOf]
    public static class FormattingPropertiesDefOf
    {
        public static FormattingPropertiesDef Decimal;

        static FormattingPropertiesDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(FormattingPropertiesDefOf));
        }
    }
}
