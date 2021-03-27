using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using RimWorld;
using UnityEngine;

namespace TynanTyrannical
{
    public class TTModSettings : ModSettings
    {
        public int ticksBetweenPatchNotes = 25000;
        public int fieldsChangedPerPatch = 1;
        public int defsChangedPerPatch = 5;

        public bool debugShowPatchGeneration = false;

        public Dictionary<Def, float> currentDefValues = new Dictionary<Def, float>();

        internal List<PatchInfo> patchNotes = new List<PatchInfo>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksBetweenPatchNotes, "ticksBetweenPatchNotes", 25000);
            Scribe_Values.Look(ref fieldsChangedPerPatch, "fieldsChangedPerPatch", 1);
            Scribe_Values.Look(ref defsChangedPerPatch, "defsChangedPerPatch", 5);

            Scribe_Values.Look(ref debugShowPatchGeneration, "debugShowPatchGeneration", false);

            Scribe_Collections.Look(ref currentDefValues, "currentDefValues", LookMode.Def, LookMode.Value);
            //Scribe_Collections.Look(ref patchNotes, "patchNotes", LookMode.Deep);
        }
    }

    public class TTMod : Mod
    {
        public static TTModSettings settings;

        public static Listing_Standard lister = new Listing_Standard();

        public TTMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<TTModSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);

            Rect rect = new Rect(inRect)
            {
                width = inRect.width / 4
            };
            lister.Begin(rect);

            if (lister.ButtonText("ShowAllPatchNotes".Translate(), "ShowAllPatchNotesTooltip".Translate()))
            {
                PatchWindow.OpenWindow();
            }

            lister.CheckboxLabeled("DebugShowPatchGeneration".Translate(), ref settings.debugShowPatchGeneration, "DebugShowPatchGenerationTooltip".Translate());

            lister.End();
        }

        public override string SettingsCategory()
        {
            return "TynanTyrannical".Translate();
        }
    }
}
