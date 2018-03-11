using HugsLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Harmony;
using HugsLib.Utils;
using System.Reflection;
using System.Collections;

namespace ExpandedContextMenu {
    [StaticConstructorOnStartup]
    public class ExpandedContextMenu : ModBase {
        public const BindingFlags BINDING_FLAGS = BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

        public override string ModIdentifier => "ExpandedContextMenu";
        private static Type controllerType { get; set; }
        
        private ExpandedContextMenu() {
            try {
                var assembly = Assembly.Load("AllowTool");
                if (assembly == null) {
                    Log.Message("Allow tool mod not installed.");
                    return;
                }

                controllerType = assembly.GetType("AllowTool.Context.DesignatorContextMenuController");
                if (controllerType == null) {
                    Log.Message("Couldn't find controller.");
                }
            } catch { }
        }

        public static List<FloatMenuOption> GetMenuItems(params Thing[] things) {
            List<FloatMenuOption> ret = new List<FloatMenuOption>();
            if (things == null) { return ret; }

            var thing = things[0];
            
            var gizmos = thing.GetGizmos();

            foreach (var gizmo in gizmos) {
                var command = gizmo as Command;
                if (command == null) { continue; }
                try {
                    ret.Add(new FloatMenuOption(command.LabelCap ?? command.Desc?.Split('\n')[0].CapitalizeFirst().Trim() ?? command.TutorTagSelect, () => {
                        if (!TutorSystem.AllowAction(command.TutorTagSelect)) {
                            return;
                        }
                        command.ProcessInput(null);
                    }));
                } catch {
                    Log.Message($"Unable to generate gizmo menu for:{gizmo}");
                }
            }

            var designators = Find.ReverseDesignatorDatabase.AllDesignators;

            foreach (var designator in designators) {
                if (designator.CanDesignateThing(thing).Accepted) {
                    var mainOption = new FloatMenuOption(designator.LabelCapReverseDesignating(thing), () => {
                        if (!TutorSystem.AllowAction(designator.TutorTagDesignate)) {
                            return;
                        }
                        things.Do(t => designator.DesignateThing(t));
                            
                        designator.Finalize(true);
                    });
                    ret.Add(mainOption);

                    try {
                        if (controllerType == null) { break; }
                            
                        var field = controllerType.GetProperty("MenuProviderInstances");
                        var providers = field.GetValue(null, null) as IEnumerable;
                                                        
                        foreach (var provider in providers) {
                            Type providerType = provider.GetType();
                            var designatorType = providerType.GetProperty("HandledDesignatorType").GetValue(provider, null) as Type;

                            if (designatorType.IsInstanceOfType(designator)) {
                                var listMenuEntriesMethod = providerType.GetMethod("ListMenuEntries", ExpandedContextMenu.BINDING_FLAGS);

                                if (listMenuEntriesMethod == null) {
                                    Log.Message($"Checking base type for method on {providerType.Name}");
                                    listMenuEntriesMethod = providerType.BaseType.GetMethod("ListMenuEntries", ExpandedContextMenu.BINDING_FLAGS);
                                }
                                    
                                if (listMenuEntriesMethod != null) {
                                    var extraOptions = listMenuEntriesMethod.Invoke(provider, new[] { designator }) as IEnumerable<FloatMenuOption>;
                                    var count = 0;
                                    foreach (var option in extraOptions) {
                                        option.Label = $"- {option.Label}";
                                        ret.Add(option);
                                        count++;
                                    }
                                } else {
                                    Log.Message($"Unable to find method on {providerType.Name}");
                                }
                            }
                        }
                    } catch (Exception ex) {
                        Log.Message($"Unable to deal {ex}");
                    }
                }
            }

            Log.Message($"Returning {ret.Count} items.");
            return ret;
        }
    }
}
