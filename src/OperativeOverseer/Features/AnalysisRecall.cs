using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.EntitySystem.Properties.Getters;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.UnitLogic.Progression;
using OperativeOverseer.Traversal;
using System;
using System.Linq;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using System.Xml.Schema;
using OperativeOverseer.Strings;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.EntitySystem;
using static Kingmaker.UnitLogic.Buffs.BuffCollection;

namespace OperativeOverseer.Features;

public static class AnalysisRecall
{
    public static BlueprintFeature? AddFeature(BlueprintsCache __instance)
    {
        if (__instance.Load(Strings.Overseer.CareerId) is not BlueprintCareerPath careerOverseer)
        {
            return null;
        }
        if (__instance.Load(Strings.Overseer.Keystone.Servoskull) is not BlueprintFeature featureServoskullKeystone)
        {
            return null;
        }
        if (__instance.Load(Strings.Operative.Abilities.AnalyseEnemies.FeatureId) is not BlueprintFeature featureAnalyseEnemies)
        {
            return null;
        }

        var talentAnalysisRecall = new BlueprintFeature()
        {
            AssetGuid = Strings.Overseer.Talents.AnalysisRecall.FeatureId,
            name = Strings.Overseer.Talents.AnalysisRecall.FeatureName,

            Ranks = 1,

            m_DisplayName = Strings.Overseer.Talents.AnalysisRecall.DisplayName,
            m_Description = Strings.Overseer.Talents.AnalysisRecall.Description,

            Components = [],

            Prerequisites = PrerequisiteHelper.Create(FeaturePrerequisiteComposition.And,
            [
                PrerequisiteHelper.Create(featureAnalyseEnemies),
                PrerequisiteHelper.Create(featureServoskullKeystone)
            ]),

            FeatureTypes = [BlueprintFeature.FeatureType.Universal, BlueprintFeature.FeatureType.Support, BlueprintFeature.FeatureType.Archetype],
            TalentIconInfo = new TalentIconInfo() { MainGroup = TalentGroup.Occupation, AllGroups = TalentGroup.Occupation | TalentGroup.Debuff },
            HideNotAvailibleInUI = true
        };

        careerOverseer.GetComponents<AddFeaturesToLevelUp>()
                      .First(c => c.Group == FeatureGroup.Talent)
                      .Editor_AddFeature(talentAnalysisRecall);

        __instance.AddCachedBlueprint(talentAnalysisRecall.AssetGuid, talentAnalysisRecall);

        AddAnalysisRecallToAnalyseEnemies(__instance, talentAnalysisRecall);
        AddAnalysisRecallToExposeWeakness(__instance, talentAnalysisRecall);
        AddAnalysisRecallToTacticalKnowledge(__instance, talentAnalysisRecall);

        return talentAnalysisRecall;
    }

