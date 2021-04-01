using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace OskarObnoxious
{
    public class StatPatchDef : PatchDef
    {
        #pragma warning disable CS0649
        #pragma warning disable IDE0044
        private string statDef;
        #pragma warning restore IDE0044
        #pragma warning restore CS0649
        public PatchRangeStat patch;

        public StatDef StatDef { get; private set; }

        public void ResolveStatDef()
        {
            StatDef = DefDatabase<StatDef>.GetNamed(statDef);
            if (StatDef.forInformationOnly)
            {
                Log.Warning($"Resolving stat <color=teal>{StatDef.defName}</color> when it's marked as <b>For Information Only</b>");
            }
            patch.ResolveReferences(type);
            patch.StatPatchDef = this;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors())
            {
                yield return error;
            }

            if (string.IsNullOrEmpty(statDef))
            {
                yield return $"<color=teal>statDef</color> is a required field for {defName}";
            }
            if (patch is null)
            {
                yield return $"<color=teal>patch</color> is a required field for {defName}";
            }
        }
    }
}
