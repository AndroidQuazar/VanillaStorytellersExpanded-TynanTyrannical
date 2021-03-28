using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using RimWorld;

namespace TynanTyrannical
{
    public class GameComponent_PatchNotes : GameComponent
    {
        public int timeTillNextPatchNotes;
        public Dictionary<DefPatchPair, float> currentDefValues = new Dictionary<DefPatchPair, float>();
        public List<PatchInfo> patchNotes = new List<PatchInfo>();

        private List<Pair<Def, FieldInfo>> keys = new List<Pair<Def, FieldInfo>>();
        private List<object> values = new List<object>();

        private bool storytellerLoaded = false;

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
            if (!currentDefValues.EnumerableNullOrEmpty() && storytellerLoaded)
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
            storytellerLoaded = Find.Storyteller.def == StorytellerDefOf.VSE_TynanTyrannical;
            if (storytellerLoaded)
            {
                SetInitialValues();
            }
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (storytellerLoaded)
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

        private void SetInitialValues()
        {
            if (currentDefValues is null)
            {
                currentDefValues = new Dictionary<DefPatchPair, float>();
            }
            if (storytellerLoaded)
            {
                foreach (var defValues in PatchNotes.possibleDefs)
                {
                    Def def = defValues.Key;
                    foreach (var patchData in defValues.Value)
                    {
                        PatchRange patch = patchData.Second;
                        object parent = patchData.First;
                        if (!currentDefValues.ContainsKey(new DefPatchPair(def.defName, patch.FieldInfo)))
                        {
                            float baseValue = Convert.ToSingle(patch.FieldInfo.GetValue(parent));
                            currentDefValues.Add(new DefPatchPair(def.defName, patch.FieldInfo), baseValue);
                        }
                    }
                }
            }
        }

        private void ResetPatchNoteValues()
        {
            if (storytellerLoaded)
            {
                foreach (var defValues in PatchNotes.possibleDefs)
                {
                    Def def = defValues.Key;
                    foreach (var patchData in defValues.Value)
                    {
                        PatchRange patch = patchData.Second;
                        object parent = patchData.First;
                        if (currentDefValues.TryGetValue(new DefPatchPair(def.defName, patch.FieldInfo), out float value))
                        {
                            float baseValue = Convert.ToSingle(patch.FieldInfo.GetValue(parent));
                            if (!baseValue.Equals(value))
                            {
                                try
                                {
                                    object valueConverted = Convert.ChangeType(value, patch.FieldInfo.FieldType);
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
            Scribe_Collections.Look(ref patchNotes, "patchNotes", LookMode.Deep);
        }
    }
}