    private static void AddAnalysisRecallToAnalyseEnemies(BlueprintsCache __instance, BlueprintFeature talentAnalysisRecall)
    {
        if (__instance.Load(Strings.Overseer.Buffs.Servoskull.Extrapolate) is not BlueprintBuff buffExtrapolate)
        {
            return;
        }
        if (__instance.Load(Strings.Operative.Buffs.ExploitEffect) is not BlueprintBuff buffExploitEffect)
        {
            return;
        }
        if (__instance.Load(Strings.Operative.Abilities.AnalyseEnemies.FeatureId) is not BlueprintFeature featureAnalyseEnemies)
        {
            return;
        }

        var componentId = new ComponentId(ModInfo.MOD_ID, ModInfo.FEATURE_TALENT_ANALYSIS_RECALL, 0x00);

        // remove all actions which consume Exploit ranks
        // there's some messy duplicated logic -- easier to start from scratch
        {
            var visitor = new BuffRemoverVisitor(buffExploitEffect);
            visitor.Visit(featureAnalyseEnemies);

            foreach (var (list, remover) in visitor.Matches)
            {
                var array = list.Actions;
                var index = Array.IndexOf(array, remover);
                array[index] = null!;

                list.Actions = array.Where(e => e is not null).ToArray();
            }
        }

        // make a component which computes the number of ranks to consume
        // our version counts only Exploit ranks (ignores ElderFlow?)
        var rankToRemoveCounterComponent = CreateConditionallyHalvedRankCounterComponent(componentId, featureAnalyseEnemies, talentAnalysisRecall, buffExtrapolate, buffExploitEffect, PropertyTargetType.RuleTarget);

        // make a component which consumes Exploit ranks
        // normally happens with both:
        //  * WarhammerDamageTriggerInitiator
        //  * WarhammerWeaponHitTriggerInitiator
        // using both duplicates this logic, which ends up removing too many ranks
        // this new component is the only way ranks get removed
        // further, it can remove a variable amount of ranks
        var consumeExploitRanksComponent = new WarhammerDamageTriggerInitiator()
        {
            TriggersForDamageOverTime = false,

            OwnerBlueprint = featureAnalyseEnemies,
            name = $"$WarhammerDamageTriggerInitiator${componentId.Next()}",

            Actions = new ActionList()
            {
                Actions =
                [
                    new ContextActionRemoveBuff()
                    {
                        Owner = featureAnalyseEnemies,
                        name = $"$ContextActionRemoveBuff${componentId.Next()}",

                        m_Buff = buffExploitEffect.ToReference<BlueprintBuffReference>(),

                        RemoveRank = true,
                        RemoveSeveralRanks = true,
                        RankNumber = new ContextValue()
                        {
                            PropertyName = rankToRemoveCounterComponent.Name,
                            ValueType = ContextValueType.TargetNamedProperty
                        }
                    }
                ]
            }
        };

        // add the new components
        featureAnalyseEnemies.Components =
        [
            .. featureAnalyseEnemies.Components,
            rankToRemoveCounterComponent,
            consumeExploitRanksComponent
        ];
    }

