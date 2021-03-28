using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace TynanTyrannical
{
    public class PatchStatRange : PatchRange
    {
        public string label;

        public override string PatchNoteText(object oldValue, object newValue)
        {
            return TranslatorFormattedStringExtensions.Translate("ValueChanged", label, FormatValue(oldValue), FormatValue(newValue));
        }
    }
}
