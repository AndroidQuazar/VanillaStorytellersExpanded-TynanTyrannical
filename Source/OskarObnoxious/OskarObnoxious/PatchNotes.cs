using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace OskarObnoxious
{
    [StaticConstructorOnStartup]
    public static class PatchNotes
    {
        internal static readonly Dictionary<Def, List<Tuple<object, string, PatchRange>>> possibleDefs = new Dictionary<Def, List<Tuple<object, string, PatchRange>>>();
        internal static readonly Dictionary<Def, float> defWeights = new Dictionary<Def, float>();

        public static readonly Dictionary<Type, FieldTypeDef> nestedTypes = new Dictionary<Type, FieldTypeDef>();
        public static readonly List<Tuple<object, string, PatchRange>> patchableFields = new List<Tuple<object, string, PatchRange>>();

        private static readonly List<string> defsAffected = new List<string>();
        private static readonly List<string> fieldsAffected = new List<string>();

        public static List<string> patchNoteNames = new List<string>();

        internal static bool NamePatchNotes { get; set; }

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
            List<string> debugLogs = new List<string>();
            debugLogs.Add($"Beginning patch notes generation. DefsChangedPerPatch={TTMod.settings.defsChangedPerPatch}");
            defsAffected.Clear();
            for (int i = 0; i < TTMod.settings.defsChangedPerPatch; i++)
            {
                var defToChange = possibleDefs.Where(d => !d.Value.NullOrEmpty() && !defsAffected.Contains(d.Key.defName)).RandomElementByWeightWithFallback(e => defWeights[e.Key]);
                if (defToChange.Key is null)
                {
                    Log.Error($"Unable to patch {TTMod.settings.defsChangedPerPatch} unique defs.");
                    break;
                }
                debugLogs.Add($"Patching \"{defToChange.Key.defName}\" Number of patches possible = {defToChange.Value.Count}");
                PatchFields(defToChange, stringBuilder);
            }
            if (TTMod.settings.debugShowPatchGeneration)
			{
                Log.Message("------------------------------");
                debugLogs.ForEach(log => Log.Message(log));
                Log.Message($"PatchNotes StringBuilder:\n{stringBuilder}");
                Log.Message("------------------------------");
			}
            SendPatchLetter(stringBuilder.ToString());
        }

        public static void ForceSpecificPatchNotes(FieldTypeDef fieldTypeDef)
        {
            if (!GameComponent_PatchNotes.StorytellerLoaded)
            {
                Messages.Message("Cannot initiate Patch Notes without Oskar Obnoxious as the active storyteller.", MessageTypeDefOf.RejectInput);
                return;
            }

            StringBuilder stringBuilder = new StringBuilder();
            defsAffected.Clear();
            for (int i = 0; i < TTMod.settings.defsChangedPerPatch; i++)
            {
                var defToChange = possibleDefs.Where(d => !d.Value.NullOrEmpty() && !defsAffected.Contains(d.Key.defName) && 
                    d.Value.Any(p => fieldTypeDef.fields.Contains(p.Item3))).RandomElementByWeightWithFallback(e => defWeights[e.Key]);
                if (defToChange.Key is null)
                {
                    break;
                }
                if (TTMod.settings.debugShowPatchGeneration)
                {
                    Log.Message($"Patching: {defToChange.Key.defName} FieldsToChange: {defToChange.Value.Count}");
                }
                PatchFields(defToChange, stringBuilder);
            }
            SendPatchLetter(stringBuilder.ToString());
        }

        public static void ForceSpecificPatchNotes(StatPatchDef statPatchDef)
        {
            if (!GameComponent_PatchNotes.StorytellerLoaded)
            {
                Messages.Message("Cannot initiate Patch Notes without Oskar Obnoxious as the active storyteller.", MessageTypeDefOf.RejectInput);
                return;
            }

            StringBuilder stringBuilder = new StringBuilder();
            defsAffected.Clear();
            for (int i = 0; i < TTMod.settings.defsChangedPerPatch; i++)
            {
                var defToChange = possibleDefs.Where(d => !d.Value.NullOrEmpty() && !defsAffected.Contains(d.Key.defName) && 
                    d.Value.Any(p => statPatchDef.patch == p.Item3)).RandomElementByWeightWithFallback(e => defWeights[e.Key]);
                if (defToChange.Key is null)
                {
                    break;
                }
                if (TTMod.settings.debugShowPatchGeneration)
                {
                    Log.Message($"Patching: {defToChange.Key.defName} FieldsToChange: {defToChange.Value.Count}");
                }
                PatchFields(defToChange, stringBuilder);
            }
            SendPatchLetter(stringBuilder.ToString());
        }

        private static void PatchFields(KeyValuePair<Def, List<Tuple<object, string, PatchRange>>> defPair, StringBuilder stringBuilder)
        {
            defsAffected.Add(defPair.Key.defName);
            string displayLabel = string.IsNullOrEmpty(defPair.Key.LabelCap) ? defPair.Key.defName : defPair.Key.LabelCap.ToString();
            stringBuilder.AppendLine($"<color=green>{displayLabel}</color>");
            fieldsAffected.Clear();
            for (int j = 0; j < Mathf.Min(TTMod.settings.fieldsChangedPerDef, defPair.Value.Count); j++)
            {
                var objectPair = defPair.Value.Where(p => !fieldsAffected.Contains(p.Item3.name)).RandomElementWithFallback();
                if (objectPair == default)
                {
                    break;
                }
                object parent = objectPair.Item1;
                string statDefName = objectPair.Item2;
                PatchRange patch = objectPair.Item3;
                fieldsAffected.Add(patch.name);
                object oldValue = patch.FieldInfo.GetValue(parent);
                float value = patch.NewRandomValue(defPair.Key);
                if (patch.limits.HasValue)
                {
                    value = value.Clamp(patch.limits.Value.min, patch.limits.Value.max);
                }
                object valueConverted = Convert.ChangeType(value, patch.FieldInfo.FieldType);
                if (!valueConverted.Equals(oldValue))
                {
                    patch.FieldInfo.SetValue(parent, valueConverted);
                    stringBuilder.AppendLine(patch.PatchNoteChanged(oldValue, valueConverted));
                    var defPatch = new DefPatchPair(defPair.Key.defName, statDefName, patch.FieldInfo);
                    if (!GameComponent_PatchNotes.Instance.currentDefValues.ContainsKey(defPatch))
                    {
                        Log.Message($"STORING Def: {defPatch.defName} Field: {patch.DisplayName} Default: {oldValue} Stored: {valueConverted} Parent: {parent}");
                        GameComponent_PatchNotes.Instance.currentDefValues.Add(defPatch, value);
                    }
                    GameComponent_PatchNotes.Instance.currentDefValues[defPatch] = value;
                }
                else
                {
                    stringBuilder.AppendLine(patch.PatchNoteUnchanged(oldValue));
                }
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
                    if (defObj is StatPatchDef statPatchDef && statPatchDef.patch.ApplyToDef(statPatchDef.StatDef))
                    {
                        BuildDefList(statPatchDef, statPatchDef.StatDef, statPatchDef.patch.DisplayName, new List<PatchRange>() { statPatchDef.patch });
                    }
                    else if (defObj is Def def && !def.generated)
                    {
                        BuildDefList(def, def, null, patchDef.fields);
                    }
                }
                CalculateWeightFactors();
            }
            catch (Exception ex)
            {
                Log.Error($"[OskarObnoxious] Exception thrown while attempting to build PatchNotes. Exception=\"{ex.Message}\"");
            }
        }

        private static void BuildDefList(Def def, object parent, string statDefName, List<PatchRange> fields)
        {
            try
            {
                foreach (PatchRange patch in fields.Where(p => p.DefIsPatchable(def)))
                {
                    if (patch.FieldInfo.FieldType.IsNumericType())
                    {
                        bool retry = false;
                        RevalidateStat:;
                        try
                        {
                            float baseValue = Convert.ToSingle(patch.FieldInfo.GetValue(parent));
                            if (baseValue != patch.ignoreIfValue && !patch.originalValues.ContainsKey(def))
                            {
                                patch.originalValues.Add(def, baseValue);
                                patchableFields.Add(new Tuple<object, string, PatchRange>(parent, statDefName, patch));
                            }
                        }
                        catch (ArgumentException ex)
                        {
                            if (parent is StatModifier stat && !retry)
							{
                                string statName = patch.name;
                                Type type = typeof(StatModifier);
                                if (statName == "value")
								{
                                    statName = "defaultBaseValue";
                                    type = typeof(StatDef);
                                }
                                else if (statName == "defaultBaseValue")
								{
                                    statName = "value";
                                    type = typeof(StatModifier);
                                }
                                retry = true;
                                patch.name = statName;
                                patch.ResolveReferences(type);
                                goto RevalidateStat;
							}
                            Log.Warning($"Exception thrown when registering {patch.DisplayName} patch. Patch will not apply ingame.\n\nDef=\"{def}\" Parent=\"{parent}\" Exception=\"{ex.Message}\"");
                        }
                    }
                    else if (nestedTypes.TryGetValue(patch.FieldInfo.FieldType, out FieldTypeDef fieldTypeDef))
                    {
                        FieldInfo field = parent.GetType().GetField(patch.name);
                        object fieldValue = field?.GetValue(parent);
                        if (fieldValue != null)
                        {
                            BuildDefList(def, fieldValue, statDefName, fieldTypeDef.fields);
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
                                        BuildDefList(def, stat, statPatchDef.patch.DisplayName, new List<PatchRange>() { statPatchDef.patch });
                                    }
                                }
                            }
                        }
                    }
                    else if (patch.FieldInfo.FieldType == typeof(List<VerbProperties>))
                    {
                        List<VerbProperties> verbs = (List<VerbProperties>)patch.FieldInfo.GetValue(parent);
                        if (!verbs.NullOrEmpty())
                        { 
                            foreach (VerbProperties verb in verbs)
                            {
                                FieldTypeDef verbTypeDef = nestedTypes[typeof(VerbProperties)];
                                BuildDefList(def, verb, statDefName, verbTypeDef.fields);
                            }
                        }
                    }
                    else
                    {
                        Log.Warning($"[OskarObnoxious] Unable to apply PatchNotes to {patch.name}. Field must be either a numeric type or registered as a nested type using FieldTypeDef. Type=\"{patch.FieldInfo.FieldType}\"");
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
                        possibleDefs.Add(def, new List<Tuple<object, string, PatchRange>>());
                    }
                    possibleDefs[def].AddRange(patchableFields);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[OskarObnoxious] Exception thrown while building PatchNotes for Def {def?.defName ?? "Null"}. Exception=\"{ex.Message}\"");
            }
        }
        
        private static void CalculateWeightFactors()
        {
            foreach (Def def in possibleDefs.Keys)
            {
                List<PatchRange> patches = possibleDefs[def].Select(p => p.Item3).ToList();
                float weight = patches.Average(p => p.weightToPatch);
                defWeights[def] = weight;
            }
        }
    }
}