    private static void AddAnalysisRecallToTacticalKnowledge(BlueprintsCache __instance, BlueprintFeature talentAnalysisRecall)
    {
        if (__instance.Load(Strings.Overseer.Buffs.Servoskull.Extrapolate) is not BlueprintBuff buffExtrapolate)
        {
            return;
        }
        if (__instance.Load(Strings.Operative.Buffs.Exploit) is not BlueprintBuff buffExploit)
        {
            return;
        }
        if (__instance.Load(Strings.Operative.Buffs.ExploitEffect) is not BlueprintBuff buffExploitEffect)
        {
            return;
        }
        if (__instance.Load(Strings.Operative.Buffs.TacticalKnowledgeArmor) is not BlueprintBuff buffTacticalKnowledgeArmor)
        {
            return;
        }
        if (__instance.Load(Strings.Operative.Buffs.TacticalKnowledgeDamageCounter) is not BlueprintBuff buffTacticalKnowledgeDamageCounter)
        {
            return;
        }
        if (__instance.Load(Strings.Operative.Abilities.TacticalKnowledge.AbilityId) is not BlueprintAbility abilityTacticalKnowledge)
        {
            return;
        }

        // Tactical Knowledge ability is structured as:
        //  RunAction ->
        //    Conditional ->
        //      IfTrue -> (single target stuff, involves looping for each debuff rank, too complicated)
        //      IfFalse -> (not relevant)
        // we'll add a component which computes the number of ranks to remove
        // then replace the IfTrue branch to remove the correct number of ranks

        var componentId = new ComponentId(ModInfo.MOD_ID, ModInfo.FEATURE_TALENT_ANALYSIS_RECALL, 0x01);

        var buffRankTracking = new BlueprintBuff()
        {
            AssetGuid = Strings.Operative.Buffs.TacticalKnowledgeRankTracker,
            name = $"Adept_TacticalKnowledge_ExploitRankTrackingBuff",

            Stacking = StackingType.Rank,
            Ranks = 100,

            m_Flags = BlueprintBuff.Flags.HiddenInUi,

            Components = [],

            // crap necessary to avoid crashes?
            m_DisplayName = Localize.CreateShared("6f52dc6f-0083-4b4c-9334-2bffac0dd0d4"),
            m_Description = Localize.CreateShared("11c86592-34c5-436e-bab6-5bf3a3727175"),
            FxOnStart = new Kingmaker.ResourceLinks.PrefabLink() { AssetId = ""},
            FxOnRemove = new Kingmaker.ResourceLinks.PrefabLink() { AssetId = "" },
            m_FXSettings = null,
            m_Icon = buffTacticalKnowledgeArmor.m_Icon,
            m_SoundTypeSwitch = buffTacticalKnowledgeArmor.m_SoundTypeSwitch,
            m_MuffledTypeSwitch = buffTacticalKnowledgeArmor.m_MuffledTypeSwitch,
            ResourceAssetIds = [],
            m_AbilityGroups = []
        };

        // make a component which counts the number of tracking ranks
        var trackingRankCounterComponent = CreateRankCounterComponent(componentId, abilityTacticalKnowledge, buffRankTracking, ContextPropertyName.Bonus1, PropertyTargetType.ContextCaster);

        // make a component which counts the number of Exploit ranks
        var exploitRankCounterComponent = CreateRankCounterComponent(componentId, abilityTacticalKnowledge, buffExploitEffect, ContextPropertyName.Value1, PropertyTargetType.CurrentTarget);

        // make a component which computes the number of Exploit ranks to consume
        var exploitRankToRemoveCounterComponent = CreateConditionallyHalvedRankCounterComponent(componentId, abilityTacticalKnowledge, talentAnalysisRecall, buffExtrapolate, buffExploitEffect, PropertyTargetType.CurrentTarget);

        // make a component which runs on all targets in the area
        var runOnTargetsComponent = new AbilityEffectRunAction()
        {
            OwnerBlueprint = abilityTacticalKnowledge,
            name = $"$RunActionOnTargetsInAreaEffect${componentId.Next()}",

            Actions = new ActionList()
            {
                Actions =
                [
                    new Conditional()
                    {
                        Owner = abilityTacticalKnowledge,
                        name = $"$Conditional${componentId.Next()}",

                        // check if there's only one target
                        ConditionsChecker = new ConditionsChecker()
                        {
                            Conditions =
                            [
                                new ContextConditionProperty()
                                {
                                    Owner = abilityTacticalKnowledge,
                                    name = $"$ContextConditionProperty${componentId.Next()}",

                                    Property = new PropertyCalculator()
                                    {
                                        Operation = PropertyCalculator.OperationType.LE,
                                        TargetType = PropertyTargetType.CurrentEntity,

                                        Getters =
                                        [
                                            new AbilityTargetsInPatternGetter()
                                            {
                                                Owner = abilityTacticalKnowledge,
                                                name = $"$AbilityTargetsInPatternGetter${componentId.Next()}",

                                                Settings = CreateEmptyGetterSettings()
                                            },
                                            new ContextValueGetter()
                                            {
                                                Owner = abilityTacticalKnowledge,
                                                name = $"$ContextValueGetter${componentId.Next()}",

                                                Settings = CreateEmptyGetterSettings(),
                                                Value = new ContextValue()
                                                {
                                                    Value = 1,
                                                    ValueType = ContextValueType.Simple
                                                }
                                            }
                                        ]
                                    }
                                }
                            ]
                        },

                        // do single-target effect
                        IfTrue = new ActionList()
                        {
                            Actions =
                            [
                                // increment tracking buff ranks by [number of Exploit ranks]
                                new ContextActionApplyBuff()
                                {
                                    Owner = abilityTacticalKnowledge,
                                    name = $"$ContextActionApplyBuff${componentId.Next()}",

                                    m_Buff = buffRankTracking.ToReference<BlueprintBuffReference>(),

                                    Permanent = true,
                                    ToCaster = true,

                                    BuffEndCondition = Kingmaker.UnitLogic.Buffs.BuffEndCondition.CombatEnd,

                                    Ranks = new ContextValue()
                                    {
                                        PropertyName = exploitRankCounterComponent.Name,
                                        ValueType = ContextValueType.TargetNamedProperty
                                    },
                                },

                                // consume exploit ranks
                                new ContextActionRemoveBuff()
                                {
                                    Owner = abilityTacticalKnowledge,
                                    name = $"$ContextActionRemoveBuff${componentId.Next()}",

                                    m_Buff = buffExploitEffect.ToReference<BlueprintBuffReference>(),

                                    RemoveRank = true,
                                    RemoveSeveralRanks = true,
                                    RankNumber = new ContextValue()
                                    {
                                        PropertyName = exploitRankToRemoveCounterComponent.Name,
                                        ValueType = ContextValueType.TargetNamedProperty
                                    }
                                }
                            ]
                        },

                        // do aoe-effect
                        IfFalse = new ActionList()
                        {
                            Actions =
                            [
                                // increment tracking buff ranks by one
                                new ContextActionApplyBuff()
                                {
                                    Owner = abilityTacticalKnowledge,
                                    name = $"$ContextActionApplyBuff${componentId.Next()}",

                                    m_Buff = buffRankTracking.ToReference<BlueprintBuffReference>(),

                                    Permanent = true,
                                    ToCaster = true,

                                    BuffEndCondition = Kingmaker.UnitLogic.Buffs.BuffEndCondition.CombatEnd,

                                    Ranks = new ContextValue()
                                    {
                                        ValueType = ContextValueType.Simple,
                                        Value = 1
                                    },
                                },

                                // consume exploit ranks
                                new ContextActionRemoveBuff()
                                {
                                    Owner = abilityTacticalKnowledge,
                                    name = $"$ContextActionRemoveBuff${componentId.Next()}",

                                    m_Buff = buffExploitEffect.ToReference<BlueprintBuffReference>(),

                                    RemoveRank = true,
                                    RemoveSeveralRanks = true,
                                    RankNumber = new ContextValue()
                                    {
                                        ValueType = ContextValueType.Simple,
                                        Value = 1
                                    }
                                }
                            ]
                        }
                    }
                ]
            }
        };

        var modifyBuffsComponent = new AbilityEffectRunAction()
        {
            OwnerBlueprint = abilityTacticalKnowledge,
            name = $"$AbilityEffectRunAction${componentId.Next()}",

            Actions = new ActionList()
            {
                Actions =
                [
                    // apply armor buff to allies
                    new ContextActionOnAllUnitsInCombat()
                    {
                        Owner = abilityTacticalKnowledge,
                        name = $"$ContextActionOnAllUnitsInCombat${componentId.Next()}",

                        OnlyAllies = true,
                        OnlyEnemies = false,

                        Actions = new ActionList()
                        {
                            Actions =
                            [
                                // add armor buff ranks
                                new ContextActionApplyBuff()
                                {
                                    Owner = abilityTacticalKnowledge,
                                    name = $"$ContextActionApplyBuff${componentId.Next()}",

                                    m_Buff = buffTacticalKnowledgeArmor.ToReference<BlueprintBuffReference>(),

                                    Permanent = true,
                                    BuffEndCondition = Kingmaker.UnitLogic.Buffs.BuffEndCondition.CombatEnd,

                                    ToCaster = false,
                                    AsChild = true,

                                    Ranks = new ContextValue()
                                    {
                                        ValueType = ContextValueType.CasterNamedProperty,
                                        PropertyName = trackingRankCounterComponent.Name
                                    }
                                }
                            ]
                        }
                    },

                    // apply damage buff to caster (using counter mechanism)
                    new ContextActionApplyBuff()
                    {
                        Owner = abilityTacticalKnowledge,
                        name = $"$ContextActionApplyBuff${componentId.Next()}",

                        m_Buff = buffTacticalKnowledgeDamageCounter.ToReference<BlueprintBuffReference>(),

                        Permanent = true,
                        BuffEndCondition = Kingmaker.UnitLogic.Buffs.BuffEndCondition.CombatEnd,

                        ToCaster = true,
                        AsChild = true,

                        Ranks = new ContextValue()
                        {
                            ValueType = ContextValueType.CasterNamedProperty,
                            PropertyName = trackingRankCounterComponent.Name
                        }
                    },

                    // remove the rank tracking buff
                    new ContextActionRemoveBuff()
                    {
                        Owner = abilityTacticalKnowledge,
                        name = $"$ContextActionRemoveBuff${componentId.Next()}",

                        m_Buff = buffRankTracking.ToReference<BlueprintBuffReference>(),

                        RemoveRank = false,
                        ToCaster = true
                    }
                ]
            }
        };

        // put the components together in the right order
        var targetPattern = abilityTacticalKnowledge.Components.OfType<AbilityTargetsInPattern>().First();

        targetPattern.m_Condition = new ConditionsChecker()
        {
            Conditions =
            [
                new ContextConditionProperty()
                {
                    Owner = abilityTacticalKnowledge,
                    name = $"$ContextConditionProperty${componentId.Next()}",

                    Property = new PropertyCalculator()
                    {
                        Getters =
                        [
                            new HasFactGetter()
                            {
                                Owner = abilityTacticalKnowledge,
                                name = $"$HasFactGetter${componentId.Next()}",

                                m_Fact = buffExploitEffect.ToReference<BlueprintUnitFactReference>()
                            }
                        ]
                    }
                }
            ]
        };

        abilityTacticalKnowledge.Components =
        [
            targetPattern,

            trackingRankCounterComponent,
            exploitRankCounterComponent,
            exploitRankToRemoveCounterComponent,

            runOnTargetsComponent,

            modifyBuffsComponent
        ];

        __instance.AddCachedBlueprint(buffRankTracking.AssetGuid, buffRankTracking);
    }

