using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.Sound;
using static Verse.Widgets;

namespace ABsVisiblePants
{

    public class ABsVisiblePantsMod : Mod
    {
        public static AVPSettings settings;
        public static ModContentPack thisMod;
        private static string search = "";
        public ABsVisiblePantsMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<AVPSettings>();
            thisMod = content;
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            AVPUtility.InitialSettingData();
            Listing_Standard ls1 = new Listing_Standard();
            ls1.Begin(inRect.TopPart(0.05f).RightHalf());
            search = ls1.TextEntry(search);
            ls1.End();
            Rect rect = inRect.BottomPart(0.95f);
            float LabelHeigh = 30f;
            Rect outRect = new Rect(rect.x, rect.y, rect.width, rect.height - 120f);
            DrawWindowBackground(outRect);
            Rect viewRect = new Rect(-5f, -5f, outRect.width-30f, (LabelHeigh + 5f) * (float)AVPUtility.AllPants.Where(se=>se.label.IndexOf(search) != -1).Count() + 5f);
            Rect rect1 = new Rect(LabelHeigh + 5f, 0f, 300f, LabelHeigh);
            Rect rect2 = new Rect(0, 0f, LabelHeigh, LabelHeigh);
            Rect rect3 = new Rect(rect1.x + rect1.width, 0f, 4 * LabelHeigh, LabelHeigh);
            Widgets.BeginScrollView(outRect, ref this.loc, viewRect, true);
            if (!AVPUtility.AllPants.NullOrEmpty() && AVPSettings.VisiblePantsEnabled)
            {
                foreach (ThingDef x in AVPUtility.AllPants)
                {
                    if (x.label.IndexOf(search) != -1) {
                        TextAnchor anchor = Text.Anchor;
                        rect1.y += 5f;
                        Widgets.Label(rect1, x.label);
                        DrawBox(rect2);
                        GUI.DrawTexture(rect2, x.uiIcon);
                        Widgets.DrawLineVertical(rect3.x - 5f, rect3.y, rect3.height);
                        Text.Anchor = TextAnchor.MiddleCenter;
                        if (Widgets.RadioButtonLabeled(rect3, "IsPants".Translate(), AVPSettings.IsPants[x.defName]))
                        {
                            AVPSettings.IsPants[x.defName] = true;
                        }
                        Text.Anchor = anchor;
                        Rect rect4 = rect3;
                        rect4.x += rect3.width + 10f;
                        Widgets.DrawLineVertical(rect4.x - 5f, rect4.y, rect4.height);
                        Widgets.DrawLineVertical(rect4.x + rect4.width + 5f, rect4.y, rect4.height);
                        if (Widgets.RadioButtonLabeled(rect4, "IsNotPants".Translate(), !AVPSettings.IsPants[x.defName]))
                        {
                            AVPSettings.IsPants[x.defName] = false;
                        }
                        if (AVPSettings.IsPants[x.defName] && !AVPSettings.ModLayerAdd)
                        {
                            if (!AVPSettings.CoverOnSkinApparel.ContainsKey(x.defName))
                            {
                                AVPSettings.CoverOnSkinApparel.Add(x.defName, false);
                            }
                            Rect rect6 = rect4;
                            rect6.x += rect4.width + 10f;
                            Widgets.DrawLineVertical(rect6.x - 5f, rect6.y, rect6.height);
                            Widgets.DrawLineVertical(rect6.x + rect6.width + 5f, rect6.y, rect6.height);
                            Widgets.Label(new Rect(rect6.x, rect6.y + 5f, rect.width - 24f, rect.height), "Cover_OnSkin".Translate());
                            Widgets.CheckboxDraw(rect6.x + rect6.width - 24f, rect6.y + (rect6.height - 24f) / 2f, AVPSettings.CoverOnSkinApparel[x.defName], false);
                            if (Mouse.IsOver(rect6))
                            {
                                Widgets.DrawHighlight(rect6);
                            }
                            MouseoverSounds.DoRegion(rect6);
                            DraggableResult draggableResult = Widgets.ButtonInvisibleDraggable(rect6);
                            if (draggableResult == DraggableResult.Pressed)
                            {
                                AVPSettings.CoverOnSkinApparel[x.defName] = !AVPSettings.CoverOnSkinApparel[x.defName];
                                ResolveAllApparelGraphics();
                                if (AVPSettings.CoverOnSkinApparel[x.defName])
                                {
                                    SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
                                }
                                else
                                {
                                    SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
                                }
                            }
                        }
                        rect1.y += LabelHeigh;
                        rect2.y += (LabelHeigh + 5f);
                        rect3.y += (LabelHeigh + 5f);
                    }
                }
            }
            Widgets.EndScrollView();
            Rect rect5 = new Rect(outRect.x,outRect.y + outRect.height + 5f, rect.width, 4 * LabelHeigh);
            Listing_Standard ls = new Listing_Standard();
       
