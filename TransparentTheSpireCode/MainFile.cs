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

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        ModConfigRegistry.Register(ModId, new Config());

        Harmony harmony = new(ModId);

        harmony.PatchAll();

        if (Engine.GetMainLoop() is SceneTree tree)
        {
            tree.NodeAdded += OnNodeAdded;
        }
    }

    private static void OnNodeAdded(Node node)
    {
        if (node is Control control)
        {
            control.OnReady(() => ProcessNode(control));
        }
    }

    private static void ProcessNode(Control control)
    {
        if (!Config.SuperTransparentMode)
        {
            switch (control.Name)
            {
                case "OvergrowthBackground":
                case "OvergrowthBossBackground":
                case "KaiserCrabBossBackground":
                    control.GetNode<Control>("Layer_01").Visible = false;
                    control.GetNode<Control>("Layer_02").Visible = false;
                    control.GetNode<Control>("Layer_03").Visible = false;

                    // Vantom
                    if (control.GetNodeOrNull<ColorRect>("oil shader") is not null)
                    {
                        control.GetNode<Control>("Layer_00").Visible = false;
                        control.GetNode<Control>("Layer_01").Visible = true;
                    }
                    break;
                case "UnderdocksBackground":
                case "CeremonialBeastBackground":
                    control.GetNode<Control>("Layer_00").Visible = false;
                    control.GetNode<Control>("Layer_01").Visible = false;
                    control.GetNode<Control>("Layer_02").Visible = false;
                    control.GetNode<Control>("Layer_03").Visible = false;
                    break;
                case "HiveBackground":
                case "KnowledgeDemonBackground":
                    control.GetNode<Control>("Layer_01").Visible = false;
                    control.GetNode<Control>("Layer_02").Visible = false;
                    break;
                case "TheInsatiableBackground":
                    control.GetNode<Control>("Layer_00").Visible = false;
                    control.GetNode<Control>("Layer_01").Visible = false;
                    control.GetNode<Control>("Layer_02").Visible = false;
                    break;
                case "GloryBackground":
                case "TestSubjectBackground":
                    control.GetNode<Control>("Layer_01").Visible = false;
                    control.GetNode<Control>("Layer_02").Visible = false;
                    break;
                case "MainMenu":
                    if (control is not NMainMenu mainMenu)
                        break;
                    mainMenu.DisableBackstopInstantly();
                    break;
                case "MainMenuBg":
                    control.GetNode<CanvasItem>("BgContainer/Bg").Visible = false;
                    control.GetNode<CanvasItem>("%Logo/light_large").Visible = false;
                    break;
            }
        }
        else
        {
            switch (control.Name)
            {
                case "TheInsatiableBackground":
                    control.GetNode<Control>("Layer_00").Visible = false;
                    control.GetNode<Control>("Layer_01").Visible = false;
                    control.GetNode<Control>("Layer_02").Visible = false;
                    break;

                case "FakeMerchantBackground":
                case "GloryBackground":
                case "HiveBackground":
                case "KnowledgeDemonBackground":
                case "QueenBackground":
                case "TestSubjectBackground":
                    control.GetNode<Control>("Layer_00").Visible = false;
                    control.GetNode<Control>("Layer_01").Visible = false;
                    control.GetNode<Control>("Layer_02").Visible = false;
                    control.GetNode<Control>("Layer_03").Visible = false;
                    break;

                case "CeremonialBeastBackground":
                case "KaiserCrabBossBackground":
                case "OvergrowthBackground":
                case "OvergrowthBossBackground":
                case "UnderdocksBackground":
                    control.GetNode<Control>("Layer_00").Visible = false;
                    control.GetNode<Control>("Layer_01").Visible = false;
                    control.GetNode<Control>("Layer_02").Visible = false;
                    control.GetNode<Control>("Layer_03").Visible = false;
                    control.GetNode<Control>("Layer_04").Visible = false;
                    break;

                case "MainMenu":
                    if (control is not NMainMenu mainMenu)
                        break;
                    mainMenu.DisableBackstopInstantly();
                    break;
                case "MainMenuBg":
                    control.GetNode<CanvasItem>("BgContainer/Bg").Visible = false;
                    control.GetNode<CanvasItem>("%Logo/light_large").Visible = false;
                    break;
            }
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