    private static void AddAnalysisRecallToExposeWeakness(BlueprintsCache __instance, BlueprintFeature talentAnalysisRecall)
    {
        if (__instance.Load(Strings.Overseer.Buffs.Servoskull.Extrapolate) is not BlueprintBuff buffExtrapolate)
        {
            return;
        }
        if (__instance.Load(Strings.Operative.Buffs.ExploitEffect) is not BlueprintBuff buffExploitEffect)
        {
            return;
        }
        if (__instance.Load(Strings.Operative.Abilities.ExposeWeakness.AbilityId) is not BlueprintAbility abilityExposeWeakness)
        {
            return;
        }

        var componentId = new ComponentId(ModInfo.MOD_ID, ModInfo.FEATURE_TALENT_ANALYSIS_RECALL, 0x02);

        // make a component which computes the number of ranks to consume
        // our version counts only Exploit ranks (ignores ElderFlow?)
        var rankToRemoveCounterComponent = CreateConditionallyHalvedRankCounterComponent(componentId, abilityExposeWeakness, talentAnalysisRecall, buffExtrapolate, buffExploitEffect, PropertyTargetType.CurrentTarget);

        // remove all actions which consume Exploit ranks
        {
            var visitor = new BuffRemoverVisitor(buffExploitEffect);
            visitor.Visit(abilityExposeWeakness);

            var (_, removeExpolitRanksComponent) = visitor.Matches.Single();

            removeExpolitRanksComponent.RemoveRank = true;
            removeExpolitRanksComponent.RemoveSeveralRanks = true;
            removeExpolitRanksComponent.RankNumber = new ContextValue()
            {
                PropertyName = rankToRemoveCounterComponent.Name,
                ValueType = ContextValueType.TargetNamedProperty
            };
        }

        abilityExposeWeakness.Components =
        [
            .. abilityExposeWeakness.Components,
            rankToRemoveCounterComponent
        ];
    }

