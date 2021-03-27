using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace TynanTyrannical
{
    [StaticConstructorOnStartup]
    public static class Utility
    {
        static Utility()
        {
            Log.Message($"<color=orange>[TynanTyrannical]</color> version 1.0.0");
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
                        Log.Message($"Parent: {pairPatch.First.GetType()} Field: {pairPatch.Second.name} Value: {pairPatch.Second.originalValues[patchInfo.Key]}");
                    }
                }
            }
        }
    }
}