            ls.Begin(rect5);
            string restart = "Restart_to_apply_settings".Translate();
            ls.CheckboxLabeled("Visible_Pants_Enabled".Translate(), ref AVPSettings.VisiblePantsEnabled, restart);
            if (AVPSettings.VisiblePantsEnabled)
            {
                ls.CheckboxLabeled("Enable_OnSkinPants_Layer".Translate(), ref AVPSettings.ModLayerAdd, restart);
                string add = AVPSettings.ModLayerAdd ? restart : "";
                float height1 = Text.CalcHeight("Cover_OnSkin".Translate(), ls.ColumnWidth * 1f);
                Rect rect7 = ls.GetRect(height1);
                if (Mouse.IsOver(rect7))
                {
                    DrawHighlight(rect7);
                }
                Widgets.CheckboxLabeled(rect7, "Cover_OnSkin".Translate(), ref AVPSettings.CoverOnSkin);
                TooltipHandler.TipRegion(rect7, add);
                DraggableResult draggable = Widgets.ButtonInvisibleDraggable(rect7);
                if (draggable == DraggableResult.Pressed && !AVPSettings.CoverOnSkinApparel.NullOrEmpty())
                {
                    List<string> list = AVPSettings.CoverOnSkinApparel.Keys.ToList();
                    foreach (string x in list)
                    {
                        AVPSettings.CoverOnSkinApparel[x] = !AVPSettings.CoverOnSkin;
                        ResolveAllApparelGraphics();
                    }
                }
            }
            ls.CheckboxLabeled("Layer_Render_Fix".Translate(), ref AVPSettings.LayerRenderFix, restart);
            ls.End();
        }
        public static void ResolveAllApparelGraphics()
        {
            if (Find.Maps.NullOrEmpty())
            {
                return;
            }
            IEnumerable<Map> AllMap = Find.Maps.Where(x => x != null && x.mapPawns != null && !x.mapPawns.AllPawns.NullOrEmpty() && x.mapPawns.AllPawns.Any(c => c.Faction.IsPlayer));
            if (AllMap.EnumerableNullOrEmpty())
            {
                return;
            }
            foreach (Map map in AllMap)
            {
                foreach (Pawn pawn in map.mapPawns.AllPawns)
                {
                    if (pawn.apparel != null)
                    {
                        AccessTools.Method(typeof(Pawn_ApparelTracker), "SortWornApparelIntoDrawOrder").Invoke(pawn.apparel, new object[0]);
                        pawn.apparel.Notify_ApparelChanged();
                    }
                }
            }
        }

