using HarmonyLib;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics;
using System;

namespace OperativeOverseer;

//[HarmonyPatch(typeof(Kingmaker.UnitLogic.Buffs.Buff), "RemoveRank")]
//public static class Buff_RemoveRank_Patch
//{
//    [HarmonyPrefix]
//    public static void Prefix(Kingmaker.UnitLogic.Buffs.Buff __instance, int count)
//    {
//        var id = __instance.Blueprint.AssetGuid;
//        var name = __instance.Blueprint.name;
//        var ranks = __instance.Rank;

//        Main.log.Log($"Buff.RemoveRank: Removing {count}/{ranks} ranks from '{name}' ({id}).");
//    }
//}

//[HarmonyPatch(typeof(Kingmaker.UnitLogic.Buffs.Buff), "Remove")]
//public static class Buff_Remove_Patch
//{
//    [HarmonyPrefix]
//    public static void Prefix(Kingmaker.UnitLogic.Buffs.Buff __instance)
//    {
//        var id = __instance.Blueprint.AssetGuid;
//        var name = __instance.Blueprint.name;

//        Main.log.Log($"Buff.Remove: Removing buff '{name}' ({id}).");
//    }
//}

//[HarmonyPatch(typeof(Kingmaker.UnitLogic.Buffs.Buff), "OnRemove")]
//public static class Buff_OnRemove_Patch
//{
//    [HarmonyPrefix]
//    public static void Prefix(Kingmaker.UnitLogic.Buffs.Buff __instance)
//    {
//        if (__instance.m_StoredFacts is not List<EntityFact> children || children.Count == 0)
//        {
//            return;
//        }

//        var id = __instance.Blueprint.AssetGuid;
//        var name = __instance.Blueprint.name;

//        var names = string.Join(", ", children.Select(c => c.Blueprint.name));

//        Main.log.Log($"Removing {children.Count} buffs under '{name}' ({id}).\n\t{names}");
//    }
//}

//[HarmonyPatch(typeof(EntityFactsManager), "Remove", typeof(EntityFact), typeof(bool))]
//public static class EntityFactsManager_Remove_Patch
//{
//    [HarmonyPrefix]
//    public static void Prefix(EntityFactsManager __instance, EntityFact fact, bool dispose)
//    {
//        if (fact is Buff buff)
//        {
//            var id = fact.Blueprint.AssetGuid;
//            var name = fact.Blueprint.name;

//            Main.log.Log($"EntityFactsManager.Remove: Removing buff '{name}' ({id}).");
//        }
//    }
//}

//[HarmonyPatch(typeof(BuffCollection), "OnFactWillDetach", typeof(Buff))]
//public static class BuffCollection_OnFactWillDetach_Patch
//{
//    [HarmonyPrefix]
//    public static void Prefix(BuffCollection __instance, Buff fact)
//    {
//        if (fact is not null)
//        {
//            var id = fact.Blueprint.AssetGuid;
//            var name = fact.Blueprint.name;

//            Main.log.Log($"BuffCollection.OnFactWillDetach: About to remove buff '{name}' ({id}).");
//        }
//        else
//        {
//            Main.log.Log($"BuffCollection.OnFactWillDetach: About to remove null buff !!!!");
//        }
//    }
//}

//[HarmonyPatch(typeof(ContextActionRemoveBuff), "RunAction")]
//public static class ContextActionRemoveBuff_RunAction_Patch
//{
//    [HarmonyPrefix]
//    public static void Prefix(ContextActionRemoveBuff __instance)
//    {
//        var componentId = __instance.AssetGuid;
//        var ownerId = __instance.Owner?.AssetGuid ?? "???";

//        try
//        {
//            var mechanicsContext = ContextData<MechanicsContext.Data>.Current?.Context;

//            if (mechanicsContext == null)
//            {
//                return;
//            }

//            var mechanicEntity = (__instance.ToCaster ? mechanicsContext.MaybeCaster : __instance.Target.Entity);

//            if (mechanicEntity is null)
//            {
//                Main.log.Log($"ContextActionRemoveBuff.RunAction: mechanicEntity is null");
//            }

//            using BuffCollection.RemoveByRank removeByRank = ContextData<BuffCollection.RemoveByRank>.RequestIf(__instance.RemoveRank);
//            using SingleUnitData<CasterUnitData> singleUnitData = ContextData<CasterUnitData>.RequestIf(__instance.m_CasterRanksRemovalPolicy != 0 && mechanicsContext.MaybeCaster is BaseUnitEntity)?.Setup((BaseUnitEntity)mechanicsContext.MaybeCaster);

//            var blueprint = __instance.Buff;
//            var buff = mechanicEntity?.Buffs.GetBuff(blueprint);

//            if (buff is null)
//            {
//                Main.log.Log($"ContextActionRemoveBuff.RunAction (RemoveSeveralRanks): Couldn't find buff matching '{blueprint.AssetGuid}'. [{componentId} of {ownerId}]");
//                return;
//            }

//            var ranks = buff.Rank;

//            if (__instance.RemoveSeveralRanks)
//            {
//                var count = __instance.RankNumber.Calculate(__instance.Context);

