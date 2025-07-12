using HarmonyLib;
using System.Reflection;
using static UnityModManagerNet.UnityModManager.ModEntry;
using static UnityModManagerNet.UnityModManager;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Localization;
using System.IO;
using Kingmaker.Localization.Enums;
using Kingmaker.Localization.Shared;
using OperativeOverseer.Features;

namespace OperativeOverseer;

static class Main
{
    [HarmonyPatch(typeof(BlueprintsCache), "Init")]
    public static class BlueprintsCache_Init_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(BlueprintsCache __instance)
        {
            AnalysisRecall.AddFeature(__instance);
        }
    }

    private static Assembly _assembly = null!;

    private static void LoadLocalizationPacks(Locale locale)
    {
        log.Log($"Loading localization '{locale}'.");

        var assemblyPath = _assembly.Location;
        var file = new FileInfo(assemblyPath);

        var parent = file.DirectoryName;

        var localeFile = Path.Combine(parent, "Localization", $"{locale}.json");

        if (!File.Exists(localeFile))
        {
            localeFile = Path.Combine(parent, "Localization", $"{Locale.enGB}.json");
        }

        var pack = LocalizationManager.Instance.LoadPack(localeFile, locale);
        LocalizationManager.Instance.CurrentPack.AddStrings(pack);
    }

    internal static Harmony HarmonyInstance;

    internal static ModLogger log;

    private static bool Load(ModEntry modEntry)
    {
        log = modEntry.Logger;
        HarmonyInstance = new Harmony(modEntry.Info.Id);
        _assembly = Assembly.GetExecutingAssembly();

        // load default locale, and setup loading new locales
        var localizationManager = (ILocalizationProvider)LocalizationManager.Instance;
        localizationManager.LocaleChanged += LoadLocalizationPacks;

        // load the mod
        HarmonyInstance.PatchAll(_assembly);

        return true;
    }

    private static void OnGUI(ModEntry modEntry)
    {
    }
}