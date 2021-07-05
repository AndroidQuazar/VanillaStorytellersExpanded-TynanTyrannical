using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace OskarObnoxious
{
    public class PatchWindow : Window
    {
        public const float WindowWidth = 500;
        public const float WindowHeight = 750;
        public const float WindowContractBy = 5;
        public const float ScrollbarPadding = 20;

        public Listing_Standard lister = new Listing_Standard();

        public static Vector2 scrollPosition;

        public float PatchNotesHeight { get; private set; }

        public override Vector2 InitialSize => new Vector2(WindowWidth, WindowHeight);

        public PatchWindow()
        {
            doCloseX = true;
            closeOnClickedOutside = true;
        }

        public override void PostOpen()
        {
            base.PostOpen();
            RecacheHeight();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect rect = inRect;
            Rect viewRect = inRect;
            viewRect.height = PatchNotesHeight;
            viewRect.width -= ScrollbarPadding;

            var font = Text.Font;

            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
            lister.Begin(viewRect);
            foreach (PatchInfo patchNote in GameComponent_PatchNotes.Instance.patchNotes)
            {
                Text.Font = GameFont.Medium;
                lister.Label($"<b>{patchNote.title}</b>");
                lister.Label(patchNote.dateOfPatch);
                Text.Font = GameFont.Small;
                lister.Label(patchNote.text);
            }
            lister.End();
            Widgets.EndScrollView();

            Text.Font = font;
        }

        public void RecacheHeight()
        {
            float patchNotesHeight = 0;
            var font = Text.Font;
            int i = 0;
            float width = WindowWidth - ScrollbarPadding - (Margin * 2);
            foreach (PatchInfo patchNote in GameComponent_PatchNotes.Instance.patchNotes)
            {
                i++;
                Text.Font = GameFont.Medium;
                patchNotesHeight += Text.CalcHeight($"<b>{patchNote.title}</b>", width) * 1.25f;
                patchNotesHeight += Text.CalcHeight(patchNote.dateOfPatch, width);
                Text.Font = GameFont.Small;
                patchNotesHeight += Text.CalcHeight(patchNote.text, width);
            }
            PatchNotesHeight = patchNotesHeight;
            Text.Font = font;
        }

        public static void OpenWindow()
        {
            Find.WindowStack.Add(new PatchWindow());
        }
    }
}
