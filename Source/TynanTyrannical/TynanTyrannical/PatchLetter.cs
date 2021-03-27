﻿using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace TynanTyrannical
{
    public class PatchLetter : LetterWithTimeout
    {
        public override void OpenLetter()
        {
            PatchWindow.OpenWindow();
        }

        protected override string GetMouseoverText()
        {
            return "PatchNotes".Translate();
        }
    }
}
