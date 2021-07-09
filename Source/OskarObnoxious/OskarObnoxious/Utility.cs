using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Verse;
using UnityEngine;

namespace OskarObnoxious
{
    [StaticConstructorOnStartup]
    public static class Utility
    {
        public const string PatchNamesFile = "PatchNames.txt";
        public static readonly ModContentPack mod;

        static Utility()
        {
            Log.Message($"<color=orange>[OskarObnoxious]</color> version 1.3.0010");
            mod = LoadedModManager.RunningMods.FirstOrDefault((ModContentPack mod) => mod.assemblies.loadedAssemblies.Contains(Assembly.GetExecutingAssembly()));
            ImportNames();
        }

        private static void ImportNames()
        {
            try
            {
                string[] lines = File.ReadAllLines(Path.Combine(mod.RootDir, PatchNamesFile));
                if (lines.NullOrEmpty())
                {
                    throw new IOException("Empty file");
                }
                PatchNotes.patchNoteNames.AddRange(lines);
                PatchNotes.NamePatchNotes = true;
            }
            catch (IOException ex)
            {
                Log.Error($"Exception thrown while attempting to import names for patches. Disabling naming feature. Exception=\"{ex.Message}\"");
                PatchNotes.NamePatchNotes = false;
            }
        }

        public static bool IsNumericType(this Type o)
        {   
            switch (Type.GetTypeCode(o))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            if (val.CompareTo(max) > 0) return max;
            return val;
        }

        public static float RoundTo(this object num, float decimalPlace)
        {
            if (!num.GetType().IsNumericType())
            {
                Log.Error($"Cannot round non-numeric object.");
                return float.NaN;
            }
            return Mathf.Round(Convert.ToSingle(num) / decimalPlace) * decimalPlace;
        }

        public static void OutputAllPatchTypesDefs()
        {
            foreach (var patchInfo in PatchNotes.possibleDefs)
            {
                if (!patchInfo.Value.NullOrEmpty())
                {
                    Log.Message($"Def: {patchInfo.Key.defName} Type: {patchInfo.Key.GetType()}");
                    foreach (var pairPatch in patchInfo.Value)
                    {
                        Log.Message($"Parent: {pairPatch.Item1.GetType()} ForDef: {pairPatch.Item2} Field: {pairPatch.Item3.name} Value: {pairPatch.Item3.originalValues[patchInfo.Key]}");
                    }
                }
            }
        }
    }
}
