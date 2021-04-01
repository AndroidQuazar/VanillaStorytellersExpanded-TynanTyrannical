using System;
using System.Collections.Generic;
using Verse;

namespace OskarObnoxious
{
    public class PatchTypeDef : PatchDef
    {
        public List<PatchRange> fields = new List<PatchRange>();

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors())
            {
                yield return error;
            }
            if (!typeof(Def).IsAssignableFrom(type))
            {
                yield return $"<color=teal>type</color> in PatchTypeDef must be a <color=orange>Def</color> type.";
            }
            foreach (PatchRange field in fields)
            {
                foreach (string error in field.ConfigErrors())
                {
                    yield return error;
                }
            }
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            foreach (PatchRange field in fields)
            {
                field.ResolveReferences(type);
            }
            PatchNotes.BuildEffectableDefs(this);
        }
    }
}