//                Main.log.Log($"ContextActionRemoveBuff.RunAction (RemoveSeveralRanks): Removing {count}/{ranks} of '{blueprint.AssetGuid}'. [{componentId} of {ownerId}] {{{__instance.RankNumber.ValueType}: {__instance.RankNumber.PropertyName}}}");
//            }
//            else if (__instance.m_CasterRanksRemovalPolicy == ContextActionRemoveBuff.CasterRanksRemovalPolicy.All)
//            {
//                var caster = mechanicsContext.MaybeCaster;

//                if (caster is not null)
//                {
//                    Main.log.Log($"ContextActionRemoveBuff.RunAction (CasterRanksRemovalPolicy.All): Removing {ranks}/{ranks} of '{blueprint.AssetGuid}' matching caster '{caster.UniqueId}'. [{componentId} of {ownerId}]");
//                }
//                else
//                {
//                    Main.log.Log($"ContextActionRemoveBuff.RunAction (CasterRanksRemovalPolicy.All): Couldn't find caster of '{blueprint.AssetGuid}' to remove  {ranks}/{ranks}. [{componentId} of {ownerId}]");
//                }
//            }
//            else
//            {
//                Main.log.Log($"ContextActionRemoveBuff.RunAction (default): Removing {ranks}/{ranks} of '{blueprint.AssetGuid}'. [{componentId} of {ownerId}]");
//            }
//        }
//        catch (Exception ex)
//        {
//            Main.log.LogException($"ContextActionRemoveBuff.RunAction (exception): Removing buff failed. [{componentId} of {ownerId}]", ex);
//        }
//    }
//}

//[HarmonyPatch(typeof(ContextActionApplyBuff), "RunAction")]
//public static class ContextActionApplyBuff_RunAction_Patch
//{
//    [HarmonyPrefix]
//    public static void Prefix(ContextActionApplyBuff __instance)
//    {
//        var blueprint = __instance.Buff;
//        var componentId = __instance.AssetGuid;
//        var ownerId = __instance.Owner?.AssetGuid ?? "???";

//        var rankValueType = __instance.Ranks?.ValueType.ToString() ?? "???";
//        var rankValueName = __instance.Ranks?.PropertyName.ToString() ?? "???";
//        var rankinfo = $"{{{rankValueType}: {rankValueName}}}";

//        var mechanicsContext = ContextData<MechanicsContext.Data>.Current?.Context;

//        MechanicEntity buffTarget = __instance.GetBuffTarget(mechanicsContext);

//        var num = !__instance.Buff.HasRanks
//                  ? 0
//                  : Math.Max(__instance.Ranks?.Calculate(mechanicsContext) ?? 0, 1);

//        var count = num == 1 ? 0 : (num - 1);


//        Main.log.Log($"ContextActionApplyBuff.RunAction: Adding buff '{blueprint.AssetGuid}' plus {count} ranks. [{componentId} of {ownerId}] {rankinfo}");
//    }
//}

//[HarmonyPatch(typeof(PropertyCalculatorComponent), "GetValue", typeof(PropertyContext))]
//public static class PropertyCalculatorComponent_GetValue_Patch
//{
//    [HarmonyPrefix]
//    public static void Prefix(PropertyCalculatorComponent __instance, PropertyContext context)
//    {
//        var name = __instance.name;

//        var blueprint = __instance.OwnerBlueprint;
//        var ownerId = blueprint?.AssetGuid ?? "???";

//        var propertyName = __instance.Name;

//        var calculator = __instance.Value;

//        try
//        {
//            var targetEntity = context.GetTargetEntity(calculator.TargetType, calculator.TargetEvaluator);

//            if (targetEntity is null)
//            {
//                Main.log.Log($"Computing property '{propertyName}': invalid target of type {calculator.TargetType}.");
//                return;
//            }

//            context = context.WithCurrentEntity(targetEntity);

//            using var data = ContextData<PropertyContextData>.Request().Setup(context);

//            var result = PropertyCalculatorHelper.CalculateValue(calculator.Getters, calculator.Operation, calculator);

//            Main.log.Log($"Computing property '{propertyName}' as {result}. [{name} from {ownerId}]");
//        }
//        catch (Exception ex)
//        {
//            Main.log.LogException($"Computing property '{propertyName}' resulted in exception. [{name} from {ownerId}]", ex);
//        }
//    }
//}

//[HarmonyPatch(typeof(WarhammerDamageTriggerInitiator), "OnTrigger", typeof(RuleDealDamage))]
//public static class WarhammerDamageTriggerInitiator_OnTrigger_Patch
//{
//    [HarmonyPrefix]
//    public static void Prefix(WarhammerDamageTriggerInitiator __instance, RuleDealDamage rule)
//    {
//        var initiator = rule.Initiator?.UniqueId ?? "???";
//        var target = rule.Target?.UniqueId ?? "???";

//        string itemText = "unknown";
//        string abilityText = "unknown";

//        if (rule.Reason.Item is Kingmaker.Items.ItemEntity item)
//        {
//            itemText = item.Blueprint?.AssetGuid ?? "unknown";
//        }
//        if (rule.Reason.Ability is AbilityData ability)
//        {
//            abilityText = ability.Blueprint?.AssetGuid ?? "unknown";
//        }

//        Main.log.Log($"WarhammerDamageTriggerInitiator.OnTrigger: {initiator} is dealing damage to {target}. [item:{itemText}, ability:{abilityText}]");
//    }
//}
