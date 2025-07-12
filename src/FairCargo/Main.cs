using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using static UnityModManagerNet.UnityModManager.ModEntry;
using static UnityModManagerNet.UnityModManager;
using Kingmaker.Cargo;
using Kingmaker.Controllers;
using Kingmaker.Enums;

namespace FairCargo;

static class Main
{
    [HarmonyPatch(typeof(CargoEntity), "ReputationPointsCost", MethodType.Getter)]
    public static class CargoEntity_ReputationPointsCost_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(CargoEntity __instance, ref int __result)
        {
            __result = __result * __instance.FilledVolumePercent / 100;
        }
    }

    internal static Harmony HarmonyInstance;

    internal static ModLogger log;

    private static bool Load(ModEntry modEntry)
    {
        //IL_0016: Unknown result type (might be due to invalid IL or missing references)
        //IL_0020: Expected O, but got Unknown
        log = modEntry.Logger;
        HarmonyInstance = new Harmony(modEntry.Info.Id);
        HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        return true;
    }

    private static void OnGUI(ModEntry modEntry)
    {
    }
}