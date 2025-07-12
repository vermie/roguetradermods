using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Prerequisites;
using Kingmaker.UnitLogic.Progression;
using System.Linq;

namespace OperativeOverseer;

public static class PrerequisiteHelper
{
    public static Prerequisite Create(BlueprintFeature prereq)
    {
        return new PrerequisiteFact()
        {
            m_Fact = prereq.ToReference<BlueprintUnitFactReference>(),
            MinRank = 1
        };
    }

    public static PrerequisitesList Create(FeaturePrerequisiteComposition composition, BlueprintFeature[] prereq)
    {
        return new()
        {
            Composition = composition,
            List = prereq.Select(p => Create(p)).ToArray()
        };
    }

    public static PrerequisitesList Create(FeaturePrerequisiteComposition composition, params IFeaturePrerequisite[] prereqs)
    {
        var list = new Prerequisite[prereqs.Length];

        for (var i = 0; i < prereqs.Length; ++i)
        {
            var entry = prereqs[i];

            if (entry is PrerequisitesList prereqList)
            {
                if (prereqList.List.Length == 1)
                {
                    list[i] = prereqList.List[0];
                }
                if (prereqList.List.Length > 1)
                {
                    list[i] = new PrerequisiteComposite()
                    {
                        Prerequisites = prereqList
                    };
                }
            }

            if (entry is Prerequisite prereq)
            {
                list[i] = prereq;
            }
        }

        return new()
        {
            Composition = composition,
            List = list
        };
    }
}
