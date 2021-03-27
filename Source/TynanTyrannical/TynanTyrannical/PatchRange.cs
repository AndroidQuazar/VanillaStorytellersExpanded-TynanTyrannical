using System;
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
        public float ignoreIfValue = 0;
        public bool multiplier = true;

        public FormattingPropertiesDef formatting;

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
            randValue = randValue.RoundTo(formatting.setValueRoundTo);
            if (TTMod.settings.debugShowPatchGeneration)
            {
                Log.Message($"Setting {name} to new value {randValue}. Range: {range} IgnoreIfValue: {ignoreIfValue} Multiplier: {multiplier}");
            }
            return randValue;
        }

        public string FormatValue(object value)
        {
            if (value is null || !value.GetType().IsNumericType())
            {
                return "NaN";
            }
            float numericValue = Convert.ToSingle(value);
            numericValue *= formatting.multiplyBy;
            return $"{formatting.startSymbol}{numericValue.RoundTo(formatting.roundTo)}{formatting.endSymbol}";
        }

        public void ResolveReferences(Type type)
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
            if (formatting is null)
            {
                formatting = FormattingPropertiesDefOf.Decimal;
            }
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
