using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace TynanTyrannical
{
    public class PatchRangeStat : PatchRange
    {
        public string label;
        public bool applyToDef = false;

        public override string PatchNoteUnchanged(object value)
        {
            return TranslatorFormattedStringExtensions.Translate("ValueUnchanged", label, FormatValue(value));
        }

        public override string PatchNoteChanged(object oldValue, object newValue)
        {
            return TranslatorFormattedStringExtensions.Translate("ValueChanged", label, FormatValue(oldValue), FormatValue(newValue));
        }
    }
}
