using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using RimWorld;
using UnityEngine;
using SmashTools;

namespace OskarObnoxious
{
    public class TTModSettings : ModSettings
    {
        private const int TicksBetweenPatchNotes = 25000;
        private const int DefsChangedPerPatch = 5;
        private const int FieldsChangedPerDef = 1;
        private const int PatchNotesStored = 5;

        public int ticksBetweenPatchNotes = TicksBetweenPatchNotes;
        public int defsChangedPerPatch = DefsChangedPerPatch;
        public int fieldsChangedPerDef = FieldsChangedPerDef;
        public int patchNotesStored = PatchNotesStored;

        public bool debugShowPatchGeneration = false;

        public void ResetToDefault()
        {
            ticksBetweenPatchNotes = TicksBetweenPatchNotes;
            defsChangedPerPatch = DefsChangedPerPatch;
            fieldsChangedPerDef = FieldsChangedPerDef;
            patchNotesStored = PatchNotesStored;
            debugShowPatchGeneration = false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksBetweenPatchNotes, "ticksBetweenPatchNotes", TicksBetweenPatchNotes);

            Scribe_Values.Look(ref defsChangedPerPatch, "defsChangedPerPatch", DefsChangedPerPatch);
            Scribe_Values.Look(ref fieldsChangedPerDef, "fieldsChangedPerPatch", FieldsChangedPerDef);
            Scribe_Values.Look(ref patchNotesStored, "patchNotesStored", PatchNotesStored);

            Scribe_Values.Look(ref debugShowPatchGeneration, "debugShowPatchGeneration", false);
        }
    }

    public class TTMod : Mod
    {
        public static TTModSettings settings;

        public static Listing_Standard lister = new Listing_Standard();

        public TTMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<TTModSettings>();
            ParseHelper.Parsers<PatchVersion>.Register(new Func<string, PatchVersion>(PatchVersion.FromString));
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);

            Rect rect = new Rect(inRect)
            {
                width = inRect.width / 3
            };
            lister.Begin(rect);

            lister.IntegerBox("TicksBetweenPatchNotes".Translate(), "TicksBetweenPatchNotesTooltip".Translate(), ref settings.ticksBetweenPatchNotes, 200, 0, 1);
            lister.Gap(2);

            lister.SliderLabeled("DefsChangedPerPatch".Translate(), "DefsChangedPerPatchTooltip".Translate(), string.Empty, ref settings.defsChangedPerPatch, 1, 10);
            lister.SliderLabeled("FieldsChangedPerDef".Translate(), "FieldsChangedPerDefTooltip".Translate(), string.Empty, ref settings.fieldsChangedPerDef, 1, 10);

            lister.SliderLabeled("PatchNotesStored".Translate(), "PatchNotesStoredTooltip".Translate(), string.Empty, ref settings.patchNotesStored, 1, 20);

            lister.GapLine(8);

            if (lister.ButtonText("ResetPatchNotesToDefault".Translate()))
            {
                settings.ResetToDefault();
            }
            if (lister.ButtonText("ShowAllPatchNotes".Translate()))
            {
                PatchWindow.OpenWindow();
            }

            lister.CheckboxLabeled("DebugShowPatchGeneration".Translate(), ref settings.debugShowPatchGeneration, "DebugShowPatchGenerationTooltip".Translate());

            lister.End();
        }

        public override string SettingsCategory()
        {
            return "OskarObnoxious".Translate();
        }
    }
}
