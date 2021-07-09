using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using RimWorld;

namespace OskarObnoxious
{
    public class GameComponent_PatchNotes : GameComponent
    {
        public int timeTillNextPatchNotes;
        public Dictionary<DefPatchPair, float> currentDefValues = new Dictionary<DefPatchPair, float>();
        public List<PatchInfo> patchNotes = new List<PatchInfo>();

        public static bool StorytellerLoaded => Find.Storyteller?.def == StorytellerDefOf.VSE_OskarObnoxious;

        /* Do Not Modify */
        public PatchVersion latestVersion;

        public static GameComponent_PatchNotes Instance { get; private set; }

        public GameComponent_PatchNotes(Game game)
        {
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            timeTillNextPatchNotes = TTMod.settings.ticksBetweenPatchNotes;
        }

        public override void LoadedGame()
        {
            base.LoadedGame();
            if (!currentDefValues.EnumerableNullOrEmpty() && StorytellerLoaded)
            {
                ResetPatchNoteValues();
            }
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            Instance = this;
            if (patchNotes is null)
            {
                patchNotes = new List<PatchInfo>();
            }
            if (currentDefValues is null)
            {
                currentDefValues = new Dictionary<DefPatchPair, float>();
            }
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (StorytellerLoaded)
            {
                timeTillNextPatchNotes--;
                if (timeTillNextPatchNotes <= 0)
                {
                    InitiatePatchNotes();
                }
            }
        }

        public static void InitiatePatchNotes()
        {
            Instance.timeTillNextPatchNotes = TTMod.settings.ticksBetweenPatchNotes;
            Rand.PushState();
            PatchNotes.ReceivePatchNotes();
            Rand.PopState();
        }

        public void RegisterPatch(string notes)
        {
            if (patchNotes.Count >= TTMod.settings.patchNotesStored)
            {
                int index = TTMod.settings.patchNotesStored - 1;
                patchNotes.RemoveRange(index, patchNotes.Count - index);
            }
            patchNotes.Insert(0, new PatchInfo(notes));
        }

        private void ResetPatchNoteValues()
        {
            if (StorytellerLoaded)
            {
                foreach (var defValues in PatchNotes.possibleDefs)
                {
                    Def def = defValues.Key;
                    foreach (var patchData in defValues.Value)
                    {
                        object parent = patchData.Item1;
                        string statDefName = patchData.Item2;
                        PatchRange patch = patchData.Item3;
                        DefPatchPair defPatch = new DefPatchPair(def.defName, statDefName, patch.FieldInfo);
                        if (currentDefValues.TryGetValue(defPatch, out float value))
                        {
                            float baseValue = Convert.ToSingle(patch.FieldInfo.GetValue(parent));
                            if (!baseValue.Equals(value))
                            {
                                try
                                {
                                    object valueConverted = Convert.ChangeType(value, patch.FieldInfo.FieldType);
                                    Log.Message($"Def: {def} Field: {patch.DisplayName} Default: {baseValue} Stored: {valueConverted}");
                                    patch.FieldInfo.SetValue(parent, valueConverted);
                                }
                                catch (Exception ex)
                                {
                                    Log.Error($"Exception thrown for {def.defName} field={patch.name}\nFailed to convert {baseValue.GetType()} to {value.GetType()}. Exception=\"{ex.Message}\"");
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref timeTillNextPatchNotes, "timeTillNextPatchNotes");
            Scribe_Collections.Look(ref currentDefValues, "currentDefValues", LookMode.Deep, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.Saving)
			{
                foreach (var defValue in currentDefValues)
				{
                    Log.Message($"Def: {defValue.Key.defName} Parent: {defValue.Key.statDefName} Field: {defValue.Key.name}");
                }
			}
            Scribe_Collections.Look(ref patchNotes, "patchNotes", LookMode.Deep);
            Scribe_Values.Look(ref latestVersion, "latestVersion");
        }
    }
}
