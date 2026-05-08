using BaseLib.Config;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

namespace TransparentTheSpire.TransparentTheSpireCode;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "TransparentTheSpire";

    private static readonly List<CanvasItem> TrackedNodes = [];

    private static readonly List<StringName> BgGroupA =
    [
        "OvergrowthBackground",
        "OvergrowthBossBackground",
        "KaiserCrabBossBackground",
    ];
    private static readonly List<StringName> BgGroupB =
    [
        "UnderdocksBackground",
        "CeremonialBeastBackground",
    ];
    private static readonly List<StringName> BgGroupC =
    [
        "HiveBackground",
        "KnowledgeDemonBackground",
        "GloryBackground",
        "TestSubjectBackground",
        "FakeMerchantBackground",
        "QueenBackground",
    ];

    private static readonly List<StringName> NodeGroup = ["RestSiteBG", "RestSiteBG2"];

    private static readonly StringName SnInsatiable = "TheInsatiableBackground";

    private static readonly StringName SnMerchantRoom = "MerchantRoom";
    private static readonly StringName SnMainMenu = "MainMenu";
    private static readonly StringName SnMainMenuBg = "MainMenuBg";

    private static readonly HashSet<StringName> RelevantNames =
    [
        .. BgGroupA,
        .. BgGroupB,
        .. BgGroupC,
        .. NodeGroup,
        SnInsatiable,
        SnMainMenu,
        SnMainMenuBg,
        SnMerchantRoom,
    ];

    public static void Initialize()
    {
        ModConfigRegistry.Register(ModId, new Config());

        Harmony harmony = new(ModId);

        harmony.PatchAll();

        if (Engine.GetMainLoop() is SceneTree tree)
        {
            tree.NodeAdded += OnNodeAdded;
        }

        // prevent black boxes around light particles
        // GD.Load<CanvasItemMaterial>(
        //     "res://themes/canvas_item_material_additive_shared.tres"
        // )?.LightMode = CanvasItemMaterial.LightModeEnum.LightOnly;
    }

    private static void OnNodeAdded(Node node)
    {
        if (node is not CanvasItem canvasItem || !RelevantNames.Contains(canvasItem.Name))
            return;

        canvasItem.OnReady(() =>
        {
            if (canvasItem is NMainMenu mainMenu && canvasItem.Name == SnMainMenu)
            {
                mainMenu.DisableBackstopInstantly();
            }

            if (IsInstanceValid(canvasItem) && !TrackedNodes.Contains(canvasItem))
            {
                TrackedNodes.Add(canvasItem);
                canvasItem.TreeExited += () => TrackedNodes.Remove(canvasItem);
            }
            ApplyConfigToNode(canvasItem);
        });
    }

    public static void ApplyConfigToAllNodes()
    {
        TrackedNodes.RemoveAll(n => !IsInstanceValid(n));
        foreach (var node in TrackedNodes)
            ApplyConfigToNode(node);
    }

    private static void ApplyConfigToNode(CanvasItem canvasItem)
    {
        bool super = Config.SuperTransparentMode;
        StringName name = canvasItem.Name;

        void SetLayerVisible(string name, bool visible)
        {
            canvasItem.GetNodeOrNull<CanvasItem>(name)?.Visible = visible;
        }

        if (BgGroupA.Contains(name))
        {
            bool isVantom = canvasItem.GetNodeOrNull<ColorRect>("oil shader") != null;
            SetLayerVisible("Layer_00", !super && !isVantom);
            SetLayerVisible("Layer_01", !super && isVantom);
            SetLayerVisible("Layer_02", false);
            SetLayerVisible("Layer_03", false);
            SetLayerVisible("Layer_04", !super);
        }
        else if (BgGroupB.Contains(name))
        {
            SetLayerVisible("Layer_00", !super);
            SetLayerVisible("Layer_01", false);
            SetLayerVisible("Layer_02", false);
            SetLayerVisible("Layer_03", false);
            SetLayerVisible("Layer_04", !super);
        }
        else if (BgGroupC.Contains(name))
        {
            SetLayerVisible("Layer_00", !super);
            SetLayerVisible("Layer_01", false);
            SetLayerVisible("Layer_02", false);
            SetLayerVisible("Layer_03", !super);
        }
        else if (NodeGroup.Contains(name))
        {
            canvasItem.Visible = false;
        }
        else if (name == SnInsatiable)
        {
            SetLayerVisible("Layer_00", false);
            SetLayerVisible("Layer_01", false);
            SetLayerVisible("Layer_02", false);
        }
        else if (name == SnMainMenuBg)
        {
            SetLayerVisible("BgContainer/Bg", false);
        }
        else if (name == SnMerchantRoom)
        {
            SetLayerVisible("SceneContainer/BgContainer/SpineSprite", !super);
        }
    }

    [HarmonyPatch(typeof(NMainMenu))]
    static class NMainMenuPatches
    {
        [HarmonyPatch(nameof(NMainMenu.DisableBackstop))]
        [HarmonyPrefix]
        public static void DisableBackstop(NMainMenu __instance)
        {
            __instance.BlurBackstop.Visible = false;
        }

        [HarmonyPatch(nameof(NMainMenu.DisableBackstopInstantly))]
        [HarmonyPrefix]
        public static void DisableBackstopInstantly(NMainMenu __instance)
        {
            __instance.BlurBackstop.Visible = false;
        }

        [HarmonyPatch(nameof(NMainMenu.EnableBackstop))]
        [HarmonyPrefix]
        public static void EnableBackstop(NMainMenu __instance)
        {
            __instance.UpdateShaderLod(1f);
            __instance.UpdateShaderMix(0f);
            __instance.BlurBackstop.Visible = true;
        }

        [HarmonyPatch(nameof(NMainMenu.EnableBackstopInstantly))]
        [HarmonyPrefix]
        public static void EnableBackstopInstantly(NMainMenu __instance)
        {
            __instance.BlurBackstop.Visible = true;
        }
    }
}

public static class NodeExtensions
{
    public static void OnReady(this Node node, Action action)
    {
        if (node.IsNodeReady())
            action();
        else
            node.Ready += action;
    }
}
