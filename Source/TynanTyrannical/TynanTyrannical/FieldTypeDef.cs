using System;
using System.Collections.Generic;
using Verse;

namespace TynanTyrannical
{
    public class FieldTypeDef : Def
    {
        public Type type;
        public List<PatchRange> fields = new List<PatchRange>();

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
            PatchNotes.RegisterNestedType(this, type);
        }
    }
}