    private static PropertyGetterSettings CreateEmptyGetterSettings()
    {
        return new()
        {
            Progression = PropertyGetterSettings.ProgressionType.AsIs,
            Limit = PropertyGetterSettings.LimitType.None,
            m_CustomProgression = []
        };
    }

    private static PropertyGetter CreateBuffRankGetter(ComponentId componentId, BlueprintBuff buff)
    {
        return new FactRankGetter()
        {
            name = $"$FactRankGetter${componentId.Next()}",
            m_Fact = buff.ToReference<BlueprintUnitFactReference>(),
            Settings = CreateEmptyGetterSettings()
        };
    }

    private static PropertyCalculator CreateHalvedRankCounter(ComponentId componentId, BlueprintBuff buffExploitEffect, PropertyTargetType targetType)
    {
        return new PropertyCalculator()
        {
            TargetType = targetType,
            Operation = PropertyCalculator.OperationType.Sum,
            Getters =
            [
                new PropertyCalculatorGetter()
                {
                    name = $"$PropertyCalculatorGetter${componentId.Next()}",
                    Settings = new() { Progression = PropertyGetterSettings.ProgressionType.Div2 },
                    Value = new PropertyCalculator()
                    {
                        Operation = PropertyCalculator.OperationType.Sum,
                        TargetType = targetType,

                        Getters =
                        [
                            CreateBuffRankGetter(componentId, buffExploitEffect)
                        ]
                    }
                }
            ]
        };
    }

