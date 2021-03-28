﻿using System;
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
    [StaticConstructorOnStartup]
    public static class PatchNotes
    {
        internal static readonly Dictionary<Def, List<Pair<object, PatchRange>>> possibleDefs = new Dictionary<Def, List<Pair<object, PatchRange>>>();
        internal static readonly Dictionary<Def, float> defWeights = new Dictionary<Def, float>();

        private static readonly Dictionary<Type, FieldTypeDef> nestedTypes = new Dictionary<Type, FieldTypeDef>();

        private static readonly List<Pair<object, PatchRange>> patchableFields = new List<Pair<object, PatchRange>>();
        private static readonly List<string> defsAffected = new List<string>();

        static PatchNotes()
        {
            foreach (StatPatchDef statPatchDef in DefDatabase<StatPatchDef>.AllDefs)
            {
                statPatchDef.ResolveStatDef();
            }
        }

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
                if (defToChange.Key is null)
                {
                    Log.Error($"Unable to patch {TTMod.settings.defsChangedPerPatch} unique defs.");
                    break;
                }
                PatchFields(defToChange, stringBuilder);
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
                    d.Value.Any(p => fieldTypeDef.fields.Contains(p.Second))).RandomElementByWeightWithFallback(e => defWeights[e.Key]);
                if (defToChange.Key is null)
                {
                    break;
                }
                if (TTMod.settings.debugShowPatchGeneration)
                {
                    Log.Message($"Patching: {defToChange.Key.defName} FieldsToChange: {defToChange.Value.Count} DefsChangedPerPatch={TTMod.settings.defsChangedPerPatch} FieldsChangedPerPatch={TTMod.settings.fieldsChangedPerPatch}");
                }
                PatchFields(defToChange, stringBuilder);
            }
            SendPatchLetter(stringBuilder.ToString());
        }

        public static void ForceSpecificPatchNotes(StatPatchDef statPatchDef)
        {
            StringBuilder stringBuilder = new StringBuilder();
            defsAffected.Clear();
            for (int i = 0; i < TTMod.settings.defsChangedPerPatch; i++)
            {
                var defToChange = possibleDefs.Where(d => !d.Value.NullOrEmpty() && !defsAffected.Contains(d.Key.defName) && 
                    d.Value.Any(p => statPatchDef.patch == p.Second)).RandomElementByWeightWithFallback(e => defWeights[e.Key]);
                if (defToChange.Key is null)
                {
                    break;
                }
                if (TTMod.settings.debugShowPatchGeneration)
                {
                    Log.Message($"Patching: {defToChange.Key.defName} FieldsToChange: {defToChange.Value.Count} DefsChangedPerPatch={TTMod.settings.defsChangedPerPatch} FieldsChangedPerPatch={TTMod.settings.fieldsChangedPerPatch}");
                }
                PatchFields(defToChange, stringBuilder);
            }
            SendPatchLetter(stringBuilder.ToString());
        }

        private static void PatchFields(KeyValuePair<Def, List<Pair<object, PatchRange>>> defPair, StringBuilder stringBuilder)
        {
            defsAffected.Add(defPair.Key.defName);
            stringBuilder.AppendLine($"<color=green>{defPair.Key.defName}</color>");
            for (int j = 0; j < Mathf.Min(TTMod.settings.fieldsChangedPerPatch, defPair.Value.Count); j++)
            {
                var objectPair = defPair.Value.RandomElement();
                object parent = objectPair.First;
                PatchRange patch = objectPair.Second;
                object oldValue = patch.FieldInfo.GetValue(parent);
                float value = patch.NewRandomValue(defPair.Key);
                if (patch.limits.HasValue)
                {
                    value = value.Clamp(patch.limits.Value.min, patch.limits.Value.max);
                }
                object valueConverted = Convert.ChangeType(value, patch.FieldInfo.FieldType);
                patch.FieldInfo.SetValue(parent, valueConverted);
                stringBuilder.AppendLine(patch.PatchNoteText(oldValue, valueConverted));
            }
        }

        private static void SendPatchLetter(string patchNotes)
        {
            GameComponent_PatchNotes.Instance.RegisterPatch(patchNotes);
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
                    else if (patch.FieldInfo.FieldType == typeof(List<StatModifier>))
                    {
                        List<StatModifier> statModifiers = (List<StatModifier>)patch.FieldInfo.GetValue(parent);
                        if (!statModifiers.NullOrEmpty())
                        {
                            foreach (StatModifier stat in statModifiers)
                            {
                                foreach (StatPatchDef statPatchDef in DefDatabase<StatPatchDef>.AllDefsListForReading)
                                {
                                    if (statPatchDef.StatDef.defName == stat.stat.defName)
                                    {
                                        Log.Message($"Assigned: {statPatchDef.StatDef.defName}");
                                        BuildDefList(def, stat, new List<PatchRange>() { statPatchDef.patch });
                                    }
                                }
                                
                            }
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
