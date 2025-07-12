using Kingmaker.Localization;

namespace OperativeOverseer.Strings;

public class TalentStrings
{
    public string FeatureId { get; init; }
    public ComponentId ComponentId { get; init; }

    public string? FeatureName { get; init; }

    public LocalizedString? DisplayName { get; init; }
    public LocalizedString? Description { get; init; }
}