        private Vector2 loc = Vector2.zero;
        public override string SettingsCategory()
        {
            return "ABs_Visible_Pants".Translate();
        }
    }
    public class AVPSettings : ModSettings
    {
        public static Dictionary<string, bool> IsPants = new Dictionary<string, bool>();
        public static Dictionary<string, bool> CoverOnSkinApparel = new Dictionary<string, bool>();

        internal static bool VisiblePantsEnabled = true;
        internal static bool LayerRenderFix = true;
        internal static bool CoverOnSkin = false;
        internal static bool ModLayerAdd = false;
        public static string a = "Things/Pawn/Humanlike/Apparel/Pants/Pants";
        public static string b = "Things/Pawn/Humanlike/Apparel/Trousers/Trousers";
        public static string c = "Things/Pawn/Humanlike/Apparel/Jeans/Jeans";
        public static string d = "Things/Pawn/Humanlike/Apparel/Shorts/Shorts";
        public static string e = "Things/Pawn/Humanlike/Apparel/Skirt/Skirt";
        public static string PantGraphicPath = a;
        public static string SuitPantsGraphicPath = b;
        public static string JeansGraphicPath = c;
        public static string ShortsGraphicPath = d;
        public static string DressesGraphicPath = e;

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref IsPants, "IsPants", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref CoverOnSkinApparel, "CoverOnSkinApparel", LookMode.Value, LookMode.Value);
            Scribe_Values.Look(ref VisiblePantsEnabled, "VisiblePantsEnabled", true);
            Scribe_Values.Look(ref LayerRenderFix, "LayerRenderFix", true);
            Scribe_Values.Look(ref CoverOnSkin, "CoverOnSkin", false);
            Scribe_Values.Look(ref ModLayerAdd, "ModLayerEnabled", false);
            Scribe_Values.Look(ref PantGraphicPath, "PantGraphicPath", a, true);
            Scribe_Values.Look(ref SuitPantsGraphicPath, "SuitPantsGraphicPath", b, true);
            Scribe_Values.Look(ref JeansGraphicPath, "JeansGraphicPath", c, true);
            Scribe_Values.Look(ref ShortsGraphicPath, "ShortsGraphicPath", d, true);
            Scribe_Values.Look(ref DressesGraphicPath, "DressesGraphicPath", e, true);
        }
    }

    [StaticConstructorOnStartup]
    public static class AVPUtility
    {
        internal static List<ThingDef> AllPants = new List<ThingDef>();
        internal static List<string> FullApparel = new List<string>();
        internal static List<string> OnSkinFullApparel = new List<string>();
        internal static List<string> AllPantsStr = new List<string>();
        internal static List<string> AllJeans = new List<string>();
        internal static List<string> AllSuitPants = new List<string>();
        internal static List<string> AllDresses = new List<string>();
        internal static List<string> AllShorts = new List<string>();
        internal static List<string> OnlyPants = new List<string>();

        static AVPUtility()
        {
            foreach (ThingDef def in GetApparelPants())
            {
                if (!def.apparel.layers.NullOrEmpty() && def.apparel.layers.Contains(ApparelLayerDefOf.OnSkin))
                {
                    if (def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
                    {
                        FullApparel.Add(def.defName);
                        if (def.apparel.layers.Count == 1)
                        {
                            OnSkinFullApparel.Add(def.defName);
                        }
                    }
                    else
                    {
                        AllPants.Add(def);
                        AllPantsStr.Add(def.defName);
                    }
                }
            }
            ABsVisiblePantsMod.settings.ExposeData();
            InitialSettingData();
            ABsVisiblePantsMod.settings.Write();
            SortPantsCategory();
            if (AVPSettings.VisiblePantsEnabled)
            {
                AddNewPants();
                if (AVPSettings.ModLayerAdd)
                {
                    AddLayer();
                    if (AVPSettings.CoverOnSkin)
                    {
                        AVPLayerDefOf.OnSkinPants.drawOrder = 5;
                    }
                }
            }
        }



        public static void SortPantsCategory()
        {
            foreach (ThingDef def in GetApparelPants())
            {
                if (AVPSettings.IsPants.ContainsKey(def.defName))
                {
                    if (AVPSettings.IsPants[def.defName])
                    {
                        if (def.defName.IndexOf("skirt") != -1 || def.defName.IndexOf("Skirt") != -1)
                        {
                            AllDresses.Add(def.defName);
                        }
                        else
                        if (def.defName.IndexOf("suit") != -1 || def.defName.IndexOf("Suit") != -1 || def.defName.IndexOf("Trousers") != -1)
                        {
                            AllSuitPants.Add(def.defName);
                        }
                        else
                        if (def.defName.IndexOf("Jeans") != -1 || def.defName.IndexOf("jeans") != -1)
                        {
                            AllJeans.Add(def.defName);
                        }
                        else
                        if (def.defName.IndexOf("Shorts") != -1 || def.defName.IndexOf("shorts") != -1)
                        {
                            AllShorts.Add(def.defName);
                        }
                        else
                        {
                            OnlyPants.Add(def.defName);
                        }
                    }
                    else
                    {
                        if (!AllDresses.NullOrEmpty() && AllDresses.Contains(def.defName))
                        {
                            AllDresses.Remove(def.defName);
                        }
                        if (!AllSuitPants.NullOrEmpty() && AllSuitPants.Contains(def.defName))
                        {
                            AllSuitPants.Remove(def.defName);
                        }
                        if (!AllJeans.NullOrEmpty() && AllJeans.Contains(def.defName))
                        {
                            AllJeans.Remove(def.defName);
                        }
                        if (!AllShorts.NullOrEmpty() && AllShorts.Contains(def.defName))
                        {
                            AllShorts.Remove(def.defName);
                        }
                        if (!OnlyPants.NullOrEmpty() && OnlyPants.Contains(def.defName))
                        {
                            OnlyPants.Remove(def.defName);
                        }
                    }
                }
            }
            if (!AllDresses.NullOrEmpty())
            {
                PantsListDefOf.AllDresses.TargetApparelDefs = AllDresses;
            }
            if (!AllJeans.NullOrEmpty())
            {
                PantsListDefOf.AllJeans.TargetApparelDefs = AllJeans;
            }
            if (!AllSuitPants.NullOrEmpty())
            {
                PantsListDefOf.AllSuitPants.TargetApparelDefs = AllSuitPants;
            }
            if (!AllShorts.NullOrEmpty())
            {
                PantsListDefOf.AllShorts.TargetApparelDefs = AllShorts;
            }
            if (!OnlyPants.NullOrEmpty())
            {
                PantsListDefOf.OnlyPants.TargetApparelDefs = OnlyPants;
            }
            if (!OnSkinFullApparel.NullOrEmpty())
            {
                PantsListDefOf.OnSkinFullApparel.TargetApparelDefs = OnSkinFullApparel;
            }
        }




        public static void CheckGraphicPath()
        {
            if (AVPSettings.PantGraphicPath != AVPSettings.a && !TestGraphicPath(AVPSettings.PantGraphicPath))
            {
                AVPSettings.PantGraphicPath = AVPSettings.a;
                Log.Warning("Path not available:" + AVPSettings.PantGraphicPath);
            }
            if (AVPSettings.SuitPantsGraphicPath != AVPSettings.b && !TestGraphicPath(AVPSettings.SuitPantsGraphicPath))
            {
                AVPSettings.SuitPantsGraphicPath = AVPSettings.b;
                Log.Warning("Path not available:" + AVPSettings.SuitPantsGraphicPath);
            }
            if (AVPSettings.JeansGraphicPath != AVPSettings.c && !TestGraphicPath(AVPSettings.JeansGraphicPath))
            {
                AVPSettings.JeansGraphicPath = AVPSettings.c;
                Log.Warning("Path not available:" + AVPSettings.JeansGraphicPath);
            }
            if (AVPSettings.ShortsGraphicPath != AVPSettings.d && !TestGraphicPath(AVPSettings.ShortsGraphicPath))
            {
                AVPSettings.ShortsGraphicPath = AVPSettings.d;
                Log.Warning("Path not available:" + AVPSettings.ShortsGraphicPath);
            }
            if (AVPSettings.DressesGraphicPath != AVPSettings.e && !TestGraphicPath(AVPSettings.DressesGraphicPath))
            {
                AVPSettings.DressesGraphicPath = AVPSettings.e;
                Log.Warning("Path not available:" + AVPSettings.DressesGraphicPath);
            }
        }



        public static void InitialSettingData()
        {
            if (AVPSettings.IsPants == null)
            {
                AVPSettings.IsPants = new Dictionary<string, bool>();
            }
            if (AVPSettings.CoverOnSkinApparel == null)
            {
                AVPSettings.CoverOnSkinApparel = new Dictionary<string, bool>();
            }
            if (AllPantsStr.NullOrEmpty())
            {
                return;
            }
            foreach (string x in AllPantsStr)
            {
                if (!AVPSettings.IsPants.ContainsKey(x))
                {
                    AVPSettings.IsPants.Add(x, true);
                }
            }
            if (!AVPSettings.ModLayerAdd)
            {
                foreach (string x in AllPantsStr)
                {
                    if (AVPSettings.IsPants.ContainsKey(x))
                    {
                        if (AVPSettings.IsPants[x] && !AVPSettings.CoverOnSkinApparel.ContainsKey(x))
                        {
                            AVPSettings.CoverOnSkinApparel.Add(x, false);
                        }
                        if (!AVPSettings.IsPants[x] && AVPSettings.CoverOnSkinApparel.ContainsKey(x))
                        {
                            AVPSettings.CoverOnSkinApparel.Remove(x);
                        }
                    }
                }
            }
        }





        public static bool TestGraphicPath(string path)
        {
            bool a = ContentFinder<Texture2D>.Get(path + "_Male" + "_north", false) == null ? ContentFinder<Texture2D>.Get(path + "_Male", false) != null : true;
            bool b = ContentFinder<Texture2D>.Get(path + "_Fat" + "_north", false) == null ? ContentFinder<Texture2D>.Get(path + "_Fat", false) != null : true;
            bool c = ContentFinder<Texture2D>.Get(path + "_Female" + "_north", false) == null ? ContentFinder<Texture2D>.Get(path + "_Female", false) != null : true;
            bool d = ContentFinder<Texture2D>.Get(path + "_Hulk" + "_north", false) == null ? ContentFinder<Texture2D>.Get(path + "_Hulk", false) != null : true;
            bool e = ContentFinder<Texture2D>.Get(path + "_Thin" + "_north", false) == null ? ContentFinder<Texture2D>.Get(path + "_Thin", false) != null : true;
            return a && b && c && d && e;
        }




        public static IEnumerable<ThingDef> GetApparelPants()
        {
            IEnumerable<ThingDef> thingDef = from x in DefDatabase<ThingDef>.AllDefs
                                             where !x.defName.EnumerableNullOrEmpty()
                                             && !x.thingCategories.NullOrEmpty()
                                             && (x.IsApparel && !x.apparel.bodyPartGroups.NullOrEmpty())
                                             && (x.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs))
                                             select x;
            return thingDef;
        }




        public static void AddNewPants()
        {
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(x => AllPantsStr.Contains(x.defName)))
            {
                if (AVPSettings.IsPants.ContainsKey(thingDef.defName) && AVPSettings.IsPants[thingDef.defName])
                {
                    if (AVPSettings.ModLayerAdd && AVPLayerDefOf.OnSkinPants != null)
                    {
                        if (!thingDef.apparel.layers.Contains(AVPLayerDefOf.OnSkinPants))
                        {
                            if (thingDef.apparel.layers.Contains(ApparelLayerDefOf.OnSkin))
                            {
                                thingDef.apparel.layers.Remove(ApparelLayerDefOf.OnSkin);
                            }
                            thingDef.apparel.layers.Add(AVPLayerDefOf.OnSkinPants);
                        }
                    }
                    if (thingDef.apparel.wornGraphicPath.EnumerableNullOrEmpty())
                    {
                        if (AllJeans.Contains(thingDef.defName))
                        {
                            thingDef.apparel.wornGraphicPath = AVPSettings.JeansGraphicPath;
                        }
                        else
                        if (AllDresses.Contains(thingDef.defName))
                        {
                            thingDef.apparel.wornGraphicPath = AVPSettings.DressesGraphicPath;
                        }
                        else
                        if (AllSuitPants.Contains(thingDef.defName))
                        {
                            thingDef.apparel.wornGraphicPath = AVPSettings.SuitPantsGraphicPath;
                        }
                        else
                        if (AllShorts.Contains(thingDef.defName))
                        {
                            thingDef.apparel.wornGraphicPath = AVPSettings.ShortsGraphicPath;
                        }
                        else
                        {
                            thingDef.apparel.wornGraphicPath = AVPSettings.PantGraphicPath;
                        }

                    }
                }
            }
        }



        public static void AddLayer()
        {
            if (AVPLayerDefOf.OnSkinPants == null)
            {
                return;
            }
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(x => FullApparel.Contains(x.defName)))
            {
                if (!thingDef.apparel.layers.Contains(AVPLayerDefOf.OnSkinPants) && thingDef.apparel.layers.Contains(ApparelLayerDefOf.OnSkin))
                {
                    thingDef.apparel.layers.Add(AVPLayerDefOf.OnSkinPants);
                }
            }
        }

    }
    [DefOf]
    public static class AVPLayerDefOf
    {
        public static ApparelLayerDef OnSkinPants;
    }



    [DefOf]
    public static class PantsListDefOf
    {
        public static PantsListDef AllDresses;
        public static PantsListDef AllSuitPants;
        public static PantsListDef AllJeans;
        public static PantsListDef AllShorts;
        public static PantsListDef OnlyPants;
        public static PantsListDef OnSkinFullApparel;
    }





    public class PantsListDef : Def
    {
        public List<string> TargetApparelDefs = new List<string>();
    }
    [StaticConstructorOnStartup]



    public static class HarmonyPatchA4
    {
        static HarmonyPatchA4()
        {
            ABsVisiblePantsMod.settings.ExposeData();
            Harmony harmony = new Harmony("AVP.DrawPawnBody.fix");
            if (AVPSettings.LayerRenderFix)
            {
                harmony.Patch(AccessTools.Method(typeof(PawnRenderer), "DrawPawnBody"), transpiler: new HarmonyMethod(typeof(HarmonyPatchA4), nameof(InputDrawPawnBody)));
            }
            if (AVPSettings.VisiblePantsEnabled && !AVPSettings.ModLayerAdd)
            {
                harmony.Patch(AccessTools.Method(typeof(Pawn_ApparelTracker), "SortWornApparelIntoDrawOrder"), transpiler: new HarmonyMethod(typeof(HarmonyPatchA4), nameof(InputApparelOrder)));
            }
        }
        public static IEnumerable<CodeInstruction> InputDrawPawnBody(IEnumerable<CodeInstruction> codes, ILGenerator generator)
        {
            Label l1 = generator.DefineLabel();
            Label l2 = generator.DefineLabel();
            foreach (CodeInstruction code in codes)
            {
                if (code.opcode == OpCodes.Ldc_R4 && (float)code.operand == 0.00289575267f)
                {
                    //0.00289575267f * 3f / (list.count > 3 ? list.count : 3)
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 0.00868725801f);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(List<Material>), nameof(List<Material>.Count)));
                    yield return new CodeInstruction(OpCodes.Ldc_I4_3);
                    yield return new CodeInstruction(OpCodes.Bgt_S, l1);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_3);
                    yield return new CodeInstruction(OpCodes.Br_S, l2);
                    yield return new CodeInstruction(OpCodes.Ldloc_3)
                    {
                        labels = new List<Label>() { l1}
                    };
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(List<Material>), nameof(List<Material>.Count)));
                    yield return new CodeInstruction(OpCodes.Conv_R4)
                    {
                        labels = new List<Label>() { l2}
                    };
                    yield return new CodeInstruction(OpCodes.Div);

                }
                else
                {
                    yield return code;
                }
            }
        }
        public static IEnumerable<CodeInstruction> InputApparelOrder(IEnumerable<CodeInstruction> codes)
        {

            MethodInfo SortByMethod = AccessTools.Method(typeof(HarmonyPatchA4), nameof(HarmonyPatchA4.SortBy));
            foreach (CodeInstruction code in codes)
            {
                if (code.opcode == OpCodes.Ldsfld)
                {
                    yield return new CodeInstruction(OpCodes.Callvirt, SortByMethod);
                    yield return new CodeInstruction(OpCodes.Ret);
                    break;
                }
                else
                {
                    yield return code;
                }
            }
        }
        public static void SortBy(List<Apparel> apparels)
        {

            apparels.SortBy(delegate (Apparel a)
            {
                int c = a.def.apparel.LastLayer.drawOrder;
                if (!AVPSettings.CoverOnSkinApparel.NullOrEmpty() && AVPSettings.CoverOnSkinApparel.ContainsKey(a.def.defName))
                {
                    int x = AVPSettings.CoverOnSkinApparel[a.def.defName] ? 1 : -1;
                    c += x;
                }
                return c;
            });
        }
    }
}
