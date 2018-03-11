using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace ExpandedContextMenu {
    //[HarmonyPatch(typeof(FloatMenuMakerMap), "ChoicesAtFor")]
    //static class FloatMenuMakerMap_ChoicesAtFor_Patch {
    //    public static void Postfix(Vector3 clickPos, Pawn pawn, ref List<FloatMenuOption> __result) {
    //        if (!clickPos.ToIntVec3().IsInside(pawn)) { return; }

    //        if (__result == null) { __result = new List<FloatMenuOption>(); }
    //        __result.AddRange(ExpandedContextMenu.GetMenuItems(pawn));
    //    }
    //}
}
