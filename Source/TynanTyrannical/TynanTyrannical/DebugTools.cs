using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace TynanTyrannical
{
    public static class DebugTools
    {
        [DebugAction("Tynan Tyrannical", "Send Patch Notes", allowedGameStates = AllowedGameStates.Playing)]
        private static void TestPatchNotesSingle()
        {
            if (!GameComponent_PatchNotes.StorytellerLoaded)
            {
                Messages.Message("Cannot initiate Patch Notes without Tynan Tyrannical as the active storyteller.", MessageTypeDefOf.RejectInput);
                return;
            }
            GameComponent_PatchNotes.InitiatePatchNotes();
        }

        [DebugAction("Tynan Tyrannical", "Send Specific Patch Note", allowedGameStates = AllowedGameStates.Playing)]
        private static void TestPatchNotesChoice()
        {
            List<DebugMenuOption> list = new List<DebugMenuOption>();
	        foreach (PatchTypeDef patchTypeDef in DefDatabase<PatchTypeDef>.AllDefs.Where(p => !p.fields.NullOrEmpty()))
	        {
		        list.Add(new DebugMenuOption(patchTypeDef.defName, DebugMenuOptionMode.Action, delegate()
		        {
                    List<DebugMenuOption> list2 = new List<DebugMenuOption>();
                    foreach (PatchRange patchRange in patchTypeDef.fields.Where(p => !p.FieldInfo.FieldType.IsNumericType()))
                    {
                        FieldTypeDef fieldTypeDef = DefDatabase<FieldTypeDef>.AllDefsListForReading.FirstOrDefault(f => f.type == patchRange.FieldInfo.FieldType);
                        if (fieldTypeDef != null)
                        {
                            list2.Add(new DebugMenuOption(patchRange.name, DebugMenuOptionMode.Action, delegate ()
                            {
                            
                                PatchNotes.ForceSpecificPatchNotes(fieldTypeDef);
                            }));
                        }
                    }
                    foreach (PatchRange patchRange in patchTypeDef.fields.Where(p => p.FieldInfo.FieldType == typeof(List<VerbProperties>)))
                    {
                        FieldTypeDef verbTypeDef = PatchNotes.nestedTypes[typeof(VerbProperties)];
                        list2.Add(new DebugMenuOption(verbTypeDef.defName, DebugMenuOptionMode.Action, delegate ()
                        {
                            PatchNotes.ForceSpecificPatchNotes(verbTypeDef);
                        }));
                    }
                    Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
		        }));
	        }
            list.Add(new DebugMenuOption("Stats", DebugMenuOptionMode.Action, delegate ()
            {
                List<DebugMenuOption> list2 = new List<DebugMenuOption>();
                foreach (StatPatchDef statPatchDef in DefDatabase<StatPatchDef>.AllDefs)
                {
                    list2.Add(new DebugMenuOption(statPatchDef.patch.label, DebugMenuOptionMode.Action, delegate ()
                    {
                        PatchNotes.ForceSpecificPatchNotes(statPatchDef);
                    }));
                }
                Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
            }));
            if (list.NullOrEmpty())
            {
                Messages.Message("Cannot execute, no PatchTypeDefs loaded.", MessageTypeDefOf.RejectInput);
                return;
            }
	        Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
        }

        [DebugAction("Tynan Tyrannical", "Send Patch Notes x5", allowedGameStates = AllowedGameStates.Playing)]
        private static void TestPatchNotesBatch()
        {
            if (!GameComponent_PatchNotes.StorytellerLoaded)
            {
                Messages.Message("Cannot initiate Patch Notes without Tynan Tyrannical as the active storyteller.", MessageTypeDefOf.RejectInput);
                return;
            }
            for (int i = 0; i < 5; i++)
            {
                GameComponent_PatchNotes.InitiatePatchNotes();
            }
        }

        [DebugAction("Tynan Tyrannical", "Output PatchTypeDef Data", allowedGameStates = AllowedGameStates.Playing)]
        private static void OutputPatchTypeDefs()
        {
            Utility.OutputAllPatchTypesDefs();
        }

        [DebugAction("Tynan Tyrannical", "Clear Patch Notes", allowedGameStates = AllowedGameStates.Playing)]
        private static void ClearPatchNotes()
        {
            GameComponent_PatchNotes.Instance.patchNotes.Clear();
        }
    }
}
