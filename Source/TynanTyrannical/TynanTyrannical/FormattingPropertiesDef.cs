using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace TynanTyrannical
{
    public class FormattingPropertiesDef : Def
    {
        public float roundTo = 0.01f;
        public float setValueRoundTo = 0.01f;
        public float multiplyBy = 1;
        public string startSymbol;
        public string endSymbol;
    }
}
