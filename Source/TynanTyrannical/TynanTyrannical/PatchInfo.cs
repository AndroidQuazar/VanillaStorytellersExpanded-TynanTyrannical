using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace TynanTyrannical
{
    public class PatchInfo : IExposable
    {
        public string dateOfPatch;
        public string text;

        public PatchInfo()
        {
        }

        public PatchInfo(string text)
        {
            Vector2 location = Find.CurrentMap != null ? Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile) : default;
            dateOfPatch = GenDate.DateFullStringAt(Find.TickManager.TicksAbs, location);
            this.text = text;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref dateOfPatch, "dateOfPatch");
            Scribe_Values.Look(ref text, "text");
        }
    }
}
