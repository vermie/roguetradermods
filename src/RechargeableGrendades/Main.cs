using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager.ModEntry;
using static UnityModManagerNet.UnityModManager;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Weapons;
using System.Linq;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;

namespace RechargeableGrenades;

static class Main
{
    [HarmonyPatch(typeof(BlueprintsCache), "Init")]
    public static class BlueprintsCache_Init_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(BlueprintsCache __instance)
        {
            FixItems(__instance);
        }

        private static void FixItems(BlueprintsCache __instance)
        {
            foreach (var item in _items)
            {
                if (__instance.Load(item) is not BlueprintItemEquipmentUsable equipment)
                {
                    continue;
                }

                equipment.RestoreChargesAfterCombat = true;
                equipment.RemoveFromSlotWhenNoCharges = false;
            }
        }

        private static readonly string[] _items =
        [
            /* fire */"9a7233892a414a52a5cd4fb4367a5d99",
            /* frag */"0088d2c1ea084c1428266b7ffdeb6ab1",
            /* krak */"20650117b63acf740998b602e108c51a",
            /* stun */"53a441db74d4461396938c775513dee6",
        ];
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