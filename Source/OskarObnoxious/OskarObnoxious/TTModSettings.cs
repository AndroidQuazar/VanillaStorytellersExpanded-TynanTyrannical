using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using Verse.Sound;
using RimWorld;
using UnityEngine;

namespace OskarObnoxious
{
    public class TTModSettings : ModSettings
    {
        private const int TicksBetweenPatchNotes = 180000;
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

            IntegerBox(lister, "TicksBetweenPatchNotes".Translate(), "TicksBetweenPatchNotesTooltip".Translate(), ref settings.ticksBetweenPatchNotes, 200, 0, 1);
            lister.Gap(2);

            SliderLabeled(lister, "DefsChangedPerPatch".Translate(), "DefsChangedPerPatchTooltip".Translate(), string.Empty, ref settings.defsChangedPerPatch, 1, 10);
            SliderLabeled(lister, "FieldsChangedPerDef".Translate(), "FieldsChangedPerDefTooltip".Translate(), string.Empty, ref settings.fieldsChangedPerDef, 1, 10);

            SliderLabeled(lister, "PatchNotesStored".Translate(), "PatchNotesStoredTooltip".Translate(), string.Empty, ref settings.patchNotesStored, 1, 20);

            lister.GapLine(8);

            if (lister.ButtonText("ResetPatchNotesToDefault".Translate()))
            {
                settings.ResetToDefault();
            }
            if (lister.ButtonText("ShowAllPatchNotes".Translate()))
            {
                if (Current.ProgramState == ProgramState.Playing)
				{
                    PatchWindow.OpenWindow();
                }
				else
				{
                    SoundDefOf.ClickReject.PlayOneShotOnCamera();
				}
            }

            lister.CheckboxLabeled("DebugShowPatchGeneration".Translate(), ref settings.debugShowPatchGeneration, "DebugShowPatchGenerationTooltip".Translate());

            lister.End();
        }

        public override string SettingsCategory()
        {
            return "OskarObnoxious".Translate();
        }

        public static void IntegerBox(Listing lister, string text, string tooltip, ref int value, float labelLength, float padding, int min = int.MinValue, int max = int.MaxValue)
        {
            Rect rect = lister.GetRect(Text.LineHeight);

            Rect rectLeft = new Rect(rect.x, rect.y, labelLength, rect.height);
            Rect rectRight = new Rect(rect.x + labelLength + padding, rect.y, rect.width - labelLength - padding, rect.height);

            Color color = GUI.color;
            Widgets.Label(rectLeft, text);

            var align = Text.CurTextFieldStyle.alignment;
            Text.CurTextFieldStyle.alignment = TextAnchor.MiddleLeft;
            string buffer = value.ToString();
            Widgets.TextFieldNumeric(rectRight, ref value, ref buffer, min, max);

            if (!tooltip.NullOrEmpty())
            {
                DoTooltipRegion(rect, tooltip, true);
            }
            Text.CurTextFieldStyle.alignment = align;
            GUI.color = color;
        }

        public static void SliderLabeled(Listing lister, string label, string tooltip, string endSymbol, ref int value, int min, int max, string maxValueDisplay = "", string minValueDisplay = "")
        {
            lister.Gap(12f);
            Rect rect = lister.GetRect(24f);
            string format = string.Format("{0}" + endSymbol, value);
            if (!maxValueDisplay.NullOrEmpty())
            {
                if (value == max)
                {
                    format = maxValueDisplay;
                }
            }
            if (!minValueDisplay.NullOrEmpty())
            {
                if (value == min)
                {
                    format = minValueDisplay;
                }
            }
            if (!tooltip.NullOrEmpty())
            {
                DoTooltipRegion(rect, tooltip, true);
            }
            value = (int)Widgets.HorizontalSlider(rect, value, min, max, false, null, label, format);
        }

        public static void DoTooltipRegion(Rect rect, string tooltip, bool slider = false)
        {
            if (slider)
            {
                rect.y -= Mathf.Round(rect.height / 2f) + 3;
            }
            TooltipHandler.TipRegion(rect, tooltip);
        }
    }
}
