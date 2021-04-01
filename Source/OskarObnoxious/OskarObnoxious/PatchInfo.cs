using Verse;
using RimWorld;
using UnityEngine;


namespace OskarObnoxious
{
    public class PatchInfo : IExposable
    {
        public string title;
        public string dateOfPatch;
        public string text;

        public PatchInfo()
        {
        }

        public PatchInfo(string text)
        {
            Vector2 location = Find.CurrentMap != null ? Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile) : default;
            dateOfPatch = GenDate.DateFullStringAt(Find.TickManager.TicksAbs, location);
            if (PatchNotes.NamePatchNotes)
            {
                title = TranslatorFormattedStringExtensions.Translate("PatchTitle", PatchVersion.NextVersion.ToString(), PatchNotes.patchNoteNames.RandomElement());
            }
            else
            {
                title = TranslatorFormattedStringExtensions.Translate("PatchTitleNoName", PatchVersion.NextVersion.ToString());
            }
            this.text = text;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref title, "name");
            Scribe_Values.Look(ref dateOfPatch, "dateOfPatch");
            Scribe_Values.Look(ref text, "text");
        }
    }
}
