using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace OskarObnoxious
{
    public class PatchRangeStatDefault : PatchRangeStat
    {
        public override string PatchNoteUnchanged(object value)
        {
            return TranslatorFormattedStringExtensions.Translate("ValueUnchangedDefault", label, FormatValue(value));
        }

        public override string PatchNoteChanged(object oldValue, object newValue)
        {
            return TranslatorFormattedStringExtensions.Translate("ValueChangedDefault", label, FormatValue(oldValue), FormatValue(newValue));
        }
    }
}
