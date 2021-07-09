using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using RimWorld;

namespace OskarObnoxious
{
    public class DefPatchPair : IExposable
    {
        public string defName;
        public string statDefName;
        public string name;
        public string type;

        private FieldInfo fieldInfo;

        public DefPatchPair()
        {
        }

        public DefPatchPair(string defName, string statDefName, FieldInfo fieldInfo)
        {
            this.defName = defName;
            this.statDefName = statDefName;
            name = fieldInfo.Name;
            type = fieldInfo.FieldType.Name;
        }

        public DefPatchPair(string defName, string statDefName, string name, string type)
        {
            this.defName = defName;
            this.statDefName = statDefName;
            this.name = name;
            this.type = type;
        }

        public FieldInfo FieldInfo
        {
            get
            {
                if (fieldInfo is null)
                {
                    fieldInfo = Type.GetType(type).GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                }
                return fieldInfo;
            }
        }

        public override bool Equals(object obj) => obj is DefPatchPair comparison && Equals(comparison);

        public bool Equals(DefPatchPair comparison) => defName == comparison.defName && name == comparison.name && type == comparison.type && statDefName == comparison.statDefName;

        public override int GetHashCode()
        {
            return Gen.HashCombine(Gen.HashCombine(Gen.HashCombine(Gen.HashCombine(0, defName), name), type), statDefName);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref defName, "defName");
            Scribe_Values.Look(ref statDefName, "statDefName");
            Scribe_Values.Look(ref name, "name");
            Scribe_Values.Look(ref type, "type");
        }
    }
}