    private static PropertyCalculator CreateConditionallyHalvedRankCounter(ComponentId componentId, BlueprintFeature talentAnalysisRecall, BlueprintBuff buffExtrapolate, BlueprintBuff buffExploitEffect, PropertyTargetType targetType)
    {
        return new PropertyCalculator()
        {
            TargetType = targetType,
            Operation = PropertyCalculator.OperationType.Sum,

            Getters =
            [
                new ConditionalGetter()
                {
                    name = $"$ConditionalGetter${componentId.Next()}",
                    Settings = CreateEmptyGetterSettings(),
                    Condition = new PropertyCalculator()
                    {
                        TargetType = targetType,
                        Operation = PropertyCalculator.OperationType.And,
                        Getters =
                        [
                            new CheckAbilityCasterHasFactGetter()
                            {
                                name = $"$CheckAbilityCasterHasFactGetter${componentId.Next()}",
                                CheckedFact = talentAnalysisRecall.ToReference<BlueprintUnitFactReference>(),
                                Settings = CreateEmptyGetterSettings()
                            },
                            new HasBuffFromMyPetGetter()
                            {
                                name = $"HasBuffFromMyPetGetter${componentId.Next()}",
                                m_Fact = buffExtrapolate.ToReference<BlueprintBuffReference>()
                            }
                        ]
                    },
                    True = CreateHalvedRankCounter(componentId, buffExploitEffect, targetType),
                    False = new PropertyCalculator()
                    {
                        Operation = PropertyCalculator.OperationType.Sum,
                        TargetType = targetType,

                        Getters =
                        [
                            CreateBuffRankGetter(componentId, buffExploitEffect)
                        ]
                    }
                }
            ]
        };
    }

    private static PropertyCalculatorComponent CreateConditionallyHalvedRankCounterComponent(ComponentId componentId, BlueprintScriptableObject owner, BlueprintFeature talentAnalysisRecall, BlueprintBuff buffExtrapolate, BlueprintBuff buffExploitEffect, PropertyTargetType targetType)
    {
        return new PropertyCalculatorComponent()
        {
            Name = ContextPropertyName.Value2,
            SaveToContext = PropertyCalculatorComponent.SaveToContextType.No,

            OwnerBlueprint = owner,
            name = $"$PropertyCalculatorComponent${componentId.Next()}",

            Value = CreateConditionallyHalvedRankCounter(componentId, talentAnalysisRecall, buffExtrapolate, buffExploitEffect, targetType),
        };
    }

    private static PropertyCalculatorComponent CreateRankCounterComponent(ComponentId componentId, BlueprintScriptableObject owner, BlueprintBuff buff, ContextPropertyName propertyName, PropertyTargetType targetType)
    {
        return new PropertyCalculatorComponent()
        {
            Name = propertyName,
            SaveToContext = PropertyCalculatorComponent.SaveToContextType.No,

            OwnerBlueprint = owner,
            name = $"$PropertyCalculatorComponent${componentId.Next()}",

            Value = new PropertyCalculator()
            {
                TargetType = targetType,
                Operation = PropertyCalculator.OperationType.Sum,

                Getters =
                [
                    CreateBuffRankGetter(componentId, buff)
                ]
            }
        };
    }
}
