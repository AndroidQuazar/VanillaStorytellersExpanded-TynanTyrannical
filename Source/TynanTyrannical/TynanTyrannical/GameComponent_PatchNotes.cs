using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace TynanTyrannical
{
    public class GameComponent_PatchNotes : GameComponent
    {
        internal int timeTillNextPatchNotes;
        public static GameComponent_PatchNotes Instance { get; private set; }

        public GameComponent_PatchNotes(Game game)
        {
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            timeTillNextPatchNotes = TTMod.settings.ticksBetweenPatchNotes;
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            Instance = this;
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            timeTillNextPatchNotes--;
            if (timeTillNextPatchNotes <= 0)
            {
                InitiatePatchNotes();
            }
        }

        public static void InitiatePatchNotes()
        {
            Instance.timeTillNextPatchNotes = TTMod.settings.ticksBetweenPatchNotes;
            Rand.PushState();
            PatchNotes.ReceivePatchNotes();
            Rand.PopState();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref timeTillNextPatchNotes, "timeTillNextPatchNotes");
        }
    }
}
