using Kingmaker.Localization;
using System.Runtime.InteropServices;

namespace OperativeOverseer.Strings;

public static class Overseer
{
    //public static string ClassId { get; } = "";
    public static string CareerId { get; } = "21b0fc8cfbe940ecbef0114d5d27b44a";

    public static class Keystone
    {
        public static string Servoskull { get; } = "dc7ce6fdeb964544b613e749c2610623";
    }

    public static class Talents
    {
        public static TalentStrings AnalysisRecall = new()
        {
            FeatureId = "9dfa01f37b964c718db626788a7c4045",
            ComponentId = (ModInfo.MOD_ID, ModInfo.FEATURE_TALENT_ANALYSIS_RECALL),

            FeatureName = "Overseer_Operative_AnalysisRecall_Talent",

            DisplayName = Localize.CreateShared("82c6b9c7-1f7c-4824-a1af-338a57d67413"),
            Description = Localize.CreateShared("f9d3632c-dad3-4402-8237-12014513a04f")
        };
    }

    public static class Abilities
    {
        public static class Eagle
        {

        }

        public static class Mastiff
        {

        }

        public static class Raven
        {

        }

        public static class Servoskull
        {

        }
    }

    public static class Buffs
    {
        public static class Eagle
        {

        }

        public static class Mastiff
        {

        }

        public static class Raven
        {

        }

        public static class Servoskull
        {
            public static string Extrapolate { get; } = "9562d825f46c48c482846ccc883310a7";
        }
    }
}
