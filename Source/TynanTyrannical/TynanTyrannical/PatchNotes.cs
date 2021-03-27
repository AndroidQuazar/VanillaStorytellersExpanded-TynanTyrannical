using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace TynanTyrannical
{
    public static class PatchNotes
    {
        internal static readonly Dictionary<Def, List<Pair<object, PatchRange>>> possibleDefs = new Dictionary<Def, List<Pair<object, PatchRange>>>();
        internal static readonly Dictionary<Def, float> defWeights = new Dictionary<Def, float>();

        private static readonly Dictionary<Type, FieldTypeDef> nestedTypes = new Dictionary<Type, FieldTypeDef>();

        private static readonly List<Pair<object, PatchRange>> patchableFields = new List<Pair<object, PatchRange>>();
        private static readonly List<string> defsAffected = new List<string>();

        public static void RegisterNestedType(FieldTypeDef fieldTypeDef, Type type)
        {
            nestedTypes.Add(type, fieldTypeDef);
        }

        public static void ReceivePatchNotes()
        {
            StringBuilder stringBuilder = new StringBuilder();
            defsAffected.Clear();
            for (int i = 0; i < TTMod.settings.defsChangedPerPatch; i++)
            {
                var defToChange = possibleDefs.Where(d => !d.Value.NullOrEmpty() && !defsAffected.Contains(d.Key.defName)).RandomElementByWeightWithFallback(e => defWeights[e.Key]);
                defsAffected.Add(defToChange.Key.defName);
                stringBuilder.AppendLine($"<color=green>{defToChange.Key.defName}</color>");
                for (int j = 0; j < Mathf.Min(TTMod.settings.fieldsChangedPerPatch, defToChange.Value.Count); j++)
                {
                    var objectPair = defToChange.Value.RandomElement();
                    object parent = objectPair.First;
                    PatchRange patch = objectPair.Second;
                    object oldValue = patch.FieldInfo.GetValue(parent);
                    float value = patch.NewRandomValue(defToChange.Key);
                    if (patch.limits.HasValue)
                    {
                        value = value.Clamp(patch.limits.Value.min, patch.limits.Value.max);
                    }
                    object valueConverted = Convert.ChangeType(value, patch.FieldInfo.FieldType);
                    patch.FieldInfo.SetValue(parent, valueConverted);
                    stringBuilder.AppendLine(TranslatorFormattedStringExtensions.Translate("ValueChanged", patch.name, patch.FormatValue(oldValue), patch.FormatValue(valueConverted)));
                }
            }
            SendPatchLetter(stringBuilder.ToString());
        }

        public static void ForceSpecificPatchNotes(FieldTypeDef fieldTypeDef)
        {
            StringBuilder stringBuilder = new StringBuilder();
            defsAffected.Clear();
            for (int i = 0; i < TTMod.settings.defsChangedPerPatch; i++)
            {
                var defToChange = possibleDefs.Where(d => !d.Value.NullOrEmpty() && !defsAffected.Contains(d.Key.defName) && 
                    d.Value.Any(p => fieldTypeDef.fields.Contains(p.Second))).RandomElementWithFallback();
                defsAffected.Add(defToChange.Key.defName);
                stringBuilder.AppendLine($"<color=green>{defToChange.Key.defName}</color>");
                for (int j = 0; j < Mathf.Min(TTMod.settings.fieldsChangedPerPatch, defToChange.Value.Count); j++)
                {
                    var objectPair = defToChange.Value.RandomElement();
                    object parent = objectPair.First;
                    PatchRange patch = objectPair.Second;
                    object oldValue = patch.FieldInfo.GetValue(parent);
                    float value = patch.NewRandomValue(defToChange.Key);
                    if (patch.limits.HasValue)
                    {
                        value = value.Clamp(patch.limits.Value.min, patch.limits.Value.max);
                    }
                    object valueConverted = Convert.ChangeType(value, patch.FieldInfo.FieldType);
                    patch.FieldInfo.SetValue(parent, valueConverted);
                    stringBuilder.AppendLine(TranslatorFormattedStringExtensions.Translate("ValueChanged", patch.name, oldValue.RoundTo(0.01f).ToStringSafe(), valueConverted.RoundTo(0.01f).ToStringSafe()));
                }
            }
            SendPatchLetter(stringBuilder.ToString());
        }

        private static void SendPatchLetter(string patchNotes)
        {
            if (TTMod.settings.patchNotes.Count == 5)
            {
                TTMod.settings.patchNotes.RemoveAt(0);
            }
            TTMod.settings.patchNotes.Insert(0, new PatchInfo(patchNotes));
            PatchLetter letter = (PatchLetter)LetterMaker.MakeLetter(PatchLetterDefOf.PatchLetter);
            letter.label = PatchLetterDefOf.PatchLetter.label;
            Find.LetterStack.ReceiveLetter(letter);
        }

        public static void BuildEffectableDefs(PatchTypeDef patchDef)
        {
            try
            {
                Type databaseType = typeof(DefDatabase<>).MakeGenericType(new Type[] { patchDef.type });
                var objCollection = databaseType.GetProperty("AllDefsListForReading")?.GetValue(null) as IEnumerable;
                foreach (object defObj in objCollection)
                {
                    patchableFields.Clear();
                    if (defObj is Def def && !def.generated)
                    {
                        BuildDefList(def, def, patchDef.fields);
                    }
                }
                CalculateWeightFactors();
            }
            catch (Exception ex)
            {
                Log.Error($"[TynanTyrannical] Exception thrown while attempting to build PatchNotes. Exception=\"{ex.Message}\"");
            }
        }

        private static void BuildDefList(Def def, object parent, List<PatchRange> fields)
        {
            try
            {
                foreach (PatchRange patch in fields)
                {
                    if (patch.FieldInfo.FieldType.IsNumericType())
                    {
                        float baseValue = Convert.ToSingle(patch.FieldInfo.GetValue(parent));
                        if (baseValue != patch.ignoreIfValue)
                        {
                            patch.originalValues.Add(def, baseValue);
                            patchableFields.Add(new Pair<object, PatchRange>(parent, patch));
                        }
                    }
                    else if (nestedTypes.TryGetValue(patch.FieldInfo.FieldType, out FieldTypeDef fieldTypeDef))
                    {
                        FieldInfo field = parent.GetType().GetField(patch.name);
                        object fieldValue = field?.GetValue(parent);
                        if (fieldValue != null)
                        {
                            BuildDefList(def, fieldValue, fieldTypeDef.fields);
                        }
                    }
                    else
                    {
                        Log.Warning($"[TynanTyrannical] Unable to apply PatchNotes to {patch.name}. Field must be either a numeric type or registered as a nested type using FieldTypeDef.");
                    }
                }
                if (!patchableFields.NullOrEmpty())
                {
                    if (!defWeights.ContainsKey(def))
                    {
                        defWeights.Add(def, 0);
                    }
                    if (!possibleDefs.ContainsKey(def))
                    {
                        possibleDefs.Add(def, new List<Pair<object, PatchRange>>());
                    }
                    Log.Message($"BuildingDefList for {def.defName} with parent {parent}. Fields: {patchableFields.Count}");
                    possibleDefs[def].AddRange(patchableFields);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[TynanTyrannical] Exception thrown while building PatchNotes for Def {def?.defName ?? "Null"}. Exception=\"{ex.Message}\"");
            }
        }
        
        private static void CalculateWeightFactors()
        {
            foreach (Def def in possibleDefs.Keys)
            {
                List<PatchRange> patches = possibleDefs[def].Select(p => p.Second).ToList();
                float weight = patches.Average(p => p.weightToPatch);
                defWeights[def] = weight;
            }
        }
    }
}
