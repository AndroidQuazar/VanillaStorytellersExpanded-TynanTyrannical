﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace TynanTyrannical
{
    public class PatchRange
    {
        public string name;
        public FloatRange range = new FloatRange(-3, 3);
        public FloatRange? limits;
        public float ignoreIfValue = -99999;
        public bool multiplier = true;

        public ToStringStyle toStringStyle;
        public float roundTo = 0.01f;

        public float weightToPatch = 1;

        internal Dictionary<Def, float> originalValues = new Dictionary<Def, float>();

        public FieldInfo FieldInfo { get; set; }

        public float NewRandomValue(Def def)
        {
            float randValue = range.RandomInRange;
            if (multiplier)
            {
                randValue *= originalValues[def];
            }
            randValue = randValue.RoundTo(roundTo);
            GameComponent_PatchNotes.Instance.currentDefValues[new DefPatchPair(def.defName, FieldInfo)] = randValue;
            return randValue;
        }

        public string FormatValue(object value)
        {
            if (value is null || !value.GetType().IsNumericType())
            {
                return "NaN";
            }
            float numericValue = Convert.ToSingle(value);
			return numericValue.ToStringByStyle(toStringStyle);
        }

        public virtual string PatchNoteUnchanged(object value)
        {
            return TranslatorFormattedStringExtensions.Translate("ValueUnchanged", name, FormatValue(value));
        }

        public virtual string PatchNoteChanged(object oldValue, object newValue)
        {
            return TranslatorFormattedStringExtensions.Translate("ValueChanged", name, FormatValue(oldValue), FormatValue(newValue));
        }

        public virtual bool DefIsPatchable(Def def)
        {
            return true;
        }

        public virtual void ResolveReferences(Type type)
        {
            FieldInfo fieldInfo = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo is null)
            {
                Log.Error($"Could not resolve {name} in {type}.");
                if (TTMod.settings.debugShowPatchGeneration)
                {
                    Log.Message("=============== All Fields ===============");
                    foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(f => f.FieldType.IsNumericType()))
                    {
                        Log.Message($"Name: {field.Name} Type: {field.FieldType} Public: {field.IsPublic}");
                    }
                    Log.Message("==========================================");
                }
            }
            FieldInfo = fieldInfo;
        }

        public IEnumerable<string> ConfigErrors()
        {
            if (name.NullOrEmpty())
            {
                yield return "<color=teal>field</color> cannot be empty.";
            }
            if (weightToPatch < 0)
            {
                yield return "<color=teal>weightToPatch</color> cannot be less than 0.";
            }
        }

        public override string ToString()
        {
            return $"{name}, {range}, {limits?.ToString() ?? "Null"}, {ignoreIfValue}";
        }
    }
}
