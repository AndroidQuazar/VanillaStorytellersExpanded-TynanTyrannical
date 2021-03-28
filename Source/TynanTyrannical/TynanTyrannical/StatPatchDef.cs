using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace TynanTyrannical
{
    public class StatPatchDef : PatchDef
    {
        public string statDef;
        public PatchStatRange patch;

        public StatDef StatDef { get; private set; }

        public void ResolveStatDef()
        {
            StatDef = DefDatabase<StatDef>.GetNamed(statDef);
            patch.ResolveReferences(type);
        }
    }
}
