using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager.ModEntry;
using static UnityModManagerNet.UnityModManager;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints;
using System.Linq;
using Kingmaker.UnitLogic.Buffs.Blueprints;

namespace BuffSpam;

static class Main
{
    [HarmonyPatch(typeof(BlueprintsCache), "Init")]
    public static class BlueprintsCache_Init_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(BlueprintsCache __instance)
        {
            foreach (var buffId in _buffsToHide)
            {
                if (__instance.Load(buffId) is not BlueprintBuff buffBlueprint)
                {
                    continue;
                }

                buffBlueprint.m_Flags |= BlueprintBuff.Flags.HiddenInUi;
            }

            foreach (var buffId in _buffsToSilence)
            {
                if (__instance.Load(buffId) is not BlueprintBuff buffBlueprint)
                {
                    continue;
                }

                buffBlueprint.m_Flags |= BlueprintBuff.Flags.ShowInLogOnlyOnYourself;
            }
        }

        private static readonly string[] _buffsToHide =
        [
            "d274864dc8b34a23a0c9e4847a98d801", // emperor's voice
            "80d8f7b11612492aa26f4f21a6945cff", // influential servant
        ];

        private static readonly string[] _buffsToSilence =
        [
            "8608f3b05df145dda225b03215821fa8", // flagbearer
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