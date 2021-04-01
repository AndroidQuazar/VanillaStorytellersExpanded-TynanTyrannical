using System;
using System.Collections.Generic;
using Verse;

namespace OskarObnoxious
{
    public abstract class PatchDef : Def
    {
        public Type type;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors())
            {
                yield return error;
            }
            if (type is null)
            {
                yield return $"<color=teal>type</color> cannot be null.";
            }
        }
    }
}
